# Ambiente local (Windows, sem Docker)

Credenciais **apenas de desenvolvimento local**. Nunca use em produção.

| O quê | Valor |
|---|---|
| PostgreSQL | 17, serviço `postgresql-17`, porta 5432 |
| Superusuário | `postgres` / `postgres_dev` |
| Usuário da API | `portal` / `portal_dev` |
| Banco | `portal_fornecedores` |

## Instalação (feita uma vez)

```powershell
winget install --id PostgreSQL.PostgreSQL.17 -e --override "--mode unattended --unattendedmodeui none --superpassword postgres_dev --serverport 5432 --servicename postgresql-17 --disable-components stackbuilder"

# cria usuário e banco
$env:PGPASSWORD = 'postgres_dev'
& 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -U postgres -h localhost -f backend/db/setup-local.sql

# cria as tabelas
cd backend
dotnet tool restore
dotnet ef database update --project src/PortalFornecedores.Api
```

## Consultar o banco

```powershell
$env:PGPASSWORD = 'portal_dev'
& 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -U portal -h localhost -d portal_fornecedores
```

## Alterou uma classe em `Domain/`?

```powershell
cd backend
dotnet ef migrations add NomeDaMudanca --project src/PortalFornecedores.Api --output-dir Data/Migrations
dotnet ef database update --project src/PortalFornecedores.Api
```
