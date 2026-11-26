# XMLs de exemplo

Arquivos **fictícios**, com CNPJs gerados para teste. Nunca coloque notas reais aqui.

| Arquivo | Resultado esperado |
|---|---|
| `nfe-valida.xml` | **Aceita**: CNPJ válido, chave correta, soma dos itens = 249,50 |
| `nfe-valida.xml` (enviado de novo) | **409**: "Esta nota já foi enviada." |
| `nfe-cnpj-invalido.xml` | **Rejeitada**: "O CNPJ do emitente é inválido." (último dígito trocado) |
| `nfe-total-divergente.xml` | **Rejeitada**: "A soma dos itens não confere…" (`vNF` 250,00, itens 249,50) |
| `arquivo-invalido.xml` | **400**: "O arquivo não é um XML de NF-e válido." |

Cada arquivo de rejeição quebra **uma regra só**: as chaves de acesso deles têm dígito verificador correto.

Testar pela linha de comando (com a API rodando):

```powershell
curl.exe -F "arquivo=@samples/nfe-valida.xml" http://localhost:5080/api/notas
```
