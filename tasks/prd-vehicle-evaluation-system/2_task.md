## markdown

## status: completed

<task_context>
<domain>engine/infra</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
</task_context>

# Tarefa 2.0: Implementação do Domínio Puro e Schema do Banco

## Visão Geral

Implementar as entidades de domínio puras (sem JPA) following Repository Pattern com Mappers, e criar o schema completo no PostgreSQL através de migrations Flyway. Esta tarefa estabelece a fundação do modelo de domínio e persistência.

<requirements>
- Entidades de domínio puras (sem annotations JPA)
- Value Objects imutáveis (EvaluationId, Plate, Money)
- Enums para status e tipos
- Schema vehicle_evaluation no PostgreSQL
- Todas as tabelas com relacionamentos e índices
- Repository interfaces no domínio
- Entidades JPA separadas na infraestrutura

</requirements>

## Subtarefas

- [x] 2.1 Criar Value Objects (EvaluationId, Plate, Money, VehicleInfo)
- [x] 2.2 Implementar entidade VehicleEvaluation (domínio puro)
- [x] 2.3 Implementar entidade EvaluationChecklist (domínio puro)
- [x] 2.4 Implementar entidades de suporte (Photo, DepreciationItem, Accessory)
- [x] 2.5 Criar enums (EvaluationStatus, PhotoType, ChecklistSection)
- [x] 2.6 Definir interfaces de Repository no domínio
- [x] 2.7 Implementar entidades JPA na infraestrutura
- [x] 2.8 Criar migrations Flyway V1-V8 (todas as tabelas)
- [x] 2.9 Implementar exceções de domínio customizadas

## Detalhes de Implementação

### Entidades de Domínio (Puras)

```java
// VehicleEvaluation - Sem annotations JPA
public class VehicleEvaluation {
    private final EvaluationId id;
    private final EvaluatorId evaluatorId;
    private final Plate plate;
    private final FipeCode fipeCode;
    private final VehicleInfo vehicleInfo;
    private final Money mileage;
    private EvaluationStatus status;
    // ... outros campos
    private final List<DomainEvent> domainEvents;

    // Factory methods e regras de negócio
    public static VehicleEvaluation create(...) { ... }
    public void addPhoto(...) { ... }
    public void submitForApproval() { ... }
    public void approve(...) { ... }
}
```

### Schema Database

```sql
-- vehicle_evaluations
CREATE TABLE vehicle_evaluation.vehicle_evaluations (
    id BIGSERIAL PRIMARY KEY,
    evaluation_id UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
    evaluator_id UUID NOT NULL,
    plate VARCHAR(7) UNIQUE NOT NULL,
    -- ... outros campos
    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT'
);

-- evaluation_photos
-- evaluation_checklists
-- depreciation_items
-- evaluation_accessories
-- checklist_critical_issues
```

### Repository Pattern

```java
// Interface no domínio
public interface VehicleEvaluationRepository {
    Optional<VehicleEvaluation> findById(EvaluationId id);
    VehicleEvaluation save(VehicleEvaluation evaluation);
    // ... métodos de busca
}

// Implementação na infra com JPA
@Repository
public class VehicleEvaluationRepositoryImpl implements VehicleEvaluationRepository {
    private final VehicleEvaluationJpaRepository jpaRepository;
    private final VehicleEvaluationMapper mapper;
    // implementação com conversão via mapper
}
```

## Critérios de Sucesso

- [x] Todas as entidades de domínio implementadas sem JPA
- [x] Value Objects são imutáveis e validados
- [x] Regras de negócio nas entidades (transições de status)
- [x] Schema criado com todas as tabelas e relacionamentos
- [x] Migrations executam sem erros
- [x] Índices criados para performance
- [x] Repository interfaces definidas no domínio
- [x] Entidades JPA criadas separadamente
- [x] Validções de domínio funcionando

## Sequenciamento

- Bloqueado por: 1.0 (Configuração inicial)
- Desbloqueia: 3.0, 4.0, 5.0, 6.0, 7.0
- Paralelizável: Não

## Tempo Estimado

- Entidades de domínio: 8 horas
- Migrations Flyway: 4 horas
- Repository pattern setup: 4 horas
- Total: 16 horas

## Conclusão da Tarefa

- [x] 2.0 Implementação do Domínio Puro e Schema do Banco ✅ CONCLUÍDA
  - [x] 2.1 Implementação completada
  - [x] 2.2 Definição da tarefa, PRD e tech spec validados
  - [x] 2.3 Análise de regras e conformidade verificadas
  - [x] 2.4 Revisão de código completada
  - [x] 2.5 Pronto para deploy
