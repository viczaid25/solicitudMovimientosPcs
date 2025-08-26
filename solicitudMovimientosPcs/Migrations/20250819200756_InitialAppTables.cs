using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class InitialAppTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_CLASES",
                columns: table => new
                {
                    CLASS_CODE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_CLASES", x => x.CLASS_CODE);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_COD_MOVIMIENTOS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PRC_ID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RSN_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CONTENT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_COD_MOVIMIENTOS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_CODIGO",
                columns: table => new
                {
                    CODIGO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DESCRIPCION = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_CODIGO", x => x.CODIGO);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_CODIGO_LINEA",
                columns: table => new
                {
                    AREA_CODE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AREA_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_CODIGO_LINEA", x => x.AREA_CODE);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_REQUEST",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FECHA = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SOLICITANTE = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DEPARTAMENTO = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LINEA = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    COMENTARIOS = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    URGENCIA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    REQUEST_STATUS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_REQUEST", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_UBICACION",
                columns: table => new
                {
                    UBICACION = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AREA = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_UBICACION", x => x.UBICACION);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_APROBACIONES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    REQUEST_ID = table.Column<int>(type: "int", nullable: false),
                    MNG = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MNG_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MNG_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JPN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JPN_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JPN_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MC_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MC_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PL_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PL_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PCMNG = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PCMNG_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PCMNG_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PCJPN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PCJPN_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PCJPN_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FINMNG = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FINMNG_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FINMNG_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FINJPN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FINJPN_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FINJPN_DATE = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_APROBACIONES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PC_MOVIMIENTOS_APROBACIONES_PC_MOVIMIENTOS_REQUEST_REQUEST_ID",
                        column: x => x.REQUEST_ID,
                        principalTable: "PC_MOVIMIENTOS_REQUEST",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PC_MOVIMIENTOS_ITEMS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_SOLICITUD = table.Column<int>(type: "int", nullable: false),
                    NUMERO = table.Column<int>(type: "int", nullable: false),
                    NUM_PARTE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DESCRIPCION = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CASE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    COD_MOV = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ESTATUS_A = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UBICACION_A = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CLASE_A = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CANTIDAD_A = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ESTATUS_D = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UBICACION_D = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CLASE_D = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CANTIDAD_D = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DIFERENCIA = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MONEDA = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    COSTO_U = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_MOVIMIENTOS_ITEMS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PC_MOVIMIENTOS_ITEMS_PC_MOVIMIENTOS_REQUEST_ID_SOLICITUD",
                        column: x => x.ID_SOLICITUD,
                        principalTable: "PC_MOVIMIENTOS_REQUEST",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PC_MOVIMIENTOS_APROBACIONES_REQUEST_ID",
                table: "PC_MOVIMIENTOS_APROBACIONES",
                column: "REQUEST_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PC_MOVIMIENTOS_COD_MOVIMIENTOS_PRC_ID_RSN_CD",
                table: "PC_MOVIMIENTOS_COD_MOVIMIENTOS",
                columns: new[] { "PRC_ID", "RSN_CD" });

            migrationBuilder.CreateIndex(
                name: "IX_PC_MOVIMIENTOS_CODIGO_DESCRIPCION",
                table: "PC_MOVIMIENTOS_CODIGO",
                column: "DESCRIPCION");

            migrationBuilder.CreateIndex(
                name: "IX_PC_MOVIMIENTOS_CODIGO_LINEA_AREA_NAME",
                table: "PC_MOVIMIENTOS_CODIGO_LINEA",
                column: "AREA_NAME");

            migrationBuilder.CreateIndex(
                name: "IX_PC_MOVIMIENTOS_ITEMS_ID_SOLICITUD",
                table: "PC_MOVIMIENTOS_ITEMS",
                column: "ID_SOLICITUD");

            migrationBuilder.CreateIndex(
                name: "IX_PC_MOVIMIENTOS_UBICACION_AREA",
                table: "PC_MOVIMIENTOS_UBICACION",
                column: "AREA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_APROBACIONES");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_CLASES");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_COD_MOVIMIENTOS");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_CODIGO");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_CODIGO_LINEA");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_ITEMS");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_UBICACION");

            migrationBuilder.DropTable(
                name: "PC_MOVIMIENTOS_REQUEST");
        }
    }
}
