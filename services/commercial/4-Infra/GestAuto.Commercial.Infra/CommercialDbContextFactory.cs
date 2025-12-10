using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestAuto.Commercial.Infra;

public class CommercialDbContextFactory : IDesignTimeDbContextFactory<CommercialDbContext>
{
    public CommercialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommercialDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=gestauto;Username=gestauto;Password=gestauto123;Include Error Detail=true");

        return new CommercialDbContext(optionsBuilder.Options);
    }
}