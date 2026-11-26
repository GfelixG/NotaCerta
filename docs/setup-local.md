# Ambiente local (Windows, sem Docker)

**Nenhuma senha fica no repositório.** Você escolhe as suas (use só letras e números) e guarda em:

- `.env` na raiz do projeto, ignorado pelo git (copie de `.env.example`);
- `user-secrets` do .NET, que fica fora da pasta do projeto e é de onde a API lê a senha.

| O quê | Valor |
|---|---|
| PostgreSQL | 17, serviço `postgresql-17`, porta 5432 |
| Superusuário | `postgres` / senha `POSTGRES_ADMIN_PASSWORD` |
| Usuário da API | `portal` / senha `PORTAL_DB_PASSWORD` |
| Banco | `portal_fornecedores` |

## Instalação (feita uma vez)

```powershell
winget install --id PostgreSQL.PostgreSQL.17 -e --override "--mode unattended --unattendedmodeui none --superpassword SUA_SENHA_ADMIN --serverport 5432 --servicename postgresql-17 --disable-components stackbuilder"

# cria usuário e banco
& 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -U postgres -h localhost -v senha_portal="SUA_SENHA_PORTAL" -f backend/db/setup-local.sql

# informa a senha à API (fica fora do repositório)
cd backend
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=portal_fornecedores;Username=portal;Password=SUA_SENHA_PORTAL" --project src/PortalFornecedores.Api

# cria as tabelas
dotnet tool restore
dotnet ef database update --project src/PortalFornecedores.Api
```

## Consultar o banco

```powershell
# o psql vai pedir a senha do usuário portal
& 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -U portal -h localhost -d portal_fornecedores
```

## Alterou uma classe em `Domain/`?

```powershell
cd backend
dotnet ef migrations add NomeDaMudanca --project src/PortalFornecedores.Api --output-dir Data/Migrations
dotnet ef database update --project src/PortalFornecedores.Api
```
