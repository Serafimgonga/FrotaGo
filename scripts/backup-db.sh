#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKUP_DIR="$ROOT_DIR/backups"
DATE_SUFFIX="$(date +%Y-%m-%d-%H%M%S)"
FILE_NAME="frotago_${DATE_SUFFIX}.sql"

mkdir -p "$BACKUP_DIR"

cd "$ROOT_DIR"

if [ -f .env ]; then
  set -a
  source .env
  set +a
fi

POSTGRES_DB="${POSTGRES_DB:-frotago_db}"
POSTGRES_USER="${POSTGRES_USER:-frotago_user}"

docker compose exec -T postgres pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" > "$BACKUP_DIR/$FILE_NAME"

printf 'Backup created: %s\n' "$BACKUP_DIR/$FILE_NAME"
