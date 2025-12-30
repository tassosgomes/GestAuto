using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Infra.Entities;

namespace GestAuto.Commercial.Infra;

public class CommercialDbContext : DbContext
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Interaction> Interactions => Set<Interaction>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalItem> ProposalItems => Set<ProposalItem>();
    public DbSet<TestDrive> TestDrives => Set<TestDrive>();

    public DbSet<UsedVehicleEvaluation> UsedVehicleEvaluations => Set<UsedVehicleEvaluation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();

    public CommercialDbContext(DbContextOptions<CommercialDbContext> options) : base(options) 
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommercialDbContext).Assembly);
        
        // Configure schema for commercial module
        modelBuilder.HasDefaultSchema("commercial");
        
        base.OnModelCreating(modelBuilder);
    }
}