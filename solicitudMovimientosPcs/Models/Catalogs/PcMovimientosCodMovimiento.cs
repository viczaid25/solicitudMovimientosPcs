using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("PC_MOVIMIENTOS_COD_MOVIMIENTOS")]
    public class PcMovimientosCodMovimiento
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; } // PK numérica

        [MaxLength(10)]
        [Column("PRC_ID")]
        public string? PrcId { get; set; }

        [MaxLength(10)]
        [Column("RSN_CD")]
        public string? RsnCd { get; set; }

        [MaxLength(100)]
        [Column("CONTENT")]
        public string? Content { get; set; }
    }
}
