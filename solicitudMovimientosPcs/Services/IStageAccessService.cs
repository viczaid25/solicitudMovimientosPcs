namespace solicitudMovimientosPcs.Services
{
    public interface IStageAccessService
    {
        Task<bool> HasAccessAsync(string stage, string displayName);
        Task<Dictionary<string, HashSet<string>>> SnapshotAsync(); // para UI
        Task GrantAsync(string stage, string displayName);
        Task RevokeAsync(int entryId);
        Task InvalidateAsync(); // limpiar caché cuando cambien entradas
    }
}