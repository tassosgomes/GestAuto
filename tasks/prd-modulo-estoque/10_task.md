---
status: completed
parallelizable: false
blocked_by: ["4.0", "5.0", "6.0", "7.0", "8.0", "9.0"]
---

<task_context>
<domain>testing/integration</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database|temporal|http_server</dependencies>
<unblocks>""</unblocks>
</task_context>

# Tarefa 10.0: Testes (unitários + integração) e validações de contrato

## Visão Geral
Consolidar a suíte de testes do serviço Stock com foco em regras críticas (concorrência de reserva, expiração automática, RBAC, check-in por categoria, publicação via outbox), incluindo testes de integração com Postgres (e opcionalmente RabbitMQ) via Testcontainers.

## Requisitos
- Testes unitários para handlers principais (já previstos nas tarefas 5–9).
- Testes de integração cobrindo:
  - Reserva ativa única sob concorrência.
  - Expiração automática de reserva `padrao`.
  - Check-in por categoria (novo/seminovo/demonstração) com validações obrigatórias.
  - Check-out por venda/baixa atualizando status corretamente.
- Se Docker não estiver disponível, testes de integração devem ser **skipados** de forma limpa (conforme regras do repo).

## Subtarefas
- [x] 10.1 Criar infraestrutura de testes (fixtures Testcontainers: Postgres; RabbitMQ opcional)
- [x] 10.2 Implementar testes de integração para endpoints principais (HTTP)
- [x] 10.3 Implementar teste de concorrência (duas requisições simultâneas de reserva)
- [x] 10.4 Implementar teste do job de expiração (simular tempo/forçar execução)
- [x] 10.5 Validar publicação no outbox (e, se aplicável, consumo no RabbitMQ)
- [x] 10.6 Criar um Dockerfile e adicionar no docker-compose
- [x] 10.7 Realizar testes basicos de conectividade no container um request em uma rota de health-check e em alguma rota que acesse o banco de dados

## Sequenciamento
- Bloqueado por: 4.0, 5.0, 6.0, 7.0, 8.0, 9.0
- Desbloqueia: nenhum
- Paralelizável: Não

## Detalhes de Implementação
- Reaproveitar padrão de fixtures do serviço Comercial (`services/commercial/5-Tests/Shared`).
- Preferir testes determinísticos; evitar dependência em relógio real quando possível.

## Critérios de Sucesso
- Suíte de testes executa localmente e no CI.
- Regras críticas do PRD/Tech Spec têm cobertura.
- Falhas retornam ProblemDetails coerentes (códigos e mensagens acionáveis).
