// Models/Security/StageAccessEntry.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Security
{
    [NotMapped]
    public class StageAccessEntry
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Stage { get; set; } = "";  // MNG, JPN, MC, PL, PCMNG, PCJPN, FINMNG, FINJPN

        [Required, MaxLength(120)]
        public string DisplayName { get; set; } = ""; // “Alberto Diaz”, “Kensaku Kuroki”, etc.
    }
}
