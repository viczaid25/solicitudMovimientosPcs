using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;

namespace solicitudMovimientosPcs.Controllers
{
    // Rutas:
    // GET    /Aprobaciones/{stage}
    // GET    /Aprobaciones/{stage}/Details/{id}
    // POST   /Aprobaciones/{stage}/APPROVED/{id}
    // POST   /Aprobaciones/{stage}/REJECTED/{id}
    [Route("Aprobaciones/{stage}")]
    public class AprobacionesEtapaController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AprobacionesEtapaController(ApplicationDbContext db) => _db = db;

        private string CurrentUser() =>
            User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

        // Orden del flujo (para "gating": exigir aprobaciones previas)
        private static readonly string[] Flow = new[]
        {
            "MNG","JPN","MC","PL","PCMNG","PCJPN","FINMNG","FINJPN"
        };

        private bool TryNormalizeStage(string stage, out string norm)
        {
            norm = stage?.Trim().ToUpperInvariant() ?? "";
            return Flow.Contains(norm);
        }

        // ======= LISTA =======
        [HttpGet("")]
        public async Task<IActionResult> Index(string stage)
        {
            if (!TryNormalizeStage(stage, out var st)) return NotFound();

            IQueryable<PcMovimientosAprobaciones> q = _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)!.ThenInclude(r => r.Items);

            // Filtro de "pendientes de esta etapa" + gating de etapas previas aprobadas
            q = st switch
            {
                "MNG" => q.Where(a => a.MngStatus == ApprovalStatus.PENDING || a.MngStatus == null),
                "JPN" => q.Where(a => (a.JpnStatus == ApprovalStatus.PENDING || a.JpnStatus == null)
                                       && a.MngStatus == ApprovalStatus.APPROVED),
                "MC" => q.Where(a => (a.McStatus == ApprovalStatus.PENDING || a.McStatus == null)
                                       && a.JpnStatus == ApprovalStatus.APPROVED),
                "PL" => q.Where(a => (a.PlStatus == ApprovalStatus.PENDING || a.PlStatus == null)
                                       && a.McStatus == ApprovalStatus.APPROVED),
                "PCMNG" => q.Where(a => (a.PcMngStatus == ApprovalStatus.PENDING || a.PcMngStatus == null)
                                       && a.PlStatus == ApprovalStatus.APPROVED),
                "PCJPN" => q.Where(a => (a.PcJpnStatus == ApprovalStatus.PENDING || a.PcJpnStatus == null)
                                       && a.PcMngStatus == ApprovalStatus.APPROVED),
                "FINMNG" => q.Where(a => (a.FinMngStatus == ApprovalStatus.PENDING || a.FinMngStatus == null)
                                       && a.PcJpnStatus == ApprovalStatus.APPROVED),
                "FINJPN" => q.Where(a => (a.FinJpnStatus == ApprovalStatus.PENDING || a.FinJpnStatus == null)
                                       && a.FinMngStatus == ApprovalStatus.APPROVED),
                _ => q
            };

            var list = await q.OrderByDescending(a => a.Solicitud!.Fecha).ToListAsync();

            ViewBag.Stage = st;
            return View("Index", list);
        }

        // ======= DETALLE =======
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(string stage, int id)
        {
            if (!TryNormalizeStage(stage, out var st)) return NotFound();

            var req = await _db.PcMovimientosRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return NotFound();

            var apro = await _db.PcMovimientosAprobaciones
                .FirstOrDefaultAsync(a => a.RequestId == id);

            ViewBag.Stage = st;
            ViewBag.Aprob = apro;
            return View("Details", req);
        }

        // ======= APROBAR =======
        [HttpPost("APPROVED/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> APPROVED(string stage, int id)
        {
            if (!TryNormalizeStage(stage, out var st)) return NotFound();

            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);
            if (apro == null) return NotFound();

            if (!IsPending(apro, st))
            {
                TempData["Warn"] = "Esta solicitud ya fue atendida en esta etapa.";
                return RedirectToAction(nameof(Index), new { stage = st });
            }
            if (!PrevApproved(apro, st))
            {
                TempData["Warn"] = "La solicitud no ha completado las etapas previas.";
                return RedirectToAction(nameof(Index), new { stage = st });
            }

            SetApproval(apro, st, CurrentUser(), ApprovalStatus.APPROVED, DateTime.Now);

            // Opcional: primer avance pone la solicitud EnProceso
            if (st == "MNG" && apro.Solicitud != null && apro.Solicitud.RequestStatus == RequestStatus.Nuevo)
                apro.Solicitud.RequestStatus = RequestStatus.EnProceso;

            await _db.SaveChangesAsync();
            TempData["Ok"] = $"Solicitud #{id} aprobada en {st}.";
            return RedirectToAction(nameof(Index), new { stage = st });
        }

        // ======= RECHAZAR =======
        [HttpPost("REJECTED/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> REJECTED(string stage, int id)
        {
            if (!TryNormalizeStage(stage, out var st)) return NotFound();

            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);
            if (apro == null) return NotFound();

            if (!IsPending(apro, st))
            {
                TempData["Warn"] = "Esta solicitud ya fue atendida en esta etapa.";
                return RedirectToAction(nameof(Index), new { stage = st });
            }
            if (!PrevApproved(apro, st))
            {
                TempData["Warn"] = "La solicitud no ha completado las etapas previas.";
                return RedirectToAction(nameof(Index), new { stage = st });
            }

            SetApproval(apro, st, CurrentUser(), ApprovalStatus.REJECTED, DateTime.Now);

            // Si se rechaza en cualquier etapa, marcamos la solicitud como Rechazado
            if (apro.Solicitud != null)
                apro.Solicitud.RequestStatus = RequestStatus.Rechazado;

            await _db.SaveChangesAsync();
            TempData["Err"] = $"Solicitud #{id} rechazada en {st}.";
            return RedirectToAction(nameof(Index), new { stage = st });
        }

        // ======= Helpers de flujo/propiedades por etapa =======
        private bool IsPending(PcMovimientosAprobaciones a, string st) => st switch
        {
            "MNG" => a.MngStatus == ApprovalStatus.PENDING || a.MngStatus == null,
            "JPN" => a.JpnStatus == ApprovalStatus.PENDING || a.JpnStatus == null,
            "MC" => a.McStatus == ApprovalStatus.PENDING || a.McStatus == null,
            "PL" => a.PlStatus == ApprovalStatus.PENDING || a.PlStatus == null,
            "PCMNG" => a.PcMngStatus == ApprovalStatus.PENDING || a.PcMngStatus == null,
            "PCJPN" => a.PcJpnStatus == ApprovalStatus.PENDING || a.PcJpnStatus == null,
            "FINMNG" => a.FinMngStatus == ApprovalStatus.PENDING || a.FinMngStatus == null,
            "FINJPN" => a.FinJpnStatus == ApprovalStatus.PENDING || a.FinJpnStatus == null,
            _ => false
        };

        private bool PrevApproved(PcMovimientosAprobaciones a, string st)
        {
            int idx = Array.IndexOf(Flow, st);
            if (idx <= 0) return true; // primera etapa no requiere previas
            // Requiere que TODAS las anteriores estén APPROVED
            for (int i = 0; i < idx; i++)
            {
                if (!IsApproved(a, Flow[i])) return false;
            }
            return true;
        }

        private bool IsApproved(PcMovimientosAprobaciones a, string st) => st switch
        {
            "MNG" => a.MngStatus == ApprovalStatus.APPROVED,
            "JPN" => a.JpnStatus == ApprovalStatus.APPROVED,
            "MC" => a.McStatus == ApprovalStatus.APPROVED,
            "PL" => a.PlStatus == ApprovalStatus.APPROVED,
            "PCMNG" => a.PcMngStatus == ApprovalStatus.APPROVED,
            "PCJPN" => a.PcJpnStatus == ApprovalStatus.APPROVED,
            "FINMNG" => a.FinMngStatus == ApprovalStatus.APPROVED,
            "FINJPN" => a.FinJpnStatus == ApprovalStatus.APPROVED,
            _ => false
        };

        private void SetApproval(PcMovimientosAprobaciones a, string st, string user, ApprovalStatus status, DateTime when)
        {
            switch (st)
            {
                case "MNG": a.Mng = user; a.MngStatus = status; a.MngDate = when; break;
                case "JPN": a.Jpn = user; a.JpnStatus = status; a.JpnDate = when; break;
                case "MC": a.Mc = user; a.McStatus = status; a.McDate = when; break;
                case "PL": a.Pl = user; a.PlStatus = status; a.PlDate = when; break;
                case "PCMNG": a.PcMng = user; a.PcMngStatus = status; a.PcMngDate = when; break;
                case "PCJPN": a.PcJpn = user; a.PcJpnStatus = status; a.PcJpnDate = when; break;
                case "FINMNG": a.FinMng = user; a.FinMngStatus = status; a.FinMngDate = when; break;
                case "FINJPN": a.FinJpn = user; a.FinJpnStatus = status; a.FinJpnDate = when; break;
            }
        }
    }
}
