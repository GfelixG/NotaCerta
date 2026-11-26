-- Cria o usuário e o banco usados pela API em desenvolvimento.
-- Rodar uma vez como superusuário:
--   psql -U postgres -h localhost -v senha_portal="SUA_SENHA" -f backend/db/setup-local.sql
-- A senha é informada na hora de rodar: nunca a escreva neste arquivo.

CREATE ROLE portal WITH LOGIN PASSWORD :'senha_portal';
CREATE DATABASE portal_fornecedores OWNER portal ENCODING 'UTF8';
