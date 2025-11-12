using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("PC_MOVIMIENTOS_CLASES")]
    public class PcMovimientosClase
    {
        [Key]
        [MaxLength(10)]
        [Column("CLASS_CODE")]
        public string ClassCode { get; set; } = string.Empty; // PK natural

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
