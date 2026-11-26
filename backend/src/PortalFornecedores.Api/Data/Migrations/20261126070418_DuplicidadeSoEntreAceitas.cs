using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalFornecedores.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DuplicidadeSoEntreAceitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notas_fiscais_chave_acesso",
                table: "notas_fiscais");

            migrationBuilder.AlterColumn<string>(
                name: "cnpj_emitente",
                table: "notas_fiscais",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.AlterColumn<string>(
                name: "chave_acesso",
                table: "notas_fiscais",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(44)",
                oldMaxLength: 44);

            migrationBuilder.CreateIndex(
                name: "ix_notas_fiscais_chave_acesso",
                table: "notas_fiscais",
                column: "chave_acesso",
                unique: true,
                filter: "status = 'Aceita'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notas_fiscais_chave_acesso",
                table: "notas_fiscais");

            migrationBuilder.AlterColumn<string>(
                name: "cnpj_emitente",
                table: "notas_fiscais",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "chave_acesso",
                table: "notas_fiscais",
                type: "character varying(44)",
                maxLength: 44,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_notas_fiscais_chave_acesso",
                table: "notas_fiscais",
                column: "chave_acesso",
                unique: true);
        }
    }
}
