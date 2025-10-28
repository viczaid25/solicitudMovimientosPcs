namespace solicitudMovimientosPcs.Models.ViewModels
{
    public class MyRequestsViewModel
    {
        public string? DisplayName { get; set; }
        public List<PcMovimientosRequest> ParaModificar { get; set; } = new();
        public List<PcMovimientosRequest> PendientesAprobacion { get; set; } = new();
        public List<PcMovimientosRequest> Recientes { get; set; } = new();
    }
}
