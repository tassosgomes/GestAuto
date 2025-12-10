## markdown

## status: pending

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
</task_context>

# Tarefa 5.0: Implementação de Checklist Técnico

## Visão Geral

Implementar checklist técnico padronizado com seções específicas (lataria, pneus, interior, mecânica, eletrônica, documentos), validações por seção, identificação de itens críticos bloqueantes, e cálculo automático de score de conservação (0-100).

<requirements>
- 6 seções obrigatórias no checklist
- Validações específicas por seção
- Itens críticos que impossibilitam aprovação
- Cálculo automático de score de conservação
- Armazenamento em JSONB no PostgreSQL
- Interface para preenchimento progressivo
- Observações detalhadas por item
- Resumo automático de problemas

</requirements>

## Subtarefas

- [ ] 5.1 Criar DTOs para cada seção do checklist
- [ ] 5.2 Implementar UpdateChecklistCommand e Handler
- [ ] 5.3 Criar validações específicas por seção
- [ ] 5.4 Implementar lógica de cálculo de score
- [ ] 5.5 Definir itens críticos bloqueantes
- [ ] 5.6 Implementar endpoint PUT /api/v1/evaluations/{id}/checklist
- [ ] 5.7 Criar mapeamento para JSONB
- [ ] 5.8 Adicionar validações de integridade
- [ ] 5.9 Implementar resumo automático

## Detalhes de Implementação

### Estrutura do Checklist

```java
public class EvaluationChecklist {
    private final ChecklistId id;
    private final EvaluationId evaluationId;
    private BodyworkChecklist bodywork;
    private TiresChecklist tires;
    private InteriorChecklist interior;
    private MechanicalChecklist mechanical;
    private ElectronicsChecklist electronics;
    private DocumentsChecklist documents;
    private Integer conservationScore;
    private final List<String> criticalIssues;

    // Seções específicas
    public static class BodyworkChecklist {
        private Boolean frontBumper;
        private Boolean rearBumper;
        private Boolean leftFender;
        private Boolean rightFender;
        private Boolean doors;
        private Boolean roof;
        private Boolean hasRust;
        private Boolean hasDents;
        private String observations;

        public int calculatePenalty() {
            int penalty = 0;
            if (hasRust) penalty += 20;
            if (hasDents) penalty += 15;
            // ... outras regras
            return penalty;
        }
    }
}
```

### DTOs de Request

```java
public record EvaluationChecklistDto(
    BodyworkDto bodywork,
    TiresDto tires,
    InteriorDto interior,
    MechanicalDto mechanical,
    ElectronicsDto electronics,
    DocumentsDto documents
) {
    public record BodyworkDto(
        Boolean frontBumperCondition,
        Boolean rearBumperCondition,
        Boolean leftFenderCondition,
        Boolean rightFenderCondition,
        Boolean doorsCondition,
        Boolean roofCondition,
        Boolean hasRust,
        Boolean hasSignificantDents,
        String observations
    ) {}

    // ... outros DTOs para cada seção
}
```

### Handler do Checklist

```java
@Component
public class UpdateChecklistHandler implements CommandHandler<UpdateChecklistCommand, Void> {
    private final VehicleEvaluationRepository evaluationRepository;
    private final EvaluationChecklistRepository checklistRepository;

    @Override
    @Transactional
    public Void handle(UpdateChecklistCommand command) {
        // 1. Buscar avaliação
        // 2. Validar status (pode editar)
        // 3. Mapear DTO para entidades de checklist
        // 4. Validar itens críticos
        // 5. Calcular score de conservação
        // 6. Salvar checklist
        // 7. Atualizar avaliação
        // 8. Publicar ChecklistCompletedEvent

        // Validação de itens críticos
        if (checklist.hasBlockingIssues()) {
            throw new BusinessException("Checklist has blocking issues that prevent approval");
        }

        return null;
    }
}
```

### Itens Críticos Bloqueantes

```java
public class CriticalIssuesValidator {
    public List<String> validateCriticalIssues(EvaluationChecklist checklist) {
        List<String> issues = new ArrayList<>();

        // Documentação crítica
        if (!checklist.getDocuments().hasValidDocuments()) {
            issues.add("Missing or invalid vehicle documents");
        }

        // Problemas mecânicos críticos
        if (checklist.getMechanical().hasEngineProblems()) {
            issues.add("Engine has significant issues");
        }

        // Problemas estruturais
        if (checklist.getBodywork().hasStructuralDamage()) {
            issues.add("Structural damage detected");
        }

        return issues;
    }
}
```

### Cálculo de Score

```java
public class ConservationScoreCalculator {
    public int calculateScore(EvaluationChecklist checklist) {
        int baseScore = 100;

        // Penalidades por seção
        baseScore -= checklist.getBodywork().getPenaltyPoints();
        baseScore -= checklist.getTires().getPenaltyPoints();
        baseScore -= checklist.getInterior().getPenaltyPoints();
        baseScore -= checklist.getMechanical().getPenaltyPoints();
        baseScore -= checklist.getElectronics().getPenaltyPoints();
        baseScore -= checklist.getDocuments().getPenaltyPoints();

        return Math.max(0, baseScore);
    }
}
```

## Critérios de Sucesso

- [x] Checklist pode ser preenchido por seções
- [x] Validações específicas por seção funcionando
- [x] Itens críticos identificados corretamente
- [x] Score de conservação calculado (0-100)
- [x] Resumo automático gerado
- [x] Dados persistidos em JSONB
- [x] Histórico de alterações mantido
- [x] Interface progressiva (salvar parcial)
- [x] Regras de negócio aplicadas

## Sequenciamento

- Bloqueado por: 2.0 (Domínio e Database)
- Desbloqueia: 6.0, 7.0
- Paralelizável: Sim (com 3.0 e 4.0)

## Tempo Estimado

- DTOs e validações: 8 horas
- Lógica de negócio: 10 horas
- Mapeamento JSONB: 4 horas
- Testes: 6 horas
- Total: 28 horas