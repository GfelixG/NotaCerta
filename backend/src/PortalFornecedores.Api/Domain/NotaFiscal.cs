namespace PortalFornecedores.Api.Domain;

public enum StatusNota
{
    Aceita,
    Rejeitada,
}

/// <summary>NF-e enviada por um fornecedor e salva no banco.</summary>
public class NotaFiscal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ChaveAcesso { get; set; } = "";
    public string Numero { get; set; } = "";
    public string CnpjEmitente { get; set; } = "";
    public string RazaoSocialEmitente { get; set; } = "";
    public decimal ValorTotal { get; set; }
    public StatusNota Status { get; set; }
    public string? MotivoRejeicao { get; set; }
    public DateTimeOffset RecebidaEm { get; set; } = DateTimeOffset.UtcNow;
}
