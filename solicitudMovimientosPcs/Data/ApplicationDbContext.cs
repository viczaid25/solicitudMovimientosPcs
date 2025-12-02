using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using solicitudMovimientosPcs.Models;
using solicitudMovimientosPcs.Models.Catalogs;
using solicitudMovimientosPcs.Models.Security;


namespace solicitudMovimientosPcs.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Tablas principales ---
        public DbSet<PcMovimientosRequest> PcMovimientosRequests { get; set; } = default!;
        public DbSet<PcMovimientosItem> PcMovimientosItems { get; set; } = default!;
        public DbSet<PcMovimientosAprobaciones> PcMovimientosAprobaciones { get; set; } = default!;


        // --- Catálogos ---
        public DbSet<PcMovimientosClase> PcMovimientosClases { get; set; } = default!;
        public DbSet<PcMovimientosCodigoLinea> PcMovimientosCodigoLineas { get; set; } = default!;
        public DbSet<PcMovimientosCodigo> PcMovimientosCodigos { get; set; } = default!;
        public DbSet<PcMovimientosCodMovimiento> PcMovimientosCodMovimientos { get; set; } = default!;
        public DbSet<PcMovimientosUbicacion> PcMovimientosUbicaciones { get; set; } = default!;
        public DbSet<StageAccess> StageAccesses { get; set; } = default!;

        public DbSet<UsersAd> UsersAd { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Conversores Enum -> string ---
            var urgenciaConverter = new EnumToStringConverter<Urgencia>();
            var statusConverter = new EnumToStringConverter<RequestStatus>();
            var approvalConverter = new EnumToStringConverter<ApprovalStatus>();

            // ===== PcMovimientosRequest =====
            modelBuilder.Entity<PcMovimientosRequest>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_REQUEST");

                e.Property(p => p.Solicitante).HasMaxLength(100).IsRequired();
                e.Property(p => p.Departamento).HasMaxLength(100).IsRequired();
                e.Property(p => p.Linea).HasMaxLength(100).IsRequired();
                e.Property(p => p.Comentarios).HasMaxLength(300);
                e.Property(p => p.Fecha).HasColumnType("datetime2(0)");
                e.Property(p => p.PcFolio).HasMaxLength(50);
                e.Property(p => p.PcDocumentoPath).HasMaxLength(260);
                e.Property(p => p.PcFinalizadoPor).HasMaxLength(100);
                e.Property(p => p.PcFinalDate);

                e.Property(p => p.Urgencia)
                 .HasConversion(urgenciaConverter)
                 .HasMaxLength(50)
                 .IsRequired();

                e.Property(p => p.RequestStatus)
                 .HasConversion(statusConverter)
                 .HasMaxLength(20)
                 .IsRequired();

                e.Property(x => x.Fecha)
                 .HasColumnName("FECHA")
                 .HasColumnType("datetime2(0)");
            });

            // ===== PcMovimientosItem =====
            modelBuilder.Entity<PcMovimientosItem>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_ITEMS");

                // FK: Request (1) -> (N) Items
                e.HasOne(i => i.Solicitud)
                 .WithMany(r => r.Items)
                 .HasForeignKey(i => i.IdSolicitud)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(p => p.NumParte).HasMaxLength(50);
                e.Property(p => p.Descripcion).HasMaxLength(100);
                e.Property(p => p.Case).HasMaxLength(50);
                e.Property(p => p.CodMov).HasMaxLength(50);
                e.Property(p => p.EstatusA).HasMaxLength(50);
                e.Property(p => p.UbicacionA).HasMaxLength(50);
                e.Property(p => p.ClaseA).HasMaxLength(50);
                e.Property(p => p.EstatusD).HasMaxLength(50);
                e.Property(p => p.UbicacionD).HasMaxLength(50);
                e.Property(p => p.ClaseD).HasMaxLength(50);
                e.Property(p => p.Moneda).HasMaxLength(3);

                // Decimales con precisión SQL Server
                e.Property(p => p.CantidadA).HasColumnType("decimal(18,2)");
                e.Property(p => p.CantidadD).HasColumnType("decimal(18,2)");
                e.Property(p => p.Diferencia).HasColumnType("decimal(18,2)");
                e.Property(p => p.CostoU).HasColumnType("decimal(18,2)");
                e.Property(p => p.Total).HasColumnType("decimal(18,2)");
            });

            // ===== PcMovimientosAprobaciones =====
            modelBuilder.Entity<PcMovimientosAprobaciones>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_APROBACIONES");

                // Relación 1:1 con Request (único por Request)
                e.HasIndex(x => x.RequestId).IsUnique();

                e.HasOne(a => a.Solicitud)
                 .WithOne(r => r.Aprobaciones)
                 .HasForeignKey<PcMovimientosAprobaciones>(a => a.RequestId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Enums de status como string
                e.Property(a => a.MngStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.JpnStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.McStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.PlStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.PcMngStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.PcJpnStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.FinMngStatus).HasConversion(approvalConverter).HasMaxLength(50);
                e.Property(a => a.FinJpnStatus).HasConversion(approvalConverter).HasMaxLength(50);

                // Longitudes de responsables (nombres/usuarios)
                e.Property(a => a.Mng).HasMaxLength(50);
                e.Property(a => a.Jpn).HasMaxLength(50);
                e.Property(a => a.Mc).HasMaxLength(50);
                e.Property(a => a.Pl).HasMaxLength(50);
                e.Property(a => a.PcMng).HasMaxLength(50);
                e.Property(a => a.PcJpn).HasMaxLength(50);
                e.Property(a => a.FinMng).HasMaxLength(50);
                e.Property(a => a.FinJpn).HasMaxLength(50);
            });

            // ===== Catálogos =====
            modelBuilder.Entity<PcMovimientosClase>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_CLASES");
                e.Property(x => x.ClassCode).HasMaxLength(10).IsRequired();
            });

            modelBuilder.Entity<PcMovimientosCodigoLinea>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_CODIGO_LINEA");
                e.Property(x => x.AreaCode).HasMaxLength(10).IsRequired();
                e.Property(x => x.AreaName).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.AreaName);
            });

            modelBuilder.Entity<PcMovimientosCodigo>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_CODIGO");
                e.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Descripcion);
            });

            modelBuilder.Entity<PcMovimientosCodMovimiento>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_COD_MOVIMIENTOS");
                e.Property(x => x.PrcId).HasMaxLength(10);
                e.Property(x => x.RsnCd).HasMaxLength(10);
                e.Property(x => x.Content).HasMaxLength(100);
                e.HasIndex(x => new { x.PrcId, x.RsnCd });
            });

            modelBuilder.Entity<PcMovimientosUbicacion>(e =>
            {
                e.ToTable("PC_MOVIMIENTOS_UBICACION");
                e.Property(x => x.Ubicacion).HasMaxLength(10).IsRequired();
                e.Property(x => x.Area).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Area);
            });

            modelBuilder.Entity<StageAccess>(e =>
            {
                e.ToTable("PC_STAGE_ACCESS");
                e.HasKey(x => x.Id);
                e.Property(x => x.Stage).HasColumnName("Stage").HasMaxLength(20).IsRequired();
                e.Property(x => x.UserName).HasColumnName("DisplayName").HasMaxLength(120).IsRequired();
            });

            modelBuilder.Entity<UsersAd>(e =>
            {
                e.ToTable("users_ad", "dbo", tb => tb.ExcludeFromMigrations());
                e.HasKey(x => x.Id);
                e.Property(x => x.PcLoginId).HasMaxLength(50).IsRequired();
                e.Property(x => x.Username).HasMaxLength(100);
                e.Property(x => x.Email).HasMaxLength(100);

                // índices útiles para búsquedas
                e.HasIndex(x => x.Username);
                e.HasIndex(x => x.Email);
                e.HasIndex(x => x.PcLoginId);
            });
        }
    }
}
