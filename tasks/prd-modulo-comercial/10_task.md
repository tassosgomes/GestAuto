---
status: completed
parallelizable: true
blocked_by: ["6.0", "8.0"]
completed_at: "2025-12-09T15:30:00Z"
reviewed_by: "GitHub Copilot"
---

<task_context>
<domain>application/test-drives</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>12.0</unblocks>
</task_context>

# Tarefa 10.0: Implementar Fluxo de Test-Drives

## Visão Geral

Implementar o fluxo completo de Test-Drives, incluindo Application Layer (Commands/Queries) e API Layer (Controller). Permite agendar, concluir e cancelar test-drives, com validação de disponibilidade do veículo.

<requirements>
- Implementar Commands e Queries para Test-Drives
- Implementar TestDriveController com endpoints REST
- Validar disponibilidade do veículo no agendamento
- Registrar checklist pré e pós test-drive
- Atualizar status do lead ao agendar test-drive
- Emitir eventos TestDriveScheduled e TestDriveCompleted
</requirements>

## Subtarefas

- [x] 10.1 Criar `ScheduleTestDriveCommand` e `ScheduleTestDriveHandler` ✅ CONCLUÍDO
- [x] 10.2 Criar `ScheduleTestDriveValidator` ✅ CONCLUÍDO
- [x] 10.3 Criar `CompleteTestDriveCommand` e `CompleteTestDriveHandler` ✅ CONCLUÍDO
- [x] 10.4 Criar `CancelTestDriveCommand` e `CancelTestDriveHandler` ✅ CONCLUÍDO
- [x] 10.5 Criar `GetTestDriveQuery` e `GetTestDriveHandler` ✅ CONCLUÍDO
- [x] 10.6 Criar `ListTestDrivesQuery` e `ListTestDrivesHandler` ✅ CONCLUÍDO
- [x] 10.7 Implementar `TestDriveController` com endpoint `POST /api/v1/test-drives` ✅ CONCLUÍDO
- [x] 10.8 Implementar endpoint `GET /api/v1/test-drives` ✅ CONCLUÍDO
- [x] 10.9 Implementar endpoint `GET /api/v1/test-drives/{id}` ✅ CONCLUÍDO
- [x] 10.10 Implementar endpoint `POST /api/v1/test-drives/{id}/complete` ✅ CONCLUÍDO
- [x] 10.11 Implementar endpoint `POST /api/v1/test-drives/{id}/cancel` ✅ CONCLUÍDO
- [x] 10.12 Criar DTOs para Test-Drive ✅ CONCLUÍDO
- [x] 10.13 Criar testes unitários e de integração ✅ CONCLUÍDO

## Sequenciamento

- **Bloqueado por:** 6.0 (API Leads), 8.0 (API Propostas)
- **Desbloqueia:** 12.0 (Testes de Integração)
- **Paralelizável:** Sim (pode executar junto com 11.0)

## Detalhes de Implementação

### ScheduleTestDriveCommand

```csharp
public record ScheduleTestDriveCommand(
    Guid LeadId,
    Guid VehicleId, // Veículo de demonstração
    DateTime ScheduledAt,
    Guid SalesPersonId,
    string? Notes
) : ICommand<TestDriveResponse>;

public class ScheduleTestDriveHandler : ICommandHandler<ScheduleTestDriveCommand, TestDriveResponse>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleTestDriveHandler(
        ITestDriveRepository testDriveRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _testDriveRepository = testDriveRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TestDriveResponse> HandleAsync(
        ScheduleTestDriveCommand command, 
        CancellationToken cancellationToken)
    {
        // Verifica se lead existe
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {command.LeadId} não encontrado");

        // Verifica disponibilidade do veículo no horário
        var isAvailable = await _testDriveRepository.CheckVehicleAvailabilityAsync(
            command.VehicleId, 
            command.ScheduledAt, 
            cancellationToken);

        if (!isAvailable)
            throw new DomainException("Veículo não está disponível no horário solicitado");

        var testDrive = TestDrive.Schedule(
            command.LeadId,
            command.VehicleId,
            command.ScheduledAt,
            command.SalesPersonId,
            command.Notes
        );

        await _testDriveRepository.AddAsync(testDrive, cancellationToken);

        // Atualiza status do lead
        lead.ChangeStatus(LeadStatus.TestDriveScheduled);
        await _leadRepository.UpdateAsync(lead, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TestDriveResponse.FromEntity(testDrive);
    }
}
```

### CompleteTestDriveCommand

```csharp
public record CompleteTestDriveCommand(
    Guid TestDriveId,
    TestDriveChecklistDto Checklist,
    string? CustomerFeedback,
    Guid CompletedByUserId
) : ICommand<TestDriveResponse>;

public record TestDriveChecklistDto(
    decimal InitialMileage,
    decimal FinalMileage,
    string FuelLevel, // "Full", "3/4", "1/2", "1/4", "Empty"
    string? VisualObservations
);

public class CompleteTestDriveHandler : ICommandHandler<CompleteTestDriveCommand, TestDriveResponse>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTestDriveHandler(
        ITestDriveRepository testDriveRepository,
        IUnitOfWork unitOfWork)
    {
        _testDriveRepository = testDriveRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TestDriveResponse> HandleAsync(
        CompleteTestDriveCommand command, 
        CancellationToken cancellationToken)
    {
        var testDrive = await _testDriveRepository.GetByIdAsync(command.TestDriveId, cancellationToken)
            ?? throw new NotFoundException($"Test-drive {command.TestDriveId} não encontrado");

        var checklist = new TestDriveChecklist(
            command.Checklist.InitialMileage,
            command.Checklist.FinalMileage,
            Enum.Parse<FuelLevel>(command.Checklist.FuelLevel, ignoreCase: true),
            command.Checklist.VisualObservations
        );

        testDrive.Complete(checklist, command.CustomerFeedback, command.CompletedByUserId);

        await _testDriveRepository.UpdateAsync(testDrive, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TestDriveResponse.FromEntity(testDrive);
    }
}
```

### TestDriveController

```csharp
[ApiController]
[Route("api/v1/test-drives")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class TestDriveController : ControllerBase
{
    private readonly ICommandHandler<ScheduleTestDriveCommand, TestDriveResponse> _scheduleHandler;
    private readonly ICommandHandler<CompleteTestDriveCommand, TestDriveResponse> _completeHandler;
    private readonly ICommandHandler<CancelTestDriveCommand, TestDriveResponse> _cancelHandler;
    private readonly IQueryHandler<GetTestDriveQuery, TestDriveResponse> _getHandler;
    private readonly IQueryHandler<ListTestDrivesQuery, PagedResponse<TestDriveListItemResponse>> _listHandler;
    private readonly ISalesPersonFilterService _salesPersonFilter;

    // ... constructor ...

    /// <summary>
    /// Agenda um test-drive
    /// </summary>
    /// <response code="201">Test-drive agendado com sucesso</response>
    /// <response code="400">Dados inválidos ou veículo indisponível</response>
    /// <response code="404">Lead não encontrado</response>
    [HttpPost]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDriveResponse>> Schedule(
        [FromBody] ScheduleTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var salesPersonId = GetCurrentUserId();

        var command = new ScheduleTestDriveCommand(
            request.LeadId,
            request.VehicleId,
            request.ScheduledAt,
            salesPersonId,
            request.Notes
        );

        var result = await _scheduleHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista test-drives com paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TestDriveListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<TestDriveListItemResponse>>> List(
        [FromQuery] Guid? leadId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId();

        var query = new ListTestDrivesQuery(salesPersonId, leadId, status, from, to, page, pageSize);
        var result = await _listHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtém um test-drive por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDriveResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTestDriveQuery(id);
        var result = await _getHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra a conclusão do test-drive
    /// </summary>
    /// <remarks>
    /// Inclui checklist com quilometragem, combustível e observações.
    /// </remarks>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestDriveResponse>> Complete(
        Guid id,
        [FromBody] CompleteTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new CompleteTestDriveCommand(
            id,
            request.Checklist,
            request.CustomerFeedback,
            userId
        );

        var result = await _completeHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancela um test-drive agendado
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestDriveResponse>> Cancel(
        Guid id,
        [FromBody] CancelTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new CancelTestDriveCommand(id, request.Reason, userId);
        var result = await _cancelHandler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
```

### DTOs

```csharp
public record ScheduleTestDriveRequest(
    Guid LeadId,
    Guid VehicleId,
    DateTime ScheduledAt,
    string? Notes
);

public record CompleteTestDriveRequest(
    TestDriveChecklistDto Checklist,
    string? CustomerFeedback
);

public record CancelTestDriveRequest(
    string Reason
);

public record TestDriveResponse(
    Guid Id,
    Guid LeadId,
    Guid VehicleId,
    string Status,
    DateTime ScheduledAt,
    DateTime? CompletedAt,
    Guid SalesPersonId,
    string? Notes,
    TestDriveChecklistResponse? Checklist,
    string? CustomerFeedback,
    string? CancellationReason,
    DateTime CreatedAt
)
{
    public static TestDriveResponse FromEntity(TestDrive testDrive) => new(
        testDrive.Id,
        testDrive.LeadId,
        testDrive.VehicleId,
        testDrive.Status.ToString(),
        testDrive.ScheduledAt,
        testDrive.CompletedAt,
        testDrive.SalesPersonId,
        testDrive.Notes,
        testDrive.Checklist != null 
            ? TestDriveChecklistResponse.FromEntity(testDrive.Checklist) 
            : null,
        testDrive.CustomerFeedback,
        testDrive.CancellationReason,
        testDrive.CreatedAt
    );
}

public record TestDriveChecklistResponse(
    decimal InitialMileage,
    decimal FinalMileage,
    string FuelLevel,
    string? VisualObservations
)
{
    public static TestDriveChecklistResponse FromEntity(TestDriveChecklist checklist) => new(
        checklist.InitialMileage,
        checklist.FinalMileage,
        checklist.FuelLevel.ToString(),
        checklist.VisualObservations
    );
}

public record TestDriveListItemResponse(
    Guid Id,
    Guid LeadId,
    string LeadName,
    string Status,
    DateTime ScheduledAt,
    string VehicleDescription
)
{
    public static TestDriveListItemResponse FromEntity(TestDrive testDrive, Lead lead) => new(
        testDrive.Id,
        testDrive.LeadId,
        lead.Name,
        testDrive.Status.ToString(),
        testDrive.ScheduledAt,
        $"Vehicle {testDrive.VehicleId}" // Seria substituído por dados reais do veículo
    );
}
```

### Validators

```csharp
public class ScheduleTestDriveValidator : AbstractValidator<ScheduleTestDriveCommand>
{
    public ScheduleTestDriveValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("Lead é obrigatório");

        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Veículo é obrigatório");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Data do test-drive deve ser no futuro")
            .LessThan(DateTime.UtcNow.AddMonths(3)).WithMessage("Data do test-drive deve ser nos próximos 3 meses");
    }
}

public class CompleteTestDriveValidator : AbstractValidator<CompleteTestDriveCommand>
{
    public CompleteTestDriveValidator()
    {
        RuleFor(x => x.TestDriveId)
            .NotEmpty().WithMessage("Test-drive é obrigatório");

        RuleFor(x => x.Checklist)
            .NotNull().WithMessage("Checklist é obrigatório");

        RuleFor(x => x.Checklist.InitialMileage)
            .GreaterThanOrEqualTo(0).WithMessage("Quilometragem inicial inválida");

        RuleFor(x => x.Checklist.FinalMileage)
            .GreaterThanOrEqualTo(x => x.Checklist.InitialMileage)
            .WithMessage("Quilometragem final deve ser maior ou igual à inicial");

        RuleFor(x => x.Checklist.FuelLevel)
            .Must(BeValidFuelLevel).WithMessage("Nível de combustível inválido");
    }

    private bool BeValidFuelLevel(string level)
    {
        return Enum.TryParse<FuelLevel>(level, ignoreCase: true, out _);
    }
}
```

## Critérios de Sucesso

- [x] Agendar test-drive valida disponibilidade do veículo ✅
- [x] Agendar test-drive atualiza status do lead ✅
- [x] Concluir test-drive registra checklist completo ✅
- [x] Cancelar test-drive registra motivo ✅
- [x] Eventos TestDriveScheduled e TestDriveCompleted são emitidos ✅
- [x] Listagem permite filtrar por data e status ✅
- [x] Validadores impedem agendamentos no passado ✅
- [x] API responde corretamente para todos os cenários ✅
- [x] Testes de integração cobrem o fluxo completo ✅

---

## ✅ TAREFA CONCLUÍDA

**Status:** APROVADA PARA PRODUÇÃO  
**Data:** 09/12/2025  
**Revisor:** GitHub Copilot  
**Branch:** feat/task-10-test-drive-flow

**Próximos Passos:**
1. ✅ Merge aprovado
2. ✅ Deploy para staging aprovado  
3. ✅ Deploy para produção aprovado

**Documentação:** Ver `10_task_review.md` para detalhes completos da revisão.
