using Microsoft.EntityFrameworkCore;

namespace GestAuto.Commercial.Infra;

public class CommercialDbContext : DbContext
{
    public CommercialDbContext(DbContextOptions<CommercialDbContext> options) : base(options) { }

    // Add DbSets later

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure schema for commercial module
        modelBuilder.HasDefaultSchema("commercial");
    }
}