using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Lista de solicitudes listas para finalización por PC (todas aprobadas y no completadas)
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

        // Controllers/PcFinalController.cs (ejemplo)
        [HttpGet]
        public async Task<IActionResult> Finalizar(int id)
        {
            var req = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();

            var vm = new PcFinalizarViewModel
            {
                RequestId = req.Id,
                Folio = req.PcFolio, // si lo manejas así
                TipoMovimiento = req.PcTipoMovimiento, // ← nuevo
                Request = req
            };
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(PcFinalizarViewModel vm)
        {
            var req = await _db.PcMovimientosRequests
                .Include(r => r.Aprobaciones)
                .FirstOrDefaultAsync(r => r.Id == vm.RequestId);

            if (req == null) return NotFound();

            // Validaciones de flujo/estado
            if (!IsFullyApproved(req.Aprobaciones) || req.RequestStatus == RequestStatus.Completado)
                ModelState.AddModelError(string.Empty, "La solicitud no está lista para finalización por PC.");

            // Validación: TipoMovimiento requerido y dentro del catálogo
            var allowedTipos = new[] { "FDO", "PDO", "MLO", "ESTATUS" };
            if (string.IsNullOrWhiteSpace(vm.TipoMovimiento) || !allowedTipos.Contains(vm.TipoMovimiento))
                ModelState.AddModelError(nameof(vm.TipoMovimiento), "Selecciona un Tipo de Movimiento válido.");

            // Validación: archivo obligatorio
            if (vm.Documento == null || vm.Documento.Length == 0)
                ModelState.AddModelError(nameof(vm.Documento), "Debes subir un documento.");

            if (!ModelState.IsValid)
            {
                vm.Request = req;
                return View(vm);
            }

            // Validar extensión
            var ext = Path.GetExtension(vm.Documento.FileName).ToLowerInvariant();
            var allowed = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(nameof(vm.Documento), "Formato no permitido (usa PDF, DOC(X), XLS(X), PNG o JPG).");
                vm.Request = req;
                return View(vm);
            }

            // Guardar archivo en wwwroot/uploads/pc/{id}/
            var folder = Path.Combine(_env.WebRootPath, "uploads", "pc", req.Id.ToString());
            Directory.CreateDirectory(folder);

            var safeBase = Path.GetFileNameWithoutExtension(vm.Documento.FileName);
            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeBase}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(fullPath))
                await vm.Documento.CopyToAsync(stream);

            var relativePath = $"/uploads/pc/{req.Id}/{fileName}";

            // === Actualizar solicitud con los nuevos campos ===
            req.PcFolio = vm.Folio?.Trim();
            req.PcTipoMovimiento = vm.TipoMovimiento?.Trim();   // ← Guarda el tipo
            req.PcDocumentoPath = relativePath;
            req.RequestStatus = RequestStatus.Completado;

            req.PcFinalizadoPor = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name;
            req.PcFinalDate = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Solicitud finalizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

    }
}
