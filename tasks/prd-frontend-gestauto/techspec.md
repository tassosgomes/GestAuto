# Tech Spec - Frontend GestAuto (Login e Menus por Perfil)

## Resumo Executivo

Esta Tech Spec define a implementação de um frontend web (SPA) para o GestAuto com autenticação via Keycloak e autorização de navegação (menus + rotas) baseada nas roles presentes na claim `roles` do token JWT. A solução proposta usa **OAuth 2.0 / OIDC com Authorization Code + PKCE** (padrão para SPAs) e aplica **RBAC no cliente** com proteção de rotas (não apenas ocultação de menus).

Como o repositório atualmente não possui frontend (não há `package.json`), esta entrega introduz um novo pacote de frontend, além de ajustes de infraestrutura (Traefik/Docker) e provisioning do Keycloak (novo client do tipo SPA). Não há consumo de APIs nesta fase (placeholders apenas), mas a arquitetura já deixa o caminho preparado para futura chamada aos serviços `commercial` e `vehicle-evaluation` usando o access token.

## Arquitetura do Sistema

### Visão Geral dos Componentes

- **Frontend Web (SPA)**
  - Responsável por login/logout, sessão no browser, renderização de menus e guarda de rotas.
  - Não executa operações de negócio nem chama APIs nesta fase.

- **Keycloak (IdP) atrás do Traefik**
  - Base URL estável: `http://keycloak.tasso.local`.
  - Realms por ambiente: `gestauto-dev` (dev), `gestauto-hml` (hml), `gestauto` (prod), mantendo mesmas roles/mappers.

- **Traefik (Reverse Proxy)**
  - Já expõe `keycloak.tasso.local`, `commercial.tasso.local`, `vehicle-evaluation.tasso.local`.
  - Exporá o frontend via `gestauto.tasso.local`.

**Fluxo de dados (alto nível):**
1. Browser acessa o host do frontend.
2. Usuário inicia login → redirecionamento para Keycloak.
3. Keycloak autentica e devolve o usuário ao frontend com code.
4. Frontend troca code por tokens via PKCE.
5. Frontend extrai `roles` do JWT e calcula menus/rotas permitidos.

## Design de Implementação

### Interfaces Principais

**Auth service (TypeScript):**

```ts
export type Role =
  | "ADMIN"
  | "MANAGER"
  | "VIEWER"
  | "SALES_PERSON"
  | "SALES_MANAGER"
  | "VEHICLE_EVALUATOR"
  | "EVALUATION_MANAGER";

export interface UserSession {
  isAuthenticated: boolean;
  username?: string;
  roles: Role[];
  accessToken?: string;
}

export interface AuthService {
  init(): Promise<void>;
  login(): Promise<void>;
  logout(): Promise<void>;
  getSession(): UserSession;
}
```

**RBAC helpers (puramente funcionais):**

```ts
export type AppMenu = "COMMERCIAL" | "EVALUATIONS" | "ADMIN";

export function getVisibleMenus(roles: Role[]): AppMenu[] {
  const hasAny = (...rs: Role[]) => rs.some(r => roles.includes(r));
  const menus: AppMenu[] = [];
  if (hasAny("SALES_PERSON", "SALES_MANAGER", "MANAGER", "ADMIN")) menus.push("COMMERCIAL");
  if (hasAny("VEHICLE_EVALUATOR", "EVALUATION_MANAGER", "MANAGER", "VIEWER", "ADMIN")) menus.push("EVALUATIONS");
  if (hasAny("ADMIN")) menus.push("ADMIN");
  return menus;
}
```

### Modelos de Dados

- **Config de runtime do frontend** (para evitar rebuild por ambiente):

```ts
export interface FrontendConfig {
  keycloakBaseUrl: string; // ex.: http://keycloak.tasso.local
  keycloakRealm: string;   // ex.: gestauto-dev | gestauto-hml | gestauto
  keycloakClientId: string; // ex.: gestauto-frontend
  appBaseUrl: string; // ex.: http://gestauto.tasso.local
}
```

- **Claims relevantes do token**
  - `roles`: array de strings com roles do realm (provisionado via protocol mapper já padronizado no repo, ver scripts de Keycloak).

### Endpoints de API

- **Não aplicável nesta fase.**
  - O frontend não chama `commercial` nem `vehicle-evaluation` no MVP.
  - A integração com APIs será tratada em uma fase futura (com mesma base de tokens).

## Pontos de Integração

### Keycloak (OIDC)

**Cliente novo (SPA):**
- `clientId`: `gestauto-frontend`
- Tipo: public client (sem secret)
- Flow: Authorization Code + PKCE
- Redirect URIs:
  - `http://gestauto.tasso.local/*`
- Web Origins:
  - `http://gestauto.tasso.local`

**Scopes/mappers obrigatórios no client do frontend:**
- Scope que inclui o mapper `roles` → claim `roles` multivalued.
- Scope de audiences (se pretendemos futuramente chamar APIs): incluir `gestauto-commercial-api` e `vehicle-evaluation-api` em `aud`.

**Nota de consistência com o repo:**
- Já existe bootstrap idempotente em `scripts/keycloak/` que cria realm roles, usuários, e mappers para APIs. A Tech Spec propõe estender esse bootstrap para também criar o client `gestauto-frontend` com as configurações acima.

### Traefik/Docker

- Adicionar um novo serviço containerizado do frontend e um router no Traefik.
- Adicionar um novo serviço containerizado do frontend e um router no Traefik.
- Hostname do frontend: `gestauto.tasso.local`.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
| --- | --- | --- | --- |
| Novo pacote de frontend | Adição | Novo diretório (ex.: `frontend/`) com build Node. Baixo risco para serviços existentes. | Criar estrutura, pipeline e docs de execução |
| Keycloak bootstrap | Mudança | Necessário criar client SPA e configurar redirect/web origins. Médio risco por ser ponto central de auth. | Atualizar `scripts/keycloak/configure_gestauto.sh` de forma idempotente |
| Traefik/Docker Compose | Mudança | Novo serviço/host para servir a SPA. Baixo risco. | Atualizar `docker-compose.yml` e `traefik/dynamic.yml` |
| Documentação | Mudança | Instruções de subir o frontend e configurar Keycloak. Baixo risco. | Atualizar README do frontend |

## Abordagem de Testes

### Testes Unitários

Foco em lógica determinística:
- `getVisibleMenus(roles)` com combinações de roles:
  - `SALES_PERSON` → vê `COMMERCIAL`
  - `VIEWER` → vê `EVALUATIONS`
  - `MANAGER` → vê `COMMERCIAL` + `EVALUATIONS`
  - `ADMIN` → vê todos
- `canAccessRoute(roles, route)` (se implementado) para garantir que acesso por URL é bloqueado.

Mocks:
- Mock do provider de auth (Keycloak) para simular sessão e roles.

### Testes de Integração

Smoke test local (manual/automatizável depois):
- Subir `docker-compose.yml` (Keycloak + Traefik).
- Rodar o frontend apontando para `http://keycloak.tasso.local`.
- Login com usuários de teste do Keycloak (ex.: `seller`, `evaluator`, `admin`) e validar menus.

Observação: testes E2E automatizados (Playwright/Cypress) são recomendáveis, mas podem ser fase 2 caso o repo ainda não tenha stack Node.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **Scaffold do frontend**
   - Criar diretório (recomendação: `frontend/`) e setup de build.

2. **Configuração de runtime**
   - Implementar leitura de config (base URL, realm, clientId) sem hardcode.

3. **Auth via Keycloak (PKCE)**
   - Implementar `AuthProvider`/`AuthService`.
   - Login/logout e estado de sessão.

4. **RBAC (menus + rotas)**
   - Implementar `getVisibleMenus` e guards.
   - Implementar páginas placeholder.

5. **Containerização + Traefik**
   - Servir build estático via Nginx.
   - Expor host via Traefik.

6. **Provisionamento Keycloak**
   - Estender scripts idempotentes para criar client do frontend.

### Dependências Técnicas

- Keycloak acessível em `http://keycloak.tasso.local` (já configurado no docker-compose local).
- Hostname do frontend atrás do Traefik: `gestauto.tasso.local`.
- Realms por ambiente: `gestauto-dev`, `gestauto-hml`, `gestauto`.

## Monitoramento e Observabilidade

- **Logs do frontend**
  - Em produção: logs mínimos (erros e eventos de auth) sem tokens nem dados sensíveis.
  - Em desenvolvimento: logs de debug opcionais.

- **Métricas**
  - Nesta fase, não será exposto endpoint de métricas. Se necessário, podemos instrumentar no futuro via RUM (sem capturar PII) ou integração com backend.

## Considerações Técnicas

### Decisões Principais

- **Flow de OIDC**: Authorization Code + PKCE (evita implicit flow).
- **Biblioteca de integração**:
  - Preferência: `keycloak-js` (alinhado ao IdP e reduz customização).
  - Alternativa: `oidc-client-ts` (mais genérica, mas com maior custo de integração/ajuste).
- **Armazenamento de token**:
  - Preferência: memória (evita persistir tokens no `localStorage`, reduzindo risco LGPD/segurança).
  - Trade-off: reload da página pode exigir revalidação/sessão (mitigável via `check-sso`).
- **RBAC no cliente**:
  - Implementar proteção de rotas + ocultação de menus.
  - O backend continua sendo fonte de verdade quando as APIs forem consumidas.

### Riscos Conhecidos

- **Configuração de redirect URIs/Web Origins** no Keycloak pode bloquear login se divergente do host real.
  - Mitigação: padronizar hostname do frontend no Traefik e documentar.

- **Divergência de realms por ambiente** (roles/mappers inconsistentes entre dev/hml/prod).
  - Mitigação: provisionamento automatizado (scripts idempotentes), checklist por ambiente.

- **Ausência de regras/padrões para Node/React no repo**.
  - Mitigação: manter setup mínimo, documentar decisões, e propor inclusão futura de `rules/frontend-*.md`.

### Requisitos Especiais

- **Segurança/LGPD**
  - Não logar tokens.
  - Não persistir tokens além do necessário.
  - Não coletar PII no frontend (além do estritamente necessário para exibir o usuário).

### Conformidade com Padrões

- Roles seguem `SCREAMING_SNAKE_CASE` conforme [rules/ROLES_NAMING_CONVENTION.md](rules/ROLES_NAMING_CONVENTION.md).
- Configuração de Keycloak (claim `roles`, audiences) segue a abordagem já existente em [scripts/keycloak/README.md](scripts/keycloak/README.md).
- Observabilidade/segurança: segue princípios gerais dos serviços (não logar dados sensíveis), alinhado às preocupações de `dotnet-observability.md` e `java-observability.md` (aplicadas por analogia ao frontend).
