using PortalFornecedores.Api.Validation;

namespace PortalFornecedores.Tests.Validation;

// Estes testes descrevem o comportamento esperado. Eles falham até você implementar
// cada regra em ValidadorNotaFiscal. Dados fictícios: nunca use CNPJ de empresa real.
public class ValidadorNotaFiscalTests
{
    // ---------- CNPJ ----------

    [Theory]
    [InlineData("11222333000181")] // emitente do samples/nfe-valida.xml
    [InlineData("99888777000100")]
    public void Cnpj_valido_e_aceito(string cnpj)
    {
        Assert.True(ValidadorNotaFiscal.CnpjValido(cnpj));
    }

    [Theory]
    [InlineData("11222333000182")]     // último dígito errado
    [InlineData("11111111111111")]     // todos iguais
    [InlineData("1122233300018")]      // 13 dígitos
    [InlineData("11.222.333/0001-81")] // com pontuação
    [InlineData("")]
    public void Cnpj_invalido_e_rejeitado(string cnpj)
    {
        Assert.False(ValidadorNotaFiscal.CnpjValido(cnpj));
    }

    // ---------- Chave de acesso ----------

    [Fact]
    public void Chave_valida_e_aceita()
    {
        Assert.True(ValidadorNotaFiscal.ChaveAcessoValida("25261111222333000181550010000001231123456780"));
    }

    [Theory]
    [InlineData("25261111222333000181550010000001231123456781")] // dígito verificador errado
    [InlineData("123")]                                          // tamanho errado
    public void Chave_invalida_e_rejeitada(string chave)
    {
        Assert.False(ValidadorNotaFiscal.ChaveAcessoValida(chave));
    }

    // ---------- Totais ----------

    [Fact]
    public void Totais_iguais_conferem()
    {
        Assert.True(ValidadorNotaFiscal.TotaisConferem(249.50m, 249.50m));
    }

    [Fact]
    public void Totais_diferentes_nao_conferem()
    {
        Assert.False(ValidadorNotaFiscal.TotaisConferem(249.50m, 250.00m));
    }
}
