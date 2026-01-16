---
status: pending
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>application/leads</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>6.0</unblocks>
</task_context>

# Tarefa 5.0: Implementar Application Layer - Leads (Commands/Queries)

## Visão Geral

Implementar a camada de aplicação para o fluxo de Leads usando CQRS nativo (sem MediatR). Inclui Commands para operações de escrita (criar, qualificar, alterar status, registrar interação) e Queries para operações de leitura (buscar lead, listar leads). Implementar também os Validators usando FluentValidation.

<requirements>
- Implementar padrão CQRS com interfaces ICommand/IQuery e Handlers
- Criar Commands para todas as operações de escrita em Leads
- Criar Queries para todas as operações de leitura em Leads
- Implementar Validators com FluentValidation
- Criar DTOs de request/response
- Garantir que handlers usam Unit of Work corretamente
</requirements>

## Subtarefas

- [ ] 5.1 Criar interfaces base `ICommand<TResponse>`, `IQuery<TResponse>`
- [ ] 5.2 Criar interfaces `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>`
- [ ] 5.3 Criar `CreateLeadCommand` e `CreateLeadHandler`
- [ ] 5.4 Criar `CreateLeadValidator` com FluentValidation
- [ ] 5.5 Criar `QualifyLeadCommand` e `QualifyLeadHandler`
- [ ] 5.6 Criar `QualifyLeadValidator`
- [ ] 5.7 Criar `ChangeLeadStatusCommand` e `ChangeLeadStatusHandler`
- [ ] 5.8 Criar `RegisterInteractionCommand` e `RegisterInteractionHandler`
- [ ] 5.9 Criar `UpdateLeadCommand` e `UpdateLeadHandler`
- [ ] 5.10 Criar `GetLeadQuery` e `GetLeadHandler`
- [ ] 5.11 Criar `ListLeadsQuery` e `ListLeadsHandler` (com paginação e filtros)
- [ ] 5.12 Criar `ListInteractionsQuery` e `ListInteractionsHandler`
- [ ] 5.13 Criar DTOs: `CreateLeadRequest`, `LeadResponse`, `LeadListResponse`, etc.
- [ ] 5.14 Configurar DI para registro automático de Handlers
- [ ] 5.15 Criar testes unitários para todos os Handlers
- [ ] 5.16 Criar testes unitários para todos os Validators

## Sequenciamento

- **Bloqueado por:** 4.0 (Repositórios)
- **Desbloqueia:** 6.0 (API Leads)
- **Paralelizável:** Sim (pode executar junto com 7.0 e 9.0)

## Detalhes de Implementação

### Interfaces Base CQRS

```csharp
// ICommand.cs
public interface ICommand<TResponse> { }

// IQuery.cs
public interface IQuery<TResponse> { }

// ICommandHandler.cs
public interface ICommandHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

// IQueryHandler.cs
public interface IQueryHandler<TQuery, TResponse> 
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

### CreateLeadCommand

```csharp
public record CreateLeadCommand(
    string Name,
    string Email,
    string Phone,
    string Source,
    Guid SalesPersonId,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor
) : ICommand<LeadResponse>;

public class CreateLeadHandler : ICommandHandler<CreateLeadCommand, LeadResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadResponse> HandleAsync(
        CreateLeadCommand command, 
        CancellationToken cancellationToken)
    {
        var email = new Email(command.Email);
        var phone = new Phone(command.Phone);
        var source = Enum.Parse<LeadSource>(command.Source, ignoreCase: true);

        var lead = Lead.Create(
            command.Name,
            email,
            phone,
            source,
            command.SalesPersonId
        );

        if (!string.IsNullOrEmpty(command.InterestedModel))
            lead.SetInterest(command.InterestedModel, command.InterestedTrim, command.InterestedColor);

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadResponse.FromEntity(lead);
    }
}
```

### CreateLeadValidator

```csharp
public class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Must(BeValidPhone).WithMessage("Telefone deve ter 10 ou 11 dígitos");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Origem é obrigatória")
            .Must(BeValidSource).WithMessage("Origem inválida. Valores permitidos: instagram, indicacao, google, loja, telefone, showroom, portal_classificados, outros");

        RuleFor(x => x.SalesPersonId)
            .NotEmpty().WithMessage("Vendedor responsável é obrigatório");
    }

    private bool BeValidPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length >= 10 && digits.Length <= 11;
    }

    private bool BeValidSource(string source)
    {
        return Enum.TryParse<LeadSource>(source, ignoreCase: true, out _);
    }
}
```

### QualifyLeadCommand

```csharp
public record QualifyLeadCommand(
    Guid LeadId,
    bool HasTradeInVehicle,
    TradeInVehicleDto? TradeInVehicle,
    string PaymentMethod,
    DateTime? ExpectedPurchaseDate,
    bool InterestedInTestDrive
) : ICommand<LeadResponse>;

public record TradeInVehicleDto(
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory
);

public class QualifyLeadHandler : ICommandHandler<QualifyLeadCommand, LeadResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LeadScoringService _scoringService;

    public QualifyLeadHandler(
        ILeadRepository leadRepository, 
        IUnitOfWork unitOfWork,
        LeadScoringService scoringService)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _scoringService = scoringService;
    }

    public async Task<LeadResponse> HandleAsync(
        QualifyLeadCommand command, 
        CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {command.LeadId} não encontrado");

        UsedVehicle? tradeInVehicle = null;
        if (command.HasTradeInVehicle && command.TradeInVehicle != null)
        {
            tradeInVehicle = UsedVehicle.Create(
                command.TradeInVehicle.Brand,
                command.TradeInVehicle.Model,
                command.TradeInVehicle.Year,
                command.TradeInVehicle.Mileage,
                new LicensePlate(command.TradeInVehicle.LicensePlate),
                command.TradeInVehicle.Color,
                command.TradeInVehicle.GeneralCondition,
                command.TradeInVehicle.HasDealershipServiceHistory
            );
        }

        var paymentMethod = Enum.Parse<PaymentMethod>(command.PaymentMethod, ignoreCase: true);

        var qualification = new Qualification(
            command.HasTradeInVehicle,
            tradeInVehicle,
            paymentMethod,
            command.ExpectedPurchaseDate,
            command.InterestedInTestDrive
        );

        lead.Qualify(qualification, _scoringService);

        await _leadRepository.UpdateAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadResponse.FromEntity(lead);
    }
}
```

### ListLeadsQuery (com paginação e filtros)

```csharp
public record ListLeadsQuery(
    Guid? SalesPersonId, // null = gerente vê todos
    string? Status,
    string? Score,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResponse<LeadListItemResponse>>;

public class ListLeadsHandler : IQueryHandler<ListLeadsQuery, PagedResponse<LeadListItemResponse>>
{
    private readonly ILeadRepository _leadRepository;

    public ListLeadsHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<PagedResponse<LeadListItemResponse>> HandleAsync(
        ListLeadsQuery query, 
        CancellationToken cancellationToken)
    {
        var status = !string.IsNullOrEmpty(query.Status) 
            ? Enum.Parse<LeadStatus>(query.Status, ignoreCase: true) 
            : (LeadStatus?)null;

        var score = !string.IsNullOrEmpty(query.Score) 
            ? Enum.Parse<LeadScore>(query.Score, ignoreCase: true) 
            : (LeadScore?)null;

        IReadOnlyList<Lead> leads;
        int totalCount;

        if (query.SalesPersonId.HasValue)
        {
            leads = await _leadRepository.ListBySalesPersonAsync(
                query.SalesPersonId.Value, status, score, 
                query.Page, query.PageSize, cancellationToken);
            totalCount = await _leadRepository.CountBySalesPersonAsync(
                query.SalesPersonId.Value, status, score, cancellationToken);
        }
        else
        {
            leads = await _leadRepository.ListAllAsync(
                status, score, query.Page, query.PageSize, cancellationToken);
            totalCount = await _leadRepository.CountAllAsync(status, score, cancellationToken);
        }

        var items = leads.Select(LeadListItemResponse.FromEntity).ToList();

        return new PagedResponse<LeadListItemResponse>(
            items, query.Page, query.PageSize, totalCount);
    }
}
```

### DTOs

```csharp
public record LeadResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Source,
    string Status,
    string Score,
    Guid SalesPersonId,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor,
    QualificationResponse? Qualification,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static LeadResponse FromEntity(Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Email.Value,
        lead.Phone.Formatted,
        lead.Source.ToString(),
        lead.Status.ToString(),
        lead.Score.ToString(),
        lead.SalesPersonId,
        lead.InterestedModel,
        lead.InterestedTrim,
        lead.InterestedColor,
        lead.Qualification != null ? QualificationResponse.FromEntity(lead.Qualification) : null,
        lead.CreatedAt,
        lead.UpdatedAt
    );
}

public record LeadListItemResponse(
    Guid Id,
    string Name,
    string Phone,
    string Source,
    string Status,
    string Score,
    string? InterestedModel,
    DateTime CreatedAt
)
{
    public static LeadListItemResponse FromEntity(Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Phone.Formatted,
        lead.Source.ToString(),
        lead.Status.ToString(),
        lead.Score.ToString(),
        lead.InterestedModel,
        lead.CreatedAt
    );
}

public record QualificationResponse(
    bool HasTradeInVehicle,
    TradeInVehicleResponse? TradeInVehicle,
    string PaymentMethod,
    DateTime? ExpectedPurchaseDate,
    bool InterestedInTestDrive
)
{
    public static QualificationResponse FromEntity(Qualification qualification) => new(
        qualification.HasTradeInVehicle,
        qualification.TradeInVehicle != null 
            ? TradeInVehicleResponse.FromEntity(qualification.TradeInVehicle) 
            : null,
        qualification.PaymentMethod.ToString(),
        qualification.ExpectedPurchaseDate,
        qualification.InterestedInTestDrive
    );
}

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### Registro de DI

```csharp
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<LeadScoringService>();

        // Handlers - Leads
        services.AddScoped<ICommandHandler<CreateLeadCommand, LeadResponse>, CreateLeadHandler>();
        services.AddScoped<ICommandHandler<QualifyLeadCommand, LeadResponse>, QualifyLeadHandler>();
        services.AddScoped<ICommandHandler<ChangeLeadStatusCommand, LeadResponse>, ChangeLeadStatusHandler>();
        services.AddScoped<ICommandHandler<RegisterInteractionCommand, InteractionResponse>, RegisterInteractionHandler>();
        services.AddScoped<ICommandHandler<UpdateLeadCommand, LeadResponse>, UpdateLeadHandler>();
        
        services.AddScoped<IQueryHandler<GetLeadQuery, LeadResponse>, GetLeadHandler>();
        services.AddScoped<IQueryHandler<ListLeadsQuery, PagedResponse<LeadListItemResponse>>, ListLeadsHandler>();
        services.AddScoped<IQueryHandler<ListInteractionsQuery, IReadOnlyList<InteractionResponse>>, ListInteractionsHandler>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<CreateLeadValidator>();

        return services;
    }
}
```

## Critérios de Sucesso

- [ ] Todos os Commands e Queries implementados conforme especificação
- [ ] Validators validam todos os campos obrigatórios
- [ ] Validators retornam mensagens de erro claras em português
- [ ] Handlers usam Unit of Work corretamente para transações
- [ ] Domain Events são disparados nas operações de escrita
- [ ] Paginação funciona corretamente com filtros
- [ ] Score é calculado corretamente ao qualificar lead
- [ ] Testes unitários cobrem cenários de sucesso e falha
- [ ] DTOs mapeiam corretamente de/para entidades
