---
status: completed
parallelizable: true
blocked_by: ["1.0", "2.0"]
---

<task_context>
<domain>infra/persistence</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>"4.0, 5.0, 6.0, 7.0, 8.0, 9.0"</unblocks>
</task_context>

# Tarefa 3.0: Persistência (EF Core/Postgres) + migrações

## Visão Geral
Implementar a persistência do serviço Stock com EF Core e PostgreSQL: `DbContext`, mappings/configurations, repositórios base e migrações. Garantir constraints críticas (unicidade de VIN/placa conforme aplicável e reserva ativa única por veículo).

## Requisitos
- Schema `stock`.
- Tabelas para `vehicles`, `check_ins`, `check_outs`, `reservations`, `test_drives`.
- Constraint: **reserva ativa única por veículo** (índice único parcial) e suporte a concorrência.
- Índices para consultas principais (status, category, vin, plate).
- Preparar `outbox_messages` (se não for criado na 4.0, deixar definido aqui).

## Subtarefas
- [x] 3.1 Criar `StockDbContext` com schema `stock` e `DbSet<>` necessários
- [x] 3.2 Criar `IEntityTypeConfiguration<>` para cada entidade e aplicar via assembly scanning
- [x] 3.3 Definir constraints/índices (VIN/placa; reserva ativa única)
- [x] 3.4 Configurar migrations e aplicação automática em startup (alinhado ao padrão do repo)
- [x] 3.5 Implementar repositórios EF (Vehicle/Reservation/TestDrive) com métodos mínimos

## Status
- [x] 3.0 Persistência (EF Core/Postgres) + migrações ✅ CONCLUÍDA
	- [x] 3.1 Implementação completada
	- [x] 3.2 Definição da tarefa, PRD e tech spec validados
	- [x] 3.3 Análise de regras e conformidade verificadas
	- [x] 3.4 Revisão de código completada
	- [x] 3.5 Pronto para deploy

## Sequenciamento
- Bloqueado por: 1.0, 2.0
- Desbloqueia: 4.0, 5.0, 6.0, 7.0, 8.0, 9.0
- Paralelizável: Sim

## Detalhes de Implementação
- Para reserva ativa única: usar índice parcial Postgres (`WHERE status = 'ativa'`).
- Tratar concorrência em nível de handler: ao violar constraint, retornar erro de negócio (409 ou 400) via ProblemDetails.

## Critérios de Sucesso
- Migrações aplicam com sucesso em Postgres.
- Operações básicas de CRUD funcionam via repositórios.
- Constraint de reserva ativa impede duplicidade mesmo sob concorrência.
