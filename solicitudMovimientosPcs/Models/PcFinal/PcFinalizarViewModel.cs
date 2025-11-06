using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace solicitudMovimientosPcs.Models.PcFinal
{
    public class PcFinalizarViewModel
    {
        public int RequestId { get; set; }

        [Required(ErrorMessage = "El folio es requerido.")]
        [MaxLength(50)]
        public string? Folio { get; set; }

        // ← NUEVO
        [Required(ErrorMessage = "El tipo de movimiento es requerido.")]
        [Display(Name = "Tipo de Movimiento")]
        public string? TipoMovimiento { get; set; }

        public IFormFile? Documento { get; set; }

        // Para mostrar datos arriba
        public PcMovimientosRequest? Request { get; set; }
    }
}
