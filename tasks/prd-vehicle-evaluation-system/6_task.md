## markdown

## status: pending

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
</task_context>

# Tarefa 6.0: Implementação de Cálculo de Valoração

## Visão Geral

Implementar sistema automático de cálculo de valor baseado na tabela FIPE com aplicação de regras de depreciação configuráveis, margens de segurança e lucro, e ajuste manual limitado com aprovação gerencial.

<requirements>
- Integração com API FIPE para valor de mercado
- Tabela de depreciação por marca/modelo/ano
- Cálculo automático com margens configuráveis
- Aplicação de depreciações baseadas no checklist
- Ajuste manual limitado a 10% com aprovação
- Detalhamento completo do cálculo
- Cache FIPE com Redis (TTL 24h)
- Regras de negócio específicas

</requirements>

## Subtarefas

- [ ] 6.1 Implementar cliente FIPE API (RestTemplate/WebClient)
- [ ] 6.2 Criar serviço de cache com Redis para FIPE
- [ ] 6.3 Implementar regras de depreciação
- [ ] 6.4 Criar CalculateValuationCommand e Handler
- [ ] 6.5 Implementar lógica de cálculo com margens
- [ ] 6.6 Adicionar configurações de percentuais
- [ ] 6.7 Implementar ajuste manual limitado
- [ ] 6.8 Criar endpoint POST /api/v1/evaluations/{id}/calculate
- [ ] 6.9 Adicionar validações de negócio

## Detalhes de Implementação

### API FIPE Integration

```java
@Service
public class FipeServiceImpl implements FipeService {
    private final WebClient webClient;
    private final RedisTemplate<String, Object> redisTemplate;
    private static final String CACHE_PREFIX = "fipe:";

    @Override
    @Cacheable(value = "fipe-vehicle-info", key = "#fipeCode")
    public Optional<FipeVehicleInfo> getVehicleInfo(String fipeCode) {
        // 1. Tentar cache Redis
        // 2. Se não tiver, chamar API
        // 3. Salvar no cache por 24h
        // 4. Retornar informações
    }

    @Override
    @Cacheable(value = "fipe-price", key = "{#fipeCode, #year}")
    public Optional<BigDecimal> getCurrentPrice(String fipeCode, int year) {
        String cacheKey = String.format("%s:price:%s:%d", CACHE_PREFIX, fipeCode, year);
        // Implementation com retry e circuit breaker
    }
}
```

### Cálculo de Valoração

```java
@Service
public class ValuationService {
    private final FipeService fipeService;
    private final DepreciationRuleRepository depreciationRuleRepository;
    private final ValuationConfigRepository valuationConfigRepository;

    public ValuationResult calculateValuation(VehicleEvaluation evaluation) {
        // 1. Obter valor FIPE
        BigDecimal fipePrice = fipeService.getCurrentPrice(
            evaluation.getFipeCode().getValue(),
            evaluation.getVehicleInfo().getYear()
        ).orElseThrow(() -> new BusinessException("FIPE price not available"));

        // 2. Calcular depreciações
        BigDecimal totalDepreciation = calculateTotalDepreciation(evaluation);

        // 3. Aplicar margens
        ValuationConfig config = valuationConfigRepository.getActiveConfig();
        BigDecimal safetyMargin = fipePrice.multiply(
            BigDecimal.valueOf(config.getSafetyMarginPercentage()).divide(BigDecimal.valueOf(100))
        );
        BigDecimal profitMargin = fipePrice.multiply(
            BigDecimal.valueOf(config.getProfitMarginPercentage()).divide(BigDecimal.valueOf(100))
        );

        // 4. Calcular valor sugerido
        BigDecimal suggestedValue = fipePrice
            .subtract(totalDepreciation)
            .add(safetyMargin)
            .add(profitMargin);

        return new ValuationResult(
            fipePrice,
            totalDepreciation,
            safetyMargin,
            profitMargin,
            suggestedValue,
            getDepreciationDetails(evaluation)
        );
    }
}
```

### Regras de Depreciação

```java
@Entity
public class DepreciationRule {
    private String brand;
    private String modelPattern;
    private int minYear;
    private int maxYear;
    private BigDecimal ageDepreciation;
    private BigDecimal mileageDepreciationPer10k;
    private Map<String, BigDecimal> conditionDepreciation;

    public BigDecimal calculateDepreciation(VehicleEvaluation evaluation) {
        BigDecimal depreciation = BigDecimal.ZERO;

        // Depreciação por idade
        int vehicleAge = LocalDate.now().getYear() - evaluation.getVehicleInfo().getYear();
        depreciation = depreciation.add(ageDepreciation.multiply(BigDecimal.valueOf(vehicleAge)));

        // Depreciação por quilometragem
        int mileage = evaluation.getMileage().getValue().intValue();
        if (mileage > 10000) {
            int extra10k = (mileage - 10000) / 10000;
            depreciation = depreciation.add(
                mileageDepreciationPer10k.multiply(BigDecimal.valueOf(extra10k))
            );
        }

        // Depreciação por condições do checklist
        if (evaluation.getChecklist() != null) {
            depreciation = depreciation.add(
                calculateConditionDepreciation(evaluation.getChecklist())
            );
        }

        return depreciation;
    }
}
```

### Handler de Cálculo

```java
@Component
public class CalculateValuationHandler implements CommandHandler<CalculateValuationCommand, ValuationResultDto> {
    private final VehicleEvaluationRepository evaluationRepository;
    private final ValuationService valuationService;
    private final AuditService auditService;

    @Override
    @Transactional
    public ValuationResultDto handle(CalculateValuationCommand command) {
        // 1. Buscar avaliação
        VehicleEvaluation evaluation = evaluationRepository.findById(
            EvaluationId.from(command.evaluationId())
        ).orElseThrow(() -> new EntityNotFoundException("Evaluation not found"));

        // 2. Validar status
        if (!canCalculate(evaluation.getStatus())) {
            throw new BusinessException("Cannot calculate valuation in current status");
        }

        // 3. Calcular valoração
        ValuationResult result = valuationService.calculateValuation(evaluation);

        // 4. Atualizar avaliação
        evaluation.calculateValuation(result);

        // 5. Salvar
        evaluationRepository.save(evaluation);

        // 6. Auditoria
        auditService.log("valuation_calculated", evaluation.getId(), result);

        // 7. Publicar evento
        eventPublisher.publish(new ValuationCalculatedEvent(
            evaluation.getId(),
            result.getFipePrice(),
            result.getSuggestedValue()
        ));

        return ValuationResultDto.from(result);
    }
}
```

### Configurações

```yaml
app:
  valuation:
    safety-margin-percentage: ${SAFETY_MARGIN:10}
    profit-margin-percentage: ${PROFIT_MARGIN:15}
    max-manual-adjustment: ${MAX_ADJUSTMENT:10}
    require-manager-approval: ${REQUIRE_APPROVAL:true}
```

## Critérios de Sucesso

- [x] Integração com API FIPE funcionando
- [x] Cache Redis com TTL 24h operacional
- [x] Regras de depreciação aplicadas corretamente
- [x] Margens configuráveis funcionando
- [x] Cálculo detalhado exibido ao usuário
- [x] Ajuste manual limitado a 10%
- [x] Histórico de valorações mantido
- [x] Fallback para falhas da API FIPE
- [x] Performance < 2s por cálculo

## Sequenciamento

- Bloqueado por: 2.0, 3.0, 5.0
- Desbloqueia: 7.0
- Paralelizável: Sim (com 7.0)

## Tempo Estimado

- API FIPE integration: 8 horas
- Cache e configurações: 4 horas
- Lógica de negócio: 10 horas
- Testes: 6 horas
- Total: 28 horas