using Microsoft.EntityFrameworkCore;
using PortalFornecedores.Api.Domain;

namespace PortalFornecedores.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<NotaFiscal> Notas => Set<NotaFiscal>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<NotaFiscal>(e =>
        {
            e.ToTable("notas_fiscais");
            // O banco impede duas notas ACEITAS com a mesma chave (dois pagamentos).
            // Rejeitadas ficam de fora: o fornecedor pode corrigir e reenviar.
            e.HasIndex(n => n.ChaveAcesso).IsUnique().HasFilter("status = 'Aceita'");
            // Chave e CNPJ sem limite de tamanho: notas rejeitadas são guardadas exatamente
            // como chegaram (ex.: CNPJ com máscara), para quem for investigar ver o que veio.
            e.Property(n => n.ValorTotal).HasPrecision(15, 2);
            e.Property(n => n.Status).HasConversion<string>(); // salva "Aceita"/"Rejeitada" em vez de 0/1
        });
    }
}
