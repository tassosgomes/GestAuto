using System.Linq;
using FluentAssertions;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.Repositories;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.IntegrationTest.Repository;

[Collection("Postgres")]
public class ProposalRepositoryTests
{
    private readonly PostgresFixture _postgresFixture;

    public ProposalRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProposalWithItems()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new ProposalRepository(context);

        var proposal = Proposal.Create(
            leadId: Guid.NewGuid(),
            vehicleModel: "Corolla",
            vehicleTrim: "XEi",
            vehicleColor: "Prata",
            vehicleYear: 2025,
            isReadyDelivery: true,
            vehiclePrice: new Money(150_000),
            tradeInValue: Money.Zero,
            paymentMethod: PaymentMethod.Cash);

        var accessory = ProposalItem.Create("Película", new Money(1_200));
        proposal.AddItem(accessory);

        await repository.AddAsync(proposal);
        await context.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(proposal.Id);
        saved.Should().NotBeNull();
        saved!.VehicleModel.Should().Be("Corolla");
        saved.Items.Should().HaveCount(1);
        saved.Items.First().Description.Should().Be("Película");
    }

    [Fact]
    public async Task ListByLeadAsync_ShouldReturnOrderedByCreated()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new ProposalRepository(context);
        var leadId = Guid.NewGuid();

        var p1 = Proposal.Create(leadId, "Corolla", "XEi", "Prata", 2025, true, new Money(150_000), Money.Zero, PaymentMethod.Cash);
        var p2 = Proposal.Create(leadId, "Yaris", "XS", "Branco", 2024, true, new Money(120_000), Money.Zero, PaymentMethod.Cash);

        await repository.AddAsync(p1);
        await repository.AddAsync(p2);
        await context.SaveChangesAsync();

        var list = await repository.ListByLeadAsync(leadId, CancellationToken.None);

        list.Should().HaveCount(2);
        list.First().VehicleModel.Should().Be(p2.VehicleModel); // p2 created last => first when ordered desc CreatedAt
    }

    [Fact]
    public async Task ApplyDiscountAbove5Percent_ShouldSetAwaitingApproval()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new ProposalRepository(context);

        var proposal = Proposal.Create(
            leadId: Guid.NewGuid(),
            vehicleModel: "Corolla",
            vehicleTrim: "Altis",
            vehicleColor: "Preto",
            vehicleYear: 2025,
            isReadyDelivery: true,
            vehiclePrice: new Money(200_000),
            tradeInValue: Money.Zero,
            paymentMethod: PaymentMethod.Cash);

        proposal.ApplyDiscount(new Money(12_000), "Fidelizacao", Guid.NewGuid());

        await repository.AddAsync(proposal);
        await context.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(proposal.Id);
        saved!.Status.Should().Be(ProposalStatus.AwaitingDiscountApproval);
        saved.DiscountAmount.Amount.Should().Be(12_000);
    }
}
