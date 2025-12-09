using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GestAuto.Commercial.Infra.Repositories;
using GestAuto.Commercial.Infra.UnitOfWork;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
