# Revisão da Tarefa 10.0 — Testes (unitários + integração) e validações de contrato

Data: 2026-01-15
Branch: `test/modulo-estoque-10-testes`

## 1) Validação da Definição da Tarefa (tarefa → PRD → techspec)

### 1.1 Requisitos da tarefa (tasks/prd-modulo-estoque/10_task.md)

- Testes unitários para handlers principais (já previstos nas tarefas 5–9)
  - A tarefa 10.0 foca em consolidação + testes de integração e validações de contrato; os unit tests já existiam/foram feitos nas tarefas anteriores.

- Testes de integração cobrindo:
  - Reserva ativa única sob concorrência
    - Coberto por `ReservationConcurrencyTests`.
  - Expiração automática de reserva `padrao`
    - Coberto por `ReservationExpirationTests` via execução direta do runner (sem depender de relógio real).
  - Check-in por categoria (novo/seminovo/demonstração) com validações obrigatórias
    - Coberto por `VehicleCheckInCategoryTests`.
  - Check-out por venda/baixa atualizando status corretamente
    - Coberto por `VehicleCheckOutTests`.

- Skip limpo quando Docker não estiver disponível
  - Todos os testes de integração utilizam `SkippableFact` + `Skip.IfNot(_postgresFixture.IsAvailable, ...)`.

### 1.2 Alinhamento com PRD (tasks/prd-modulo-estoque/prd.md)

- F3 (status único do veículo) e F4/F5 (check-in/check-out)
  - Validado por testes que exercitam endpoints de check-in/check-out e verificam status resultante.
- F1 (obrigatoriedade por categoria)
  - Validado por testes que garantem retorno 422 quando faltam campos obrigatórios (placa em seminovo; demo purpose em demonstração; source no check-in de novo).
- F6 (reserva explícita e exclusividade)
  - Validado por teste concorrente garantindo apenas 1 reserva ativa por veículo.

### 1.3 Alinhamento com Tech Spec (tasks/prd-modulo-estoque/techspec.md)

- Base `/api/v1` para API
  - Testes chamam `/api/v1/...` e foram ajustados para refletir o esquema real de roteamento.
- Concurrency via índice parcial de reserva ativa
  - Teste concorrente valida o contrato observado (1 Created + 1 Conflict).
- Outbox
  - Testes validam persistência de mensagens na tabela `stock.outbox_messages` após operações HTTP.
- Job de expiração
  - Refatorado para permitir execução determinística via `ReservationExpirationRunner`.

## 2) Análise de Regras e Conformidade

Regras consideradas relevantes:
- `rules/dotnet-testing.md`
  - Integração usa Testcontainers com skip limpo quando Docker não está disponível.
- `rules/dotnet-architecture.md`
  - Mantido o padrão Clean Architecture: testes exercitam a API via HTTP; regras de negócio permanecem no domínio/application.
- `rules/restful.md`
  - Validações retornam `ProblemDetails` e códigos coerentes (422 para regras de negócio/validação, 409 para conflito de reserva ativa).

## 3) Principais mudanças implementadas

### 3.1 Infra de testes de integração
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Shared/CustomWebApplicationFactory.cs`
  - Configura ambiente `Testing` e substitui DbContext para Postgres do Testcontainers.
  - Adiciona autenticação fake por header (handler dedicado) e policies compatíveis com o `Program.cs`.
- `services/stock/5-Tests/Shared/TestAuthHandler.cs`
  - Emite claims `sub` e `roles` a partir de `X-Test-UserId` e `X-Test-Role`.
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Shared/TestHttpClientExtensions.cs`
  - Helper para aplicar autenticação de teste no `HttpClient`.

### 3.2 Suíte de testes (integração)
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Controllers/ReservationConcurrencyTests.cs`
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Controllers/ReservationExpirationTests.cs`
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Controllers/VehicleCheckInCategoryTests.cs`
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Controllers/VehicleCheckOutTests.cs`
- `services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Controllers/OutboxFromHttpOperationsTests.cs`

### 3.3 Ajustes para estabilidade e consistência da API
- Roteamento de reservas em `/api/v1`
  - `services/stock/1-Services/GestAuto.Stock.API/Controllers/ReservationsController.cs`
- Persistência de histórico (check-in/out/test-drive) sem falsos UPDATEs
  - `services/stock/4-Infra/GestAuto.Stock.Infra/Repositories/VehicleRepository.cs`

### 3.4 Expiração e health-check com dependência de DB
- Runner de expiração (executável em testes e background)
  - `services/stock/1-Services/GestAuto.Stock.API/Services/ReservationExpirationRunner.cs`
  - `services/stock/1-Services/GestAuto.Stock.API/Services/ReservationExpirationService.cs` (passa a delegar para o runner)
- Health check de DB
  - `services/stock/1-Services/GestAuto.Stock.API/Services/StockDbHealthCheck.cs`
  - Registrado em `services/stock/1-Services/GestAuto.Stock.API/Program.cs`

### 3.5 Dockerização
- `services/stock/Dockerfile`
- `docker-compose.yml`
  - Adicionado `stock-api` com rota no Traefik e porta local `8089:8080` para testes básicos.

## 4) Validação (build/test) e conectividade

### 4.1 Comandos executados
- `dotnet test services/stock/GestAuto.Stock.sln -c Release`
  - Resultado: sucesso.
  - Total: 49 testes; Falhas: 0.

### 4.2 Teste básico em container
- `docker compose up -d stock-api`
- `curl http://localhost:8089/health`
  - Resultado: `200 OK`.
  - Observação: o endpoint `/health` inclui verificação de conectividade com o banco via `StockDbHealthCheck`.

## 5) Conclusão

- A suíte de testes de integração do Stock cobre regras críticas do PRD/Tech Spec (concorrência de reserva, expiração, RBAC, check-in/out e outbox).
- Execução local validada (49 testes passando) e conectividade básica em container validada via `/health`.
