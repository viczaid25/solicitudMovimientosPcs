using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRequestFechaToDateTime2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PC_STAGE_ACCESS_Stage_DisplayName",
                table: "PC_STAGE_ACCESS");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "datetime2(0)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)");

            migrationBuilder.CreateIndex(
                name: "IX_PC_STAGE_ACCESS_Stage_DisplayName",
                table: "PC_STAGE_ACCESS",
                columns: new[] { "Stage", "DisplayName" },
                unique: true);
        }
    }
}
