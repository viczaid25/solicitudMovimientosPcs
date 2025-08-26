using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models
{
    [Table("PC_MOVIMIENTOS_ITEMS")]
    public class PcMovimientosItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // FK
        [Required]
        [Column("ID_SOLICITUD")]
        public int IdSolicitud { get; set; }

        [ForeignKey(nameof(IdSolicitud))]
        public PcMovimientosRequest? Solicitud { get; set; }

        // Campos
        [Column("NUMERO")]
        public int Numero { get; set; }

        [MaxLength(50)]
        [Column("NUM_PARTE")]
        [Required]
        public string NumParte { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("DESCRIPCION")]
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("CASE")]
        public string? Case { get; set; }

        // Cod de movimiento (catálogo)
        [MaxLength(50)]
        [Column("COD_MOV")]
        [Required]
        public string? CodMov { get; set; }

        // ====== AHORA ======
        [MaxLength(1)]
        [Column("ESTATUS_A")]
        [RegularExpression("^(Y|H|N)$", ErrorMessage = "Estatus A debe ser Y, H o N.")]
        public string? EstatusA { get; set; }

        [MaxLength(50)]
        [Column("UBICACION_A")]
        public string? UbicacionA { get; set; }   // Catálogo

        [MaxLength(50)]
        [Column("CLASE_A")]
        public string? ClaseA { get; set; }       // Catálogo

        [Column("CANTIDAD_A", TypeName = "decimal(18,2)")]
        public decimal? CantidadA { get; set; }

        // ====== DESPUÉS ======
        [MaxLength(1)]
        [Column("ESTATUS_D")]
        [RegularExpression("^(Y|H|N)$", ErrorMessage = "Estatus D debe ser Y, H o N.")]
        public string? EstatusD { get; set; }

        [MaxLength(50)]
        [Column("UBICACION_D")]
        public string? UbicacionD { get; set; }   // Catálogo

        [MaxLength(50)]
        [Column("CLASE_D")]
        public string? ClaseD { get; set; }       // Catálogo

        [Column("CANTIDAD_D", TypeName = "decimal(18,2)")]
        public decimal? CantidadD { get; set; }

        // Totales
        [Column("DIFERENCIA", TypeName = "decimal(18,2)")]
        public decimal? Diferencia { get; set; }  // = (CantidadD - CantidadA)

        [MaxLength(3)]
        [Column("MONEDA")]
        [RegularExpression("^(MXN|USD|JPY)$", ErrorMessage = "Moneda debe ser MXN, USD o JPY.")]
        public string? Moneda { get; set; }       // MXN, USD, JPY

        [Column("COSTO_U", TypeName = "decimal(18,2)")]
        public decimal? CostoU { get; set; }

        [Column("TOTAL", TypeName = "decimal(18,2)")]
        public decimal? Total { get; set; }       // = CostoU * (CantidadD ?? CantidadA ?? 0)
    }
}
