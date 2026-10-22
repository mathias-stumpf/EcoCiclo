using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCiclo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IdentificacaoVisual = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pontos_coleta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    HorarioFuncionamento = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    FonteInformacao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataVerificacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UrlMapa = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pontos_coleta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sugestoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeResiduo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    EmailContato = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sugestoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeUsuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "residuos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 170, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    InstrucoesPreparacao = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    InstrucoesDescarte = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    OQueNaoFazer = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    Cuidados = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    DestinoRecomendado = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PodeSerReciclado = table.Column<bool>(type: "INTEGER", nullable: true),
                    ImpactoAmbiental = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    FonteInformacao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataRevisao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residuos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_residuos_categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "residuos_pontos_coleta",
                columns: table => new
                {
                    ResiduoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PontoColetaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FonteConfirmacao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataConfirmacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residuos_pontos_coleta", x => new { x.ResiduoId, x.PontoColetaId });
                    table.ForeignKey(
                        name: "FK_residuos_pontos_coleta_pontos_coleta_PontoColetaId",
                        column: x => x.PontoColetaId,
                        principalTable: "pontos_coleta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_residuos_pontos_coleta_residuos_ResiduoId",
                        column: x => x.ResiduoId,
                        principalTable: "residuos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sinonimos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResiduoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NomeNormalizado = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sinonimos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sinonimos_residuos_ResiduoId",
                        column: x => x.ResiduoId,
                        principalTable: "residuos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_Slug",
                table: "categorias",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pontos_coleta_Estado_Cidade_Ativo",
                table: "pontos_coleta",
                columns: new[] { "Estado", "Cidade", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_residuos_CategoriaId_Ativo",
                table: "residuos",
                columns: new[] { "CategoriaId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_residuos_Slug",
                table: "residuos",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_residuos_pontos_coleta_PontoColetaId",
                table: "residuos_pontos_coleta",
                column: "PontoColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_sinonimos_NomeNormalizado",
                table: "sinonimos",
                column: "NomeNormalizado");

            migrationBuilder.CreateIndex(
                name: "IX_sinonimos_ResiduoId",
                table: "sinonimos",
                column: "ResiduoId");

            migrationBuilder.CreateIndex(
                name: "IX_sugestoes_Status_DataEnvio",
                table: "sugestoes",
                columns: new[] { "Status", "DataEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_NomeUsuario",
                table: "usuarios",
                column: "NomeUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "residuos_pontos_coleta");

            migrationBuilder.DropTable(
                name: "sinonimos");

            migrationBuilder.DropTable(
                name: "sugestoes");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "pontos_coleta");

            migrationBuilder.DropTable(
                name: "residuos");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}
