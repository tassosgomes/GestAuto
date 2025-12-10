## markdown

## status: completed

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
</task_context>

# Tarefa 3.0: Implementação de Criação e Gestão de Avaliações

## Visão Geral

Implementar o fluxo completo de criação, atualização e consulta de avaliações de veículos, incluindo endpoints REST, commands/queries CQRS, handlers e validações. Esta funcionalidade permite aos avaliadores iniciar novas avaliações com dados básicos do veículo.

<requirements>
- Endpoint POST /api/v1/evaluations para criar avaliação
- Command/Query pattern CQRS
- Validação de placa única
- Busca automática de dados do veículo
- Endpoints de consulta GET /api/v1/evaluations/{id}
- Listagem de avaliações por avaliador e status
- Autenticação e autorização (roles: EVALUATOR, MANAGER, ADMIN)
- DTOs para request/response

</requirements>

## Subtarefas

- [x] 3.1 Criar DTOs (CreateEvaluationCommand, VehicleEvaluationDto)
- [x] 3.2 Implementar validadores (Bean Validation)
- [x] 3.3 Implementar CreateEvaluationCommand e Handler
- [x] 3.4 Implementar UpdateEvaluationCommand e Handler
- [x] 3.5 Implementar Queries (GetEvaluation, ListEvaluations)
- [x] 3.6 Implementar Controller REST VehicleEvaluationController
- [x] 3.7 Implementar mapper para VehicleEvaluation
- [x] 3.8 Adicionar testes unitários dos handlers
- [x] 3.9 Configurar segurança com @PreAuthorize

## Detalhes de Implementação

### Command Structure

```java
public record CreateEvaluationCommand(
    @NotBlank String plate,
    @NotNull Integer year,
    @Min(0) Integer mileage,
    String color,
    String version,
    String fuelType,
    String gearbox,
    List<String> accessories,
    String internalNotes
) {}

@Component
public class CreateEvaluationHandler implements CommandHandler<CreateEvaluationCommand, UUID> {
    private final VehicleEvaluationRepository repository;
    private final FipeService fipeService;
    private final DomainEventPublisher eventPublisher;

    @Override
    @Transactional
    public UUID handle(CreateEvaluationCommand command) {
        // 1. Validar placa única
        // 2. Obter informações FIPE
        // 3. Criar entidade VehicleEvaluation
        // 4. Salvar
        // 5. Publicar evento
        return evaluation.getId().getValue();
    }
}
```

### API Endpoints

```java
@RestController
@RequestMapping("/api/v1/evaluations")
@PreAuthorize("hasAnyRole('EVALUATOR', 'MANAGER', 'ADMIN')")
public class VehicleEvaluationController {

    @PostMapping
    public ResponseEntity<UUID> createEvaluation(@Valid @RequestBody CreateEvaluationCommand command) {
        UUID evaluationId = commandBus.execute(command);
        return ResponseEntity.created(URI.create("/api/v1/evaluations/" + evaluationId))
                             .body(evaluationId);
    }

    @GetMapping("/{id}")
    public ResponseEntity<VehicleEvaluationDto> getEvaluation(@PathVariable UUID id) {
        // Implementation
    }

    @GetMapping
    public ResponseEntity<PagedResult<VehicleEvaluationSummaryDto>> listEvaluations(
            @RequestParam(required = false) UUID evaluatorId,
            @RequestParam(required = false) EvaluationStatus status,
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "20") int size) {
        // Implementation
    }
}
```

### Validações de Negócio

- Placa deve ser única no sistema
- Ano do veículo não pode ser futuro
- KM não pode ser negativo
- Acessórios devem ser da lista pré-definida
- Somente usuários com role EVALUATOR+ podem criar

## Critérios de Sucesso

- [x] Endpoint de criação funciona com validações
- [x] Validação de placa duplicada funcionando
- [x] Integração com FIPE mockada para buscar dados
- [x] Avaliação é salva com status DRAFT
- [x] Evento EvaluationCreatedEvent publicado
- [x] Endpoint de detalhes retorna DTO completo
- [x] Listagem funciona com filtros e paginação
- [x] Segurança aplicada corretamente
- [x] Testes unitários com >80% cobertura

## Sequenciamento

- Bloqueado por: 2.0 (Domínio e Database)
- Desbloqueia: 4.0, 5.0, 6.0
- Paralelizável: Sim (com 4.0 e 5.0)

## Tempo Estimado

- Commands/Handlers: 8 horas
- Controllers/DTOs: 6 horas
- Testes: 4 horas
- Total: 18 horas