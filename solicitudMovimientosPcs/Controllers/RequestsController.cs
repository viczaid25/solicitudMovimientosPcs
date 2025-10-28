using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Models.ViewModels;

namespace solicitudMovimientosPcs.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RequestsController(ApplicationDbContext db) => _db = db;

        private async Task LoadItemCombosAsync()
        {
            // Clases
            ViewBag.Clases = await _db.PcMovimientosClases
                .OrderBy(c => c.ClassCode)
                .Select(c => new SelectListItem { Value = c.ClassCode, Text = c.ClassCode })
                .ToListAsync();

            // Ubicaciones
            ViewBag.Ubicaciones = await _db.PcMovimientosUbicaciones
                .OrderBy(u => u.Area).ThenBy(u => u.Ubicacion)
                .Select(u => new SelectListItem { Value = u.Ubicacion, Text = u.Ubicacion + " - " + u.Area })
                .ToListAsync();

            // CodMov (valor guardado en Items.CodMov)
            ViewBag.CodMovs = await _db.PcMovimientosCodMovimientos
                .OrderBy(m => m.PrcId).ThenBy(m => m.RsnCd)
                .Select(m => new SelectListItem
                {
                    Value = (m.PrcId + " " + m.RsnCd).Trim(),
                    Text = $"{m.PrcId} - {m.RsnCd} - {m.Content}"
                })
                .ToListAsync();

            // Estatus (Y/H/N)
            ViewBag.Estatus = new List<SelectListItem> {
                new("Y","Y"), new("H","H"), new("N","N")
            };

                    // Monedas (MXN/USD/JPY)
                    ViewBag.Monedas = new List<SelectListItem> {
                new("MXN","MXN"), new("USD","USD"), new("JPY","JPY")
            };

            // LÍNEAS (PC_MOVIMIENTOS_CODIGO_LINEA)
            ViewBag.Lineas = await _db.PcMovimientosCodigoLineas
                .OrderBy(x => x.AreaCode)
                .Select(x => new SelectListItem
                {
                    Value = x.AreaCode,                        // se guardará en Request.Linea
                    Text = $"{x.AreaCode} - {x.AreaName}"     // texto visible al usuario
                })
                .ToListAsync();
        }

        // GET: /Requests/Create

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var solicitante = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

            var model = new PcMovimientosRequest
            {
                Fecha = DateTime.Now,
                Solicitante = solicitante,   // <- se muestra en la vista
                Urgencia = Urgencia.Media,
                RequestStatus = RequestStatus.Nuevo,
                Items = new List<PcMovimientosItem> { new PcMovimientosItem { Numero = 1 } }
            };

            await LoadItemCombosAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        [Bind("Fecha,Departamento,Linea,Comentarios,Urgencia,Items")]
        PcMovimientosRequest form)
        {
            // 1) Fija el solicitante desde el usuario autenticado
            var solicitante = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;
            form.Solicitante = solicitante;

            // 2) Quita errores previos de ModelState para este campo y revalida
            ModelState.Remove(nameof(PcMovimientosRequest.Solicitante));
            // Si quieres revalidar todo el objeto con el nuevo valor:
            TryValidateModel(form);

            // 3) Tus demás validaciones
            if (form.Items == null || form.Items.Count == 0)
                ModelState.AddModelError("", "Debes agregar al menos un item.");

            if (!ModelState.IsValid)
            {
                await LoadItemCombosAsync();
                return View(form);
            }

            // 4) Continúa con tu lógica de guardado
            form.RequestStatus = RequestStatus.Nuevo;

            for (int i = 0; i < form.Items.Count; i++)
            {
                var it = form.Items[i];
                it.Id = 0;
                it.IdSolicitud = 0;
                if (it.Numero <= 0) it.Numero = i + 1;

                var a = it.CantidadA ?? 0m;
                var diff = it.Diferencia ?? 0m;

                it.CantidadD = a - diff;

                var qtyBase = it.CantidadD != 0 ? it.CantidadD : a;
                it.Total = (it.CostoU ?? 0m) * qtyBase;
            }


            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.PcMovimientosRequests.Add(form);
                await _db.SaveChangesAsync();

                foreach (var it in form.Items)
                    it.IdSolicitud = form.Id;

                _db.PcMovimientosAprobaciones.Add(new PcMovimientosAprobaciones
                {
                    RequestId = form.Id,
                    MngStatus = ApprovalStatus.PENDING,
                    JpnStatus = ApprovalStatus.PENDING,
                    McStatus = ApprovalStatus.PENDING,
                    PlStatus = ApprovalStatus.PENDING,
                    PcMngStatus = ApprovalStatus.PENDING,
                    PcJpnStatus = ApprovalStatus.PENDING,
                    FinMngStatus = ApprovalStatus.PENDING,
                    FinJpnStatus = ApprovalStatus.PENDING
                });

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = form.Id });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                await LoadItemCombosAsync();
                return View(form);
            }
        }


        // GET: /Requests/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .Include(r => r.Aprobaciones)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();
            return View(request);
        }

        // GET: /Requests/My
        // Muestra las solicitudes del usuario actual (o de 'solicitante' si lo envías por querystring)
        [HttpGet]
        public async Task<IActionResult> My()
        {
            var display = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? "";

            var q = _db.PcMovimientosRequests
                       .AsNoTracking()
                       .Include(r => r.Items)
                       .Where(r => r.Solicitante == display);

            var vm = new MyRequestsViewModel
            {
                DisplayName = display,
                ParaModificar = await q.Where(r => r.RequestStatus == RequestStatus.PorModificar)
                                       .OrderByDescending(r => r.Fecha)
                                       .ToListAsync(),
                PendientesAprobacion = await q.Where(r => r.RequestStatus == RequestStatus.Nuevo
                                                       || r.RequestStatus == RequestStatus.EnProceso)
                                              .OrderByDescending(r => r.Fecha)
                                              .ToListAsync(),
                Recientes = await q.OrderByDescending(r => r.Fecha).Take(10).ToListAsync()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var display = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? "";
            var req = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .Include(r => r.Aprobaciones)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();

            // Solo el dueño y cuando está PorModificar
            if (!string.Equals(req.Solicitante, display, StringComparison.OrdinalIgnoreCase)
                || req.RequestStatus != RequestStatus.PorModificar)
                return Forbid();

            // Cargar catálogos a ViewBag como en Create
            await LoadItemCombosAsync();
            return View("Create", req); // reutiliza la vista Create
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PcMovimientosRequest model)
        {
            var display = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? "";

            var req = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .Include(r => r.Aprobaciones)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();

            if (!string.Equals(req.Solicitante, display, StringComparison.OrdinalIgnoreCase)
                || req.RequestStatus != RequestStatus.PorModificar)
                return Forbid();

            if (!ModelState.IsValid)
            {
                await LoadItemCombosAsync();
                return View("Create", model);
            }

            // Actualiza campos de cabecera que permites cambiar
            req.Departamento = model.Departamento;
            req.Linea = model.Linea;
            req.Comentarios = model.Comentarios;
            req.Urgencia = model.Urgencia;

            // Reemplaza items
            _db.PcMovimientosItems.RemoveRange(req.Items);
            req.Items = model.Items ?? new List<PcMovimientosItem>();
            foreach (var it in req.Items) it.IdSolicitud = req.Id;

            // Resetea aprobaciones a PENDING y limpia firmas/fechas
            if (req.Aprobaciones != null)
                ResetApprovals(req.Aprobaciones);

            // Regresa al flujo normal
            req.RequestStatus = RequestStatus.Nuevo;

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Solicitud actualizada y reenviada al flujo de aprobación.";
            return RedirectToAction(nameof(My));
        }

        private void ResetApprovals(PcMovimientosAprobaciones a)
        {
            a.Mng = a.Jpn = a.Mc = a.Pl = a.PcMng = a.PcJpn = a.FinMng = a.FinJpn = null;
            a.MngDate = a.JpnDate = a.McDate = a.PlDate = a.PcMngDate = a.PcJpnDate = a.FinMngDate = a.FinJpnDate = null;

            a.MngStatus = a.JpnStatus = a.McStatus = a.PlStatus =
            a.PcMngStatus = a.PcJpnStatus = a.FinMngStatus = a.FinJpnStatus = ApprovalStatus.PENDING;
        }



    }
}
