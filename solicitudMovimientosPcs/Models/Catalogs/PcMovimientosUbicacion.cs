using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("PC_MOVIMIENTOS_UBICACION")]
    public class PcMovimientosUbicacion
    {
        [Key]
        [MaxLength(10)]
        [Column("UBICACION")]
        public string Ubicacion { get; set; } = string.Empty; // PK natural

        [MaxLength(20)]
        [Column("AREA")]
        public string Area { get; set; } = string.Empty;
    }
}
