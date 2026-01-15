using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Dto;

namespace GestAuto.Stock.Application.TestDrives.Commands;

public sealed record StartTestDriveCommand(
    Guid VehicleId,
    Guid SalesPersonId,
    StartTestDriveRequest Request) : ICommand<StartTestDriveResponse>;
