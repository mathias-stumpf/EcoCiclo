using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCiclo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResiduoNomeNormalizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeNormalizado",
                table: "residuos",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_residuos_NomeNormalizado",
                table: "residuos",
                column: "NomeNormalizado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_residuos_NomeNormalizado",
                table: "residuos");

            migrationBuilder.DropColumn(
                name: "NomeNormalizado",
                table: "residuos");
        }
    }
}
