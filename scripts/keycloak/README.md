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
