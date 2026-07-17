-- Initial PostgreSQL setup for FrotaGo
CREATE EXTENSION IF NOT EXISTS pgcrypto;

COMMENT ON DATABASE current_database() IS 'FrotaGo MVP database';
