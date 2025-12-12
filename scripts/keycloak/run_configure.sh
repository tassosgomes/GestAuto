#!/usr/bin/env bash
set -euo pipefail

# Runs configure_gestauto.sh with variables from a .env file.
# Usage:
#   ./run_configure.sh            # loads ./.env if present, else ./.env.example
#   ./run_configure.sh /path/to/.env

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="${1:-}"

if [[ -z "${ENV_FILE}" ]]; then
  if [[ -f "$SCRIPT_DIR/.env" ]]; then
    ENV_FILE="$SCRIPT_DIR/.env"
  else
    ENV_FILE="$SCRIPT_DIR/.env.example"
  fi
fi

if [[ ! -f "$ENV_FILE" ]]; then
  echo "ERROR: env file not found: $ENV_FILE" >&2
  exit 1
fi

set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

exec "$SCRIPT_DIR/configure_gestauto.sh"
