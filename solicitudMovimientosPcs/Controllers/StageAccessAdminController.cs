using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models.Security;
using solicitudMovimientosPcs.Services;
using solicitudMovimientosPcs.Models.Catalogs;


namespace solicitudMovimientosPcs.Controllers
{
    [Route("Admin/StageAccess")]
    public class StageAccessAdminController : Controller
    {
        private static readonly string[] Flow = { "MNG", "JPN", "MC", "PL", "PCMNG", "PCJPN", "FINMNG", "FINJPN" };
        private readonly ApplicationDbContext _db;
        private readonly IStageAccessService _svc;

        public StageAccessAdminController(ApplicationDbContext db, IStageAccessService svc)
        { _db = db; _svc = svc; }

        // IMPORTANTE: agrega tu propio [Authorize(Roles="Admin")] o tu filtro de seguridad aquí.

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var rows = await _db.StageAccesses
                .AsNoTracking()
                .OrderBy(x => x.Stage).ThenBy(x => x.UserName)
                .ToListAsync();

            ViewBag.Flow = Flow;
            return View(rows);
        }

        [HttpPost("Grant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grant(string stage, string displayName)
        {
            await _svc.GrantAsync(stage, displayName);
            TempData["Ok"] = "Permiso otorgado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Revoke/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(int id)
        {
            await _svc.RevokeAsync(id);
            TempData["Ok"] = "Permiso revocado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/StageAccess/SearchUsers?term=zaid
        [HttpGet("SearchUsers")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? term)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length < 2)
                return Json(Array.Empty<object>());

            // Busca por USERNAME o EMAIL (contiene)
            var q = _db.UsersAd.AsNoTracking();

            var results = await q
                .Where(u =>
                    (u.Username != null && u.Username.Contains(term)) ||
                    (u.Email != null && u.Email.Contains(term)) ||
                    u.PcLoginId.Contains(term)
                )
                .OrderBy(u => u.Username)
                .ThenBy(u => u.Email)
                .Select(u => new {
                    username = u.Username ?? u.PcLoginId, // fallback por si viene null
                    email = u.Email ?? "",
                    login = u.PcLoginId
                })
                .Take(20)
                .ToListAsync();

            return Json(results);
        }
    }
}
