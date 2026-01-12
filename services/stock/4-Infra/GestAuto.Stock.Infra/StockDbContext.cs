using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.Infra;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
    {
    }
}
