---
status: completed
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/stock-domain</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>"3.0, 5.0, 6.0, 7.0, 8.0"</unblocks>
</task_context>

# Tarefa 2.0: Modelagem de domínio e regras do estoque

## Visão Geral
Implementar o núcleo de domínio do Estoque: entidades (Vehicle, Reservation, CheckIn/Out, TestDrive), enums de status/categoria e regras de negócio (status único, obrigatoriedade por categoria, transições válidas/ inválidas), além dos Domain Events base.

## Requisitos
- Representar status vigente único do veículo (RF3.1).
- Implementar validações por categoria (RF1.5–RF1.8).
- Implementar regras de reserva (RF6.7–RF6.12) e permissões (RF6.6) no nível de domínio/handler.
- Modelar test-drive controlado (RF7.1–RF7.4).
- Preparar Domain Events com `EventId` e `OccurredAt`.
- Garantir que **multi-loja** permaneça fora do MVP (apenas enums/valores previstos, sem fluxo).

## Subtarefas
- [x] 2.1 Criar enums: `VehicleCategory`, `VehicleStatus`, `ReservationType`, `ReservationStatus`, `CheckInSource`, `CheckOutReason`, `DemoPurpose`
- [x] 2.2 Criar entidade `Vehicle` com invariantes e transições de status
- [x] 2.3 Criar entidade `Reservation` com regras de expiração (48h / sem expiração / prazo banco)
- [x] 2.4 Criar entidades de histórico: `CheckInRecord`, `CheckOutRecord`, `TestDriveSession`
- [x] 2.5 Criar exceções de domínio (NotFound/Forbidden/DomainException) se não existirem no serviço
- [x] 2.6 Criar Domain Events mínimos para o PRD (check-in, status-changed, reservation.*, sold, test-drive.*, written-off)

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 5.0, 6.0, 7.0, 8.0
- Paralelizável: Sim (após bootstrap)

## Detalhes de Implementação
- Regras de obrigatoriedade por categoria devem ser aplicadas em comandos (check-in) e/ou construtores/fábricas do domínio.
- Regras RBAC sensíveis (ex.: cancelar reserva de outro vendedor) devem existir nos handlers (com `currentUserId`/claims), não só em policies.

## Critérios de Sucesso
- Conjunto de entidades/enums cobre todos RFs do PRD.
- Transições inválidas geram `DomainException`.
- Domain Events possuem `EventId` e `OccurredAt` e podem ser coletados pelo UnitOfWork.
