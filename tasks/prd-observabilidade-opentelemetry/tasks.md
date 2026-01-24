# Resumo de Tarefas de Implementação de Observabilidade com OpenTelemetry

## Visão Geral

Este documento organiza as tarefas para implementação de observabilidade distribuída no sistema GestAuto utilizando OpenTelemetry. A solução instrumentará 4 aplicações (`commercial`, `stock`, `vehicle-evaluation`, `frontend`) para gerar traces e logs correlacionados.

**Duração Estimada Total:** ~7.5 dias

## Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         FASE 1: INFRAESTRUTURA                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    1.0 Validação de Infraestrutura              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└───────────────────────────────────┬─────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      FASE 2: INSTRUMENTAÇÃO BACKEND                     │
│                                                                          │
│  ┌─────────────────┐                                                    │
│  │ 2.0 commercial  │─────────────────┐                                  │
│  │     (.NET)      │                 │                                  │
│  └────────┬────────┘                 │                                  │
│           │                          ▼                                  │
│           │              ┌─────────────────────────┐                   │
│           └─────────────►│      3.0 stock-api      │                   │
│                          │        (.NET)           │                   │
│                          │   (cópia commercial)    │                   │
│                          └─────────────────────────┘                   │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │             4.0 vehicle-evaluation (Java/Spring)                │   │
│  │                    (paralelo com 2.0)                           │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└───────────────────────────────────┬─────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                       FASE 3: INSTRUMENTAÇÃO FRONTEND                   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      5.0 frontend (React)                       │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└───────────────────────────────────┬─────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        FASE 4: VALIDAÇÃO E DOCS                         │
│                                                                          │
│  ┌─────────────────┐              ┌─────────────────────────────────┐  │
│  │  6.0 Testes E2E │              │    7.0 Dashboards & Docs        │  │
│  │                 │──────────────►│        (após E2E)               │  │
│  └─────────────────┘              └─────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

## Trilhas Paralelas

| Trilha | Tarefas | Observação |
|--------|---------|------------|
| **Trilha .NET** | 1.0 → 2.0 → 3.0 | Sequencial (3.0 copia padrão de 2.0) |
| **Trilha Java** | 1.0 → 4.0 | Paralela com Trilha .NET |
| **Trilha Frontend** | 2.0, 3.0, 4.0 → 5.0 | Depende de pelo menos um backend |
| **Trilha Validação** | 5.0 → 6.0 → 7.0 | Sequencial final |

## Tarefas

- [ ] 1.0 Validação de Infraestrutura e Conectividade OTel
- [ ] 2.0 Instrumentação OpenTelemetry - commercial-api (.NET)
- [ ] 3.0 Instrumentação OpenTelemetry - stock-api (.NET)
- [ ] 4.0 Instrumentação OpenTelemetry - vehicle-evaluation (Java/Spring)
- [ ] 5.0 Instrumentação OpenTelemetry - frontend (React)
- [ ] 6.0 Testes de Integração E2E e Validação de Traces
- [ ] 7.0 Dashboards Grafana e Documentação Operacional

## Caminho Crítico

```
1.0 → 2.0 → 5.0 → 6.0 → 7.0
```

O caminho crítico passa pela instrumentação do commercial-api (que serve de template), frontend (que depende de backends), e validação final.

## Estimativas por Tarefa

| Tarefa | Duração | Paralelizável |
|--------|---------|---------------|
| 1.0 | 0.5 dia | Não (primeira) |
| 2.0 | 2 dias | Sim (com 4.0) |
| 3.0 | 0.5 dia | Não (após 2.0) |
| 4.0 | 2 dias | Sim (com 2.0) |
| 5.0 | 1.5 dias | Não (após backends) |
| 6.0 | 1 dia | Não (após 5.0) |
| 7.0 | 0.5 dia | Não (final) |
