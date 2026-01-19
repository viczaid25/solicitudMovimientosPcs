using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;


namespace solicitudMovimientosPcs.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public RequestsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

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

        // Clases que requieren aprobación MC
        private static readonly HashSet<string> McRequiredClases =
            new(new[] { "112", "122", "211", "221", "221KT", "221ST", "311", "312", "321", "322", "511", "812", "822" },
                StringComparer.OrdinalIgnoreCase);

        private static bool RequiresMc(IEnumerable<PcMovimientosItem>? items)
            => items != null && items.Any(i => !string.IsNullOrWhiteSpace(i.ClaseA) && McRequiredClases.Contains(i.ClaseA.Trim()));

        // GET: /Requests/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var solicitante = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

            var model = new PcMovimientosRequest
            {
                Fecha = DateTime.Now, // guarda fecha + hora
                Solicitante = solicitante,
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
            [Bind("Departamento,Linea,Comentarios,Urgencia,Items")] PcMovimientosRequest form,
            [FromForm] string[]? TiposMovimiento,
            List<IFormFile>? Evidencias)
        {
            form.Fecha = DateTime.Now; // fecha + hora

            var solicitante = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;
            form.Solicitante = solicitante;

            ModelState.Remove(nameof(PcMovimientosRequest.Solicitante));
            TryValidateModel(form);

            // Normaliza selección (CSV) para persistir
            var tiposSet = (TiposMovimiento ?? Array.Empty<string>())
                .Select(t => t?.Trim().ToUpperInvariant())
                .Where(t => t is "FDO" or "PDO" or "WDO" or "MLO" or "ESTATUS")
                .Distinct()
                .ToArray();
            form.PcTiposMovimiento = string.Join(",", tiposSet);

            // Validaciones
            if (string.IsNullOrWhiteSpace(form.Comentarios))
                ModelState.AddModelError(nameof(form.Comentarios), "Comentarios es requerido.");

            if (form.Items == null || form.Items.Count == 0)
                ModelState.AddModelError("", "Debes agregar al menos un item.");

            if (!ModelState.IsValid)
            {
                await LoadItemCombosAsync();
                return View(form);
            }

            // Cálculos de items (regla: Total = |Diferencia| * CostoU, y CantidadD = CantidadA - Diferencia)
            for (int i = 0; i < form.Items.Count; i++)
            {
                var it = form.Items[i];
                it.Id = 0;
                it.IdSolicitud = 0;
                if (it.Numero <= 0) it.Numero = i + 1;

                var a = it.CantidadA ?? 0m;           // cantidad actual
                var costo = it.CostoU ?? 0m;

                // Si Diferencia viene nula, la inferimos con CantidadD; si no, usamos la proporcionada
                decimal diff;
                if (it.Diferencia.HasValue)
                {
                    diff = it.Diferencia.Value;       // positiva = baja, negativa = alta
                }
                else
                {
                    var d = it.CantidadD ?? a;
                    diff = a - d;                      // positiva = baja, negativa = alta
                    it.Diferencia = diff;
                }

                // Asegurar consistencia: CantidadD = CantidadA - Diferencia
                it.CantidadD = a - diff;

                // Total monetario siempre positivo (magnitud del movimiento)
                it.Total = decimal.Round(Math.Abs(diff) * costo, 2, MidpointRounding.AwayFromZero);
            }

            // Reglas de aprobación:
            // FIN requiere aprobación si hay FDO/PDO/WDO (aunque también esté ESTATUS).
            bool requireFin = tiposSet.Any(t => t is "FDO" or "PDO" or "WDO");
            // MC requiere aprobación si al menos un item está en las clases establecidas.
            bool requireMc = RequiresMc(form.Items);

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.PcMovimientosRequests.Add(form);
                await _db.SaveChangesAsync();

                foreach (var it in form.Items)
                    it.IdSolicitud = form.Id;

                var now = DateTime.Now;
                var sys = "SYSTEM";

                var aprob = new PcMovimientosAprobaciones
                {
                    RequestId = form.Id,

                    // Siempre pendientes de inicio:
                    MngStatus = ApprovalStatus.PENDING,
                    JpnStatus = ApprovalStatus.PENDING,
                    PlStatus = ApprovalStatus.PENDING,
                    PcMngStatus = ApprovalStatus.PENDING,
                    PcJpnStatus = ApprovalStatus.PENDING,

                    // MC
                    McStatus = requireMc ? ApprovalStatus.PENDING : ApprovalStatus.APPROVED,
                    Mc = requireMc ? null : sys,
                    McDate = requireMc ? null : now,

                    // FIN
                    FinMngStatus = requireFin ? ApprovalStatus.PENDING : ApprovalStatus.APPROVED,
                    FinJpnStatus = requireFin ? ApprovalStatus.PENDING : ApprovalStatus.APPROVED,
                    FinMng = requireFin ? null : sys,
                    FinJpn = requireFin ? null : sys,
                    FinMngDate = requireFin ? null : now,
                    FinJpnDate = requireFin ? null : now
                };

                _db.PcMovimientosAprobaciones.Add(aprob);
                await _db.SaveChangesAsync();

                await SaveEvidenceFilesAsync(form.Id, Evidencias);


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

            ViewBag.Evidence = GetEvidenceList(id);
            ViewBag.PcDocs = GetPcFinalDocs(id);

            return View(request);
        }

        // GET: /Requests/My
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

            await LoadItemCombosAsync();
            return View("Create", req); // reutiliza la vista Create
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PcMovimientosRequest model,
            [FromForm] string[]? TiposMovimiento,
            List<IFormFile>? Evidencias)
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

            // Normaliza selección (CSV)
            var tiposSet = (TiposMovimiento ?? Array.Empty<string>())
               .Select(t => t?.Trim().ToUpperInvariant())
               .Where(t => t is "FDO" or "PDO" or "WDO" or "MLO" or "ESTATUS")
               .Distinct()
               .ToArray();
            model.PcTiposMovimiento = string.Join(",", tiposSet);

            if (!ModelState.IsValid)
            {
                await LoadItemCombosAsync();
                return View("Create", model);
            }

            // === Cálculos por item ===
            if (model.Items is null || model.Items.Count == 0)
            {
                ModelState.AddModelError("", "Debes agregar al menos un item.");
                await LoadItemCombosAsync();
                return View("Create", model);
            }

            for (int i = 0; i < model.Items.Count; i++)
            {
                var it = model.Items[i];

                // Asegura numeración
                if (it.Numero <= 0) it.Numero = i + 1;

                var a = it.CantidadA ?? 0m;
                var dDest = it.CantidadD ?? a;
                var costo = it.CostoU ?? 0m;

                // Si Diferencia es nula, la inferimos con A y D
                decimal diff = it.Diferencia ?? (a - dDest);
                it.Diferencia = diff;

                // Consistenciar destino: D = A - Diferencia
                it.CantidadD = a - diff;

                // Total monetario por magnitud del movimiento
                it.Total = decimal.Round(Math.Abs(diff) * costo, 2, MidpointRounding.AwayFromZero);
            }

            // === Actualiza cabecera permitida ===
            req.Departamento = model.Departamento;
            req.Linea = model.Linea;
            req.Comentarios = model.Comentarios;
            req.Urgencia = model.Urgencia;
            req.PcTiposMovimiento = model.PcTiposMovimiento;

            // === Reemplaza items (reinsertando limpios) ===
            _db.PcMovimientosItems.RemoveRange(req.Items);
            req.Items = model.Items;

            foreach (var it in req.Items)
            {
                it.Id = 0;                // reinsertar como nuevos
                it.IdSolicitud = req.Id;  // FK
            }

            // === Reglas de aprobación ===
            bool requireFin = tiposSet.Any(t => t is "FDO" or "PDO" or "WDO");
            bool requireMc = RequiresMc(req.Items);

            if (req.Aprobaciones != null)
            {
                ResetApprovals(req.Aprobaciones, requireFin: requireFin, requireMc: requireMc);
            }

            // Regresa al flujo normal
            req.RequestStatus = RequestStatus.Nuevo;

            await _db.SaveChangesAsync();
            await SaveEvidenceFilesAsync(req.Id, Evidencias);
            TempData["Msg"] = "Solicitud actualizada y reenviada al flujo de aprobación.";
            return RedirectToAction(nameof(My));
        }


        private void ResetApprovals(PcMovimientosAprobaciones a, bool requireFin, bool requireMc)
        {
            // Limpia firmas/fechas
            a.Mng = a.Jpn = a.Mc = a.Pl = a.PcMng = a.PcJpn = a.FinMng = a.FinJpn = null;
            a.MngDate = a.JpnDate = a.McDate = a.PlDate = a.PcMngDate = a.PcJpnDate = a.FinMngDate = a.FinJpnDate = null;

            // Etapas base a pendientes
            a.MngStatus = ApprovalStatus.PENDING;
            a.JpnStatus = ApprovalStatus.PENDING;
            a.PlStatus = ApprovalStatus.PENDING;
            a.PcMngStatus = ApprovalStatus.PENDING;
            a.PcJpnStatus = ApprovalStatus.PENDING;

            var now = DateTime.Now;

            // MC
            if (requireMc)
            {
                a.McStatus = ApprovalStatus.PENDING;
            }
            else
            {
                a.McStatus = ApprovalStatus.APPROVED;
                a.Mc = "SYSTEM";
                a.McDate = now;
            }

            // FIN
            if (requireFin)
            {
                a.FinMngStatus = ApprovalStatus.PENDING;
                a.FinJpnStatus = ApprovalStatus.PENDING;
            }
            else
            {
                a.FinMngStatus = ApprovalStatus.APPROVED;
                a.FinJpnStatus = ApprovalStatus.APPROVED;
                a.FinMng = "SYSTEM";
                a.FinJpn = "SYSTEM";
                a.FinMngDate = now;
                a.FinJpnDate = now;
            }
        }

        private static readonly HashSet<string> AllowedExts = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf",".jpg",".jpeg",".png",".heic",".xlsx",".xls",".csv",".txt",".doc",".docx",".zip" };

        private const long MaxSize = 20L * 1024 * 1024; // 20 MB

        private async Task SaveEvidenceFilesAsync(int requestId, IEnumerable<IFormFile>? files)
        {
            if (files == null) return;

            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(wwwroot, "uploads", "solicitudes", requestId.ToString());
            Directory.CreateDirectory(folder);

            foreach (var f in files.Where(x => x != null && x.Length > 0))
            {
                var ext = Path.GetExtension(f.FileName);
                if (!AllowedExts.Contains(ext) || f.Length > MaxSize)
                    continue; // saltar inválidos

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var path = Path.Combine(folder, fileName);
                using var stream = System.IO.File.Create(path);
                await f.CopyToAsync(stream);
            }
        }

        private List<EvidenceItem> GetEvidenceList(int requestId)
        {
            var list = new List<EvidenceItem>();
            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(wwwroot, "uploads", "solicitudes", requestId.ToString());

            if (!Directory.Exists(folder))
                return list;

            foreach (var p in Directory.GetFiles(folder))
            {
                var fi = new FileInfo(p);
                list.Add(new EvidenceItem
                {
                    FileName = fi.Name,
                    Url = $"/uploads/solicitudes/{requestId}/{fi.Name}",
                    Ext = fi.Extension.TrimStart('.').ToLowerInvariant(),
                    Size = fi.Length
                });
            }
            return list;
        }

        // GET: /Requests/All
        [HttpGet]
        public async Task<IActionResult> All(
            string? q,
            RequestStatus? status,
            string? departamento,
            string? linea,
            DateTime? desde,
            DateTime? hasta
        )
        {
            // 1) Valores efectivos de filtros (por defecto: Completado)
            var effectiveStatus = status ?? RequestStatus.Completado;

            // 2) Query base
            var query = _db.PcMovimientosRequests
                .AsNoTracking()
                .Include(r => r.Items)
                .Include(r => r.Aprobaciones)
                .AsQueryable();

            // 3) Filtros
            query = query.Where(r => r.RequestStatus == effectiveStatus);

            if (!string.IsNullOrWhiteSpace(departamento))
                query = query.Where(r => r.Departamento == departamento);

            if (!string.IsNullOrWhiteSpace(linea))
                query = query.Where(r => r.Linea == linea);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim().ToLower();
                query = query.Where(r =>
                    (r.Solicitante ?? "").ToLower().Contains(ql) ||
                    (r.Comentarios ?? "").ToLower().Contains(ql) ||
                    (r.PcFolio ?? "").ToLower().Contains(ql)
                );
            }

            if (desde.HasValue)
                query = query.Where(r => r.Fecha >= desde.Value.Date);

            if (hasta.HasValue)
            {
                var to = hasta.Value.Date.AddDays(1); // fin del día
                query = query.Where(r => r.Fecha < to);
            }

            var list = await query
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            // 4) Combos SIN C# en atributos de <option>
            ViewBag.StatusItems = Enum.GetValues(typeof(RequestStatus))
                .Cast<RequestStatus>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString(),
                    Selected = s == effectiveStatus
                })
                .ToList();

            ViewBag.DepItems = new[] { "MELX", "SAL", "IT", "PD", "LGL", "PRC", "TOP", "HR", "FIN", "QC", "PC", "FC" }
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d,
                    Selected = d == (departamento ?? "")
                })
                .ToList();

            var lineasSel = await _db.PcMovimientosCodigoLineas
                .OrderBy(x => x.AreaCode)
                .Select(x => new SelectListItem
                {
                    Value = x.AreaCode,
                    Text = x.AreaCode + " - " + x.AreaName
                })
                .ToListAsync();

            foreach (var li in lineasSel)
                li.Selected = li.Value == (linea ?? "");

            ViewBag.Lineas = lineasSel;

            // 5) Valores para mantener filtros en inputs
            ViewBag.Q = q ?? "";
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd") ?? "";

            return View("All", list);
        }

        // Al final del controlador (RequestsController)
        private List<EvidenceItem> GetPcFinalDocs(int requestId)
        {
            var list = new List<EvidenceItem>();
            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(wwwroot, "uploads", "pc", requestId.ToString());

            if (!Directory.Exists(folder))
                return list;

            foreach (var p in Directory.GetFiles(folder))
            {
                var fi = new FileInfo(p);
                list.Add(new EvidenceItem
                {
                    FileName = fi.Name,
                    Url = $"/uploads/pc/{requestId}/{fi.Name}",
                    Ext = fi.Extension.TrimStart('.').ToLowerInvariant(),
                    Size = fi.Length
                });
            }
            return list;
        }


    }
}
