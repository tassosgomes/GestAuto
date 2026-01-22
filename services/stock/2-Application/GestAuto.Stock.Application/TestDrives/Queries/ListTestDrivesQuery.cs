using GestAuto.Stock.Application.Common;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Dto;

namespace GestAuto.Stock.Application.TestDrives.Queries;

public sealed record ListTestDrivesQuery(
    int Page,
    int Size,
    TestDriveStatus? Status,
    Guid? VehicleId,
    Guid? SalesPersonId,
    string? CustomerRef,
    DateTime? From,
    DateTime? To) : IQuery<PagedResponse<TestDriveListItem>>;
