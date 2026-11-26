# NotaCerta

Portal de fornecedores para recebimento de NF-e. O fornecedor envia o XML da nota, o sistema confere as regras básicas e salva a nota como **Aceita** ou **Rejeitada**, com o motivo em linguagem clara.

> Projeto de estudo. Todos os dados são fictícios.

## O que é conferido (em linguagem simples)

| Regra | Por que importa |
|---|---|
| CNPJ do emitente válido | Evita nota com CNPJ digitado errado, que depois não bate na conciliação. |
| Chave de acesso válida (44 números com dígito verificador) | É a "identidade" da nota na SEFAZ. Com um número errado, ela não pode ser consultada. |
| Soma dos itens igual ao total | Se não bate, o financeiro pagaria o valor errado. |
| Nota não aceita antes | A mesma nota não pode gerar dois pagamentos. Uma nota **rejeitada** pode ser corrigida e reenviada. |

## Respostas da API (`POST /api/notas`)

| Situação | Resposta |
|---|---|
| Nota aceita ou rejeitada pelas regras | `200` com a nota salva (status e motivo) |
| Arquivo que não é XML de NF-e | `400` |
| Já existe uma nota **aceita** com a mesma chave | `409` |

Notas rejeitadas também são salvas, exatamente como chegaram, para quem for investigar ver o que o fornecedor enviou.

## Stack

C# / ASP.NET Core (.NET 10) · Entity Framework Core · PostgreSQL · Next.js · xUnit

## Estrutura

```
backend/
  src/PortalFornecedores.Api/
    Controllers/   endpoints: GET e POST /api/notas
    Domain/        NotaFiscal (o que é salvo no banco)
    Services/      leitura do XML
    Validation/    regras de negócio
    Data/          acesso ao PostgreSQL
  tests/           testes das regras e da leitura do XML (xUnit)
frontend/src/
  app/page.tsx     página única: envio do XML + lista de notas
  components/      formulário de envio e etiqueta de status
  lib/api.ts       chamadas à API
samples/           XMLs fictícios: um válido e um para cada tipo de rejeição
docs/              setup local e guia de implementação
```

## Decisões técnicas

- **Regras separadas do controller** (`Validation/`), testáveis sem banco nem HTTP.
- **Duplicidade garantida no banco**: índice único na chave de acesso, filtrado para notas aceitas. Duas requisições simultâneas com a mesma nota não passam.
- **`decimal` para dinheiro** e leitura com `InvariantCulture`: `"249.50"` não vira `24950` num Windows em português.
- **Leitura de XML com DTD bloqueado**, proteção contra ataque XXE.
- **Status salvo como texto** (`Aceita`/`Rejeitada`) para consultas SQL legíveis durante uma investigação.

## Como rodar

Pré-requisitos: .NET 10 SDK, Node.js e PostgreSQL ([setup no Windows](docs/setup-local.md)).

```bash
# API (http://localhost:5080)
cd backend
dotnet ef database update --project src/PortalFornecedores.Api
dotnet run --project src/PortalFornecedores.Api

# Frontend (http://localhost:3000)
cd frontend
npm install
npm run dev

# Testes
cd backend
dotnet test
```
