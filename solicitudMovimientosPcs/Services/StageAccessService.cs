using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models.Security;

namespace solicitudMovimientosPcs.Services
{
    public class StageAccessService : IStageAccessService
    {
        private readonly ApplicationDbContext _db;
        private Dictionary<string, HashSet<string>>? _cache;
        private DateTime _cacheAt = DateTime.MinValue;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

        public StageAccessService(ApplicationDbContext db) => _db = db;

        private async Task EnsureCacheAsync()
        {
            if (_cache != null && DateTime.UtcNow - _cacheAt < _ttl) return;

            var rows = await _db.StageAccesses.AsNoTracking().ToListAsync();
            _cache = rows
                .GroupBy(r => r.Stage.ToUpperInvariant())
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<string>(g.Select(r => r.UserName.Trim()),
                                             StringComparer.OrdinalIgnoreCase)
                );
            _cacheAt = DateTime.UtcNow;
        }

        public async Task<bool> HasAccessAsync(string stage, string displayName)
        {
            await EnsureCacheAsync();
            var st = (stage ?? "").ToUpperInvariant();
            if (!_cache!.TryGetValue(st, out var set) || set.Count == 0)
                return true; // si no hay lista, acceso libre (configurable)
            return set.Contains(displayName?.Trim() ?? "");
        }

        public async Task<Dictionary<string, HashSet<string>>> SnapshotAsync()
        {
            await EnsureCacheAsync();
            // copia para UI
            return _cache!.ToDictionary(k => k.Key, v => new HashSet<string>(v.Value, StringComparer.OrdinalIgnoreCase));
        }

        public async Task GrantAsync(string stage, string displayName)
        {
            stage = (stage ?? "").Trim().ToUpperInvariant();
            var userName = (displayName ?? "").Trim(); 

            if (string.IsNullOrWhiteSpace(stage) || string.IsNullOrWhiteSpace(userName))
                return;

            bool exists = await _db.StageAccesses.AnyAsync(x => x.Stage == stage && x.UserName == userName);
            if (exists) return;

            _db.StageAccesses.Add(new StageAccess
            {
                Stage = stage,
                UserName = userName,
                CanView = true,
                CanApprove = true,
                EmailOverride = null
            });

            await _db.SaveChangesAsync();
            await InvalidateAsync();
        }


        public async Task RevokeAsync(int entryId)
        {
            var ent = await _db.StageAccesses.FindAsync(entryId);
            if (ent == null) return;
            _db.StageAccesses.Remove(ent);
            await _db.SaveChangesAsync();
            await InvalidateAsync();
        }

        public Task InvalidateAsync()
        {
            _cache = null;
            _cacheAt = DateTime.MinValue;
            return Task.CompletedTask;
        }
    }
}
