using System;
using System.Collections.Generic;

namespace solicitudMovimientosPcs.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public string DisplayName { get; set; } = "";
        public int MyTotal { get; set; }
        public int MyOpen { get; set; }
        public int MyCompleted { get; set; }
        public int MyRejected { get; set; }

        // Grafiquita por estatus (solo "mis solicitudes")
        public Dictionary<string, int> MyByStatus { get; set; } = new();

        // Listas
        public List<MiniRequest> RecentMyRequests { get; set; } = new();
        public int PendingApprovalsTotal { get; set; }
        public List<PendingApprovalRow> RecentPendingApprovals { get; set; } = new();

        public class MiniRequest
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public string Departamento { get; set; } = "";
            public string Linea { get; set; } = "";
            public Urgencia Urgencia { get; set; }
            public RequestStatus RequestStatus { get; set; }
            public int ItemsCount { get; set; }
        }

        public class PendingApprovalRow
        {
            public int RequestId { get; set; }
            public DateTime Fecha { get; set; }
            public string Solicitante { get; set; } = "";
            public string Departamento { get; set; } = "";
            public Urgencia Urgencia { get; set; }
            public string Stage { get; set; } = "PENDING";
            public int AgeDays { get; set; }
        }
    }
}
