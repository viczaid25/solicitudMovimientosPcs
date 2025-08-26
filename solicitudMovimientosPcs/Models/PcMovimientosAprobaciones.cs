using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models
{
    [Table("PC_MOVIMIENTOS_APROBACIONES")]
    public class PcMovimientosAprobaciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // FK 1:1 con Request
        [Required]
        [Column("REQUEST_ID")]
        public int RequestId { get; set; }

        [ForeignKey(nameof(RequestId))]
        public PcMovimientosRequest? Solicitud { get; set; }

        // MNG
        [MaxLength(50)][Column("MNG")] public string? Mng { get; set; }
        [Column("MNG_STATUS")] public ApprovalStatus? MngStatus { get; set; }
        [Column("MNG_DATE")] public DateTime? MngDate { get; set; }

        // JPN
        [MaxLength(50)][Column("JPN")] public string? Jpn { get; set; }
        [Column("JPN_STATUS")] public ApprovalStatus? JpnStatus { get; set; }
        [Column("JPN_DATE")] public DateTime? JpnDate { get; set; }

        // MC
        [MaxLength(50)][Column("MC")] public string? Mc { get; set; }
        [Column("MC_STATUS")] public ApprovalStatus? McStatus { get; set; }
        [Column("MC_DATE")] public DateTime? McDate { get; set; }

        // PL
        [MaxLength(50)][Column("PL")] public string? Pl { get; set; }
        [Column("PL_STATUS")] public ApprovalStatus? PlStatus { get; set; }
        [Column("PL_DATE")] public DateTime? PlDate { get; set; }

        // PCMNG
        [MaxLength(50)][Column("PCMNG")] public string? PcMng { get; set; }
        [Column("PCMNG_STATUS")] public ApprovalStatus? PcMngStatus { get; set; }
        [Column("PCMNG_DATE")] public DateTime? PcMngDate { get; set; }

        // PCJPN
        [MaxLength(50)][Column("PCJPN")] public string? PcJpn { get; set; }
        [Column("PCJPN_STATUS")] public ApprovalStatus? PcJpnStatus { get; set; }
        [Column("PCJPN_DATE")] public DateTime? PcJpnDate { get; set; }

        // FINMNG
        [MaxLength(50)][Column("FINMNG")] public string? FinMng { get; set; }
        [Column("FINMNG_STATUS")] public ApprovalStatus? FinMngStatus { get; set; }
        [Column("FINMNG_DATE")] public DateTime? FinMngDate { get; set; }

        // FINJPN
        [MaxLength(50)][Column("FINJPN")] public string? FinJpn { get; set; }
        [Column("FINJPN_STATUS")] public ApprovalStatus? FinJpnStatus { get; set; }
        [Column("FINJPN_DATE")] public DateTime? FinJpnDate { get; set; }

    

    }
}
