---
status: completed
parallelizable: false
blocked_by: []
---

<task_context>
<domain>infra/stock-service</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>high</complexity>
<dependencies>http_server|database</dependencies>
<unblocks>"2.0, 3.0, 4.0, 5.0"</unblocks>
</task_context>

# Tarefa 1.0: Bootstrap do serviço Stock (.NET)

## Visão Geral
Criar a estrutura do novo serviço **GestAuto.Stock** (API + Application + Domain + Infra + Tests), alinhada ao padrão do repositório (Clean Architecture), incluindo autenticação Keycloak JWT, policies base, middleware de ProblemDetails e health check.

## Requisitos
- Criar solução/projetos com a mesma organização de camadas dos serviços existentes.
- Configurar `api/v1` e Swagger/OpenAPI.
- Configurar autenticação JWT (Keycloak) com claim `roles` e `RoleClaimType = "roles"`.
- Incluir middleware de exceções retornando `application/problem+json`.
- Incluir endpoint `/health`.

## Subtarefas
- [x] 1.1 Criar estrutura de diretórios e projetos (API/Application/Domain/Infra/Tests)
- [x] 1.2 Configurar DI e pipeline HTTP (controllers, swagger, health)
- [x] 1.3 Configurar JWT Bearer (Authority/Audience, normalização de roles se necessário)
- [x] 1.4 Criar policies mínimas usando roles existentes no repo (`SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`)
- [x] 1.5 Adicionar middleware de exceções (mapear DomainException/NotFound/Forbidden)

## Sequenciamento
- Bloqueado por: nenhum
- Desbloqueia: 2.0, 3.0, 4.0, 5.0
- Paralelizável: Não (é o bootstrap)

## Detalhes de Implementação
- Basear-se no padrão do serviço Comercial (`Program.cs`, policies, middleware de exceções, swagger).
- Seguir `rules/dotnet-architecture.md`, `rules/restful.md`, `rules/dotnet-observability.md`.

## Critérios de Sucesso
- Serviço compila e sobe localmente expondo `/health` e Swagger.
- Requests inválidas retornam `application/problem+json` com códigos 400/401/403/404/500.
- Policies reconhecem roles vindas do Keycloak (claim `roles`).

## Checklist de conclusão

- [x] 1.0 Bootstrap do serviço Stock (.NET) ✅ CONCLUÍDA
  - [x] 1.1 Implementação completada
  - [x] 1.2 Definição da tarefa, PRD e tech spec validados
  - [x] 1.3 Análise de regras e conformidade verificadas
  - [x] 1.4 Revisão de código completada
  - [x] 1.5 Pronto para deploy
