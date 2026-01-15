using Microsoft.Extensions.DependencyInjection;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Commands;
using GestAuto.Stock.Application.TestDrives.Commands;
using GestAuto.Stock.Application.Vehicles.Commands;
using GestAuto.Stock.Application.Vehicles.Queries;

namespace GestAuto.Stock.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateVehicleCommand, Vehicles.Dto.VehicleResponse>, CreateVehicleCommandHandler>();
        services.AddScoped<IQueryHandler<GetVehicleQuery, Vehicles.Dto.VehicleResponse>, GetVehicleQueryHandler>();
        services.AddScoped<IQueryHandler<GetVehicleHistoryQuery, Vehicles.Dto.VehicleHistoryResponse>, GetVehicleHistoryQueryHandler>();
        services.AddScoped<IQueryHandler<ListVehiclesQuery, Common.PagedResponse<Vehicles.Dto.VehicleListItem>>, ListVehiclesQueryHandler>();
        services.AddScoped<ICommandHandler<ChangeVehicleStatusCommand, bool>, ChangeVehicleStatusCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCheckInCommand, Vehicles.Dto.CheckInResponse>, CreateCheckInCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCheckOutCommand, Vehicles.Dto.CheckOutResponse>, CreateCheckOutCommandHandler>();

        services.AddScoped<ICommandHandler<CreateReservationCommand, Reservations.Dto.ReservationResponse>, CreateReservationCommandHandler>();
        services.AddScoped<ICommandHandler<CancelReservationCommand, Reservations.Dto.ReservationResponse>, CancelReservationCommandHandler>();
        services.AddScoped<ICommandHandler<ExtendReservationCommand, Reservations.Dto.ReservationResponse>, ExtendReservationCommandHandler>();

        services.AddScoped<ICommandHandler<StartTestDriveCommand, TestDrives.Dto.StartTestDriveResponse>, StartTestDriveCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteTestDriveCommand, TestDrives.Dto.CompleteTestDriveResponse>, CompleteTestDriveCommandHandler>();

        return services;
    }
}
