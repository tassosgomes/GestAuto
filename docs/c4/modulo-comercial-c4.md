# C4 - Módulo Comercial (GestAuto) — Análise (a partir do PRD)

Fonte: `tasks/prd-modulo-comercial/prd.md`.

## Níveis gerados

- **C1 (System Context)**: informação suficiente no PRD (atores, propósito do sistema e integrações com outros módulos).
- **C2 (Container)**: informação suficiente no PRD (REST API, comunicação assíncrona via RabbitMQ, persistência necessária).
- **C3 (Component)**: gerado com **decomposição inferida** a partir das funcionalidades F1..F7. O PRD descreve capacidades e regras de negócio, mas não define explicitamente a arquitetura interna.
- **C4 (Code)**: gerado a partir do **código real** do domínio (classes em `services/commercial/3-Domain/GestAuto.Commercial.Domain`).

## Elementos explícitos do PRD refletidos nos diagramas

- **Atores**: Vendedor, Gerente Comercial, Administrativo.
- **Integrações**:
  - Comunicação assíncrona via **RabbitMQ**.
  - Integração com **Módulo de Seminovos** (solicitação e resposta de avaliação).
  - Integração com **Módulo Financeiro** (evento de venda fechada e eventos de status do pedido).
- **Interfaces**:
  - Operações via **API REST** (backend-first).
- **Conceitos de domínio**:
  - Lead e seus status.
  - Qualificação (Diamante/Ouro/Prata/Bronze) e bonificações.
  - Test-drive.
  - Proposta e seus status.
  - Fechamento (impedir alterações após fechado).
  - Acompanhamento do pedido via eventos do financeiro.
- **Requisitos não-funcionais**:
  - Auditoria, idempotência e consistência eventual (representados como responsabilidades do backend e persistência).
- **RBAC**:
  - Vendedor (visão própria) vs Gerente (visão geral) (representado no C3 como componente de autorização).

## Inferências (explicitamente assumidas)

As inferências abaixo foram feitas para conseguir desenhar C3 de forma útil, sem contradizer o PRD:

- A API do módulo comercial possui uma camada de **Controllers** (REST) e serviços internos por caso de uso (Lead, Scoring, Test-Drive, Propostas, Fechamento).
- Existe um componente dedicado a **publicar/consumir eventos** (cliente AMQP) e componentes de **integração** (Seminovos / Financeiro).
- A persistência foi modelada como **Relational Database** porque o PRD exige rastreabilidade, auditoria e listagens com filtro/ordenação; o PRD não define o SGBD.

## O que ficou de fora (por não estar especificado no PRD)

- Tecnologias/linguagens/frameworks concretos do backend.
- Detalhamento de endpoints REST, payloads, schemas de eventos, versionamento e contratos.
- Modelo de dados (tabelas, agregados, entidades) e políticas de consistência/locking.
- Mecanismo exato de autenticação (IdP, OIDC, JWT, etc.).

## Escopo do C4 (Code)

O diagrama C4 (Code) foi construído com base nas classes do **Domain Layer**:

- **Entities**: `Lead`, `Interaction`, `Proposal`, `ProposalItem`, `TestDrive`, `UsedVehicle`, `UsedVehicleEvaluation`, `Order`, `PaymentMethodEntity` e `BaseEntity`.
- **ValueObjects**: `Qualification`, `TradeInVehicle`, `TestDriveChecklist`, `Money`, `Email`, `Phone`, `LicensePlate`.
- **Enums** e **Events**: incluídos no diagrama para mostrar dependências e eventos emitidos.

Elementos de Application/Infra (Controllers, Repositories, DbContext, Messaging, Outbox, Auth) não foram detalhados no C4 (Code) para manter o diagrama fiel ao que foi fornecido (classes do domínio) e legível.

## Próximos passos sugeridos (se você quiser evoluir para C4 nível Code)

Para evoluir o C4 (Code) para cobrir também Application/Infra (sem inventar informação), o ideal é incluir no diagrama:

- Handlers CQRS (Commands/Queries) e seus DTOs.
- Interfaces (Repositories, UnitOfWork, EventPublisher) e implementações (EF Core, RabbitMQ, Outbox).
