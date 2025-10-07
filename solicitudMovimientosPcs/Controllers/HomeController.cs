using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace solicitudMovimientosPcs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        // ? ÚNICO constructor: DI no se confunde
        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var displayName = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? "Usuario";
            var mine = _db.PcMovimientosRequests
                          .AsNoTracking()
                          .Where(r => r.Solicitante == displayName);

            var myTotal = await mine.CountAsync();
            var myOpen = await mine.CountAsync(r => r.RequestStatus == RequestStatus.Nuevo || r.RequestStatus == RequestStatus.EnProceso);
            var myCompleted = await mine.CountAsync(r => r.RequestStatus == RequestStatus.Completado);
            var myRejected = await mine.CountAsync(r => r.RequestStatus == RequestStatus.Rechazado || r.RequestStatus == RequestStatus.Cancelado);

            var byStatus = await mine
                .GroupBy(r => r.RequestStatus)
                .Select(g => new { Status = g.Key, Cnt = g.Count() })
                .ToListAsync();

            var dictByStatus = byStatus.ToDictionary(k => k.Status.ToString(), v => v.Cnt);

            var recentMine = await mine
                .OrderByDescending(r => r.Fecha)
                .Take(8)
                .Select(r => new HomeDashboardViewModel.MiniRequest
                {
                    Id = r.Id,
                    Fecha = r.Fecha,
                    Departamento = r.Departamento,
                    Linea = r.Linea,
                    Urgencia = r.Urgencia,
                    RequestStatus = r.RequestStatus,
                    ItemsCount = r.Items.Count
                })
                .ToListAsync();

            var approvalPendingQuery = _db.PcMovimientosAprobaciones
                .AsNoTracking()
                .Include(a => a.Solicitud)
                .Where(a =>
                    a.MngStatus == ApprovalStatus.PENDING ||
                    a.JpnStatus == ApprovalStatus.PENDING ||
                    a.McStatus == ApprovalStatus.PENDING ||
                    a.PlStatus == ApprovalStatus.PENDING ||
                    a.PcMngStatus == ApprovalStatus.PENDING ||
                    a.PcJpnStatus == ApprovalStatus.PENDING ||
                    a.FinMngStatus == ApprovalStatus.PENDING ||
                    a.FinJpnStatus == ApprovalStatus.PENDING
                );

            var pendingTotal = await approvalPendingQuery.CountAsync();

            var recentPending = await approvalPendingQuery
                .OrderByDescending(a => a.Solicitud!.Fecha)
                .Take(8)
                .Select(a => new HomeDashboardViewModel.PendingApprovalRow
                {
                    RequestId = a.RequestId,
                    Fecha = a.Solicitud!.Fecha,
                    Solicitante = a.Solicitud!.Solicitante,
                    Departamento = a.Solicitud!.Departamento,
                    Urgencia = a.Solicitud!.Urgencia,
                    Stage =
                        a.MngStatus == ApprovalStatus.PENDING ? "MNG" :
                        a.JpnStatus == ApprovalStatus.PENDING ? "JPN" :
                        a.McStatus == ApprovalStatus.PENDING ? "MC" :
                        a.PlStatus == ApprovalStatus.PENDING ? "PL" :
                        a.PcMngStatus == ApprovalStatus.PENDING ? "PC MNG" :
                        a.PcJpnStatus == ApprovalStatus.PENDING ? "PC JPN" :
                        a.FinMngStatus == ApprovalStatus.PENDING ? "FIN MNG" :
                        a.FinJpnStatus == ApprovalStatus.PENDING ? "FIN JPN" : "—",
                    AgeDays = EF.Functions.DateDiffDay(a.Solicitud!.Fecha, DateTime.Now)
                })
                .ToListAsync();

            var vm = new HomeDashboardViewModel
            {
                DisplayName = displayName,
                MyTotal = myTotal,
                MyOpen = myOpen,
                MyCompleted = myCompleted,
                MyRejected = myRejected,
                MyByStatus = dictByStatus,
                RecentMyRequests = recentMine,
                PendingApprovalsTotal = pendingTotal,
                RecentPendingApprovals = recentPending
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
