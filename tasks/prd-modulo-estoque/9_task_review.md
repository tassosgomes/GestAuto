````markdown
```markdown
# Revisão da Tarefa 9.0 — Histórico (timeline) e auditoria

## 1) Validação da Definição da Tarefa (tarefa → PRD → techspec)

### Dependências
- A tarefa 9.0 estava marcada como bloqueada por 6.0, 7.0 e 8.0.
- As tarefas 6.0 (reservas), 7.0 (check-in/check-out) e 8.0 (test-drive) constam como `completed` e possuem relatórios de revisão, então o bloqueio não se aplica mais.

### Requisitos da tarefa (tasks/prd-modulo-estoque/9_task.md)

- `GET /api/v1/vehicles/{vehicleId}/history` (RF9.2)
  - Implementado no controller de veículos com rota `GET /vehicles/{id}/history` (prefixo `/api/v1` aplicado por convenção global do serviço).
  - Arquivo principal:
    - services/stock/1-Services/GestAuto.Stock.API/Controllers/VehiclesController.cs

- Deve incluir: quem fez, quando, o que mudou (RF9.1)
  - Modelo de resposta implementado com:
    - `type` (tipo do evento)
    - `occurredAtUtc` (timestamp)
    - `userId` (responsável)
    - `details` (dados adicionais)
  - DTOs:
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Dto/VehicleHistoryResponse.cs
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Dto/VehicleHistoryItemResponse.cs

- Operações críticas sem usuário autenticado devem ser bloqueadas (RF9.3)
  - O endpoint de histórico exige autenticação via `[Authorize]`.
  - Para as entradas de auditoria de mudança de status, o domínio exige `ResponsibleUserId != Guid.Empty` (garante que o histórico de status sempre possua responsável).
  - Arquivos:
    - services/stock/1-Services/GestAuto.Stock.API/Controllers/VehiclesController.cs
    - services/stock/3-Domain/GestAuto.Stock.Domain/Entities/AuditEntry.cs

### Subtarefas
- 9.1 Definir modelo de resposta da timeline
  - Implementado via `VehicleHistoryResponse` + `VehicleHistoryItemResponse`.
- 9.2 Implementar query `GetVehicleHistoryQuery` + handler (ordenação)
  - Handler agrega múltiplas fontes e ordena cronologicamente por `OccurredAtUtc`.
  - Arquivos:
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Queries/GetVehicleHistoryQuery.cs
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Queries/GetVehicleHistoryQueryHandler.cs
- 9.3 Implementar endpoint no controller
  - Implementado em `VehiclesController`.
- 9.4 Criar tabela `audit_entries` (opcional) para “status-changed”
  - Criada e integrada no fluxo de commit (persistida no mesmo UnitOfWork da outbox).
  - Arquivos:
    - services/stock/3-Domain/GestAuto.Stock.Domain/Entities/AuditEntry.cs
    - services/stock/3-Domain/GestAuto.Stock.Domain/Interfaces/IAuditEntryRepository.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/Repositories/AuditEntryRepository.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/EntityConfigurations/AuditEntryConfiguration.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/Migrations/*AddAuditEntries*
- 9.5 Testes
  - Adicionados testes unitários para validação de ordenação e presença de responsável.
  - Arquivo:
    - services/stock/5-Tests/GestAuto.Stock.UnitTest/Application/Vehicles/VehicleHistoryQueryHandlerTests.cs

### Alinhamento com PRD (tasks/prd-modulo-estoque/prd.md)
- F9 (Auditoria e Responsabilidade): a resposta inclui responsável/tempo/detalhes por evento.
- F3 (status único + histórico): mudanças de status passam a ser persistidas como trilha auditável para compor timeline.

### Alinhamento com Tech Spec (tasks/prd-modulo-estoque/techspec.md)
- Endpoint `GET /api/v1/vehicles/{vehicleId}/history` implementado conforme seção “Endpoints de API”.
- CQRS nativo: `IQueryHandler<,>` sem MediatR.
- Persistência via EF Core (schema `stock`) com migration para `audit_entries`.

## 2) Análise de Regras e Conformidade

Regras consideradas relevantes para os arquivos alterados:
- rules/dotnet-architecture.md
  - Mantido o fluxo Clean Architecture (API → Application → Domain; Infra para persistência/UnitOfWork).
  - CQRS com handlers registrados via DI.

- rules/restful.md
  - Rota segue padrão de recursos em inglês e plural (`vehicles`) e sub-recurso (`history`).
  - Endpoint protegido com autenticação (`[Authorize]`).

- rules/dotnet-testing.md
  - Testes unitários adicionados com fakes e asserts determinísticos.
  - Observação: o projeto usa FluentAssertions (padrão já presente no repo).

- rules/dotnet-performance.md
  - Repositórios de leitura usam `AsNoTracking()`.

## 3) Resumo da Revisão de Código

### Decisões principais
- Timeline agregada na camada Application (query handler), mantendo controller fino.
- Para “status-changed”, foi adicionada persistência em `audit_entries` a partir de `VehicleStatusChangedEvent` durante o `UnitOfWork.CommitAsync`, garantindo rastreabilidade consistente e ordenável.
- A resposta é genérica por tipo (`type` + `details`) para evitar explosão de DTOs para cada evento.

### Pontos de atenção (recomendações)
- Alguns eventos derivados de reserva (ex.: `reservation-expired` / `reservation-completed`) usam `UpdatedAt` como timestamp quando não há um campo dedicado para o evento em si; se for necessário rigor de auditoria, considerar persistir timestamps explícitos para esses estados (ou audit_entries também para reservas).
- O campo `details` é um dicionário com `object?`; se o frontend exigir contratos mais estáveis/fortes, considerar DTOs específicos por tipo de evento.
- O endpoint exige autenticação, mas não aplica policy específica; se houver necessidade de restringir histórico por perfil (ex.: apenas gestores), adicionar policy dedicada.

## 4) Problemas encontrados e correções

- Migrações EF Core:
  - Problema: tentativa inicial de gerar migration usando um startup project sem dependência de EF Core Design.
  - Correção: geração realizada usando o projeto Infra, que já possui Design-time factory e referência ao pacote `Microsoft.EntityFrameworkCore.Design`.

- Conformidade de código:
  - Ajustado comentário no handler para manter consistência de idioma em trechos novos.

## 5) Validação (build/test) e prontidão

### Comandos executados
- `dotnet test GestAuto.Stock.sln -c Release` (executado a partir de `services/stock`)
  - Resultado: sucesso.
  - Total: 40 testes; Falhas: 0.

### Pronto para deploy?
- Sim, para o escopo da tarefa 9.0.

## 6) Conclusão
- Implementado endpoint de histórico do veículo com timeline cronológica.
- Cada item inclui responsável, timestamp e detalhes para auditoria.
- Mudanças de status agora geram trilha persistida (`audit_entries`) para compor “status-changed” de forma confiável.

Artefatos atualizados:
- tasks/prd-modulo-estoque/9_task.md (status + checklist)
- tasks/prd-modulo-estoque/9_task_review.md (este relatório)
```
````
