using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.History;
using GestAuto.Stock.Infra.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.Infra;

public class StockDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<CheckInRecord> CheckIns => Set<CheckInRecord>();
    public DbSet<CheckOutRecord> CheckOuts => Set<CheckOutRecord>();
    public DbSet<TestDriveSession> TestDrives => Set<TestDriveSession>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockDbContext).Assembly);

        // Configure schema for stock module
        modelBuilder.HasDefaultSchema("stock");

        base.OnModelCreating(modelBuilder);
    }
}
