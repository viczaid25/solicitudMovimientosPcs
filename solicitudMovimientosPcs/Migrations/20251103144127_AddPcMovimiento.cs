using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class AddPcMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PcTipoMovimiento",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PcTipoMovimiento",
                table: "PC_MOVIMIENTOS_REQUEST");
        }
    }
}
