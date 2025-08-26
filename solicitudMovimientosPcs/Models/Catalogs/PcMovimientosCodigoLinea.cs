using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("PC_MOVIMIENTOS_CODIGO_LINEA")]
    public class PcMovimientosCodigoLinea
    {
        [Key]
        [MaxLength(10)]
        [Column("AREA_CODE")]
        public string AreaCode { get; set; } = string.Empty; // PK natural

        [MaxLength(50)]
        [Column("AREA_NAME")]
        public string AreaName { get; set; } = string.Empty;
    }
}
