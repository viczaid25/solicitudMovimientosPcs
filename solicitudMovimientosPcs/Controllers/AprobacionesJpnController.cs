using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;

namespace solicitudMovimientosPcs.Controllers
{
    // [Authorize(Roles = "JPN")] // habilita si manejas roles
    public class AprobacionesJpnController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AprobacionesJpnController(ApplicationDbContext db) => _db = db;

        private string CurrentUserDisplay() =>
            User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

        // GET: /AprobacionesJpn
        public async Task<IActionResult> Index()
        {
            var pendings = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                    .ThenInclude(r => r.Items)
                // Pendientes de JPN; y opcionalmente solo si MNG ya aprobó:
                .Where(a =>
                    (a.JpnStatus == ApprovalStatus.PENDING || a.JpnStatus == null) &&
                    (a.MngStatus == ApprovalStatus.APPROVED)) // <-- quita esta línea si no deseas gating
                .OrderByDescending(a => a.Solicitud!.Fecha)
                .ToListAsync();

            return View(pendings);
        }

        // GET: /AprobacionesJpn/Details/5   (5 = RequestId)
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

        // POST: /AprobacionesJpn/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);

            if (apro == null) return NotFound();

            // Evita doble atención
            if (apro.JpnStatus != null && apro.JpnStatus != ApprovalStatus.PENDING)
            {
                TempData["Warn"] = "Esta solicitud ya fue atendida por JPN.";
                return RedirectToAction(nameof(Index));
            }

            // (Opcional) exige que MNG haya aprobado antes
            if (apro.MngStatus != ApprovalStatus.APPROVED)
            {
                TempData["Warn"] = "La solicitud aún no ha sido aprobada por Manager.";
                return RedirectToAction(nameof(Index));
            }

            apro.Jpn = CurrentUserDisplay();
            apro.JpnStatus = ApprovalStatus.APPROVED;
            apro.JpnDate = DateTime.Now;

            // (Opcional) mover estatus de la solicitud
            if (apro.Solicitud != null && apro.Solicitud.RequestStatus == RequestStatus.EnProceso)
            {
                // aquí podrías dejar EnProceso o pasar a otra etapa si tienes flujo definido
            }

            await _db.SaveChangesAsync();
            TempData["Ok"] = $"Solicitud #{id} aprobada por {apro.Jpn}";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AprobacionesJpn/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);

            if (apro == null) return NotFound();

            if (apro.JpnStatus != null && apro.JpnStatus != ApprovalStatus.PENDING)
            {
                TempData["Warn"] = "Esta solicitud ya fue atendida por JPN.";
                return RedirectToAction(nameof(Index));
            }

            // (Opcional) exige que MNG haya aprobado antes
            if (apro.MngStatus != ApprovalStatus.APPROVED)
            {
                TempData["Warn"] = "La solicitud aún no ha sido aprobada por Manager.";
                return RedirectToAction(nameof(Index));
            }

            apro.Jpn = CurrentUserDisplay();
            apro.JpnStatus = ApprovalStatus.REJECTED;
            apro.JpnDate = DateTime.Now;

            // Al rechazar, marcamos la solicitud
            if (apro.Solicitud != null)
                apro.Solicitud.RequestStatus = RequestStatus.Rechazado;

            await _db.SaveChangesAsync();
            TempData["Err"] = $"Solicitud #{id} rechazada por {apro.Jpn}";
            return RedirectToAction(nameof(Index));
        }
    }
}
