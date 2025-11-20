using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Services;

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
        private readonly IEmailService _email;
        private readonly Dictionary<string, string> _destinos;

        private static readonly string[] Flow = new[] { "MNG", "JPN", "MC", "PL", "PCMNG", "PCJPN", "FINMNG", "FINJPN" };

        public AprobacionesEtapaController(
        ApplicationDbContext db,
        IEmailService email,
        Dictionary<string, string> destinos) 
        {
            _db = db;
            _email = email;
            _destinos = destinos; 
        }


        // Helper para saber la siguiente etapa
        private string? NextStage(string current)
        {
            var idx = Array.IndexOf(Flow, current);
            if (idx < 0 || idx + 1 >= Flow.Length) return null;
            return Flow[idx + 1];
        }

        private string CurrentUser() =>
            User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? string.Empty;

        private string? ResolveEmail(string displayOrEmail)
        {
            if (string.IsNullOrWhiteSpace(displayOrEmail)) return null;
            // Si ya viene correo, lo usamos
            if (displayOrEmail.Contains("@")) return displayOrEmail.Trim();

            // TODO: Cambia a tu lógica real (ej. consulta AD/tabla de usuarios)
            // Mientras tanto, convención temporal:
            // p.ej. "Juan Perez" -> "juan.perez@meax.mx"
            var norm = displayOrEmail.Trim().ToLowerInvariant().Replace(' ', '.');
            return $"{norm}@meax.mx";
        }

        // Notificar al solicitante
        private async Task NotifyAsync(PcMovimientosRequest req, string subject, string html)
        {
            var to = ResolveEmail(req.Solicitante);
            if (to == null) return;
            try { await _email.SendAsync(to, subject, html); } catch { }
        }

        // (Opcional) notificar también al siguiente aprobador de la cadena:
        private string[] NextStageRecipients(string stage, PcMovimientosAprobaciones a)
        {
            // TODO: Implementa si tienes mapeo de correos por etapa/responsable
            return Array.Empty<string>();
        }



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
        public async Task<IActionResult> APPROVED(string stage, int id, [FromForm] string? comentario)
        {
            if (!TryNormalizeStage(stage, out var st)) return NotFound();

            var apro = await _db.PcMovimientosAprobaciones
                .Include(a => a.Solicitud)
                .FirstOrDefaultAsync(a => a.RequestId == id);
            if (apro == null) return NotFound();

            if (!IsPending(apro, st)) { TempData["Warn"] = "Esta solicitud ya fue atendida en esta etapa."; return RedirectToAction(nameof(Index), new { stage = st }); }
            if (!PrevApproved(apro, st)) { TempData["Warn"] = "La solicitud no ha completado las etapas previas."; return RedirectToAction(nameof(Index), new { stage = st }); }

            SetApproval(apro, st, CurrentUser(), ApprovalStatus.APPROVED, DateTime.Now);

            if (st == "MNG" && apro.Solicitud != null && apro.Solicitud.RequestStatus == RequestStatus.Nuevo)
                apro.Solicitud.RequestStatus = RequestStatus.EnProceso;

            await _db.SaveChangesAsync();

            // Notifica al solicitante (ya lo tienes si quieres)
            if (apro.Solicitud != null)
            {
                var req = apro.Solicitud;
                var subj = $"Solicitud #{req.Id} aprobada en {st}";
                var body = $@"<p>Hola {req.Solicitante},</p>
                          <p>Tu solicitud <strong>#{req.Id}</strong> fue <strong>aprobada</strong> en <strong>{st}</strong>.</p>
                          {(string.IsNullOrWhiteSpace(comentario) ? "" : $"<p><em>Comentario:</em> {System.Net.WebUtility.HtmlEncode(comentario)}</p>")}";
                await _email.SendAsync(ResolveEmail(req.Solicitante) ?? "", subj, body);
            }

            // <<< NUEVO: Notificar al siguiente aprobador, si existe mapeo >>>
            var stSiguiente = NextStage(st);
            if (stSiguiente != null && _destinos.TryGetValue(stSiguiente, out var correo) && !string.IsNullOrWhiteSpace(correo))
            {
                var bodyNext = $@"<p>Hay una solicitud lista para tu etapa <strong>{stSiguiente}</strong>.</p>
                              <p><strong>ID:</strong> {id}</p>";
                await _email.SendAsync(new[] { correo }, $"Solicitud #{id} lista para {stSiguiente}", bodyNext);
            }

            TempData["Ok"] = $"Solicitud #{id} aprobada en {st}.";
            return RedirectToAction(nameof(Index), new { stage = st });
        }

        // ======= RECHAZAR =======
        [HttpPost("REJECTED/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> REJECTED(string stage, int id, [FromForm] string? comentario)
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

            if (apro.Solicitud != null)
                apro.Solicitud.RequestStatus = RequestStatus.Rechazado;

            await _db.SaveChangesAsync();

            if (apro.Solicitud != null)
            {
                var req = apro.Solicitud;
                var subj = $"Solicitud #{req.Id} rechazada en {st}";
                var body = $@"
            <p>Hola {req.Solicitante},</p>
            <p>Tu solicitud <strong>#{req.Id}</strong> fue <strong>rechazada</strong> en la etapa <strong>{st}</strong>.</p>
            {(string.IsNullOrWhiteSpace(comentario) ? "" : $"<p><em>Motivo:</em> {System.Net.WebUtility.HtmlEncode(comentario)}</p>")}
            <p>Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}</p>";
                await NotifyAsync(req, subj, body);
            }

            TempData["Err"] = $"Solicitud #{id} rechazada en {st}.";
            return RedirectToAction(nameof(Index), new { stage = st });
        }

        // ======= MODIFICAR =======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(int id, string stage, [FromForm] string? comentario)
        {
            var user = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? "user";
            var a = await _db.PcMovimientosAprobaciones
                .Include(x => x.Solicitud)
                .FirstOrDefaultAsync(x => x.RequestId == id);

            if (a == null) return NotFound();

            bool ok = SetStageStatus(a, stage, ApprovalStatus.MODIFY, user, DateTime.Now);
            if (!ok) return BadRequest("Etapa inválida.");

            if (a.Solicitud != null)
                a.Solicitud.RequestStatus = RequestStatus.PorModificar;

            await _db.SaveChangesAsync();

            if (a.Solicitud != null)
            {
                var req = a.Solicitud;
                var subj = $"Solicitud #{req.Id} devuelta para modificación ({stage})";
                var body = $@"
            <p>Hola {req.Solicitante},</p>
            <p>Tu solicitud <strong>#{req.Id}</strong> fue devuelta para <strong>modificación</strong> en la etapa <strong>{stage}</strong>.</p>
            {(string.IsNullOrWhiteSpace(comentario) ? "" : $"<p><em>Comentarios del aprobador:</em> {System.Net.WebUtility.HtmlEncode(comentario)}</p>")}
            <p>Por favor realiza los ajustes y reenvíala.</p>";
                await NotifyAsync(req, subj, body);
            }

            TempData["Msg"] = "Solicitud enviada a modificación.";
            return RedirectToAction(nameof(Index), new { stage });
        }


        private void SetStageComment(PcMovimientosAprobaciones a, string st, string comments)
        {
            switch (st)
            {
                case "MNG": a.MngComments = comments; break;
                case "JPN": a.JpnComments = comments; break;
                case "MC": a.McComments = comments; break;
                case "PL": a.PlComments = comments; break;
                case "PCMNG": a.PcMngComments = comments; break;
                case "PCJPN": a.PcJpnComments = comments; break;
                case "FINMNG": a.FinMngComments = comments; break;
                case "FINJPN": a.FinJpnComments = comments; break;
            }
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

        /// <summary>
        /// Actualiza una etapa específica a un status/datos.
        /// stage: MNG, JPN, MC, PL, PCMNG, PCJPN, FINMNG, FINJPN
        /// </summary>
        private bool SetStageStatus(PcMovimientosAprobaciones a, string stage, ApprovalStatus status, string user, DateTime when)
        {
            switch (stage?.ToUpperInvariant())
            {
                case "MNG":
                    a.MngStatus = status; a.Mng = user; a.MngDate = when; return true;
                case "JPN":
                    a.JpnStatus = status; a.Jpn = user; a.JpnDate = when; return true;
                case "MC":
                    a.McStatus = status; a.Mc = user; a.McDate = when; return true;
                case "PL":
                    a.PlStatus = status; a.Pl = user; a.PlDate = when; return true;
                case "PCMNG":
                    a.PcMngStatus = status; a.PcMng = user; a.PcMngDate = when; return true;
                case "PCJPN":
                    a.PcJpnStatus = status; a.PcJpn = user; a.PcJpnDate = when; return true;
                case "FINMNG":
                    a.FinMngStatus = status; a.FinMng = user; a.FinMngDate = when; return true;
                case "FINJPN":
                    a.FinJpnStatus = status; a.FinJpn = user; a.FinJpnDate = when; return true;
            }
            return false;
        }

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
