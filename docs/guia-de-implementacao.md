# Guia de implementação

O que cada função a implementar recebe, o que devolve e quais regras precisa seguir.
Implemente **na ordem abaixo**: cada etapa usa a anterior.

```
 XML enviado ──► LeitorXmlNfe.Ler ──► NotaLida ──► ValidadorNotaFiscal.Validar ──► erros
                                                                                     │
           NotasController.Enviar: monta NotaFiscal (Aceita/Rejeitada) e salva ◄──────┘
                                                                                     │
           UploadXml (frontend): mostra o resultado e atualiza a tabela ◄───────────┘
```

---

## 1. `ValidadorNotaFiscal.CnpjValido`

📄 `backend/src/PortalFornecedores.Api/Validation/ValidadorNotaFiscal.cs`

```csharp
public static bool CnpjValido(string cnpj)
```

| | |
|---|---|
| **Entrada** | `cnpj`: texto como vem do XML, **sem pontuação**. Ex.: `"11222333000181"` |
| **Saída** | `true` se o CNPJ for válido, `false` caso contrário. **Nunca lança exceção.** |

**Regras, nesta ordem** (falhou uma, devolve `false`):
1. Ter exatamente 14 caracteres.
2. Todos os caracteres serem dígitos (`char.IsDigit`).
3. Não ter todos os dígitos iguais (`"11111111111111"`).
4. O 13º dígito ser o 1º dígito verificador (DV) correto.
5. O 14º dígito ser o 2º DV correto.

**Como calcular um DV:**
- Multiplique cada dígito pelo peso da mesma posição e some tudo.
- `resto = soma % 11`. Se `resto < 2`, o DV é `0`; senão, o DV é `11 - resto`.

| DV | Dígitos usados | Pesos |
|---|---|---|
| 1º | os 12 primeiros | `5 4 3 2 9 8 7 6 5 4 3 2` |
| 2º | os 13 primeiros | `6 5 4 3 2 9 8 7 6 5 4 3 2` |

> Dica: o valor numérico de um caractere dígito é `c - '0'`. Uma função auxiliar
> `private static int CalcularDv(string digitos, int[] pesos)` evita repetir a conta.

**Exemplos (são os testes):**

| Entrada | Saída | Por quê |
|---|---|---|
| `"11222333000181"` | `true` | |
| `"99888777000100"` | `true` | resto < 2 → DV 0 |
| `"11222333000182"` | `false` | 2º DV errado |
| `"11111111111111"` | `false` | todos iguais |
| `"1122233300018"` | `false` | 13 dígitos |
| `"11.222.333/0001-81"` | `false` | tem pontuação |
| `""` | `false` | vazio |

---

## 2. `ValidadorNotaFiscal.ChaveAcessoValida`

📄 mesmo arquivo

```csharp
public static bool ChaveAcessoValida(string chave)
```

| | |
|---|---|
| **Entrada** | `chave`: os 44 dígitos da chave de acesso, **já sem o prefixo `"NFe"`** |
| **Saída** | `true` se válida, `false` caso contrário. **Nunca lança exceção.** |

**Regras:**
1. Ter exatamente 44 caracteres, todos dígitos.
2. O último dígito (posição 44) ser o DV dos 43 primeiros.

**Como calcular o DV da chave** (diferente do CNPJ!):
- Percorra os 43 primeiros dígitos **da direita para a esquerda**.
- Os pesos vão `2, 3, 4, 5, 6, 7, 8, 9` e voltam para `2`: `2,3,4,5,6,7,8,9,2,3,…`
- `resto = soma % 11`. Se `resto < 2`, o DV é `0`; senão, o DV é `11 - resto`.

**Exemplos:**

| Entrada | Saída |
|---|---|
| `"25261111222333000181550010000001231123456780"` | `true` |
| `"25261111222333000181550010000001231123456781"` | `false` (DV errado) |
| `"123"` | `false` (tamanho) |

---

## 3. `ValidadorNotaFiscal.TotaisConferem`

📄 mesmo arquivo

```csharp
public static bool TotaisConferem(decimal somaItens, decimal valorTotal)
```

| | |
|---|---|
| **Entrada** | `somaItens`: soma dos `<vProd>` dos itens. `valorTotal`: o `<vNF>` da nota |
| **Saída** | `true` se forem iguais, `false` se diferentes |

**Regra:** comparar direto (`==`). Os dois já são `decimal`, que é exato para dinheiro; **não converta para `double`**.

| Entrada | Saída |
|---|---|
| `249.50m, 249.50m` | `true` |
| `249.50m, 250.00m` | `false` |

✅ Depois das etapas 1 a 3, os 12 testes devem passar: `cd backend; dotnet test`

> `Validar(NotaLida nota)` **já está pronta**: ela chama as três funções acima e devolve
> uma `List<string>` com as mensagens de erro (lista vazia = nota aceita).

---

## 4. `LeitorXmlNfe.Ler`

📄 `backend/src/PortalFornecedores.Api/Services/LeitorXmlNfe.cs`

```csharp
public NotaLida Ler(Stream xml)
```

| | |
|---|---|
| **Entrada** | `xml`: o conteúdo do arquivo enviado (no controller vem de `arquivo.OpenReadStream()`) |
| **Saída** | um `NotaLida` preenchido (record já definido no mesmo arquivo) |
| **Erro** | se o XML estiver malformado, `XDocument.Load` lança `XmlException`; deixe subir, o controller trata |

**De onde vem cada campo de `NotaLida`** (veja `samples/nfe-valida.xml`):

| Campo | Tipo | Onde está no XML | Valor no exemplo |
|---|---|---|---|
| `ChaveAcesso` | `string` | atributo `Id` de `<infNFe>`, **tirando o prefixo `"NFe"`** | `"2526111122…780"` |
| `Numero` | `string` | `<ide><nNF>` | `"123"` |
| `CnpjEmitente` | `string` | `<emit><CNPJ>` | `"11222333000181"` |
| `RazaoSocialEmitente` | `string` | `<emit><xNome>` | `"Fornecedor Exemplo Ltda"` |
| `ValorTotal` | `decimal` | `<total><ICMSTot><vNF>` | `249.50` |
| `SomaItens` | `decimal` | soma de **todos** os `<det><prod><vProd>` | `155.00 + 94.50 = 249.50` |

**Pontos de atenção:**
- Todos os elementos estão no namespace da NF-e. Busque com `Ns + "nome"`, ex.: `doc.Descendants(Ns + "emit")`. Sem o `Ns`, a busca não encontra nada.
- Valores usam **ponto** decimal. Use `decimal.Parse(texto, CultureInfo.InvariantCulture)`; sem isso, no Windows em português `"249.50"` vira `24950`.
- Para somar os itens: `Descendants(Ns + "vProd")` + `Sum(...)`.

---

## 5. `NotasController.Enviar`

📄 `backend/src/PortalFornecedores.Api/Controllers/NotasController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> Enviar(IFormFile arquivo)
```

> Troque a assinatura atual por esta (`async Task<…>`), pois você vai usar `await` no banco.

| | |
|---|---|
| **Entrada** | `arquivo`: o XML enviado em `multipart/form-data` no campo `"arquivo"` |
| **Saída** | uma resposta HTTP (veja tabela) |

**Passo a passo:**
1. `leitor.Ler(arquivo.OpenReadStream())` → `NotaLida`.
2. Se já existe nota com a mesma `ChaveAcesso` no banco (`db.Notas.AnyAsync(...)`), devolver `409`.
3. `validador.Validar(notaLida)` → lista de erros.
4. Criar `new NotaFiscal { ... }` copiando os campos de `NotaLida`:
   - lista vazia → `Status = StatusNota.Aceita`, `MotivoRejeicao = null`
   - com erros → `Status = StatusNota.Rejeitada`, `MotivoRejeicao = string.Join(" ", erros)`
5. `db.Notas.Add(nota)` e `await db.SaveChangesAsync()`.
6. Devolver `Ok(nota)`.

**Respostas:**

| Situação | Retorno | Código |
|---|---|---|
| Nota aceita **ou** rejeitada pelas regras | `Ok(nota)` | 200 |
| Chave já cadastrada | `Conflict("Esta nota já foi enviada.")` | 409 |
| Arquivo não é um XML válido | `BadRequest("O arquivo não é um XML de NF-e válido.")` | 400 |

> Uma nota **rejeitada** devolve 200, não 400: o envio funcionou, a nota é que tem problema,
> e ela fica salva com o motivo. O 400 é só para arquivo ilegível (`catch (XmlException)`).

**Como testar** (com a API rodando):

```powershell
curl.exe -F "arquivo=@samples/nfe-valida.xml" http://localhost:5080/api/notas
```

Rode duas vezes: a primeira devolve a nota `Aceita`, a segunda `409`.

---

## 6. `UploadXml` (frontend)

📄 `frontend/src/components/upload-xml.tsx`

```tsx
export function UploadXml()   // sem props
```

| | |
|---|---|
| **Entrada** | nenhuma prop; o usuário escolhe um arquivo `.xml` na tela |
| **Saída** | o formulário na tela + uma mensagem de resultado |
| **Usa** | `enviarXml(arquivo: File): Promise<NotaFiscal>` de `@/lib/api` (já pronta; lança `Error` com o texto da API quando o status não é 2xx) |

**Estados que o componente precisa guardar** (`useState`):

| Estado | Tipo | Para quê |
|---|---|---|
| `arquivo` | `File \| null` | arquivo escolhido no `<input type="file" accept=".xml">` |
| `mensagem` | `string \| null` | texto do resultado |
| `enviando` | `boolean` | desabilitar o botão durante o envio |

**Mensagens a mostrar:**

| Resultado de `enviarXml` | Mensagem |
|---|---|
| `nota.status === "Aceita"` | "Nota {numero} aceita." |
| `nota.status === "Rejeitada"` | "Nota rejeitada: {motivoRejeicao}" |
| lançou erro (409, 400, API fora) | `erro.message` |

Depois de enviar, chame `router.refresh()` (`useRouter` de `"next/navigation"`) para a tabela recarregar.

---

## Checklist final

- [ ] `dotnet test`: 12 aprovados
- [ ] `curl` com `nfe-valida.xml`: 200 / Aceita; repetindo: 409
- [ ] Criar `samples/nfe-cnpj-invalido.xml` (troque o CNPJ para `11222333000182`): 200 / Rejeitada com motivo
- [ ] Na tela: enviar, ver a mensagem e a nota aparecer na tabela
