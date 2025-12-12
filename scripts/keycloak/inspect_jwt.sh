#!/usr/bin/env bash
set -euo pipefail

# Decodes a JWT (header/payload) and prints key fields.
# Requires: jq, base64
#
# Usage:
#   ./inspect_jwt.sh "<token>"
#   cat token.txt | ./inspect_jwt.sh

require() {
  command -v "$1" >/dev/null 2>&1 || { echo "ERROR: missing $1" >&2; exit 1; }
}

require jq
require base64

TOKEN="${1:-}"
if [[ -z "$TOKEN" ]]; then
  TOKEN="$(cat)"
fi

if [[ -z "$TOKEN" ]]; then
  echo "ERROR: missing token" >&2
  exit 1
fi

b64url_decode() {
  local s="$1"
  s="${s//-/+}"
  s="${s//_/\/}"
  local mod=$(( ${#s} % 4 ))
  if [[ $mod -eq 1 ]]; then
    return 1
  elif [[ $mod -eq 2 ]]; then
    s+="=="
  elif [[ $mod -eq 3 ]]; then
    s+="="
  fi
  printf '%s' "$s" | base64 -d 2>/dev/null
}

header_b64="${TOKEN%%.*}"
payload_b64="${TOKEN#*.}"
payload_b64="${payload_b64%%.*}"

header_json="$(b64url_decode "$header_b64")"
payload_json="$(b64url_decode "$payload_b64")"

echo "== header =="
echo "$header_json" | jq

echo "\n== payload (selected) =="
echo "$payload_json" | jq '{iss, sub, aud, azp, preferred_username, scope, roles, realm_access, resource_access, exp, iat}'

echo "\n== payload (full) =="
echo "$payload_json" | jq
