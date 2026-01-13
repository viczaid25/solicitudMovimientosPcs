using System.IO;
using Microsoft.AspNetCore.Hosting;
using solicitudMovimientosPcs.Models;

namespace solicitudMovimientosPcs.Utils
{
    public static class EvidenceHelper
    {
        public static List<EvidenceItem> GetEvidenceList(IWebHostEnvironment env, int requestId)
        {
            var list = new List<EvidenceItem>();
            var wwwroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(wwwroot, "uploads", "solicitudes", requestId.ToString());

            if (!Directory.Exists(folder))
                return list;

            foreach (var p in Directory.GetFiles(folder))
            {
                var fi = new FileInfo(p);
                list.Add(new EvidenceItem
                {
                    FileName = fi.Name,
                    Url = $"/uploads/solicitudes/{requestId}/{fi.Name}",
                    Ext = fi.Extension.TrimStart('.').ToLowerInvariant(),
                    Size = fi.Length
                });
            }
            return list;
        }
    }
}
