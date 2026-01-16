---
status: completed
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>domain/entities</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>none</dependencies>
<unblocks>3.0, 4.0</unblocks>
</task_context>

# Tarefa 2.0: Implementar Domain Layer - Entidades Core

## Visão Geral

Implementar as entidades principais (Aggregates e Entities) do domínio comercial: Lead, Proposal, TestDrive, UsedVehicle, UsedVehicleEvaluation, Interaction e Order. Inclui a lógica de negócio encapsulada nas entidades e o Domain Service de Lead Scoring.

<requirements>
- Implementar Lead como Aggregate Root com toda lógica de criação e qualificação
- Implementar LeadScoringService com regras de classificação (Diamante/Ouro/Prata/Bronze)
- Implementar Proposal como Aggregate Root com lógica de desconto e fechamento
- Implementar entidades secundárias (TestDrive, UsedVehicle, etc.)
- Implementar Domain Events para comunicação assíncrona
- Seguir padrões DDD (encapsulamento, invariantes, factory methods)
</requirements>

## Subtarefas

- [x] 2.1 Criar entidade `Lead` (Aggregate Root) com factory method `Create`
- [x] 2.2 Criar entidade `Qualification` (Complex Value Object)
- [x] 2.3 Criar entidade `Interaction` (histórico de contatos)
- [x] 2.4 Criar `LeadScoringService` com regras de classificação
- [x] 2.5 Implementar método `Qualify` em Lead com cálculo de score
- [x] 2.6 Criar entidade `Proposal` (Aggregate Root) com factory method
- [x] 2.7 Criar entidade `ProposalItem` (itens extras da proposta)
- [x] 2.8 Implementar métodos `ApplyDiscount`, `ApproveDiscount`, `Close` em Proposal
- [x] 2.9 Criar entidade `TestDrive` com métodos `Schedule`, `Complete`, `Cancel`
- [x] 2.10 Criar entidade `UsedVehicle` (veículo de troca)
- [x] 2.11 Criar entidade `UsedVehicleEvaluation` (avaliação de seminovo)
- [x] 2.12 Criar entidade `Order` (acompanhamento pós-venda)
- [x] 2.13 Criar Domain Events (LeadCreatedEvent, LeadScoredEvent, ProposalCreatedEvent, etc.)
- [x] 2.14 Criar interfaces dos repositórios no Domain (ILeadRepository, IProposalRepository, etc.)
- [x] 2.15 Criar testes unitários para Lead e LeadScoringService
- [x] 2.16 Criar testes unitários para Proposal (especialmente regras de desconto)

## Sequenciamento

- **Bloqueado por:** 1.0 (Infraestrutura Base)
- **Desbloqueia:** 3.0 (Value Objects), 4.0 (Repositórios)
- **Paralelizável:** Não

## Detalhes de Implementação

### Lead (Aggregate Root)

```csharp
public class Lead
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public LeadScore Score { get; private set; }
    public Guid SalesPersonId { get; private set; }
    public Qualification? Qualification { get; private set; }
    public List<Interaction> Interactions { get; private set; } = new();
    
    // Campos opcionais de interesse
    public string? InterestedModel { get; private set; }
    public string? InterestedTrim { get; private set; }
    public string? InterestedColor { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Lead() { } // EF Core

    public static Lead Create(string name, Email email, Phone phone, 
        LeadSource source, Guid salesPersonId)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone,
            Source = source,
            Status = LeadStatus.New,
            Score = LeadScore.Bronze, // Score inicial
            SalesPersonId = salesPersonId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        lead.AddEvent(new LeadCreatedEvent(lead.Id, name, source));
        return lead;
    }

    public void Qualify(Qualification qualification, LeadScoringService scoringService)
    {
        Qualification = qualification;
        Score = scoringService.Calculate(this);
        UpdatedAt = DateTime.UtcNow;
        Status = LeadStatus.InNegotiation;
        
        AddEvent(new LeadScoredEvent(Id, Score));
    }

    public void ChangeStatus(LeadStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new LeadStatusChangedEvent(Id, newStatus));
    }

    public void AddInteraction(Interaction interaction)
    {
        Interactions.Add(interaction);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### LeadScoringService (Regras de Negócio)

```csharp
public class LeadScoringService
{
    public LeadScore Calculate(Lead lead)
    {
        if (lead.Qualification == null)
            return LeadScore.Bronze;

        var hasTradeIn = lead.Qualification.HasTradeInVehicle;
        var isFinancing = lead.Qualification.PaymentMethod == PaymentMethod.Financing;
        var daysUntilPurchase = GetDaysUntilPurchase(lead.Qualification.ExpectedPurchaseDate);

        // Regra base
        var score = CalculateBaseScore(isFinancing, hasTradeIn, daysUntilPurchase);

        // Bonificações
        score = ApplyBonuses(score, lead);

        return score;
    }

    private LeadScore CalculateBaseScore(bool isFinancing, bool hasTradeIn, int daysUntilPurchase)
    {
        // Diamante: Financiado + Usado + Compra < 15 dias
        if (isFinancing && hasTradeIn && daysUntilPurchase < 15)
            return LeadScore.Diamond;

        // Ouro: (À Vista + Usado) OU (Financiado) + Compra < 15 dias
        if ((hasTradeIn || isFinancing) && daysUntilPurchase < 15)
            return LeadScore.Gold;

        // Prata: À Vista puro (sem usado, sem financiamento)
        if (!isFinancing && !hasTradeIn)
            return LeadScore.Silver;

        // Bronze: Compra > 30 dias
        if (daysUntilPurchase > 30)
            return LeadScore.Bronze;

        return LeadScore.Silver;
    }

    private LeadScore ApplyBonuses(LeadScore baseScore, Lead lead)
    {
        var score = baseScore;

        // Origem Showroom ou Telefone: +1 nível
        if (lead.Source == LeadSource.Showroom || lead.Source == LeadSource.Phone)
            score = PromoteScore(score);

        // Usado com baixa km e revisões na marca: +1 nível
        if (HasHighQualityTradeIn(lead.Qualification?.TradeInVehicle))
            score = PromoteScore(score);

        return score;
    }

    private LeadScore PromoteScore(LeadScore current) => current switch
    {
        LeadScore.Bronze => LeadScore.Silver,
        LeadScore.Silver => LeadScore.Gold,
        LeadScore.Gold => LeadScore.Diamond,
        LeadScore.Diamond => LeadScore.Diamond,
        _ => current
    };
}
```

### Proposal (Aggregate Root)

```csharp
public class Proposal
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public ProposalStatus Status { get; private set; }
    
    // Veículo
    public string VehicleModel { get; private set; }
    public string VehicleTrim { get; private set; }
    public string VehicleColor { get; private set; }
    public int VehicleYear { get; private set; }
    public bool IsReadyDelivery { get; private set; }
    
    // Valores
    public Money VehiclePrice { get; private set; }
    public Money DiscountAmount { get; private set; }
    public string? DiscountReason { get; private set; }
    public Guid? DiscountApproverId { get; private set; }
    public Money TradeInValue { get; private set; }
    
    // Pagamento
    public PaymentMethod PaymentMethod { get; private set; }
    public Money? DownPayment { get; private set; }
    public int? Installments { get; private set; }
    
    // Itens extras
    public List<ProposalItem> Items { get; private set; } = new();
    
    // Avaliação de seminovo
    public Guid? UsedVehicleEvaluationId { get; private set; }

    public Money TotalValue => CalculateTotalValue();

    public void ApplyDiscount(Money amount, string reason, Guid salesPersonId)
    {
        var discountPercentage = amount.Amount / VehiclePrice.Amount * 100;
        
        DiscountAmount = amount;
        DiscountReason = reason;
        
        if (discountPercentage > 5)
        {
            Status = ProposalStatus.AwaitingDiscountApproval;
            // DiscountApproverId permanece null até aprovação
        }
        
        AddEvent(new ProposalUpdatedEvent(Id, "Desconto aplicado"));
    }

    public void ApproveDiscount(Guid managerId)
    {
        if (Status != ProposalStatus.AwaitingDiscountApproval)
            throw new DomainException("Proposta não está aguardando aprovação de desconto");
            
        DiscountApproverId = managerId;
        Status = ProposalStatus.AwaitingCustomer;
        
        AddEvent(new ProposalUpdatedEvent(Id, "Desconto aprovado"));
    }

    public void Close(Guid salesPersonId)
    {
        ValidateCanClose();
        
        Status = ProposalStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        
        AddEvent(new SaleClosedEvent(Id, LeadId, TotalValue));
    }
}
```

### Domain Events

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public record LeadCreatedEvent(Guid LeadId, string Name, LeadSource Source) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record LeadScoredEvent(Guid LeadId, LeadScore Score) : IDomainEvent;

public record LeadStatusChangedEvent(Guid LeadId, LeadStatus NewStatus) : IDomainEvent;

public record ProposalCreatedEvent(Guid ProposalId, Guid LeadId) : IDomainEvent;

public record ProposalUpdatedEvent(Guid ProposalId, string Description) : IDomainEvent;

public record SaleClosedEvent(Guid ProposalId, Guid LeadId, Money TotalValue) : IDomainEvent;

public record UsedVehicleEvaluationRequestedEvent(
    Guid EvaluationId, 
    Guid ProposalId,
    string Brand, 
    string Model, 
    int Year, 
    int Mileage,
    string LicensePlate) : IDomainEvent;

public record TestDriveScheduledEvent(Guid TestDriveId, Guid LeadId, DateTime ScheduledAt) : IDomainEvent;

public record TestDriveCompletedEvent(Guid TestDriveId, Guid LeadId) : IDomainEvent;
```

## Critérios de Sucesso

- [x] Todas as entidades implementam encapsulamento correto (setters privados)
- [x] Factory methods (`Create`) garantem estado válido inicial
- [x] LeadScoringService calcula corretamente todos os cenários:
  - Diamante: Financiado + Usado + Compra < 15 dias
  - Ouro: (À Vista + Usado) OU (Financiado) + Compra < 15 dias
  - Prata: À Vista puro
  - Bronze: Compra > 30 dias
- [x] Bonificações de score funcionam corretamente
- [x] Proposta exige aprovação para descontos > 5%
- [x] Domain Events são emitidos em todas as operações relevantes
- [x] Testes unitários cobrem 100% das regras de negócio
- [x] Código segue convenção de nomes em inglês
