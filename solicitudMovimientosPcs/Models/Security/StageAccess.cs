// Models/Security/StageAccess.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Security
{
    [Table("PC_STAGE_ACCESS")]
    public class StageAccess
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        [Column("Stage")]                  // nvarchar(20) en DB
        public string Stage { get; set; } = string.Empty;

        // La tabla tiene DISPLAYNAME; mapeamos la propiedad UserName a esa columna
        [Required, MaxLength(120)]
        [Column("DisplayName")]            // <-- ¡clave! antes causaba "Invalid column 'UserName'"
        public string UserName { get; set; } = string.Empty;

        // Estas columnas NO existen hoy en la tabla -> marcarlas como NotMapped (o elimínalas por ahora)
        [NotMapped]
        public string? EmailOverride { get; set; }

        [NotMapped]
        public bool CanView { get; set; } = true;

        [NotMapped]
        public bool CanApprove { get; set; } = true;
    }
}
