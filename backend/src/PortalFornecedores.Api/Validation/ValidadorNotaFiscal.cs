using PortalFornecedores.Api.Services;

namespace PortalFornecedores.Api.Validation;

internal static class CnpjHelper
{
    internal static bool isDigit(string cnpj)
    {
        for (int i = 0; i < cnpj.Length; i++)
        {
            if (!char.IsDigit(cnpj[i]))
                return false;
        }

        return true;
    }

    internal static bool ValidarDigitos(string cnpj)
    {
        int[] multiplicadores1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicadores2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCnpj = cnpj.Substring(0, 12);
        int soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicadores1[i];

        int resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        string digito = resto.ToString();
        tempCnpj = tempCnpj + digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicadores2[i];

        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();

        return cnpj.EndsWith(digito);
    }
}

internal static class ChaveHelper
{
    internal static bool ValidaChave(string chave)
    {
        int[] multiplicadores = new int[43] { 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2,
                                              9, 8, 7, 6, 5, 4, 3, 2, 9, 8, 7,
                                              6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4,
                                              3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempChave = chave.Substring(0, 43);
        int soma = 0;

        for (int i = 0; i < 43; i++)
            soma += int.Parse(tempChave[i].ToString()) * multiplicadores[i];

        int resto = soma % 11;
        int digito = resto < 2 ? 0 : 11 - resto;

        return chave.EndsWith(digito.ToString());
    }

}


/// Regras de negócio da NF-e. As mensagens são para o fornecedor ler, então
/// precisam ser frases claras, não mensagens técnicas.
/// 
public class ValidadorNotaFiscal
{
    public List<string> Validar(NotaLida nota)
    {
        var erros = new List<string>();

        if (!CnpjValido(nota.CnpjEmitente))
            erros.Add("O CNPJ do emitente é inválido.");

        if (!ChaveAcessoValida(nota.ChaveAcesso))
            erros.Add("A chave de acesso é inválida.");

        if (!TotaisConferem(nota.SomaItens, nota.ValorTotal))
            erros.Add("A soma dos itens não confere com o valor total da nota.");

        return erros; 
    }

    // TODO: 14 dígitos numéricos, rejeitar todos iguais (ex.: 11111111111111)
    // e conferir os 2 dígitos verificadores (módulo 11).
    public static bool CnpjValido(string cnpj)
    {
        if  (cnpj.Length != 14)
            return false;

        if (cnpj.Distinct().Count() == 1)
            return false;

        if (!CnpjHelper.isDigit(cnpj))
            return false;

        if (!CnpjHelper.ValidarDigitos(cnpj))
            return false;
    
        return true;
    }

    public static bool ChaveAcessoValida(string chave)
    {
        // Confere tamanho e dígitos antes do módulo 11, senão Substring/int.Parse estouram.
        if (string.IsNullOrEmpty(chave) || chave.Length != 44 || !chave.All(char.IsDigit))
            return false;

        return ChaveHelper.ValidaChave(chave);
    }

    // TODO: comparar os dois valores (são decimal, compare direto, sem converter para double).
    public static bool TotaisConferem(decimal somaItens, decimal valorTotal)
    {
        return somaItens == valorTotal;
    }
}
