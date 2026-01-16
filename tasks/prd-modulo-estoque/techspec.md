# Especificação Técnica - Módulo de Estoque (Veículos)

## Resumo Executivo

Esta especificação define a implementação **backend-first** do Módulo de Estoque como um **novo serviço .NET** seguindo o padrão já adotado no repositório (Clean Architecture + CQRS nativo, sem MediatR), com persistência em **PostgreSQL via EF Core** e publicação de eventos via **Outbox + RabbitMQ**.

O serviço será a **fonte única de verdade** do status vigente do veículo (status único) e do ciclo de vida operacional (check-in/check-out, reserva, test-drive, baixa), expondo APIs REST em `/api/v1` para consumo pelo frontend e pelo módulo Comercial, e eventos assíncronos para integrações (Comercial/Financeiro/Oficina/Delivery/Seminovos).

Reusar vs construir:
- Reusar os pacotes e padrões já presentes no serviço Comercial (EF Core Npgsql, autenticação Keycloak JWT, BackgroundServices, Saunter para AsyncAPI, outbox + RabbitMQ).
- Manter CQRS nativo (interfaces simples) ao invés de introduzir MediatR.
- Evitar novas dependências sem necessidade clara; preferir middlewares/abstrações já existentes no repo.

## Arquitetura do Sistema

### Visão Geral dos Componentes

- **GestAuto.Stock.API**
  - Controllers REST (`/api/v1/...`), autenticação Keycloak JWT, policies por role.
  - Middleware de erro retornando `application/problem+json`.
  - Endpoints de documentação (Swagger/OpenAPI + AsyncAPI com Saunter, seguindo padrão do Comercial).
- **GestAuto.Stock.Application**
  - Commands/Queries e respectivos Handlers (`ICommandHandler<>` / `IQueryHandler<>`).
  - DTOs de request/response.
  - Validações (ex.: FluentValidation, se já estiver no padrão do serviço).
- **GestAuto.Stock.Domain**
  - Entidades, VOs, enums (categoria/status, reserva), regras de transição e validações de negócio.
  - Domain Events padronizados com `EventId` e `OccurredAt`.
- **GestAuto.Stock.Infra**
  - EF Core DbContext (schema `stock`), repositórios, UnitOfWork.
  - Outbox e publisher RabbitMQ (exchange `stock`).
  - BackgroundServices: processamento de Outbox e expiração automática de reservas.

Fluxos principais:
- Operações síncronas via REST -> Domain -> persistência -> gravação de outbox (na mesma transação) -> publicação assíncrona.
- Expiração automática de reservas -> job background -> atualiza reserva/status -> grava outbox -> publica eventos.

## Design de Implementação

### Interfaces Principais

Interfaces de CQRS (seguindo [rules/dotnet-architecture.md](../../rules/dotnet-architecture.md) e padrão do módulo Comercial):

```csharp
namespace GestAuto.Stock.Application.Interfaces;

public interface ICommand<TResponse> { }
public interface IQuery<TResponse> { }

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

Interfaces de domínio/repositório (exemplos):

```csharp
namespace GestAuto.Stock.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
}
```

### Modelos de Dados

#### Entidades de Domínio (mínimo MVP)

- **Vehicle**
  - `Id: Guid`
  - `Category: VehicleCategory` (`novo`, `seminovo`, `demonstracao`)
  - `CurrentStatus: VehicleStatus` (`em_transito`, `em_estoque`, `reservado`, `em_test_drive`, `em_preparacao`, `vendido`, `baixado`)
  - Identificadores: `Vin` (obrigatório em todas categorias), `Plate` (obrigatório para seminovo; obrigatório para demonstração “já emplacado”)
  - Atributos: `Make`, `Model`, `Trim?`, `YearModel`, `Color`
  - Seminovo: `MileageKm?`, `EvaluationId?` (referência externa para avaliação vinculada)
  - Demonstração: `DemoPurpose` (`test-drive` | `frota_interna`), `IsRegistered` (indica “já emplacado”)
  - `CurrentOwnerUserId: Guid?` (responsável operacional atual)
  - `CreatedAt`, `UpdatedAt`

- **CheckInRecord**
  - `Id`, `VehicleId`, `Source` (`montadora`, `compra_cliente_seminovo`, `transferencia_entre_lojas`, `frota_interna`)
  - `OccurredAt`, `ResponsibleUserId`, `Notes?`

- **CheckOutRecord**
  - `Id`, `VehicleId`, `Reason` (`venda`, `test_drive`, `transferencia`, `baixa_sinistro_perda_total`)
  - `OccurredAt`, `ResponsibleUserId`, `Notes?`

- **Reservation**
  - `Id`, `VehicleId`, `Type` (`padrao`, `entrada_paga`, `aguardando_banco`)
  - `Status` (`ativa`, `cancelada`, `concluida`, `expirada`)
  - `SalesPersonId: Guid`
  - `ContextType: string` (ex.: `lead`, `proposal`, `opportunity`) e `ContextId: Guid?`
  - `CreatedAt`, `ExpiresAt?`, `BankDeadlineAt?`
  - `CancelledAt?`, `CancelReason?`
  - `ExtendedAt?`, `ExtendedByUserId?`, `PreviousExpiresAt?`

- **TestDriveSession**
  - `Id`, `VehicleId`, `SalesPersonId`, `CustomerRef?`
  - `StartedAt`, `EndedAt?`, `Outcome` (`returned_to_stock`, `converted_to_reservation`)

#### Regras de validação (domínio)

- Status único: `Vehicle.CurrentStatus` é a fonte do status vigente.
- Obrigatoriedade por categoria:
  - `novo`: exige `Vin` e `Source=montadora` no check-in.
  - `seminovo`: exige `Vin`, `Plate`, `MileageKm`, `EvaluationId`.
  - `demonstracao`: exige `Vin` e `DemoPurpose`; exige `Plate` quando `IsRegistered=true`.
- Transições inválidas devem lançar `DomainException` (ex.: reservar veículo `vendido`/`baixado`).

#### Esquema de Banco (PostgreSQL)

Schema: `stock`.

Tabelas (nomes sugestivos; ajustar conforme convenção EF):
- `vehicles`
- `check_ins`
- `check_outs`
- `reservations`
- `test_drives`
- `outbox_messages` (mesmo padrão do Comercial)
- `audit_entries` (opcional no MVP se o “timeline” for derivado de `check_ins/check_outs/reservations/test_drives` + `status`)

Índices/constraints importantes:
- Unicidade de veículo ativo por VIN (quando presente): `UNIQUE (vin)`.
- Unicidade de placa (quando presente e ativa): `UNIQUE (plate)` (ou parcial, se houver “baixado”).
- Reserva ativa única por veículo (Postgres **partial unique index**): `UNIQUE (vehicle_id) WHERE status='ativa'`.
- Concurrency: coluna `row_version`/`xmin` (optimistic concurrency) para evitar race em reserva/status.

### Endpoints de API

Base: `/api/v1`.

#### Veículos
- `POST /api/v1/vehicles`
  - Registra veículo (dados cadastrais). Status inicial deve ser coerente com fluxo (ex.: `em_transito` para novo; `em_estoque` para seminovo/demonstração após check-in).
- `GET /api/v1/vehicles/{vehicleId}`
  - Retorna detalhe + status vigente + disponibilidade derivada.
- `GET /api/v1/vehicles`
  - Lista paginada (priorizar `?_page` e `?_size` conforme [rules/restful.md](../../rules/restful.md)) com filtros simples opcionais: `status`, `category`, `q` (VIN/placa/modelo).
- `PATCH /api/v1/vehicles/{vehicleId}/status`
  - Atualiza status manual (apenas roles autorizadas). Inclui `newStatus` e `reason`.
- `GET /api/v1/vehicles/{vehicleId}/history`
  - Timeline cronológica de check-in/out, reservas, test-drives e mudanças de status.

#### Check-in / Check-out
- `POST /api/v1/vehicles/{vehicleId}/check-ins`
  - Registra entrada, aplica validações por categoria, atualiza status coerente e owner.
- `POST /api/v1/vehicles/{vehicleId}/check-outs`
  - Registra saída (motivo), atualiza status (`vendido`/`baixado`/`em_test_drive` conforme motivo).

#### Reservas
- `POST /api/v1/vehicles/{vehicleId}/reservations`
  - Cria reserva (exclusividade comercial). Enforce: só 1 reserva ativa por veículo.
  - Regras de expiração:
    - `padrao`: `ExpiresAt = CreatedAt + 48h`.
    - `entrada_paga`: `ExpiresAt = null`.
    - `aguardando_banco`: exige `BankDeadlineAt` informado manualmente; `ExpiresAt = BankDeadlineAt`.
- `POST /api/v1/reservations/{reservationId}/cancel`
  - Cancela reserva (motivo obrigatório). Regra: vendedor só cancela a própria; gerente comercial cancela qualquer.
- `POST /api/v1/reservations/{reservationId}/extend`
  - Prorroga reserva (apenas gerente comercial). Atualiza `ExpiresAt` e registra auditoria.

#### Test-drive
- `POST /api/v1/vehicles/{vehicleId}/test-drives/start`
  - Inicia test-drive: muda status para `em_test_drive` e registra sessão.
- `POST /api/v1/test-drives/{testDriveId}/complete`
  - Finaliza: retorna para `em_estoque` ou converte para `reservado` (com criação de reserva).

Padrões de resposta/erro:
- Sucesso: JSON com DTOs.
- Erros: `application/problem+json` via middleware, mapeando `NotFoundException`->404, `ForbiddenException`->403, conflitos/concorrência -> 409 e violações de regra de negócio -> 422.

## Pontos de Integração

### Autenticação/Autorização (Keycloak)
- JWT Bearer com `RoleClaimType = "roles"`.
- Roles em SCREAMING_SNAKE_CASE (seguir [rules/ROLES_NAMING_CONVENTION.md](../../rules/ROLES_NAMING_CONVENTION.md)).
- Identificação do usuário via claim `sub`.
- Normalização de roles: replicar a estratégia do serviço Comercial (durante `OnTokenValidated`) para garantir que a claim `roles` exista como múltiplas claims, inclusive quando o Keycloak emitir um JSON array em uma única claim.

Policies sugeridas (mínimo para atender PRD):
- `SalesPerson`: `SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`.
- `SalesManager`: `SALES_MANAGER`, `MANAGER`, `ADMIN`.

Mapeamento RBAC (alinhado ao PRD e às roles existentes no bootstrap do Keycloak do repo):
- Reservar veículo: `SALES_PERSON` (e acima).
- Cancelar a própria reserva: `SALES_PERSON` (e acima).
- Cancelar reserva de outro vendedor: `SALES_MANAGER` / `MANAGER` / `ADMIN`.
- Alterar status manualmente: `MANAGER` / `ADMIN`.
- Baixar veículo (sinistro/perda total): `MANAGER` / `ADMIN`.

Observação: hoje o bootstrap do Keycloak do repo cria, entre outras, as roles `ADMIN`, `MANAGER`, `SALES_PERSON`, `SALES_MANAGER`.
Se for necessário introduzir roles específicas de estoque (ex.: `STOCK_MANAGER`), isso deve ser feito junto com a atualização do bootstrap e alinhamento do realm.

### Eventos (RabbitMQ via Outbox)

- Exchange: `stock` (topic).
- Publicação via Outbox (mesmo padrão do Comercial):
  - Persistir mudanças + mensagem outbox na mesma transação.
  - Processador em background publica com `MessageId = EventId`.
  - Semântica **at-least-once**; consumidores devem ser idempotentes.

Routing keys (proposta final para fechar a “Questão em Aberto” do PRD):
- `vehicle.checked-in`
- `vehicle.status-changed`
- `reservation.created`
- `reservation.cancelled`
- `reservation.extended`
- `reservation.expired`
- `vehicle.sold`
- `vehicle.test-drive.started`
- `vehicle.test-drive.completed`
- `vehicle.written-off`

Payload base (camelCase, alinhado ao padrão do Outbox do Comercial):
- `eventId: string` (GUID)
- `occurredAt: string` (UTC ISO-8601)
- `aggregateId: string` (VehicleId)
- Campos específicos por evento, ex.:
  - `vehicleStatusChanged`: `previousStatus`, `newStatus`, `changedByUserId`
  - `reservationCreated`: `reservationId`, `type`, `salesPersonId`, `expiresAt?`, `contextType?`, `contextId?`

Idempotência:
- Producer: `EventId` único por evento (domain event) e `MessageId` AMQP.
- Consumer: manter tabela/registro de `processed_event_ids` ou constraint idempotente no lado do consumidor.

### Integração com serviços/módulos
- **Comercial**: consulta disponibilidade (GET veículos), cria/cancela/prorroga reserva, inicia fluxo de venda (check-out por venda) e/ou consome eventos `reservation.*` e `vehicle.sold`.
- **Seminovos / Vehicle Evaluation**: ao criar seminovo, exige `EvaluationId` (referência), podendo validar existência via API externa (opcional; fora do MVP se não houver endpoint pronto).
- **Financeiro**: consome `vehicle.sold` e `reservation.created` (aguardando banco) para iniciar/acompanhar faturamento.
- **Oficina**: usa `PATCH status` para `em_preparacao` e retorno para `em_estoque`.
- **Delivery**: consome `vehicle.sold` para iniciar preparação de entrega (detalhes fora do escopo).

Tratamento de erros:
- Falhas RabbitMQ não devem impedir a API de iniciar (conexão lazy, como no Comercial).
- Outbox com retry e marcação de falha; logs estruturados para diagnosticar.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
| --- | --- | --- | --- |
| Novo serviço `Stock` | Novo componente | Adiciona API + DB + eventos. Risco médio (infra). | Provisionar deploy, env vars, secrets |
| RabbitMQ | Nova exchange/rotas | Exchange `stock` e novas routing keys. Risco baixo-médio. | Atualizar infra/helm/compose e consumers |
| Keycloak | Reuso de roles | MVP usa roles já existentes (`SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`). Risco baixo. | Garantir policies/claims `roles` e usuários de teste |
| Comercial | Integração | Comercial chamará/consumirá estoque. Risco médio (contrato). | Definir contratos e ajustar consumers |
| Banco Postgres | Novo schema | Cria schema/tabelas e migrações. Risco baixo. | Provisionar DB e migrações |

## Abordagem de Testes

### Testes Unitários

- Domain: transições de status e invariantes (status único, regras por categoria).
- Application Handlers:
  - `CreateReservationHandler`: bloqueia reserva concorrente; valida tipo/expiração.
  - `CancelReservationHandler`: regra “própria reserva vs gerente”.
  - `CheckInHandler` e `CheckOutHandler`: validações por categoria e atualização de status.
- Mock apenas de repositórios e `IUnitOfWork` (padrão de testes do Comercial com Moq).

### Testes de Integração

- Padrão Testcontainers (espelhando [services/commercial/5-Tests/Shared/...](../../services/commercial/5-Tests/Shared/)):
  - Postgres fixture com migrations.
  - RabbitMQ fixture opcional para validar publicação via outbox (pode ser teste separado e mais lento).
- Cenários críticos:
  - “Reserva ativa única por veículo” com concorrência (duas requisições simultâneas -> uma falha 409/400).
  - “Expiração automática” (reserva padrão expira e veículo volta a `em_estoque`).
  - “Check-out por venda” publica `vehicle.sold`.

Observação: manter testes determinísticos; pular testes que requerem Docker de forma limpa se Docker não estiver disponível (conforme regras do repo).

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **Domain + EF Core schema**: entidades/enums, DbContext, migrações, constraints.
2. **Commands/Queries + Handlers**: check-in/out, reservas, status, test-drive.
3. **API Controllers + Auth policies**: rotas `api/v1`, ProblemDetails, RBAC.
4. **Outbox + RabbitMQ**: publisher, routing keys e AsyncAPI docs.
5. **Jobs**: expiração automática de reservas.
6. **Testes**: unitários primeiro; integração depois.

### Dependências Técnicas

- RabbitMQ acessível no ambiente (ou conexão lazy configurada).
- Keycloak realm/clients com roles e claims (`roles`, `sub`).
- Postgres com permissões para criar schema `stock`.

## Monitoramento e Observabilidade

- **Health check**: endpoint `/health` (mínimo) e (ideal) incluir verificação de DbContext.
- **Logs**: logs estruturados com `ILogger<>`; incluir `EventId`, `VehicleId`, `ReservationId` em escopos.
- **Métricas sugeridas** (via stack existente do serviço):
  - contadores de `reservation.created`, `reservation.expired`, `vehicle.sold`.
  - duração de processamento da outbox e tamanho da fila de outbox pendente.

## Considerações Técnicas

### Decisões Principais

- **Serviço dedicado (`Stock`)** ao invés de “colocar tudo no Comercial”:
  - Mantém o estoque como fonte única e reduz acoplamento entre domínios.
  - Facilita eventos e integrações multi-módulo.
- **Outbox + RabbitMQ** (padrão existente no repo):
  - Evita perda de eventos e mantém consistência transacional.
- **Partial unique index** para reserva ativa:
  - Garante invariantes mesmo sob concorrência.

### Riscos Conhecidos

- **Definição/propagação de roles no Keycloak**: sem roles corretas, RBAC não se sustenta.
  - Mitigação: alinhar roles cedo e validar com testes de integração de auth.
- **Concorrência de reservas** (duas operações simultâneas):
  - Mitigação: constraint no banco + optimistic concurrency + retorno HTTP apropriado.
- **Expiração automática** pode gerar “surpresas” para vendedores:
  - Mitigação: eventos + logs + endpoint de histórico para auditoria.

Perguntas em aberto (derivadas do PRD):
- Como o Estoque irá expor um **catálogo/frota de veículos elegíveis para test-drive** (campos mínimos e regras de elegibilidade), para consumo do módulo Comercial?
- Qual será o comportamento oficial de status quando houver **ocorrência/dano em test-drive**: retorno para `em_preparacao` vs um status dedicado de exceção?

### Requisitos Especiais

- Segurança:
  - Nunca permitir ações críticas sem usuário autenticado.
  - Aplicar regra de “cancelar própria reserva” em nível de handler (não apenas policy).
- Performance:
  - Listagem com paginação; índices em `status`, `category`, `vin`, `plate`.

### Conformidade com Padrões

- Clean Architecture + CQRS sem MediatR (seguir [rules/dotnet-architecture.md](../../rules/dotnet-architecture.md)).
- REST `/api/v1` + erros com `application/problem+json` (seguir convenções do serviço Comercial e [rules/restful.md](../../rules/restful.md)).
- Roles em SCREAMING_SNAKE_CASE e claim `roles` (seguir [rules/ROLES_NAMING_CONVENTION.md](../../rules/ROLES_NAMING_CONVENTION.md)).
- Testes: unitários com mocks e integração com Testcontainers quando aplicável (seguir [rules/dotnet-testing.md](../../rules/dotnet-testing.md)).
- Observabilidade: health checks, CancellationToken e logs estruturados (seguir [rules/dotnet-observability.md](../../rules/dotnet-observability.md)).

Observação: o template de Tech Spec sugere o uso de [infra/monitoring](../../infra/monitoring); na implementação, usar a infraestrutura de observabilidade existente do repo (como no serviço Comercial) e integrar com o pacote/diretório padrão caso esteja disponível para .NET.
