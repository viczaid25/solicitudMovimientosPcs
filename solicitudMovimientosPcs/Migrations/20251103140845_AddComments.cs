using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class AddComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinJpnComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinMngComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JpnComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "McComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MngComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcJpnComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcMngComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlComments",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinJpnComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "FinMngComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "JpnComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "McComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "MngComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "PcJpnComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "PcMngComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropColumn(
                name: "PlComments",
                table: "PC_MOVIMIENTOS_APROBACIONES");
        }
    }
}
