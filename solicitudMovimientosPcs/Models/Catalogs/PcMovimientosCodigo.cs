using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("PC_MOVIMIENTOS_CODIGO")]
    public class PcMovimientosCodigo
    {
        [Key]
        [MaxLength(10)]
        [Column("CODIGO")]
        public string Codigo { get; set; } = string.Empty; // PK natural

        [MaxLength(100)]
        [Column("DESCRIPCION")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
