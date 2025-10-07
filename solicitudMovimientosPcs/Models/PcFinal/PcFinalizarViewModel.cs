using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace solicitudMovimientosPcs.Models.PcFinal
{
    public class PcFinalizarViewModel
    {
        public int RequestId { get; set; }

        [Required, MaxLength(50)]
        public string Folio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes subir un documento.")]
        public IFormFile Documento { get; set; } = default!;

        // Para mostrar datos en la vista
        public PcMovimientosRequest? Request { get; set; }
    }
}
