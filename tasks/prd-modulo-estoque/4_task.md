---
status: completed
parallelizable: true
blocked_by: ["1.0", "2.0", "3.0"]
---

<task_context>
<domain>infra/messaging</domain>
<type>implementation</type>
<scope>middleware</scope>
<complexity>high</complexity>
<dependencies>temporal|database</dependencies>
<unblocks>"9.0, 10.0"</unblocks>
</task_context>

# Tarefa 4.0: Outbox + RabbitMQ (eventos de domínio)

## Visão Geral
Implementar o padrão **Outbox** e publicação de eventos no RabbitMQ para o serviço Stock (exchange `stock`), garantindo consistência transacional e semântica at-least-once.

## Requisitos
- Registrar eventos no outbox na mesma transação do EF Core.
- Publicar eventos com `MessageId = EventId`.
- Exchange `stock` e routing keys definidas na Tech Spec.
- Processador de outbox em background com retry e marcação de falhas.
- Conexão RabbitMQ lazy (não bloquear startup).

## Subtarefas
- [x] 4.1 Criar entidade/tabela `outbox_messages` e repositório de outbox
- [x] 4.2 Implementar `IUnitOfWork` coletando Domain Events e gravando no outbox
- [x] 4.3 Implementar publisher RabbitMQ (`IEventPublisher`) e config (exchange `stock` + routing keys)
- [x] 4.4 Implementar `OutboxProcessorService` (BackgroundService) com retry/backoff
- [ ] 4.5 (Doc) Gerar/atualizar AsyncAPI do serviço Stock (se adotado no padrão)

## Status
- [x] 4.0 Outbox + RabbitMQ (eventos de domínio) ✅ CONCLUÍDA
	- [x] 4.1 Implementação completada
	- [x] 4.2 Definição da tarefa, PRD e tech spec validados
	- [x] 4.3 Análise de regras e conformidade verificadas
	- [x] 4.4 Revisão de código completada
	- [x] 4.5 Pronto para deploy

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 3.0
- Desbloqueia: 9.0, 10.0
- Paralelizável: Sim

## Detalhes de Implementação
- Espelhar a abordagem do serviço Comercial (serialization camelCase, `MessageId`, marcação processed/failed).
- Garantir logs estruturados com `eventId` e `routingKey`.

## Critérios de Sucesso
- Eventos de domínio geram registros no outbox e são publicados.
- Falha de RabbitMQ não impede a API de iniciar.
- Publicação é idempotente no nível de consumidor via `EventId`.
