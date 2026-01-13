using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Models.PcFinal;

namespace solicitudMovimientosPcs.Controllers
{
    [Authorize] // Si luego agregas rol/claim "PCStaff", cámbialo a: [Authorize(Roles="PCStaff")]
    public class PcFinalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PcFinalController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ===== Configuración de archivos (PC Final) =====
        private static readonly HashSet<string> AllowedExts = new(StringComparer.OrdinalIgnoreCase)
            { ".pdf",".doc",".docx",".xls",".xlsx",".png",".jpg",".jpeg" };
        private const long MaxSize = 20L * 1024 * 1024; // 20 MB
        private const int MaxCount = 10;

        private string FinalFolderFs(int requestId)
        {
            var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return Path.Combine(root, "uploads", "pc", requestId.ToString());
        }
        private string FinalFolderUrl(int requestId) => $"/uploads/pc/{requestId}";

        private List<EvidenceItem> GetFinalDocs(int requestId)
        {
            var list = new List<EvidenceItem>();
            var folder = FinalFolderFs(requestId);
            if (!Directory.Exists(folder)) return list;

            var urlBase = FinalFolderUrl(requestId);
            foreach (var p in Directory.GetFiles(folder))
            {
                var fi = new FileInfo(p);
                list.Add(new EvidenceItem
                {
                    FileName = fi.Name,
                    Url = $"{urlBase}/{fi.Name}",
                    Ext = fi.Extension.TrimStart('.').ToLowerInvariant(),
                    Size = fi.Length
                });
            }
            return list.OrderBy(x => x.FileName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private async Task<(int saved, int skipped, string[] errors)> SaveFinalDocsAsync(int requestId, IEnumerable<IFormFile>? files)
        {
            var errs = new List<string>();
            if (files == null) return (0, 0, errs.ToArray());

            var valid = files.Where(f => f != null && f.Length > 0).ToList();
            if (valid.Count > MaxCount)
            {
                errs.Add($"Máximo {MaxCount} archivos.");
                return (0, valid.Count, errs.ToArray());
            }

            var folder = FinalFolderFs(requestId);
            Directory.CreateDirectory(folder);

            int saved = 0, skipped = 0;
            foreach (var f in valid)
            {
                var ext = Path.GetExtension(f.FileName);
                if (!AllowedExts.Contains(ext))
                {
                    skipped++; errs.Add($"Extensión no permitida: {f.FileName}");
                    continue;
                }
                if (f.Length > MaxSize)
                {
                    skipped++; errs.Add($"Tamaño excedido (20MB): {f.FileName}");
                    continue;
                }

                var safeBase = Path.GetFileNameWithoutExtension(f.FileName);
                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{safeBase}{ext}";
                var path = Path.Combine(folder, fileName);

                using var stream = System.IO.File.Create(path);
                await f.CopyToAsync(stream);
                saved++;
            }

            return (saved, skipped, errs.ToArray());
        }

        // Helper: ¿todas las aprobaciones en APPROVE?
        private static bool IsFullyApproved(PcMovimientosAprobaciones? a) =>
            a != null
            && a.MngStatus == ApprovalStatus.APPROVED
            && a.JpnStatus == ApprovalStatus.APPROVED
            && a.McStatus == ApprovalStatus.APPROVED
            && a.PlStatus == ApprovalStatus.APPROVED
            && a.PcMngStatus == ApprovalStatus.APPROVED
            && a.PcJpnStatus == ApprovalStatus.APPROVED
            && a.FinMngStatus == ApprovalStatus.APPROVED
            && a.FinJpnStatus == ApprovalStatus.APPROVED;

        // ====== LISTA ======
        public async Task<IActionResult> Index()
        {
            var list = await _db.PcMovimientosRequests
                .Include(r => r.Aprobaciones)
                .Where(r => r.RequestStatus != RequestStatus.Completado
                            && r.Aprobaciones != null
                            && r.Aprobaciones.MngStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.JpnStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.McStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.PlStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.PcMngStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.PcJpnStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.FinMngStatus == ApprovalStatus.APPROVED
                            && r.Aprobaciones.FinJpnStatus == ApprovalStatus.APPROVED)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return View(list);
        }

        // ====== FINALIZAR (GET) ======
        [HttpGet]
        public async Task<IActionResult> Finalizar(int id)
        {
            var req = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();

            // Permite: FDO, PDO, WDO, MLO, ESTATUS
            var allowed = new[] { "FDO", "PDO", "WDO", "MLO", "ESTATUS" };

            var vm = new PcFinalizarViewModel
            {
                RequestId = req.Id,
                Folio = req.PcFolio,
                // ⬇️ parsea CSV guardado en la solicitud
                TipoMovimiento = (req.PcTipoMovimiento ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToUpperInvariant())
                    .Where(x => allowed.Contains(x))
                    .Distinct()
                    .ToArray(),
                Request = req
            };

            // Listado de documentos ya subidos en finalización
            ViewBag.FinalDocs = GetFinalDocs(req.Id);

            return View(vm);
        }

        // ====== FINALIZAR (POST) ======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(PcFinalizarViewModel vm)
        {
            var req = await _db.PcMovimientosRequests
                .Include(r => r.Aprobaciones)
                .FirstOrDefaultAsync(r => r.Id == vm.RequestId);

            if (req == null) return NotFound();

            // Estado del flujo
            if (!IsFullyApproved(req.Aprobaciones) || req.RequestStatus == RequestStatus.Completado)
                ModelState.AddModelError(string.Empty, "La solicitud no está lista para finalización por PC.");

            // Validación de tipos múltiples
            var allowedTipos = new[] { "FDO", "PDO", "WDO", "MLO", "ESTATUS" };
            var tipos = (vm.TipoMovimiento ?? System.Array.Empty<string>())
                .Select(t => (t ?? "").Trim().ToUpperInvariant())
                .Where(t => allowedTipos.Contains(t))
                .Distinct()
                .ToArray();

            if (tipos.Length == 0)
                ModelState.AddModelError(nameof(vm.TipoMovimiento), "Selecciona al menos un Tipo de Movimiento.");

            // Validación de documentos (si quieres forzarlo: al menos 1)
            if (vm.Documentos == null || vm.Documentos.Count == 0)
                ModelState.AddModelError(nameof(vm.Documentos), "Debes subir al menos un documento.");

            if (!ModelState.IsValid)
            {
                vm.Request = req;
                ViewBag.FinalDocs = GetFinalDocs(req.Id); // opcional
                return View(vm);
            }

            // === Guardado de documentos múltiples ===
            var folder = Path.Combine(_env.WebRootPath, "uploads", "pc", req.Id.ToString());
            Directory.CreateDirectory(folder);

            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".pdf",".doc",".docx",".xls",".xlsx",".png",".jpg",".jpeg" };

            foreach (var doc in vm.Documentos!)
            {
                if (doc == null || doc.Length == 0) continue;
                var ext = Path.GetExtension(doc.FileName);
                if (!allowedExt.Contains(ext)) continue;

                var safeBase = Path.GetFileNameWithoutExtension(doc.FileName);
                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeBase}{ext}";
                var fullPath = Path.Combine(folder, fileName);
                using var stream = System.IO.File.Create(fullPath);
                await doc.CopyToAsync(stream);
            }

            // === Actualiza campos de PC ===
            req.PcFolio = vm.Folio?.Trim();
            req.PcTipoMovimiento = string.Join(",", tipos);  // ⬅️ guarda CSV
                                                             // Si antes usabas PcDocumentoPath (único), puedes dejar de usarlo
                                                             // req.PcDocumentoPath = null;

            req.RequestStatus = RequestStatus.Completado;
            req.PcFinalizadoPor = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name;
            req.PcFinalDate = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Ok"] = "Solicitud finalizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

    }
}
