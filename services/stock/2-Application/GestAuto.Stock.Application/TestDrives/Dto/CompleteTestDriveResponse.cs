using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.Application.TestDrives.Dto;

public sealed record CompleteTestDriveResponse(
    Guid TestDriveId,
    Guid VehicleId,
    TestDriveOutcome Outcome,
    DateTime EndedAtUtc,
    VehicleStatus CurrentStatus,
    Guid? ReservationId);
