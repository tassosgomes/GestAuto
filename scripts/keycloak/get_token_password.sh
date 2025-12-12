#!/usr/bin/env bash
set -euo pipefail

# Gets an access token via password grant using the dedicated token-minting client.
# Requires: curl, jq
#
# Env vars (recommended):
#   KEYCLOAK_BASE_URL=http://localhost:8080
#   GESTAUTO_REALM=gestauto
#   GESTAUTO_TOKEN_CLIENT_ID=gestauto-dev-cli
#   GESTAUTO_TOKEN_CLIENT_SECRET=...   # only if confidential
#
# Usage:
#   ./get_token_password.sh seller 123456
#   ./get_token_password.sh --username seller --password 123456

require() {
  command -v "$1" >/dev/null 2>&1 || { echo "ERROR: missing $1" >&2; exit 1; }
}

require curl
require jq

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Convenience: load local .env (or .env.example) so users can run the script
# without having to manually export variables.
if [[ -z "${GESTAUTO_DISABLE_DOTENV:-}" ]]; then
  if [[ -f "$SCRIPT_DIR/.env" ]]; then
    set -a
    # shellcheck disable=SC1090
    source "$SCRIPT_DIR/.env"
    set +a
  elif [[ -f "$SCRIPT_DIR/.env.example" ]]; then
    set -a
    # shellcheck disable=SC1090
    source "$SCRIPT_DIR/.env.example"
    set +a
  fi
fi

KEYCLOAK_BASE_URL="${KEYCLOAK_BASE_URL:-http://localhost:8080}"
GESTAUTO_REALM="${GESTAUTO_REALM:-gestauto}"
CLIENT_ID="${GESTAUTO_TOKEN_CLIENT_ID:-gestauto-dev-cli}"
CLIENT_SECRET="${GESTAUTO_TOKEN_CLIENT_SECRET:-}"

USERNAME=""
PASSWORD=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --username) USERNAME="$2"; shift 2;;
    --password) PASSWORD="$2"; shift 2;;
    *)
      if [[ -z "$USERNAME" ]]; then USERNAME="$1"; shift; continue; fi
      if [[ -z "$PASSWORD" ]]; then PASSWORD="$1"; shift; continue; fi
      echo "ERROR: unexpected arg: $1" >&2
      exit 1
      ;;
  esac
done

if [[ -z "$USERNAME" || -z "$PASSWORD" ]]; then
  echo "ERROR: provide username and password" >&2
  exit 1
fi

TOKEN_ENDPOINT="$KEYCLOAK_BASE_URL/realms/$GESTAUTO_REALM/protocol/openid-connect/token"

data=(
  --data-urlencode 'grant_type=password'
  --data-urlencode "client_id=$CLIENT_ID"
  --data-urlencode "username=$USERNAME"
  --data-urlencode "password=$PASSWORD"
)

if [[ -n "$CLIENT_SECRET" ]]; then
  data+=(--data-urlencode "client_secret=$CLIENT_SECRET")
fi

curl -sS -X POST "$TOKEN_ENDPOINT" \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  "${data[@]}" \
  | jq -r '.access_token'
