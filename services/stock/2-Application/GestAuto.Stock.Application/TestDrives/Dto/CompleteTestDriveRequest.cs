using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.Application.TestDrives.Dto;

public sealed record CompleteTestDriveRequest(
    TestDriveOutcome Outcome,
    DateTime? EndedAt,
    CreateReservationRequest? Reservation);
