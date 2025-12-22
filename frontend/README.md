# Frontend GestAuto (SPA)

Este pacote contém o frontend (SPA) do GestAuto.

## Rodar em modo dev

```bash
cd frontend
npm install
npm run dev
```

A aplicação ficará disponível em `http://localhost:5173`.

## Configuração por ambiente (runtime)

O frontend carrega configuração em runtime (sem rebuild) a partir de:

1) `window.__APP_CONFIG__` (se existir), senão
2) `GET /app-config.json` (arquivo estático servido pelo próprio frontend)

Arquivo padrão em desenvolvimento:

- `frontend/public/app-config.json`

Formato:

```json
{
  "keycloakBaseUrl": "http://keycloak.tasso.local",
  "keycloakRealm": "gestauto-dev",
  "keycloakClientId": "gestauto-frontend",
  "appBaseUrl": "http://gestauto.tasso.local"
}
```

## Login/logout (Keycloak)

O login usa Authorization Code + PKCE via `keycloak-js`.

Nota importante (PKCE/WebCrypto): navegadores só expõem a Web Crypto API em **secure contexts** (HTTPS) ou em **localhost**. Se você acessar o app em `http://gestauto.tasso.local`, é provável ver o erro “Web Crypto API is not available”.

Importante: por padrão, o client do Keycloak está configurado para redirect em `http://gestauto.tasso.local/*`.

Para validar o fluxo completo no browser:

- Acesse a aplicação usando o hostname `gestauto.tasso.local` (via Traefik) e garanta que ele resolve localmente (ex.: entrada no `/etc/hosts`).

Alternativa de validação local (recomendada sem HTTPS):

- Rode o frontend em `http://localhost:5173` (`npm run dev`) e valide o login/RBAC por esse endereço (o client do Keycloak foi configurado para aceitar redirect/origin de localhost).

## RBAC (menus + guards)

- Menus e guards são calculados a partir das roles presentes na claim `roles` do token.
- Acesso direto via URL a uma rota não permitida exibe “Acesso negado”.

Rotas base:

- `/` (Home)
- `/commercial` (Comercial)
- `/evaluations` (Avaliações)
- `/admin` (Admin)
- `/denied` (Acesso negado)

## Testes

```bash
cd frontend
npm test
```

## Validação manual (Keycloak)

Após provisionar o Keycloak (ver `scripts/keycloak/`), valide com os usuários de teste:

- `seller` / `123456` → deve ver menu **Comercial**
- `evaluator` / `123456` → deve ver menu **Avaliações**
- `admin` / `admin` → deve ver **Comercial + Avaliações + Admin**
- `viewer` / `123456` → deve ver **somente Avaliações**
