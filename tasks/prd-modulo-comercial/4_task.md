---
status: pending
parallelizable: false
blocked_by: ["2.0", "3.0"]
---

<task_context>
<domain>infra/persistence</domain>
<type>implementation</type>
<scope>infrastructure</scope>
<complexity>high</complexity>
<dependencies>database|entity_framework</dependencies>
<unblocks>5.0, 7.0, 9.0</unblocks>
</task_context>

# Tarefa 4.0: Implementar Infra Layer - Repositórios e Persistência

## Visão Geral

Implementar a camada de infraestrutura de persistência usando Entity Framework Core com PostgreSQL. Inclui a configuração do DbContext, mapeamento das entidades, implementação dos repositórios, Unit of Work com suporte a Outbox Pattern, e criação das migrations.

<requirements>
- Configurar CommercialDbContext com EF Core
- Implementar mapeamento de entidades (Fluent API)
- Implementar repositórios para cada Aggregate Root
- Implementar Unit of Work com transações
- Configurar Outbox Pattern para eventos
- Criar migrations para o esquema de banco
- Implementar conversores para Value Objects
</requirements>

## Subtarefas

- [ ] 4.1 Configurar `CommercialDbContext` com DbSets para todas as entidades
- [ ] 4.2 Criar `EntityTypeConfiguration` para Lead (incluindo owned types)
- [ ] 4.3 Criar `EntityTypeConfiguration` para Proposal e ProposalItem
- [ ] 4.4 Criar `EntityTypeConfiguration` para TestDrive
- [ ] 4.5 Criar `EntityTypeConfiguration` para UsedVehicle e UsedVehicleEvaluation
- [ ] 4.6 Criar `EntityTypeConfiguration` para Interaction
- [ ] 4.7 Criar `EntityTypeConfiguration` para Order
- [ ] 4.8 Criar `EntityTypeConfiguration` para OutboxMessage
- [ ] 4.9 Criar `EntityTypeConfiguration` para Audit
- [ ] 4.10 Implementar conversores de Value Objects (EmailConverter, PhoneConverter, MoneyConverter, LicensePlateConverter)
- [ ] 4.11 Implementar `LeadRepository` com métodos específicos
- [ ] 4.12 Implementar `ProposalRepository` com métodos específicos
- [ ] 4.13 Implementar `TestDriveRepository`
- [ ] 4.14 Implementar `UsedVehicleEvaluationRepository`
- [ ] 4.15 Implementar `OrderRepository`
- [ ] 4.16 Implementar `OutboxRepository`
- [ ] 4.17 Implementar `UnitOfWork` com suporte a transações e coleta de domain events
- [ ] 4.18 Criar migration inicial com todas as tabelas
- [ ] 4.19 Criar índices otimizados conforme especificação
- [ ] 4.20 Testar migrations em ambiente local

## Sequenciamento

- **Bloqueado por:** 2.0 (Entidades), 3.0 (Value Objects)
- **Desbloqueia:** 5.0 (Application Leads), 7.0 (Application Propostas), 9.0 (Outbox)
- **Paralelizável:** Não

## Detalhes de Implementação

### CommercialDbContext

```csharp
public class CommercialDbContext : DbContext
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<Interaction> Interactions => Set<Interaction>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalItem> ProposalItems => Set<ProposalItem>();
    public DbSet<TestDrive> TestDrives => Set<TestDrive>();
    public DbSet<UsedVehicle> UsedVehicles => Set<UsedVehicle>();
    public DbSet<UsedVehicleEvaluation> UsedVehicleEvaluations => Set<UsedVehicleEvaluation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public CommercialDbContext(DbContextOptions<CommercialDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommercialDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Lead EntityTypeConfiguration

```csharp
public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // Value Object - Email
        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .HasConversion(new EmailConverter())
            .IsRequired();

        // Value Object - Phone
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .HasConversion(new PhoneConverter())
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnName("score")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.SalesPersonId)
            .HasColumnName("sales_person_id")
            .IsRequired();

        builder.Property(x => x.InterestedModel)
            .HasColumnName("interested_model")
            .HasMaxLength(100);

        builder.Property(x => x.InterestedTrim)
            .HasColumnName("interested_trim")
            .HasMaxLength(100);

        builder.Property(x => x.InterestedColor)
            .HasColumnName("interested_color")
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamento com Qualification (1:1)
        builder.HasOne(x => x.Qualification)
            .WithOne()
            .HasForeignKey<Qualification>("lead_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento com Interactions (1:N)
        builder.HasMany(x => x.Interactions)
            .WithOne()
            .HasForeignKey("lead_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.SalesPersonId).HasDatabaseName("idx_leads_sales_person");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_leads_status");
        builder.HasIndex(x => x.Score).HasDatabaseName("idx_leads_score");
    }
}
```

### Value Object Converters

```csharp
public class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter() 
        : base(
            v => v.Value,
            v => new Email(v))
    {
    }
}

public class PhoneConverter : ValueConverter<Phone, string>
{
    public PhoneConverter() 
        : base(
            v => v.Value,
            v => new Phone(v))
    {
    }
}

public class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter() 
        : base(
            v => v.Amount,
            v => new Money(v))
    {
    }
}

public class LicensePlateConverter : ValueConverter<LicensePlate, string>
{
    public LicensePlateConverter() 
        : base(
            v => v.Value,
            v => new LicensePlate(v))
    {
    }
}
```

### LeadRepository

```csharp
public class LeadRepository : ILeadRepository
{
    private readonly CommercialDbContext _context;

    public LeadRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Leads
            .Include(l => l.Qualification)
                .ThenInclude(q => q!.TradeInVehicle)
            .Include(l => l.Interactions)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Lead> AddAsync(Lead entity, CancellationToken cancellationToken)
    {
        await _context.Leads.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Lead entity, CancellationToken cancellationToken)
    {
        _context.Leads.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Lead>> ListBySalesPersonAsync(
        Guid salesPersonId, 
        LeadStatus? status,
        LeadScore? score,
        int page, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Leads
            .Include(l => l.Qualification)
            .Where(l => l.SalesPersonId == salesPersonId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (score.HasValue)
            query = query.Where(l => l.Score == score.Value);

        return await query
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lead>> ListAllAsync(
        LeadStatus? status,
        LeadScore? score,
        int page, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Leads.Include(l => l.Qualification).AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (score.HasValue)
            query = query.Where(l => l.Score == score.Value);

        return await query
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
```

### Unit of Work

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly CommercialDbContext _context;
    private readonly IOutboxRepository _outboxRepository;
    private IDbContextTransaction? _transaction;
    private readonly List<IDomainEvent> _domainEvents = new();

    public UnitOfWork(CommercialDbContext context, IOutboxRepository outboxRepository)
    {
        _context = context;
        _outboxRepository = outboxRepository;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        // Coletar eventos das entidades rastreadas
        CollectDomainEventsFromEntities();

        // Salvar eventos no outbox antes de commit
        foreach (var domainEvent in _domainEvents)
        {
            await _outboxRepository.AddAsync(domainEvent, cancellationToken);
        }

        var result = await _context.SaveChangesAsync(cancellationToken);
        
        _domainEvents.Clear();
        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    private void CollectDomainEventsFromEntities()
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            _domainEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

### OutboxMessage Entity

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
}

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(x => x.Error)
            .HasColumnName("error");

        // Índice para mensagens pendentes
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_outbox_pending")
            .HasFilter("processed_at IS NULL");
    }
}
```

### Migration SQL Esperado

```sql
-- Estrutura principal das tabelas conforme techspec
CREATE TABLE leads (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    source VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    score VARCHAR(50) NOT NULL,
    sales_person_id UUID NOT NULL,
    interested_model VARCHAR(100),
    interested_trim VARCHAR(100),
    interested_color VARCHAR(50),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_leads_sales_person ON leads(sales_person_id);
CREATE INDEX idx_leads_status ON leads(status);
CREATE INDEX idx_leads_score ON leads(score);
```

## Critérios de Sucesso

- [ ] CommercialDbContext configura todas as entidades corretamente
- [ ] Migrations criam as tabelas conforme esquema da techspec
- [ ] Value Objects são persistidos e recuperados corretamente
- [ ] Repositórios implementam todas as queries necessárias
- [ ] Unit of Work gerencia transações corretamente
- [ ] Domain Events são salvos no Outbox dentro da mesma transação
- [ ] Índices são criados conforme especificação
- [ ] Conexão com PostgreSQL funciona em ambiente Docker
- [ ] Testes de integração passam com Testcontainers
