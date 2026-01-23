# Resumo de Tarefas de Implementação — Cadastro de Veículo + Check-in (MVP)

## Visão Geral

Este documento lista as tarefas necessárias para implementar o fluxo unificado de cadastro de veículo com check-in no módulo de estoque. A implementação foca em:
- Nova página de frontend com UX unificada
- Orquestração de chamadas aos endpoints existentes
- Validação de `EvaluationId` para seminovos
- Testes unitários e de integração

**Estimativa Total:** ~19h de desenvolvimento

## Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          TRILHA BACKEND (Paralela)                      │
│  ┌─────────┐                                                            │
│  │  1.0    │ Testes Unitários (Cenário Seminovo)                       │
│  └────┬────┘                                                            │
│       │                                                                 │
│       ▼                                                                 │
│  ┌─────────┐                                                            │
│  │  2.0    │ Testes de Integração                                      │
│  └─────────┘                                                            │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         TRILHA FRONTEND (Sequencial)                    │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐                           │
│  │  3.0    │────▶│  4.0    │────▶│  5.0    │                           │
│  │ Página  │     │ Origin  │     │Category │                           │
│  └─────────┘     └─────────┘     └─────────┘                           │
│       │                               │                                 │
│       │         ┌─────────────────────┘                                 │
│       ▼         ▼                                                       │
│  ┌─────────────────┐     ┌─────────┐                                   │
│  │      6.0        │────▶│  7.0    │                                   │
│  │ DynamicForm     │     │ Service │                                   │
│  └─────────────────┘     └─────────┘                                   │
│                               │                                         │
│                               ▼                                         │
│                          ┌─────────┐                                   │
│                          │  8.0    │                                   │
│                          │ Rotas   │                                   │
│                          └─────────┘                                   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        TRILHA QUALIDADE (Final)                         │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐                           │
│  │  9.0    │────▶│ 10.0    │────▶│ 11.0    │                           │
│  │ Testes  │     │ A11y    │     │  Docs   │                           │
│  │ Vitest  │     │         │     │         │                           │
│  └─────────┘     └─────────┘     └─────────┘                           │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tarefas

### Fase 1: Backend — Testes (Paralela ao Frontend)

- [ ] 1.0 Implementar testes unitários para cenário de seminovo
- [ ] 2.0 Implementar testes de integração para fluxo completo

### Fase 2: Frontend — Estrutura Base

- [ ] 3.0 Criar página VehicleCheckInPage (estrutura base)
- [ ] 4.0 Implementar componente OriginSelector
- [ ] 5.0 Implementar componente CategorySelector

### Fase 3: Frontend — Formulário e Serviço

- [ ] 6.0 Implementar DynamicVehicleForm com campos por categoria
- [ ] 7.0 Implementar serviço de orquestração e validação de avaliação
- [ ] 8.0 Configurar rotas e navegação

### Fase 4: Qualidade e Documentação

- [ ] 9.0 Criar testes Vitest para VehicleCheckInPage
- [ ] 10.0 Garantir acessibilidade e refinamentos de UX
- [ ] 11.0 Atualizar documentação do módulo de estoque

## Análise de Paralelização

| Trilha | Tarefas | Pode executar em paralelo com |
|--------|---------|-------------------------------|
| Backend | 1.0, 2.0 | Todas as tarefas de Frontend |
| Frontend Base | 3.0 → 4.0 → 5.0 | Backend |
| Frontend Form | 6.0 → 7.0 → 8.0 | Backend |
| Qualidade | 9.0 → 10.0 → 11.0 | Nenhuma (depende de todas) |

## Caminho Crítico

```
3.0 → 4.0 → 5.0 → 6.0 → 7.0 → 8.0 → 9.0 → 10.0 → 11.0
```

**Tempo estimado do caminho crítico:** ~17h
