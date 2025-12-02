using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solicitudMovimientosPcs.Migrations
{
    /// <inheritdoc />
    public partial class AddStageAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PC_STAGE_ACCESS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PC_STAGE_ACCESS", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PC_STAGE_ACCESS_Stage_DisplayName",
                table: "PC_STAGE_ACCESS",
                columns: new[] { "Stage", "DisplayName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PC_STAGE_ACCESS");
        }
    }
}
