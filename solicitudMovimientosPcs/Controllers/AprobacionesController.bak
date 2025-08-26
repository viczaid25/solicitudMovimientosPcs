using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;

namespace solicitudMovimientosPcs.Controllers
{
    public class AprobacionesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AprobacionesController(ApplicationDbContext db) => _db = db;

        private string CurrentUserDisplay() =>
            User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

        // GET: /Aprobaciones
        public async Task<IActionResult> Index()
        {
            var pendings = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                    .ThenInclude(r => r.Items)
                // Si tienes registros viejos en NULL, los tratamos como pendientes:
                .Where(a => a.MngStatus == ApprovalStatus.PENDING || a.MngStatus == null)
                .OrderByDescending(a => a.Solicitud!.Fecha)
                .ToListAsync();

            return View(pendings);
        }

        // GET: /Aprobaciones/Details/5   (5 = RequestId)
        public async Task<IActionResult> Details(int id)
        {
            var req = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();

            var apro = await _db.PcMovimientosAprobaciones
                .FirstOrDefaultAsync(a => a.RequestId == id);

            ViewBag.Aprob = apro;
            return View(req);
        }

        // POST: /Aprobaciones/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);

            if (apro == null) return NotFound();
            if (apro.MngStatus != null && apro.MngStatus != ApprovalStatus.PENDING)
            {
                TempData["Warn"] = "Esta solicitud ya fue atendida por Manager.";
                return RedirectToAction(nameof(Index));
            }

            apro.Mng = CurrentUserDisplay();
            apro.MngStatus = ApprovalStatus.APPROVED;
            apro.MngDate = DateTime.Now;

            // Opcional: mover estatus de la solicitud
            if (apro.Solicitud != null && apro.Solicitud.RequestStatus == RequestStatus.Nuevo)
                apro.Solicitud.RequestStatus = RequestStatus.EnProceso;

            await _db.SaveChangesAsync();
            TempData["Ok"] = $"Solicitud #{id} aprobada por {apro.Mng}";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Aprobaciones/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);

            if (apro == null) return NotFound();
            if (apro.MngStatus != null && apro.MngStatus != ApprovalStatus.PENDING)
            {
                TempData["Warn"] = "Esta solicitud ya fue atendida por Manager.";
                return RedirectToAction(nameof(Index));
            }

            apro.Mng = CurrentUserDisplay();
            apro.MngStatus = ApprovalStatus.REJECTED;
            apro.MngDate = DateTime.Now;

            // Opcional: marcar solicitud como rechazada
            if (apro.Solicitud != null)
                apro.Solicitud.RequestStatus = RequestStatus.Rechazado;

            await _db.SaveChangesAsync();
            TempData["Err"] = $"Solicitud #{id} rechazada por {apro.Mng}";
            return RedirectToAction(nameof(Index));
        }
    }
}
