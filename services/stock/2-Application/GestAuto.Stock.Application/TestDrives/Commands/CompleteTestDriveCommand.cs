using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Dto;

namespace GestAuto.Stock.Application.TestDrives.Commands;

public sealed record CompleteTestDriveCommand(
    Guid TestDriveId,
    Guid CompletedByUserId,
    CompleteTestDriveRequest Request) : ICommand<CompleteTestDriveResponse>;
