using Microsoft.Extensions.DependencyInjection;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Commands;
using GestAuto.Stock.Application.Vehicles.Queries;

namespace GestAuto.Stock.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateVehicleCommand, Vehicles.Dto.VehicleResponse>, CreateVehicleCommandHandler>();
        services.AddScoped<IQueryHandler<GetVehicleQuery, Vehicles.Dto.VehicleResponse>, GetVehicleQueryHandler>();
        services.AddScoped<IQueryHandler<ListVehiclesQuery, Common.PagedResponse<Vehicles.Dto.VehicleListItem>>, ListVehiclesQueryHandler>();
        services.AddScoped<ICommandHandler<ChangeVehicleStatusCommand, bool>, ChangeVehicleStatusCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCheckInCommand, Vehicles.Dto.CheckInResponse>, CreateCheckInCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCheckOutCommand, Vehicles.Dto.CheckOutResponse>, CreateCheckOutCommandHandler>();

        return services;
    }
}
