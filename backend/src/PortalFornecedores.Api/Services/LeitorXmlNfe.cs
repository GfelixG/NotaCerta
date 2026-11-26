using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace PortalFornecedores.Api.Services;

/// <summary>Dados lidos do XML. SomaItens não vai para o banco, serve só para validar.</summary>
public record NotaLida(
    string ChaveAcesso,
    string Numero,
    string CnpjEmitente,
    string RazaoSocialEmitente,
    decimal ValorTotal,
    decimal SomaItens);

/// <summary>
/// Lê o XML da NF-e (layout 4.00) e extrai só os campos que o portal usa.
/// Não valida regras de negócio: isso é papel do ValidadorNotaFiscal.
/// </summary>
public class LeitorXmlNfe
{
    // Todo elemento da NF-e está neste namespace.
    public static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    // Bloqueia DTD e referências externas no XML (proteção contra ataque XXE).
    private static readonly XmlReaderSettings Seguro = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };

    /// <summary>
    /// Campos de texto ausentes viram "" (o validador rejeita com mensagem clara).
    /// Lança XmlException se o arquivo não for XML, não for uma NF-e ou tiver valor ilegível.
    /// </summary>
    public NotaLida Ler(Stream xml)
    {
        using var reader = XmlReader.Create(xml, Seguro);
        var doc = XDocument.Load(reader);

        // <infNFe> é o bloco com os dados da nota. Sem ele, o arquivo não é uma NF-e.
        var infNFe = doc.Descendants(Ns + "infNFe").FirstOrDefault()
            ?? throw new XmlException("O arquivo não contém uma NF-e (tag infNFe não encontrada).");

        // Id="NFe2526...780" -> a chave são os 44 dígitos depois de "NFe"
        var id = (string?)infNFe.Attribute("Id") ?? "";
        var chave = id.StartsWith("NFe") ? id["NFe".Length..] : id;

        // Cada item é um <det>; o valor do item fica em <det><prod><vProd>
        var somaItens = infNFe
            .Elements(Ns + "det")
            .Sum(det => LerDecimal(Texto(det, "prod", "vProd"), "vProd"));

        return new NotaLida(
            ChaveAcesso: chave,
            Numero: Texto(infNFe, "ide", "nNF"),
            CnpjEmitente: Texto(infNFe, "emit", "CNPJ"),
            RazaoSocialEmitente: Texto(infNFe, "emit", "xNome"),
            ValorTotal: LerDecimal(Texto(infNFe, "total", "ICMSTot", "vNF"), "vNF"),
            SomaItens: somaItens);
    }

    /// <summary>
    /// Desce pelo caminho de tags a partir de "pai" e devolve o texto, ou "" se alguma tag faltar.
    /// Ex.: Texto(infNFe, "emit", "CNPJ") lê infNFe/emit/CNPJ.
    /// </summary>
    private static string Texto(XElement pai, params string[] caminho)
    {
        XElement? atual = pai;
        foreach (var tag in caminho)
            atual = atual?.Element(Ns + tag);

        return atual?.Value.Trim() ?? "";
    }

    /// <summary>
    /// Valores da NF-e usam ponto decimal ("249.50"). InvariantCulture garante a leitura correta
    /// mesmo num Windows em português, onde "." é separador de milhar.
    /// </summary>
    private static decimal LerDecimal(string texto, string campo)
    {
        if (decimal.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var valor))
            return valor;

        throw new XmlException($"Valor inválido ou ausente em <{campo}>: \"{texto}\".");
    }
}
