# Resumo de Tarefas de Implementação - Módulo Comercial GestAuto

## Visão Geral

Este documento apresenta o plano de implementação do **Módulo Comercial** do GestAuto, um microserviço responsável pelo fluxo de vendas em concessionárias de veículos.

**Estimativa total:** ~28 dias (5-6 semanas)

## Análise de Paralelização

### Caminho Crítico
```
1.0 → 2.0 → 3.0 → 4.0 → 5.0 → 6.0 → 7.0 → 8.0 → 9.0 → 10.0 → 11.0 → 12.0
```

### Lanes de Execução Paralela

| Lane | Tarefas | Descrição |
|------|---------|-----------|
| **Lane A (Core)** | 1.0 → 2.0 → 3.0 → 4.0 | Infraestrutura e Domain Layer |
| **Lane B (Leads)** | 5.0 → 6.0 | Application e API de Leads |
| **Lane C (Propostas)** | 7.0 → 8.0 | Application e API de Propostas |
| **Lane D (Messaging)** | 9.0 | Outbox Pattern e RabbitMQ |
| **Lane E (Secundários)** | 10.0, 11.0 | Test-Drives, Avaliações e Consumers |
| **Lane F (Qualidade)** | 12.0, 13.0 | Testes de Integração e Documentação |

### Oportunidades de Paralelização

- **Após 4.0:** Tarefas 5.0, 7.0 e 9.0 podem iniciar em paralelo
- **Após 6.0:** Tarefas 10.0 e 11.0 podem executar em paralelo
- **Após 11.0:** Tarefas 12.0 e 13.0 podem executar em paralelo

---

## Tarefas

### Fase 1: Fundação (Infraestrutura + Domain)

- [ ] 1.0 Configurar Infraestrutura Base do Projeto
- [ ] 2.0 Implementar Domain Layer - Entidades Core
- [ ] 3.0 Implementar Domain Layer - Value Objects e Enums
- [ ] 4.0 Implementar Infra Layer - Repositórios e Persistência

### Fase 2: Fluxo de Leads

- [ ] 5.0 Implementar Application Layer - Leads (Commands/Queries)
- [ ] 6.0 Implementar API Layer - Leads Controller

### Fase 3: Fluxo de Propostas

- [ ] 7.0 Implementar Application Layer - Propostas (Commands/Queries)
- [ ] 8.0 Implementar API Layer - Propostas Controller

### Fase 4: Mensageria e Eventos

- [ ] 9.0 Implementar Outbox Pattern e RabbitMQ Publisher

### Fase 5: Fluxos Secundários

- [ ] 10.0 Implementar Fluxo de Test-Drives
- [ ] 11.0 Implementar Fluxo de Avaliações e Consumers

### Fase 6: Qualidade e Documentação

- [ ] 12.0 Implementar Testes de Integração e E2E
- [ ] 13.0 Criar Documentação OpenAPI e AsyncAPI

---

## Dependências Visuais

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FASE 1: FUNDAÇÃO                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌───────┐    ┌───────┐    ┌───────┐    ┌───────┐                          │
│   │  1.0  │───▶│  2.0  │───▶│  3.0  │───▶│  4.0  │                          │
│   │ Infra │    │ Entid.│    │ V.Obj │    │ Repos │                          │
│   └───────┘    └───────┘    └───────┘    └───┬───┘                          │
│                                              │                              │
└──────────────────────────────────────────────┼──────────────────────────────┘
                                               │
              ┌────────────────────────────────┼────────────────────────────┐
              ▼                                ▼                            ▼
┌─────────────────────────┐    ┌─────────────────────────┐    ┌─────────────────┐
│    FASE 2: LEADS        │    │   FASE 3: PROPOSTAS     │    │ FASE 4: MSG     │
├─────────────────────────┤    ├─────────────────────────┤    ├─────────────────┤
│ ┌───────┐    ┌───────┐  │    │ ┌───────┐    ┌───────┐  │    │    ┌───────┐    │
│ │  5.0  │───▶│  6.0  │  │    │ │  7.0  │───▶│  8.0  │  │    │    │  9.0  │    │
│ │ App   │    │ API   │  │    │ │ App   │    │ API   │  │    │    │Outbox │    │
│ └───────┘    └───┬───┘  │    │ └───────┘    └───┬───┘  │    │    └───┬───┘    │
└──────────────────┼──────┘    └──────────────────┼──────┘    └────────┼────────┘
                   │                              │                    │
                   └──────────────┬───────────────┴────────────────────┘
                                  ▼
              ┌─────────────────────────────────────────────────┐
              │            FASE 5: FLUXOS SECUNDÁRIOS           │
              ├─────────────────────────────────────────────────┤
              │      ┌───────┐              ┌───────┐           │
              │      │ 10.0  │              │ 11.0  │           │
              │      │ T.Drv │              │ Aval. │           │
              │      └───┬───┘              └───┬───┘           │
              └──────────┼──────────────────────┼───────────────┘
                         └──────────┬───────────┘
                                    ▼
              ┌─────────────────────────────────────────────────┐
              │         FASE 6: QUALIDADE E DOCUMENTAÇÃO        │
              ├─────────────────────────────────────────────────┤
              │      ┌───────┐              ┌───────┐           │
              │      │ 12.0  │              │ 13.0  │           │
              │      │ Testes│              │ Docs  │           │
              │      └───────┘              └───────┘           │
              └─────────────────────────────────────────────────┘
```

---

## Requisitos Técnicos

| Dependência | Status | Bloqueante? |
|-------------|--------|-------------|
| PostgreSQL (Docker) | A provisionar | Sim |
| RabbitMQ (Docker) | A provisionar | Sim |
| Logto | A configurar | Parcial (pode mockar) |
| Contratos AsyncAPI Seminovos | A definir | Não (pode simular) |

---

**Criado em:** 08/12/2025  
**Versão:** 1.0
