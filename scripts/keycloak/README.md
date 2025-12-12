# Keycloak bootstrap (GestAuto)

This folder contains idempotent scripts to provision Keycloak so tokens work with:

- Commercial API (.NET): expects claim `roles` and validates `aud`
- Vehicle Evaluation (Spring): expects claim `roles` (mapped to `ROLE_*` authorities)

## Quick start

1) Configure environment

- Copy `.env.example` to `.env` and adjust if needed.

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

## Notes

- API clients (`gestauto-commercial-api`, `vehicle-evaluation-api`) are created as `bearerOnly` resource servers.
- A dedicated client (`GESTAUTO_TOKEN_CLIENT_ID`, default `gestauto-dev-cli`) is used to mint dev/test tokens.
