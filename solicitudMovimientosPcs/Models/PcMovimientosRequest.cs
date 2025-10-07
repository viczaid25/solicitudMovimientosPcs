using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models
{
    [Table("PC_MOVIMIENTOS_REQUEST")]
    public class PcMovimientosRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Required, MaxLength(100)]
        [Column("SOLICITANTE")]
        public string Solicitante { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Column("DEPARTAMENTO")]
        public string Departamento { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Column("LINEA")]
        public string Linea { get; set; } = string.Empty;

        [MaxLength(300)]
        [Column("COMENTARIOS")]
        public string? Comentarios { get; set; }

        [Required]
        [Column("URGENCIA")]
        public Urgencia Urgencia { get; set; }

        [Required]
        [Column("REQUEST_STATUS")]
        public RequestStatus RequestStatus { get; set; }

        [MaxLength(50)]
        [Column("PC_FOLIO")]
        public string? PcFolio { get; set; }

        [MaxLength(260)]
        [Column("PC_DOC_PATH")]
        public string? PcDocumentoPath { get; set; }

        [MaxLength(100)]
        [Column("PC_FINALIZADO_POR")]
        public string? PcFinalizadoPor { get; set; }
        
        [Column("PC_FINAL_DATE")]
        public DateTime? PcFinalDate { get; set; }

        // Relaciones
        public List<PcMovimientosItem> Items { get; set; } = new();

        public PcMovimientosAprobaciones? Aprobaciones { get; set; }
    }
}
