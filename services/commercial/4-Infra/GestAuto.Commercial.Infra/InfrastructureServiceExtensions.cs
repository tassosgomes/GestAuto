using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GestAuto.Commercial.Infra.Repositories;
using GestAuto.Commercial.Infra.UnitOfWork;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Infra.Messaging.Consumers;

namespace GestAuto.Commercial.Infra;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IProposalRepository, ProposalRepository>();
        services.AddScoped<ITestDriveRepository, TestDriveRepository>();
        services.AddScoped<IUsedVehicleEvaluationRepository, UsedVehicleEvaluationRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // Background Services (Consumers)
        services.AddHostedService<UsedVehicleEvaluationRespondedConsumer>();
        services.AddHostedService<OrderUpdatedConsumer>();

        return services;
    }
}
