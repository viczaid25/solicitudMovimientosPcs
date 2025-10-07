using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;

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
                var d = it.CantidadD ?? 0m;
                it.Diferencia = d - a;

                var qtyBase = d != 0 ? d : a;
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
            var who = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

            var list = await _db.PcMovimientosRequests
                .Where(r => r.Solicitante == who)
                .Include(r => r.Items)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            ViewBag.Solicitante = who;
            return View(list);
        }


    }
}
