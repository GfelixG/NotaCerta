using System.Globalization;
using System.Text;
using System.Xml;
using PortalFornecedores.Api.Services;
using PortalFornecedores.Api.Validation;

namespace PortalFornecedores.Tests.Services;

public class LeitorXmlNfeTests
{
    private readonly LeitorXmlNfe _leitor = new();

    private NotaLida LerTexto(string xml) => _leitor.Ler(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

    // Procura samples/nfe-valida.xml subindo a partir da pasta onde os testes rodam.
    private static string CaminhoSample(string arquivo)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "samples", arquivo)))
            dir = dir.Parent;

        return dir is null
            ? throw new FileNotFoundException($"samples/{arquivo} não encontrado")
            : Path.Combine(dir.FullName, "samples", arquivo);
    }

    [Fact]
    public void Le_todos_os_campos_do_xml_de_exemplo()
    {
        using var arquivo = File.OpenRead(CaminhoSample("nfe-valida.xml"));

        var nota = _leitor.Ler(arquivo);

        Assert.Equal("25261111222333000181550010000001231123456780", nota.ChaveAcesso); // sem "NFe"
        Assert.Equal("123", nota.Numero);
        Assert.Equal("11222333000181", nota.CnpjEmitente);
        Assert.Equal("Fornecedor Exemplo Ltda", nota.RazaoSocialEmitente);
        Assert.Equal(249.50m, nota.ValorTotal);
        Assert.Equal(249.50m, nota.SomaItens); // 155.00 + 94.50
    }

    [Fact]
    public void Xml_de_exemplo_passa_em_todas_as_regras()
    {
        using var arquivo = File.OpenRead(CaminhoSample("nfe-valida.xml"));

        var erros = new ValidadorNotaFiscal().Validar(_leitor.Ler(arquivo));

        Assert.Empty(erros);
    }

    [Fact]
    public void Ponto_decimal_e_lido_certo_mesmo_com_o_windows_em_portugues()
    {
        var culturaOriginal = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR"); // em pt-BR "." é separador de milhar
        try
        {
            using var arquivo = File.OpenRead(CaminhoSample("nfe-valida.xml"));
            Assert.Equal(249.50m, _leitor.Ler(arquivo).ValorTotal); // e não 24950
        }
        finally
        {
            CultureInfo.CurrentCulture = culturaOriginal;
        }
    }

    [Fact]
    public void Tag_de_texto_ausente_vira_texto_vazio()
    {
        var nota = LerTexto("""
            <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
              <infNFe Id="NFe123">
                <emit><xNome>Sem CNPJ Ltda</xNome></emit>
                <total><ICMSTot><vNF>10.00</vNF></ICMSTot></total>
              </infNFe>
            </NFe>
            """);

        Assert.Equal("", nota.CnpjEmitente); // o validador vai rejeitar com mensagem clara
        Assert.Equal("", nota.Numero);
        Assert.Equal(0m, nota.SomaItens);    // nenhum <det>
    }

    [Fact]
    public void Arquivo_que_nao_e_xml_lanca_XmlException()
    {
        Assert.Throws<XmlException>(() => LerTexto("isto não é xml"));
    }

    [Fact]
    public void Xml_que_nao_e_nfe_lanca_XmlException()
    {
        Assert.Throws<XmlException>(() => LerTexto("<pedido><numero>1</numero></pedido>"));
    }

    [Fact]
    public void Valor_total_ilegivel_lanca_XmlException()
    {
        Assert.Throws<XmlException>(() => LerTexto("""
            <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
              <infNFe Id="NFe123">
                <total><ICMSTot><vNF>abc</vNF></ICMSTot></total>
              </infNFe>
            </NFe>
            """));
    }

    [Fact]
    public void Xml_com_DTD_e_bloqueado()
    {
        // Ataque XXE: tenta fazer o servidor ler um arquivo local através de uma entidade externa.
        Assert.Throws<XmlException>(() => LerTexto("""
            <?xml version="1.0"?>
            <!DOCTYPE NFe [ <!ENTITY xxe SYSTEM "file:///c:/windows/win.ini"> ]>
            <NFe xmlns="http://www.portalfiscal.inf.br/nfe"><infNFe Id="&xxe;"/></NFe>
            """));
    }
}
