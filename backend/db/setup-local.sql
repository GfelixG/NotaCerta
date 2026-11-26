-- Cria o usuário e o banco usados pela API em desenvolvimento.
-- Rodar uma vez como superusuário:
--   psql -U postgres -h localhost -f backend/db/setup-local.sql
-- Credenciais apenas para ambiente local (mesmas do appsettings.json e docker-compose.yml).

CREATE ROLE portal WITH LOGIN PASSWORD 'portal_dev';
CREATE DATABASE portal_fornecedores OWNER portal ENCODING 'UTF8';
