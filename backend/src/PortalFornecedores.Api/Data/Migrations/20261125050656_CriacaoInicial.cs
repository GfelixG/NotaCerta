using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalFornecedores.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notas_fiscais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave_acesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    numero = table.Column<string>(type: "text", nullable: false),
                    cnpj_emitente = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    razao_social_emitente = table.Column<string>(type: "text", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    motivo_rejeicao = table.Column<string>(type: "text", nullable: true),
                    recebida_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notas_fiscais", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notas_fiscais_chave_acesso",
                table: "notas_fiscais",
                column: "chave_acesso",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notas_fiscais");
        }
    }
}
