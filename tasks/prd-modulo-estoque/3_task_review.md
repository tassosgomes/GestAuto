# Revisão da Tarefa 3.0 — Persistência (EF Core/Postgres) + migrações

## 1) Validação da Definição da Tarefa (tarefa → PRD → techspec)

### Requisitos da tarefa ([tasks/prd-modulo-estoque/3_task.md](tasks/prd-modulo-estoque/3_task.md))
- Schema `stock`: implementado no `DbContext` e refletido nas migrações.
- Tabelas `vehicles`, `check_ins`, `check_outs`, `reservations`, `test_drives`: mapeadas via `IEntityTypeConfiguration<>` e criadas na migration inicial.
- Constraint: **reserva ativa única por veículo**: índice único parcial em `reservations(vehicle_id)` com filtro `status = 'ativa'`.
- Índices para consultas principais (status, category, vin, plate): criados nos mappings/migration.
- Preparar `outbox_messages`: entidade + configuração + tabela na migration inicial.

### Alinhamento com PRD ([tasks/prd-modulo-estoque/prd.md](tasks/prd-modulo-estoque/prd.md))
- “Fonte única de verdade do status vigente”: persistência armazena o status atual no agregado `Vehicle` e histórico em tabelas separadas (check-in/out/test-drive).
- Reserva como mecanismo de exclusividade comercial: persistência suporta reserva e garante exclusividade (1 ativa por veículo) no banco.
- Rastreabilidade/histórico: check-ins, check-outs e test-drives persistidos como registros de histórico vinculados ao veículo.

### Alinhamento com Tech Spec ([tasks/prd-modulo-estoque/techspec.md](tasks/prd-modulo-estoque/techspec.md))
- EF Core + PostgreSQL (Npgsql): utilizado.
- Clean Architecture: interfaces de repositório na camada Domain e implementação na Infra.
- Outbox: tabela `outbox_messages` já definida (processamento/publicação fica para tarefas posteriores).
- Migrações + aplicação automática: migrations criadas e aplicação automática no startup adicionada com lock advisory (padrão alinhado ao serviço Comercial).

## 2) Análise de Regras e Conformidade

Regras consideradas relevantes para os arquivos alterados:
- [rules/dotnet-architecture.md](rules/dotnet-architecture.md)
  - Interfaces no Domain e implementações na Infra (Repository Pattern): atendido.
  - Organização por camadas (Clean Architecture): atendido.
- [rules/dotnet-testing.md](rules/dotnet-testing.md)
  - Teste de integração com `WebApplicationFactory` e Postgres via Testcontainers: atendido.
  - Testes devem ser determinísticos e lidar com indisponibilidade de Docker: implementado mecanismo de skip quando Testcontainers não estiver disponível.
- [rules/dotnet-coding-standards.md](rules/dotnet-coding-standards.md)
  - Código em inglês (nomes de classes/métodos): atendido.
  - Evitar comentários desnecessários e manter métodos claros: atendido.
- [rules/dotnet-folders.md](rules/dotnet-folders.md)
  - Estrutura em `1-Services/2-Application/3-Domain/4-Infra/5-Tests`: já existente e preservada.
- [rules/restful.md](rules/restful.md)
  - Sem alteração de endpoints nesta tarefa; regra relevante para tarefas seguintes (handlers/controllers) especialmente para mapear violações de constraint para ProblemDetails.

## 3) Resumo da Revisão de Código

### Principais decisões
- `StockDbContext` com `HasDefaultSchema("stock")` e `ApplyConfigurationsFromAssembly(...)` para manter mapeamentos por entidade e escaláveis.
- Concurrency via `xmin` (Npgsql) como token de concorrência otimista.
- Índices/constraints alinhados ao PRD/techspec:
  - `ux_vehicles_vin` (VIN único)
  - `ux_vehicles_plate` (placa única com filtro `plate IS NOT NULL AND status NOT IN ('vendido','baixado')`)
  - `ux_reservations_vehicle_active` (reserva ativa única por veículo)
- Aplicação automática de migrations no startup com `pg_advisory_lock` para evitar corrida em cenários multi-instância.
- Repositórios EF mínimos (`Vehicle/Reservation/TestDrive`) com DI na Infra.

### Qualidade e manutenção
- Mapeamentos usam `HasConversion` com métodos helper estáticos para evitar limitações de expression trees do EF.
- Migrations versionadas em `4-Infra` e aplicadas em runtime no `API`.

## 4) Problemas encontrados e correções

- Erros de build em conversões EF (expression tree não suportava `switch`/`throw` dentro de lambdas): corrigido movendo conversões para métodos estáticos usados pelo `HasConversion`.
- Geração de migrations: ajustado para suportar criação via design-time `DbContextFactory` na Infra.
- Teste de integração dependente de Postgres: criado fixture com Testcontainers e `CustomWebApplicationFactory` para injetar connection string e registrar `DbContext` para o ambiente de testes.

## 5) Validação (build/test) e prontidão

### Comandos executados
- `dotnet test services/stock/GestAuto.Stock.sln -c Release`
  - Resultado: sucesso (13 testes passados).

### Avisos conhecidos
- Aviso CS0618 em `UseXminAsConcurrencyToken` (Npgsql): permanece.
  - Impacto: build não quebra; mas a abordagem está marcada como obsoleta. Recomenda-se avaliar migração para `IsRowVersion()`/`[Timestamp]` quando o modelo de concorrência do serviço estiver consolidado.

### Pronto para deploy?
- Sim para o escopo desta tarefa (persistência, migrations e validação por testes).
- Observação: o mapeamento de violações de constraint para erro de negócio (ProblemDetails 409/422) depende de handlers/controllers, que não fazem parte desta tarefa e deve ser garantido nas tarefas subsequentes.
