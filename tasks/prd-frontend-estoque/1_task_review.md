# Revisão da Tarefa 1.0 — Validar contrato da Stock API (Swagger) e gaps

Data: 2026-01-16
PRD: `tasks/prd-frontend-estoque/prd.md`
Tech Spec: `tasks/prd-frontend-estoque/techspec.md`
Tarefa: `tasks/prd-frontend-estoque/1_task.md`

## 1) Validação da definição da tarefa (tarefa → PRD → tech spec)

### Requisitos da tarefa
- Base URL e prefixo `/api/v1`: validado no Swagger do ambiente (maioria dos endpoints está sob `/api/v1`).
- Paginação `_page/_size`: validado em `GET /api/v1/vehicles` (defaults 1 e 10).
- Erros `ProblemDetails` (RFC 9457): validado via schema `ProblemDetails` no Swagger (campos `title`, `detail`, `status` presentes).
- Endpoint `POST /api/v1/test-drives/{testDriveId}/complete` e campo opcional `notes`: validado como GAP (endpoint exposto no Swagger sem `/api/v1` e request sem `notes`).

### Alinhamento com PRD
- PRD exige integração com endpoints de veículos, histórico, reservas e fluxo de test-drive; a lista de endpoints confirmados cobre o MVP descrito.
- PRD cita campos como preço/localização; contrato atual não expõe — documentado como GAP + decisão de ocultar.

### Alinhamento com Tech Spec
- Tech Spec prevê `_page/_size` e compat `page/pageSize`: contrato atual aceita ambos — documentado.
- Tech Spec reforça `ProblemDetails` e dependência de `notes` na finalização: contrato atual não tem `notes` — documentado como GAP + fallback.

## 2) Análise de regras aplicáveis (rules/*.md)

Regras relevantes para esta tarefa (documentação/contrato):
- `rules/restful.md`
  - Versionamento obrigatório via path com `v{major}`.
  - Paginação obrigatória via `_page/_size`.
  - Erros no padrão RFC 9457 (ProblemDetails).
- `rules/ROLES_NAMING_CONVENTION.md`
  - Não impacta diretamente esta tarefa (não houve alteração de roles), mas é relevante pois os endpoints são protegidos por RBAC.
- `rules/git-commit.md`
  - Mensagens em português no padrão `tipo(escopo): descrição`.

Possíveis violações identificadas (no contrato/Swagger, não em código nesta tarefa):
- Inconsistência de versionamento: endpoint de complete test-drive aparece sem `/api/v1`.
- Swagger incompleto: `GET /api/v1/vehicles` não descreve o schema de resposta 200.

## 3) Resumo da revisão

Escopo revisado:
- Conteúdo documentado em `tasks/prd-frontend-estoque/1_task.md` (endpoints, modelos, enums, paginação, ProblemDetails, gaps e fallback).
- Atualização do resumo em `tasks/prd-frontend-estoque/tasks.md` (marcação da tarefa 1.0 como concluída).

Observações relevantes:
- A pasta `tasks/` está ignorada pelo `.gitignore` (entrada `tasks`), então este fluxo de documentação não gera diff versionável no Git por padrão.

## 4) Problemas encontrados e ações

### Problemas (feedback)
1. Endpoint de complete test-drive fora do padrão `/api/v1`.
2. Ausência de `notes` em `CompleteTestDriveRequest`.
3. `GET /api/v1/vehicles` sem schema de resposta no Swagger.
4. Campos `price`/`location` não presentes em `VehicleResponse`.

### Resoluções nesta tarefa
- Todos os pontos acima foram registrados na tarefa com decisão explícita de fallback de UI.
- Checklist de conclusão adicionado conforme o fluxo.

### Recomendações (próximos passos)
- Backend/Swagger:
  - Alinhar o endpoint de complete test-drive para `/api/v1/test-drives/{id}/complete` (ou adicionar rota adicional + manter compat).
  - Adicionar `ProducesResponseType(typeof(PagedResponse<VehicleListItem>), 200)` (ou equivalente) para gerar schema do `GET /api/v1/vehicles` no Swagger.
  - Considerar adicionar `notes?: string` ao `CompleteTestDriveRequest` se a UX exigir observações.

## 5) Validação e prontidão para deploy

- Compilação/testes: não aplicável (mudanças apenas em documentação de tarefa).
- Pronto para deploy: sim, pois não há mudança de runtime; há apenas decisões documentadas e prontas para orientar as próximas tarefas.
