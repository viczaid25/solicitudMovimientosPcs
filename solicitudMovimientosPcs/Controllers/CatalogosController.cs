using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Models.Catalogs;

namespace solicitudMovimientosPcs.Controllers
{
    [Route("[controller]")]
    public class CatalogosController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CatalogosController(ApplicationDbContext db) => _db = db;

        // =========================
        // CLASES
        // =========================

        [HttpGet("Clases")]
        public async Task<IActionResult> Clases()
        {
            var list = await _db.PcMovimientosClases
                .OrderBy(x => x.ClassCode)
                .ToListAsync();
            return View("Clases", list);
        }

        [HttpPost("CreateClase")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClase([Bind("ClassCode,Description")] PcMovimientosClase m)
        {
            if (string.IsNullOrWhiteSpace(m.ClassCode))
            {
                TempData["Err"] = "La clave de clase es requerida.";
                return RedirectToAction(nameof(Clases));
            }

            var exists = await _db.PcMovimientosClases.AnyAsync(x => x.ClassCode == m.ClassCode);
            if (exists)
            {
                TempData["Err"] = $"La clase {m.ClassCode} ya existe.";
                return RedirectToAction(nameof(Clases));
            }

            _db.PcMovimientosClases.Add(m);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Clase creada.";
            return RedirectToAction(nameof(Clases));
        }

        [HttpPost("EditClase")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClase([Bind("ClassCode,Description")] PcMovimientosClase m)
        {
            var ent = await _db.PcMovimientosClases.FindAsync(m.ClassCode);
            if (ent == null) { TempData["Err"] = "Clase no encontrada."; return RedirectToAction(nameof(Clases)); }

            ent.Description = m.Description;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Clase actualizada.";
            return RedirectToAction(nameof(Clases));
        }

        [HttpPost("DeleteClase/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClase(string id)
        {
            var ent = await _db.PcMovimientosClases.FindAsync(id);
            if (ent == null) { TempData["Err"] = "Clase no encontrada."; return RedirectToAction(nameof(Clases)); }

            _db.PcMovimientosClases.Remove(ent);
            try
            {
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Clase eliminada.";
            }
            catch (DbUpdateException)
            {
                TempData["Err"] = "No se puede eliminar: existen referencias.";
            }
            return RedirectToAction(nameof(Clases));
        }

        // =========================
        // LÍNEAS / ÁREAS
        // =========================

        [HttpGet("Lineas")]
        public async Task<IActionResult> Lineas()
        {
            var list = await _db.PcMovimientosCodigoLineas
                .OrderBy(x => x.AreaCode)
                .ToListAsync();
            return View("Lineas", list);
        }

        [HttpPost("CreateLinea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLinea([Bind("AreaCode,AreaName")] PcMovimientosCodigoLinea m)
        {
            if (string.IsNullOrWhiteSpace(m.AreaCode) || string.IsNullOrWhiteSpace(m.AreaName))
            {
                TempData["Err"] = "Código y nombre son requeridos.";
                return RedirectToAction(nameof(Lineas));
            }

            if (await _db.PcMovimientosCodigoLineas.AnyAsync(x => x.AreaCode == m.AreaCode))
            {
                TempData["Err"] = $"La línea {m.AreaCode} ya existe.";
                return RedirectToAction(nameof(Lineas));
            }

            _db.PcMovimientosCodigoLineas.Add(m);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Línea creada.";
            return RedirectToAction(nameof(Lineas));
        }

        [HttpPost("EditLinea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLinea([Bind("AreaCode,AreaName")] PcMovimientosCodigoLinea m)
        {
            var ent = await _db.PcMovimientosCodigoLineas.FindAsync(m.AreaCode);
            if (ent == null) { TempData["Err"] = "Línea no encontrada."; return RedirectToAction(nameof(Lineas)); }

            ent.AreaName = m.AreaName;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Línea actualizada.";
            return RedirectToAction(nameof(Lineas));
        }

        [HttpPost("DeleteLinea/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLinea(string id)
        {
            var ent = await _db.PcMovimientosCodigoLineas.FindAsync(id);
            if (ent == null) { TempData["Err"] = "Línea no encontrada."; return RedirectToAction(nameof(Lineas)); }

            _db.PcMovimientosCodigoLineas.Remove(ent);
            try
            {
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Línea eliminada.";
            }
            catch (DbUpdateException)
            {
                TempData["Err"] = "No se puede eliminar: existen referencias.";
            }
            return RedirectToAction(nameof(Lineas));
        }

        // =========================
        // CÓDIGOS DE MOVIMIENTO (PRC/RSN)
        // =========================

        [HttpGet("Movimientos")]
        public async Task<IActionResult> Movimientos()
        {
            var list = await _db.PcMovimientosCodMovimientos
                .OrderBy(x => x.PrcId).ThenBy(x => x.RsnCd)
                .ToListAsync();
            return View("Movimientos", list);
        }

        [HttpPost("CreateMovimiento")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovimiento([Bind("PrcId,RsnCd,Content")] PcMovimientosCodMovimiento m)
        {
            if (string.IsNullOrWhiteSpace(m.PrcId) || string.IsNullOrWhiteSpace(m.RsnCd))
            {
                TempData["Err"] = "PRC y RSN son requeridos.";
                return RedirectToAction(nameof(Movimientos));
            }

            bool exists = await _db.PcMovimientosCodMovimientos
                .AnyAsync(x => x.PrcId == m.PrcId && x.RsnCd == m.RsnCd);
            if (exists)
            {
                TempData["Err"] = $"El movimiento {m.PrcId} / {m.RsnCd} ya existe.";
                return RedirectToAction(nameof(Movimientos));
            }

            _db.PcMovimientosCodMovimientos.Add(m);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Movimiento creado.";
            return RedirectToAction(nameof(Movimientos));
        }

        [HttpPost("EditMovimiento")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovimiento([Bind("PrcId,RsnCd,Content")] PcMovimientosCodMovimiento m)
        {
            var ent = await _db.PcMovimientosCodMovimientos
                .FirstOrDefaultAsync(x => x.PrcId == m.PrcId && x.RsnCd == m.RsnCd);
            if (ent == null) { TempData["Err"] = "Movimiento no encontrado."; return RedirectToAction(nameof(Movimientos)); }

            ent.Content = m.Content;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Movimiento actualizado.";
            return RedirectToAction(nameof(Movimientos));
        }

        [HttpPost("DeleteMovimiento/{prc}/{rsn}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovimiento(string prc, string rsn)
        {
            var ent = await _db.PcMovimientosCodMovimientos
                .FirstOrDefaultAsync(x => x.PrcId == prc && x.RsnCd == rsn);
            if (ent == null) { TempData["Err"] = "Movimiento no encontrado."; return RedirectToAction(nameof(Movimientos)); }

            _db.PcMovimientosCodMovimientos.Remove(ent);
            try
            {
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Movimiento eliminado.";
            }
            catch (DbUpdateException)
            {
                TempData["Err"] = "No se puede eliminar: existen referencias.";
            }
            return RedirectToAction(nameof(Movimientos));
        }

        // =========================
        // UBICACIONES
        // =========================

        [HttpGet("Ubicaciones")]
        public async Task<IActionResult> Ubicaciones()
        {
            var list = await _db.PcMovimientosUbicaciones
                .OrderBy(x => x.Area).ThenBy(x => x.Ubicacion)
                .ToListAsync();
            return View("Ubicaciones", list);
        }

        [HttpPost("CreateUbicacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUbicacion([Bind("Ubicacion,Area")] PcMovimientosUbicacion m)
        {
            if (string.IsNullOrWhiteSpace(m.Ubicacion) || string.IsNullOrWhiteSpace(m.Area))
            {
                TempData["Err"] = "Ubicación y Área son requeridas.";
                return RedirectToAction(nameof(Ubicaciones));
            }

            if (await _db.PcMovimientosUbicaciones.AnyAsync(x => x.Ubicacion == m.Ubicacion))
            {
                TempData["Err"] = $"La ubicación {m.Ubicacion} ya existe.";
                return RedirectToAction(nameof(Ubicaciones));
            }

            _db.PcMovimientosUbicaciones.Add(m);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Ubicación creada.";
            return RedirectToAction(nameof(Ubicaciones));
        }

        [HttpPost("EditUbicacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUbicacion([Bind("Ubicacion,Area")] PcMovimientosUbicacion m)
        {
            var ent = await _db.PcMovimientosUbicaciones.FindAsync(m.Ubicacion);
            if (ent == null) { TempData["Err"] = "Ubicación no encontrada."; return RedirectToAction(nameof(Ubicaciones)); }

            ent.Area = m.Area;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Ubicación actualizada.";
            return RedirectToAction(nameof(Ubicaciones));
        }

        [HttpPost("DeleteUbicacion/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUbicacion(string id)
        {
            var ent = await _db.PcMovimientosUbicaciones.FindAsync(id);
            if (ent == null) { TempData["Err"] = "Ubicación no encontrada."; return RedirectToAction(nameof(Ubicaciones)); }

            _db.PcMovimientosUbicaciones.Remove(ent);
            try
            {
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Ubicación eliminada.";
            }
            catch (DbUpdateException)
            {
                TempData["Err"] = "No se puede eliminar: existen referencias.";
            }
            return RedirectToAction(nameof(Ubicaciones));
        }

        // =========================
        // CÓDIGOS (PC_MOVIMIENTOS_CODIGO)
        // =========================

        [HttpGet("Codigos")]
        public async Task<IActionResult> Codigos()
        {
            var list = await _db.PcMovimientosCodigos
                .OrderBy(x => x.Codigo)
                .ToListAsync();
            return View("Codigos", list); // Vista: Views/Catalogos/Codigos.cshtml
        }

        [HttpPost("CreateCodigo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCodigo([Bind("Codigo,Descripcion")] PcMovimientosCodigo m)
        {
            m.Codigo = (m.Codigo ?? string.Empty).Trim().ToUpperInvariant();
            m.Descripcion = (m.Descripcion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(m.Codigo))
            {
                TempData["Err"] = "El código es requerido.";
                return RedirectToAction(nameof(Codigos));
            }

            var exists = await _db.PcMovimientosCodigos.AnyAsync(x => x.Codigo == m.Codigo);
            if (exists)
            {
                TempData["Err"] = $"El código {m.Codigo} ya existe.";
                return RedirectToAction(nameof(Codigos));
            }

            _db.PcMovimientosCodigos.Add(m);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Código creado.";
            return RedirectToAction(nameof(Codigos));
        }

        [HttpPost("EditCodigo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCodigo([Bind("Codigo,Descripcion")] PcMovimientosCodigo m)
        {
            m.Codigo = (m.Codigo ?? string.Empty).Trim().ToUpperInvariant();
            m.Descripcion = (m.Descripcion ?? string.Empty).Trim();

            var ent = await _db.PcMovimientosCodigos.FindAsync(m.Codigo);
            if (ent == null)
            {
                TempData["Err"] = "Código no encontrado.";
                return RedirectToAction(nameof(Codigos));
            }

            ent.Descripcion = m.Descripcion;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Código actualizado.";
            return RedirectToAction(nameof(Codigos));
        }

        [HttpPost("DeleteCodigo/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCodigo(string id)
        {
            id = (id ?? string.Empty).Trim().ToUpperInvariant();
            var ent = await _db.PcMovimientosCodigos.FindAsync(id);
            if (ent == null)
            {
                TempData["Err"] = "Código no encontrado.";
                return RedirectToAction(nameof(Codigos));
            }

            _db.PcMovimientosCodigos.Remove(ent);
            try
            {
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Código eliminado.";
            }
            catch (DbUpdateException)
            {
                TempData["Err"] = "No se puede eliminar: existen referencias.";
            }
            return RedirectToAction(nameof(Codigos));
        }
    }
}
