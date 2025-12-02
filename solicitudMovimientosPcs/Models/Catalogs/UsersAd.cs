using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace solicitudMovimientosPcs.Models.Catalogs
{
    [Table("users_ad", Schema = "dbo")]
    public class UsersAd
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("PC_LOGIN_ID")]
        public string PcLoginId { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("USERNAME")]
        public string? Username { get; set; }

        [MaxLength(100)]
        [Column("EMAIL")]
        public string? Email { get; set; }

        [MaxLength(50)]
        [Column("DEPARTAMENT")]
        public string? Departament { get; set; }

        [MaxLength(50)]
        [Column("POSITION")]
        public string? Position { get; set; }

        [MaxLength(50)]
        [Column("DEP_2")]
        public string? Dep2 { get; set; }

        [Required]
        [Column("AUTH")]
        public byte Auth { get; set; }
    }
}
