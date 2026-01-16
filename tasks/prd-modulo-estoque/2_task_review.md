# Revisão da Tarefa 2.0 — Modelagem de domínio e regras do estoque

Data: 2026-01-12

## 1) Resultados da validação (tarefa → PRD → techspec)

### Cobertura dos requisitos

- **RF3.1 (status vigente único)**: Implementado no agregado `Vehicle` via propriedade `CurrentStatus` (status único por entidade) e validação de transições.
- **RF1.5–RF1.8 (obrigatoriedade por categoria)**: Implementado em `Vehicle.CheckIn(...)` via `ValidateCategoryRequirementsForCheckIn(...)`.
  - `novo`: exige `Vin` e `CheckInSource.Manufacturer`.
  - `seminovo`: exige `Plate`, `MileageKm` e `EvaluationId`.
  - `demonstracao`: exige `DemoPurpose` e exige `Plate` quando `IsRegistered=true`.
- **RF6.7–RF6.12 (regras de reserva e expiração)**: Implementado em `Reservation`:
  - `Standard`: expira em 48h.
  - `PaidDeposit`: não expira automaticamente.
  - `WaitingBank`: exige `BankDeadlineAtUtc`.
  - Status e transições: `Active`, `Cancelled`, `Completed`, `Expired`.
- **RF6.6 (permissões/RBAC)**: 
  - O domínio fornece os dados necessários (ex.: `Reservation.SalesPersonId`) e exceções (`ForbiddenException`), mas **as regras de “própria reserva vs gerente” ainda dependem de handlers (Application layer)**. A `2.0` cobre a modelagem e invariantes, mas a imposição por claims/roles não aparece implementada no `2-Application` no estado atual.
- **RF7.1–RF7.4 (test-drive controlado)**: Implementado em `Vehicle.StartTestDrive(...)` e `Vehicle.CompleteTestDrive(...)`, com atualização de status e registro em `TestDriveSession`.
- **Domain Events com `EventId` e `OccurredAt`**: Implementados (interface `IDomainEvent`) e emitidos em fluxos chave.
- **Multi-loja fora do MVP**: Existem enums/valores previstos (`StoreTransfer`/`Transfer`), sem fluxo multi-loja completo.

### Critérios de sucesso

- **Transições inválidas lançam `DomainException`**: coberto por `EnsureTransitionAllowed(...)` e regras de negócio nas operações do agregado.
- **Eventos podem ser coletados por UoW**: `BaseEntity` mantém `DomainEvents` e `ClearEvents()`, permitindo coleta por um UnitOfWork (a implementação do UoW/outbox não faz parte do domínio).

## 2) Descobertas da análise de regras (rules/*.md)

- **`rules/dotnet-architecture.md`**: Padrão de camadas/DDD/CQRS respeitado no que existe hoje no domínio.
- **`rules/dotnet-testing.md`**: Repositório recomenda **AwesomeAssertions**, mas a biblioteca expõe API compatível via namespace **`FluentAssertions`** (o pacote instalado é `AwesomeAssertions`).
- **`rules/git-commit.md`**: Commit deve ser em PT-BR, no formato `<tipo>(escopo opcional): <descrição>` e com lista do que mudou.

## 3) Resumo da revisão de código

- Enums essenciais existem: `VehicleCategory`, `VehicleStatus`, `ReservationType`, `ReservationStatus`, `CheckInSource`, `CheckOutReason`, `DemoPurpose`.
- Entidades essenciais existem: `Vehicle`, `Reservation`.
- Histórico existe: `CheckInRecord`, `CheckOutRecord`, `TestDriveSession`.
- Exceções de domínio existem: `DomainException`, `NotFoundException`, `ForbiddenException`, `UnauthorizedException`.
- Domain Events mínimos para o PRD existem (check-in, status-changed, reservation.*, sold, test-drive.*, written-off).

## 4) Problemas encontrados e resoluções

### P1 — Testes não compilavam por namespace de assertions
- **Sintoma**: `dotnet test` falhava com `CS0246` ao importar `AwesomeAssertions`.
- **Causa raiz**: o pacote `AwesomeAssertions` referencia assembly/namespace compatível com `FluentAssertions`.
- **Correção aplicada**: ajustar `using` nos testes para `FluentAssertions`.

## 5) Validação (build/testes)

- Executado: `dotnet test services/stock/GestAuto.Stock.sln`
- Resultado: **13 testes**, **0 falhas**.

Observação: houve warning de HTTPS redirect no teste de health check (não bloqueante para esta tarefa).

## 6) Recomendações

- Implementar handlers (Application layer) para garantir **RF6.6** no nível de handler (ex.: cancelar reserva de outro vendedor apenas para gerente), conforme a techspec.
- Quando o UnitOfWork/outbox for implementado no Stock, seguir o padrão do serviço Comercial para coleta e limpeza de `DomainEvents` antes do commit.
