# Revisão da Tarefa 1.0 — Bootstrap do serviço Stock (.NET)

Data: 2026-01-12
Branch: `feat/modulo-estoque-1-bootstrap-stock`

## 1) Validação da definição da tarefa

### 1.1 Checklist de requisitos (tarefa 1.0)

- Estrutura de diretórios e projetos (API/Application/Domain/Infra/Tests): **OK**
  - Implementado em `services/stock/` com padrão de camadas e numeração compatíveis com o serviço Comercial.
- `api/v1` e Swagger/OpenAPI: **OK**
  - Swagger habilitado em Development.
  - Prefixo `/api/v1` aplicado para controllers via `ApiV1RoutePrefixConvention`.
- Autenticação JWT (Keycloak) com claim `roles` e `RoleClaimType = "roles"`: **OK**
  - `AddJwtBearer` com `RoleClaimType = "roles"`.
  - Normalização de roles no `OnTokenValidated` (cobre roles em array JSON e casos com mapeamento para `ClaimTypes.Role`).
- Middleware de exceções retornando `application/problem+json`: **OK**
  - Middleware `ExceptionHandlerMiddleware` com mapeamento de `DomainException`/`NotFoundException`/`ForbiddenException`/`UnauthorizedException`.
- Endpoint `/health`: **OK**
  - `MapHealthChecks("/health")`.

### 1.2 Alinhamento com PRD

- O PRD indica um módulo backend-first e um serviço dedicado como “fonte única da verdade”.
- Este bootstrap cria a base do serviço (infra HTTP + auth + ProblemDetails + health) para permitir evoluções 2.0+ sem retrabalho: **alinhado**.

### 1.3 Alinhamento com Tech Spec

- Clean Architecture: **OK** (separação em projetos por camada).
- REST em `/api/v1`: **OK** (prefixo para controllers).
- Keycloak JWT + roles: **OK** (RoleClaimType e normalização).
- Outbox/RabbitMQ/AsyncAPI: **fora do escopo desta tarefa** (previsto para tarefas 4.0+).

## 2) Análise de regras e conformidade

Regras avaliadas:
- `rules/dotnet-architecture.md`: estrutura em camadas e padrões base — **OK**.
- `rules/restful.md`: versionamento por path e respostas de erro via RFC 9457 — **OK**.
- `rules/dotnet-observability.md`: health checks — **OK** (endpoint `/health`).
- `rules/dotnet-coding-standards.md`: nomenclatura e organização — **OK** (identificadores em inglês; sem mudanças de estilo fora do padrão do repo).
- `rules/dotnet-testing.md`: recomenda AwesomeAssertions — **OK** (pacote AwesomeAssertions usado nos testes do serviço Stock; API é drop-in compatível com FluentAssertions).

## 3) Revisão de código (principais pontos)

- `ApiV1RoutePrefixConvention`:
  - **Correção aplicada**: evita duplicar prefixo se a rota já começar com `api/` ou for absoluta (`~/...` ou `/...`).
- Auth Keycloak:
  - `RoleClaimType = "roles"`.
  - Normalização robusta de roles.
  - `OnChallenge` e `OnForbidden` retornam `application/problem+json`.
- ProblemDetails:
  - Middleware centralizado cobre 400/401/403/404/500 para exceções.
  - `UseStatusCodePages` cobre 404 para rotas inexistentes.

## 4) Problemas encontrados e resolvidos

### Resolvidos
- Risco de duplicação de `/api/v1` ao criar controllers com rotas já prefixadas: **corrigido** em `ApiV1RoutePrefixConvention`.

### Pendências / Recomendações
- Testes não cobrem 401/403 via endpoint protegido (a API ainda não possui controllers/actions com `[Authorize]`): **recomendação** criar um endpoint real nas próximas tarefas (ex.: controllers de veículos) e adicionar testes de integração para 401/403 e content-type.
- Warning em testes: `Failed to determine the https port for redirect.` devido a `UseHttpsRedirection` no ambiente de teste. **Baixa severidade** (não quebra build); pode ser ajustado no futuro com configuração de porta HTTPS no perfil ou desabilitando redirection em testes.

## 5) Validação (build/test)

Executado:
- `dotnet build services/stock/GestAuto.Stock.sln`: **OK**
- `dotnet test services/stock/GestAuto.Stock.sln`: **OK** (2 testes, 0 falhas)

## 6) Conclusão

A tarefa 1.0 está **concluída** e o serviço está **pronto para seguir** para as tarefas 2.0+.
