# Keycloak bootstrap (GestAuto)

This folder contains idempotent scripts to provision Keycloak so tokens work with:

- Commercial API (.NET): expects claim `roles` and validates `aud`
- Vehicle Evaluation (Spring): expects claim `roles` (mapped to `ROLE_*` authorities)
- GestAuto Frontend (SPA): login via OIDC (Auth Code + PKCE) and reads claim `roles`

## Quick start

1) Configure environment

- Copy `.env.example` to `.env` and adjust if needed.
- If you're running Keycloak via the repo `docker-compose.yml` (behind Traefik), set:
  - `KEYCLOAK_BASE_URL=http://keycloak.tasso.local`

2) Provision Keycloak

- Run: `./run_configure.sh`

3) Mint a dev token (password grant)

- If the token client is confidential, export the secret printed by `configure_gestauto.sh`:
  - `export GESTAUTO_TOKEN_CLIENT_SECRET=...`

- Get token:
  - `./get_token_password.sh seller 123456`

4) Inspect token claims

- `./inspect_jwt.sh "$(./get_token_password.sh seller 123456)"`

You should see:
- `roles`: an array containing the user realm roles (e.g., `SALES_PERSON`)
- `aud`: includes `gestauto-commercial-api` and `vehicle-evaluation-api`

## Frontend (SPA)

The bootstrap also provisions a public SPA client:

- Client ID: `gestauto-frontend`
- Redirect URIs: `http://gestauto.tasso.local/*`
- Web Origins: `http://gestauto.tasso.local`

This client is intended for browser-based login using Authorization Code + PKCE.

### Como validar o login (Auth Code + PKCE)

Pré-requisitos:

- Keycloak acessível em `KEYCLOAK_BASE_URL` (ex.: `http://keycloak.tasso.local`).
- O host `gestauto.tasso.local` deve resolver (mesmo que não exista um frontend rodando ainda).

1) Provisione o realm/client (se ainda não fez):

- `./run_configure.sh`

2) Escolha o realm alvo e gere PKCE verifier/challenge:

```bash
export KEYCLOAK_BASE_URL=${KEYCLOAK_BASE_URL:-http://keycloak.tasso.local}
export GESTAUTO_REALM=${GESTAUTO_REALM:-gestauto-dev}

CODE_VERIFIER=$(openssl rand -base64 96 | tr -d '=+/\n' | cut -c1-128)
CODE_CHALLENGE=$(printf '%s' "$CODE_VERIFIER" | openssl dgst -sha256 -binary | openssl base64 -A | tr '+/' '-_' | tr -d '=')

echo "CODE_VERIFIER=$CODE_VERIFIER"
echo "CODE_CHALLENGE=$CODE_CHALLENGE"
```

3) Abra no browser a URL abaixo (vai redirecionar para `http://gestauto.tasso.local/` com `?code=...`):

```bash
AUTH_URL="$KEYCLOAK_BASE_URL/realms/$GESTAUTO_REALM/protocol/openid-connect/auth?client_id=gestauto-frontend&redirect_uri=http%3A%2F%2Fgestauto.tasso.local%2F&response_type=code&scope=openid&code_challenge=$CODE_CHALLENGE&code_challenge_method=S256"
echo "$AUTH_URL"
```

Após logar, copie o valor de `code` da URL de redirect.

4) Troque o `code` por tokens via `token` endpoint (PKCE):

```bash
CODE="COLE_O_CODE_AQUI"

TOKEN_JSON=$(curl -sS -X POST "$KEYCLOAK_BASE_URL/realms/$GESTAUTO_REALM/protocol/openid-connect/token" \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=authorization_code' \
  --data-urlencode 'client_id=gestauto-frontend' \
  --data-urlencode 'redirect_uri=http://gestauto.tasso.local/' \
  --data-urlencode "code=$CODE" \
  --data-urlencode "code_verifier=$CODE_VERIFIER")

echo "$TOKEN_JSON" | jq .
```

5) Verifique a claim `roles` no `access_token`:

```bash
ACCESS_TOKEN=$(echo "$TOKEN_JSON" | jq -r '.access_token')
./inspect_jwt.sh "$ACCESS_TOKEN"
```

Você deve ver:

- `roles`: array (multivalued) com as roles do usuário (ex.: `SALES_PERSON`).

Observação: se você ainda não tem um frontend rodando em `gestauto.tasso.local`, o redirect pode resultar em erro/404, mas a URL final ainda deve conter o `code` (o que já valida `redirectUris` e o fluxo do client).

## Environments / realms

The target realm is controlled by `GESTAUTO_REALM`.

Recommended convention:

- dev: `gestauto-dev`
- hml: `gestauto-hml`
- prod: `gestauto`

## Notes

- API clients (`gestauto-commercial-api`, `vehicle-evaluation-api`) are created as `bearerOnly` resource servers.
- A dedicated client (`GESTAUTO_TOKEN_CLIENT_ID`, default `gestauto-dev-cli`) is used to mint dev/test tokens.

### Test users: required profile fields

Keycloak User Profile can require `email`, `firstName` and `lastName` for users. If those fields are missing, password grant can fail with `invalid_grant` and `Account is not fully set up`.

The bootstrap ensures these fields are set for the test users. You can customize the email domain with:

- `GESTAUTO_TEST_USERS_EMAIL_DOMAIN` (default: `tasso.local`)

### Password grant and OTP (dev/test)

In some setups, the default `Direct Grant - Conditional OTP` subflow can break password grant flows used by scripts.

By default, the bootstrap disables that subflow to keep `./get_token_password.sh` functional.

- To keep Keycloak defaults, set `GESTAUTO_DIRECT_GRANT_DISABLE_OTP=false`.
