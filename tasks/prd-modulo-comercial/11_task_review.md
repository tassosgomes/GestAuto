# Relatório de Revisão - Tarefa 11.0: Implementar Fluxo de Avaliações e Consumers

## Status: ❌ REPROVADA

**Data de Revisão:** 09 de Dezembro de 2025  
**Revisor:** GitHub Copilot  
**Branch:** feat/task-11-evaluation-flow

---

## Resumo Executivo

Revisão reprovada. O fluxo de avaliações e consumidores não atende requisitos críticos: a proposta não é atualizada com o valor da avaliação, o handler de resposta do cliente não valida estado nem atualiza proposta, os consumers não implementam política de retry/DLQ configurada e os endpoints violam o padrão de paginação definido em `rules/restful.md`. Testes de integração e cobertura específica (subtarefa 11.17) estão ausentes.

---

## 1. Validação da Definição da Tarefa

- **Cobertura de requisitos da tarefa:** Parcial. Commands/queries e controllers existem, mas o fluxo de negócios está incompleto.
- **PRD / Tech Spec:** Falha nos pontos de integração e consistência de proposta (F5.5/F5.6) e conformidade REST (paginação padrão `_page`/`_size`).
- **Critérios de sucesso:** Não atendidos para atualização automática da proposta, retries/DLQ e testes de integração.

### Principais lacunas vs requisitos
1) **Atualizar proposta com valor da avaliação automaticamente** (success criteria): não há atualização da proposta em nenhum ponto.
2) **Processamento idempotente + retry/DLQ**: consumers não aplicam política de retry e não declaram/delegam DLQ efetiva.
3) **APIs REST**: paginação usa `page/pageSize` em vez de `_page/_size` (regra obrigatória em `rules/restful.md`).
4) **Testes de integração (Testcontainers)**: inexistentes para o novo fluxo.

---

## 2. Análise de Regras (rules/*.md)

- **rules/restful.md**: exige parâmetros `_page`/`_size` e Problem Details; controllers usam `page`/`pageSize` → violação.
- **dotnet-architecture.md / dotnet-coding-standards.md**: estrutura geral ok (CQRS nativo, DI), mas faltam invariantes de domínio (estado da proposta e avaliação) e uso consistente de cancellation tokens em todos os repositórios/handlers (ex.: `RequestEvaluationHandler` chama `GetByIdAsync` sem token).
- **dotnet-testing.md**: ausência de novos testes unitários/integrados para handlers e consumers.
- **dotnet-observability.md**: consumers não implementam política de retry/backoff nem DLQ declarada; apenas nack direto sem requeue.

---

## 3. Revisão de Código

### Problemas Críticos (bloqueiam aprovação)
1) **Proposta não atualiza status/valor da avaliação** – `RequestEvaluationHandler` cria avaliação mas não vincula à proposta nem muda status para `AwaitingUsedVehicleEvaluation` (requisito 11.1/PRD F5.5). Nenhum ponto seta `UsedVehicleEvaluationId` ou `TradeInValue`.  
   Arquivo: `GestAuto.Commercial.Application/Handlers/RequestEvaluationHandler.cs`

2) **Handler de resposta do cliente incompleto** – `RegisterCustomerResponseHandler` não valida que a avaliação está `Completed`, não atualiza a proposta com o valor avaliado quando aceito e não persiste alterações da proposta (requisito 11.3 e critério “Proposta é atualizada quando cliente aceita valor”).  
   Arquivo: `GestAuto.Commercial.Application/Handlers/RegisterCustomerResponseHandler.cs`

3) **Consumer de avaliação não propaga efeito na proposta** – `UsedVehicleEvaluationRespondedConsumer` marca avaliação como `Completed` mas não atualiza a proposta (status ou trade-in). Critério “Avaliação é atualizada com valor e status” e “Atualizar proposta com valor da avaliação automaticamente” não atendidos.  
   Arquivo: `GestAuto.Commercial.Infra/Messaging/Consumers/UsedVehicleEvaluationRespondedConsumer.cs`

4) **Retry/DLQ não implementados de fato** – Consumers apenas `BasicNack` com requeue=false; não há política de retry/backoff, nem declaração/configuração da DLX `commercial.dlx`/`order-updated.failed`. Requisito 11.16 violado.  
   Arquivos: `UsedVehicleEvaluationRespondedConsumer.cs`, `OrderUpdatedConsumer.cs`

5) **Paginação fora do padrão de regras** – Endpoints usam `page/pageSize` e não suportam `_page/_size` conforme `rules/restful.md`.  
   Arquivos: `EvaluationController.cs`, `OrderController.cs`

6) **Testes ausentes** – Não há testes para novos handlers/consumers; apenas `RequestEvaluationHandlerTests` existente. Subtarefa 11.17 e critério de sucesso “Testes de integração com Testcontainers” não atendidos.  
   Pastas: `services/commercial/5-Tests/...` (nenhum novo teste para avaliação/consumers)

### Problemas Médios
7) **Idempotência parcial** – `UsedVehicleEvaluationRespondedConsumer` checa status `Completed` mas não armazena message-id/processamento; `OrderUpdatedConsumer` pode aplicar estados repetidos sem marcação de processamento. Considerar rastrear messageId ou versamento de status.  

8) **CancellationToken inconsistente** – `RequestEvaluationHandler` chama `_proposalRepository.GetByIdAsync` sem passar `cancellationToken`, divergindo do padrão (dotnet-coding-standards).  

### Problemas Baixos / Observações
9) **Paginação em memória em orders** – `ListOrdersHandler` carrega tudo em memória antes de paginar; pode degradar em volume alto (performance).  

10) **Sem atualização de LeadId/TotalValue em orders criados pelo consumer** – `OrderUpdatedConsumer` cria `Order` com `LeadId` vazio e `TotalValue` zero; pode deixar dados inconsistentes para exibição posterior (PRD F7.2/relatórios).  

---

## 4. Itens Endereçados

Nenhum problema foi corrigido nesta revisão. Todos os pontos acima permanecem pendentes para aprovação da tarefa.

---

## 5. Conclusão e Próximos Passos

A tarefa **não pode ser marcada como concluída** até que os itens críticos sejam corrigidos:
1) Atualizar Proposal ao solicitar avaliação (status `AwaitingUsedVehicleEvaluation`, vínculo `UsedVehicleEvaluationId`).
2) No consumer de avaliação, atualizar Proposal com valor avaliado/status; garantir idempotência e persistência transacional.
3) No handler de resposta do cliente, validar status `Completed`, atualizar Proposal com `TradeInValue` quando aceite e persistir.
4) Implementar retry/backoff e DLQ declarada para ambos os consumers.
5) Adequar paginação para `_page`/`_size` conforme `rules/restful.md` nos endpoints de avaliações e pedidos.
6) Adicionar testes unitários e de integração (Testcontainers) cobrindo handlers e consumers; executar e registrar resultados.

Após correções, reexecute a revisão, atualize checkboxes em `11_task.md` e gere novo review.

---

## Pedido de Revisão Final

Por favor, revise as pendências listadas e confirme quando as correções forem aplicadas para que possamos reavaliar e aprovar a tarefa.