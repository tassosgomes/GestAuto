using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GestAuto.Stock.Domain.Interfaces;
using GestAuto.Stock.Infra.Repositories;

namespace GestAuto.Stock.Infra;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ITestDriveRepository, TestDriveRepository>();

        return services;
    }
}
