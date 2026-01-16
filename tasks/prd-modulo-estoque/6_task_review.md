# Revisão da Tarefa 6.0 — Fluxos de check-in/check-out

## 1) Validação da Definição da Tarefa (tarefa → PRD → techspec)

### Requisitos da tarefa (tasks/prd-modulo-estoque/6_task.md)
- `POST /api/v1/vehicles/{vehicleId}/check-ins` (RF4.1–RF4.4)
  - Implementado em controller MVC (prefixo `api/v1` via convenção do serviço).
  - Fluxo: extrai `userId` do JWT → executa command/handler → chama `Vehicle.CheckIn(...)` → persiste via repositório + `UnitOfWork.CommitAsync()`.
  - Arquivos:
    - services/stock/1-Services/GestAuto.Stock.API/Controllers/VehiclesController.cs
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Commands/CreateCheckInCommand*.cs
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Dto/CheckIn*.cs

- `POST /api/v1/vehicles/{vehicleId}/check-outs` (RF5.1–RF5.4)
  - Implementado com o mesmo padrão (controller fino + handler).
  - Transições por motivo delegadas ao domínio (`Vehicle.CheckOut(...)`).
  - Arquivos:
    - services/stock/1-Services/GestAuto.Stock.API/Controllers/VehiclesController.cs
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Commands/CreateCheckOutCommand*.cs
    - services/stock/2-Application/GestAuto.Stock.Application/Vehicles/Dto/CheckOut*.cs

- Check-in deve aplicar obrigatoriedade por categoria (RF1.5–RF1.8)
  - Validações estão no domínio em `Vehicle.ValidateCategoryRequirementsForCheckIn(...)`.
  - Cobertura de testes unitários adicionada para cenários-chave (novo, seminovo, demonstração).
  - Arquivos:
    - services/stock/3-Domain/GestAuto.Stock.Domain/Entities/Vehicle.cs
    - services/stock/5-Tests/GestAuto.Stock.UnitTest/Domain/VehicleTests.cs

- Check-out por `venda` => status `vendido`; por `baixa_sinistro_perda_total` => `baixado`
  - Mapeamento no domínio:
    - `Sale` → `VehicleStatus.Sold`
    - `TotalLoss` → `VehicleStatus.WrittenOff`
  - Obs.: o nome do status interno é `WrittenOff`, que corresponde ao “baixado” do PRD.
  - Arquivos:
    - services/stock/3-Domain/GestAuto.Stock.Domain/Entities/Vehicle.cs
    - services/stock/5-Tests/GestAuto.Stock.UnitTest/Domain/VehicleTests.cs

- Registrar responsável e timestamps
  - `ResponsibleUserId` vem do JWT (`User.GetUserId()`); `OccurredAt` aceita override no request e cai em `DateTime.UtcNow` por padrão.
  - O domínio registra em `CheckInRecord`/`CheckOutRecord` e atualiza `CurrentOwnerUserId` no check-in.

- Publicar eventos de negócio (RF9.4, RF9.7)
  - O domínio emite `VehicleCheckedInEvent`, `VehicleSoldEvent` e `VehicleWrittenOffEvent`.
  - O `UnitOfWork.CommitAsync()` persiste esses eventos via outbox (mesma transação do `SaveChangesAsync`).
  - Arquivos:
    - services/stock/3-Domain/GestAuto.Stock.Domain/Entities/Vehicle.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/UnitOfWork.cs

### Alinhamento com PRD (tasks/prd-modulo-estoque/prd.md)
- F4 (check-in): registro de origem, data/hora, responsável e histórico; atualização de status coerente; atualização do “responsável atual”.
- F5 (check-out): motivo mínimo (venda/test-drive/transferência/baixa por perda total), trilha com data/hora/responsável e histórico.
- F9 (eventos/auditoria): eventos de negócio são emitidos no domínio e enviados ao outbox.
- Regras de acesso: “baixar veículo (sinistro/perda total) = Gerente Geral/Diretor” foi implementado como `MANAGER`/`ADMIN` (conforme techspec/policies).

### Alinhamento com Tech Spec (tasks/prd-modulo-estoque/techspec.md)
- CQRS nativo (sem MediatR): commands/handlers adicionados e registrados no DI.
- Endpoints de check-in/check-out em `/api/v1`.
- Outbox + RabbitMQ: emissão por eventos de domínio e persistência no outbox via UnitOfWork.

## 2) Análise de Regras e Conformidade

Regras consideradas relevantes para os arquivos alterados:
- rules/dotnet-architecture.md
  - Clean Architecture preservada (API → Application → Domain; Infra captura eventos/outbox).
  - CQRS sem MediatR seguido (handlers registrados no DI e injetados no controller).

- rules/restful.md
  - Rotas em inglês, plural e com kebab-case (`check-ins`, `check-outs`).
  - `201 Created` usado nas operações com retorno de payload (DTO).
  - Observação: `CreatedAtAction` aponta para `GetById` do veículo (Location do veículo), não para o registro de check-in/out (que não tem endpoint de leitura dedicado no escopo).

- rules/ROLES_NAMING_CONVENTION.md
  - Roles verificadas em SCREAMING_SNAKE_CASE (`MANAGER`/`ADMIN`).

- rules/dotnet-testing.md
  - Testes unitários adicionados e determinísticos (sem dependência de infraestrutura externa).

## 3) Resumo da Revisão de Código

### Decisões principais
- Controller fino, usando handlers de commands (mesmo padrão da tarefa 5.0).
- Validações de negócio e transições permanecem no domínio (`Vehicle.CheckIn/CheckOut`).
- Persistência e emissão de eventos via outbox ficam centralizadas no `UnitOfWork`.

### Pontos de atenção (recomendações)
- RBAC para `TotalLoss` está no controller via checagem direta de claims. Recomenda-se (quando oportuno) evoluir para uma policy dedicada (ex.: `Manager`) para manter a autorização totalmente declarativa.
- Não há teste automatizado validando “evento foi persistido no outbox” para check-in/out. Hoje isso é coberto indiretamente pela infraestrutura existente (`UnitOfWork` + domínio). Se quiser elevar confiança, dá para adicionar teste de integração focado no outbox (sem RabbitMQ).

## 4) Problemas encontrados e correções

- Não foram encontrados problemas funcionais no escopo da tarefa 6.0 durante a revisão.
- Observação (fora do escopo da tarefa): o build do serviço Stock ainda emite warnings `CS0618` sobre `UseXminAsConcurrencyToken` em configurações EF Core (Infra).

## 5) Validação (build/test) e prontidão

### Comandos executados
- `dotnet test` (executado a partir de `services/stock`)
  - Resultado: sucesso.
  - Total: 28 testes; Falhas: 0.

### Pronto para deploy?
- Sim, para o escopo da tarefa 6.0.

## 6) Conclusão
- Endpoints de check-in/check-out implementados com histórico e atualização de status.
- Regras por categoria aplicadas no check-in.
- Check-out atualiza status conforme motivo (venda/baixa etc.) e bloqueia status terminal.
- Eventos de domínio persistem no outbox via UnitOfWork.

Artefatos atualizados:
- tasks/prd-modulo-estoque/6_task.md (checklist e status)
- tasks/prd-modulo-estoque/6_task_review.md (este relatório)
