using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PortalFornecedores.Api.Data;
using PortalFornecedores.Api.Domain;
using PortalFornecedores.Api.Services;
using PortalFornecedores.Api.Validation;

namespace PortalFornecedores.Api.Controllers;

[ApiController]
[Route("api/notas")]
public class NotasController(
    AppDbContext db,
    LeitorXmlNfe leitor,
    ValidadorNotaFiscal validador,
    ILogger<NotasController> logger) : ControllerBase
{
    private const string MensagemDuplicada = "Esta nota já foi enviada.";

    [HttpGet]
    public async Task<List<NotaFiscal>> Listar()
    {
        return await db.Notas.OrderByDescending(n => n.RecebidaEm).ToListAsync();
    }

    /// <summary>
    /// Recebe o XML da NF-e, valida e salva como Aceita ou Rejeitada.
    /// 200: nota salva (aceita ou rejeitada, com o motivo) · 400: arquivo ilegível · 409: nota duplicada.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Enviar(IFormFile? arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest("Selecione um arquivo XML.");

        // 1. Ler o XML
        NotaLida lida;
        try
        {
            await using var conteudo = arquivo.OpenReadStream();
            lida = leitor.Ler(conteudo);
        }
        catch (XmlException ex)
        {
            logger.LogWarning("Arquivo {Arquivo} rejeitado na leitura: {Motivo}", arquivo.FileName, ex.Message);
            return BadRequest("O arquivo não é um XML de NF-e válido.");
        }

        // 2. Nota duplicada? Só conta se já existe uma ACEITA com a mesma chave;
        //    uma rejeitada pode ser corrigida e reenviada.
        if (await db.Notas.AnyAsync(n => n.ChaveAcesso == lida.ChaveAcesso && n.Status == StatusNota.Aceita))
            return Conflict(MensagemDuplicada);

        // 3. Validar regras de negócio
        var erros = validador.Validar(lida);

        // 4. Montar a nota que vai para o banco
        var nota = new NotaFiscal
        {
            ChaveAcesso = lida.ChaveAcesso,
            Numero = lida.Numero,
            CnpjEmitente = lida.CnpjEmitente,
            RazaoSocialEmitente = lida.RazaoSocialEmitente,
            ValorTotal = lida.ValorTotal,
            Status = erros.Count == 0 ? StatusNota.Aceita : StatusNota.Rejeitada,
            MotivoRejeicao = erros.Count == 0 ? null : string.Join(" ", erros),
        };

        // 5. Salvar
        db.Notas.Add(nota);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Duas requisições com a mesma nota ao mesmo tempo: o índice único do banco barra a segunda.
            return Conflict(MensagemDuplicada);
        }

        logger.LogInformation("Nota {Numero} ({Chave}) salva como {Status}", nota.Numero, nota.ChaveAcesso, nota.Status);
        return Ok(nota);
    }
}
