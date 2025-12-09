using FluentAssertions;
using System.Linq;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Services;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.Repositories;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.IntegrationTest.Repository;

[Collection("Postgres")]
public class LeadRepositoryTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly LeadScoringService _scoringService = new();

    public LeadRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistLead()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new LeadRepository(context);

        var lead = Lead.Create(
            "Joao Silva",
            new Email("joao@email.com"),
            new Phone("11999998888"),
            LeadSource.Showroom,
            Guid.NewGuid());

        await repository.AddAsync(lead, CancellationToken.None);
        await context.SaveChangesAsync();

        var savedLead = await repository.GetByIdAsync(lead.Id, CancellationToken.None);

        savedLead.Should().NotBeNull();
        savedLead!.Name.Should().Be("Joao Silva");
        savedLead.Email.Value.Should().Be("joao@email.com");
        savedLead.Status.Should().Be(LeadStatus.New);
    }

    [Fact]
    public async Task ListBySalesPersonAsync_ShouldFilterBySalesPerson()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new LeadRepository(context);

        var salesPersonId = Guid.NewGuid();
        var otherSalesPersonId = Guid.NewGuid();

        var lead1 = Lead.Create("Lead 1", new Email("lead1@test.com"), new Phone("11999998881"), LeadSource.Google, salesPersonId);
        var lead2 = Lead.Create("Lead 2", new Email("lead2@test.com"), new Phone("11999998882"), LeadSource.Google, salesPersonId);
        var lead3 = Lead.Create("Lead 3", new Email("lead3@test.com"), new Phone("11999998883"), LeadSource.Google, otherSalesPersonId);

        await repository.AddAsync(lead1, CancellationToken.None);
        await repository.AddAsync(lead2, CancellationToken.None);
        await repository.AddAsync(lead3, CancellationToken.None);
        await context.SaveChangesAsync();

        var results = await repository.ListBySalesPersonAsync(
            salesPersonId,
            null,
            null,
            page: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        results.Should().HaveCount(2);
        results.Should().OnlyContain(l => l.SalesPersonId == salesPersonId);
    }

    [Fact]
    public async Task ListBySalesPersonAsync_ShouldFilterByScore()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new LeadRepository(context);

        var salesPersonId = Guid.NewGuid();

        var lead1 = Lead.Create("Lead Gold", new Email("gold@test.com"), new Phone("11999998884"), LeadSource.Google, salesPersonId);
        var lead2 = Lead.Create("Lead Bronze", new Email("bronze@test.com"), new Phone("11999998885"), LeadSource.ClassifiedsPortal, salesPersonId);

        var qualification = new Qualification(
            hasTradeInVehicle: false,
            tradeInVehicle: null,
            paymentMethod: PaymentMethod.Financing,
            expectedPurchaseDate: DateTime.UtcNow.AddDays(7),
            interestedInTestDrive: false);

        lead1.Qualify(qualification, _scoringService);

        await repository.AddAsync(lead1, CancellationToken.None);
        await repository.AddAsync(lead2, CancellationToken.None);
        await context.SaveChangesAsync();

        var results = await repository.ListBySalesPersonAsync(
            salesPersonId,
            null,
            LeadScore.Gold,
            page: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        results.Should().HaveCount(1);
        results.First().Score.Should().Be(LeadScore.Gold);
    }
}
