using FluentAssertions;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;

namespace GestAuto.Stock.UnitTest.Domain;

public class ReservationTests
{
    [Fact]
    public void Constructor_WhenStandard_ShouldSetExpiresAt48Hours()
    {
        var createdAt = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc);
        var reservation = new Reservation(
            vehicleId: Guid.NewGuid(),
            type: ReservationType.Standard,
            salesPersonId: Guid.NewGuid(),
            createdAtUtc: createdAt,
            contextType: "lead");

        reservation.ExpiresAtUtc.Should().Be(createdAt.AddHours(48));
    }

    [Fact]
    public void Constructor_WhenPaidDeposit_ShouldNotSetExpiresAt()
    {
        var createdAt = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc);
        var reservation = new Reservation(
            vehicleId: Guid.NewGuid(),
            type: ReservationType.PaidDeposit,
            salesPersonId: Guid.NewGuid(),
            createdAtUtc: createdAt,
            contextType: "proposal");

        reservation.ExpiresAtUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenWaitingBankWithoutDeadline_ShouldThrow()
    {
        var createdAt = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc);

        var act = () => new Reservation(
            vehicleId: Guid.NewGuid(),
            type: ReservationType.WaitingBank,
            salesPersonId: Guid.NewGuid(),
            createdAtUtc: createdAt,
            contextType: "proposal",
            bankDeadlineAtUtc: null);

        act.Should().Throw<DomainException>();
    }
}
