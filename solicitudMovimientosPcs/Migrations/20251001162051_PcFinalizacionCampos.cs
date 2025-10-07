using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class PcFinalizacionCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "PC_DOC_PATH",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PC_FINALIZADO_POR",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PC_FINAL_DATE",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PC_FOLIO",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "COD_MOV",
                table: "PC_MOVIMIENTOS_ITEMS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PC_DOC_PATH",
                table: "PC_MOVIMIENTOS_REQUEST");

            migrationBuilder.DropColumn(
                name: "PC_FINALIZADO_POR",
                table: "PC_MOVIMIENTOS_REQUEST");

            migrationBuilder.DropColumn(
                name: "PC_FINAL_DATE",
                table: "PC_MOVIMIENTOS_REQUEST");

            migrationBuilder.DropColumn(
                name: "PC_FOLIO",
                table: "PC_MOVIMIENTOS_REQUEST");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA",
                table: "PC_MOVIMIENTOS_REQUEST",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "COD_MOV",
                table: "PC_MOVIMIENTOS_ITEMS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
