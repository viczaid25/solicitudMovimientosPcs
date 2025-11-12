using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class catalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PC_MOVIMIENTOS_CLASES",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PC_MOVIMIENTOS_CLASES");
        }
    }
}
