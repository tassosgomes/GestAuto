# Revisão da Tarefa 4.0 — Outbox + RabbitMQ (eventos de domínio)

## 1) Validação da Definição da Tarefa (tarefa → PRD → techspec)

### Requisitos da tarefa (tasks/prd-modulo-estoque/4_task.md)
- Registrar eventos no outbox na mesma transação do EF Core
  - Implementado via `IUnitOfWork.CommitAsync()` escrevendo eventos no outbox e chamando `SaveChangesAsync` uma única vez.
  - Arquivos:
    - services/stock/3-Domain/GestAuto.Stock.Domain/Interfaces/IUnitOfWork.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/UnitOfWork.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/Repositories/OutboxRepository.cs
- Publicar eventos com `MessageId = EventId`
  - Implementado em `RabbitMqPublisher` com `properties.MessageId = domainEvent.EventId.ToString()`.
  - Arquivo: services/stock/4-Infra/GestAuto.Stock.Infra/Messaging/RabbitMqPublisher.cs
- Exchange `stock` e routing keys definidas na Tech Spec
  - Exchange `stock` declarado como `topic` (durable).
  - Routing keys mapeadas estritamente para as chaves listadas na techspec.
  - Arquivos:
    - services/stock/4-Infra/GestAuto.Stock.Infra/Messaging/RabbitMqConfiguration.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/Messaging/RabbitMqPublisher.cs
- Processador de outbox em background com retry e marcação de falhas
  - Implementado `OutboxProcessorService` com polling, batch e backoff exponencial (1s, 2s, 4s) e marcação de `processed`/`failed`.
  - Arquivo: services/stock/1-Services/GestAuto.Stock.API/Services/OutboxProcessorService.cs
- Conexão RabbitMQ lazy (não bloquear startup)
  - A conexão é registrada via `Lazy<IConnection>`, e o publisher só é resolvido quando há mensagens pendentes no outbox.
  - Além disso, o wiring do RabbitMQ + hosted service é evitado no ambiente `Testing` para não interferir nos testes de integração.
  - Arquivos:
    - services/stock/4-Infra/GestAuto.Stock.Infra/Messaging/RabbitMqConfiguration.cs
    - services/stock/1-Services/GestAuto.Stock.API/Program.cs

### Alinhamento com PRD (tasks/prd-modulo-estoque/prd.md)
- O PRD exige publicação de eventos de negócio relevantes (ex.: entrada no estoque, mudança de status, reserva, venda, test-drive, baixa) e idempotência via `EventId`.
- A implementação atende ao mecanismo de publicação assíncrona com semântica at-least-once, permitindo que consumidores sejam idempotentes usando o `EventId`.

### Alinhamento com Tech Spec (tasks/prd-modulo-estoque/techspec.md)
- Outbox + RabbitMQ seguindo padrão do Comercial: payload em camelCase e `MessageId` preenchido.
- Routing keys implementadas:
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

Observação: existem eventos de domínio adicionais no Stock (`ReservationReleasedEvent`, `ReservationCompletedEvent`). Eles NÃO estão definidos na lista final de routing keys da techspec, então a publicação via RabbitMQ não mapeia essas chaves nesta tarefa. Recomenda-se alinhar contrato/eventos na techspec quando esses eventos entrarem no escopo.

## 2) Análise de Regras e Conformidade

Regras consideradas relevantes para os arquivos alterados:
- rules/dotnet-architecture.md
  - Interfaces expostas na camada Domain (`IUnitOfWork`, `IEventPublisher`) e implementações na Infra/API: atendido.
  - Padrão de inversão de dependência (Domain não depende de Infra): atendido.
- rules/dotnet-testing.md
  - Testes determinísticos e integração via Testcontainers: atendido.
  - Uso de AwesomeAssertions: o pacote utilizado é `AwesomeAssertions`, porém o namespace permanece `FluentAssertions` por compatibilidade (padrão adotado nos testes existentes do Stock).
- rules/dotnet-coding-standards.md
  - Estrutura clara e responsabilidades separadas (publisher / outbox processor / unit of work): atendido.
  - Observação de baixa severidade: alguns comentários/XML docs estão em PT-BR (padrão já observado no repo em outros serviços).
- rules/dotnet-folders.md
  - Estrutura `1-Services/2-Application/3-Domain/4-Infra/5-Tests` preservada.

## 3) Resumo da Revisão de Código

### Decisões principais
- Consistência transacional: `UnitOfWork.CommitAsync()` coleta `DomainEvents` das entidades rastreadas e grava `outbox_messages` antes do `SaveChangesAsync()`.
- Lazy RabbitMQ:
  - Conexão criada apenas quando `IEventPublisher` é resolvido.
  - `OutboxProcessorService` resolve `IEventPublisher` somente quando encontra mensagens pendentes.
- Publicação:
  - Exchange `stock` (topic, durable).
  - `MessageId` no AMQP = `EventId`.
  - Logs estruturados com `eventId`, `eventType`, `routingKey` e `outboxMessageId`.

### Pontos de atenção
- `OutboxProcessorService` é um polling worker (não event-driven). Isso está alinhado ao padrão proposto e ao espelhamento do Comercial.

## 4) Problemas encontrados e correções

- Bug: marcação de mensagens como processadas/falhas não era persistida no banco
  - Causa: `MarkAsProcessedAsync/MarkAsFailedAsync` alteravam entidades rastreadas no DbContext, mas o `OutboxProcessorService` não chamava `SaveChangesAsync`.
  - Correção: `OutboxProcessorService` passou a salvar o DbContext após marcar `processed/failed`.
  - Arquivo: services/stock/1-Services/GestAuto.Stock.API/Services/OutboxProcessorService.cs

- Conformidade: routing keys “extras” fora da techspec
  - Correção: removidas chaves/mapeamentos que não estavam na lista final da techspec (mantendo apenas as chaves especificadas).
  - Arquivos:
    - services/stock/4-Infra/GestAuto.Stock.Infra/Messaging/RabbitMqConfiguration.cs
    - services/stock/4-Infra/GestAuto.Stock.Infra/Messaging/RabbitMqPublisher.cs

- Testes: namespace de assertions
  - Ajuste: mantido `using FluentAssertions;` (namespace compatível com o pacote `AwesomeAssertions`, como já adotado nos testes existentes do Stock).
  - Arquivo: services/stock/5-Tests/GestAuto.Stock.IntegrationTest/Repository/OutboxRepositoryTests.cs

## 5) Validação (build/test) e prontidão

### Comandos executados
- `dotnet test services/stock/GestAuto.Stock.sln -c Release`
  - Resultado: sucesso.

### Avisos conhecidos
- Warnings CS0618 (Npgsql `UseXminAsConcurrencyToken`) continuam aparecendo na Infra.
  - Impacto: não quebra build; recomendação é avaliar migração para `IsRowVersion()`/`[Timestamp]` quando for oportuno.

### Pronto para deploy?
- Sim, para o escopo da tarefa 4.0.
- Observação: a subtarefa 4.5 (AsyncAPI do Stock) não foi executada, pois é condicionada (“se adotado no padrão”) e não é necessária para cumprir os requisitos funcionais/infra desta tarefa.

## 6) Conclusão
- Requisitos 4.1–4.4 atendidos e validados via testes.
- Artefatos atualizados:
  - tasks/prd-modulo-estoque/4_task.md (checklist e status)
  - tasks/prd-modulo-estoque/4_task_review.md (este relatório)
