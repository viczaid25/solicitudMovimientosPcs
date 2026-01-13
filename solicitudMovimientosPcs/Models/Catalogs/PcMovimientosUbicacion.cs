using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("PC_MOVIMIENTOS_UBICACION")]
    public class PcMovimientosUbicacion
    {
        [Key]
        [StringLength(50)]
        [Unicode(false)] // opcional, para varchar
        public string Ubicacion { get; set; } = "";

        [Required, StringLength(20)]
        [Unicode(false)]
        public string Area { get; set; } = "";
    }
}
