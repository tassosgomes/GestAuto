using System.Linq;
using FluentAssertions;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Infra.Repositories;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.IntegrationTest.Repository;

[Collection("Postgres")]
public class OutboxRepositoryTests
{
    private readonly PostgresFixture _postgresFixture;

    public OutboxRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task AddAsync_AndRetrievePending_ShouldWork()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new OutboxRepository(context);

        var evt = new LeadCreatedEvent(Guid.NewGuid(), "Lead Teste", Domain.Enums.LeadSource.Google);

        await repository.AddAsync(evt, CancellationToken.None);
        await context.SaveChangesAsync();

        var pending = await repository.GetPendingMessagesAsync(10, CancellationToken.None);

        pending.Should().HaveCount(1);
        pending.First().EventType.Should().Contain(nameof(LeadCreatedEvent));
        pending.First().ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsProcessed_ShouldSetProcessedAt()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new OutboxRepository(context);
        var evt = new LeadCreatedEvent(Guid.NewGuid(), "Lead Teste", Domain.Enums.LeadSource.Showroom);

        await repository.AddAsync(evt, CancellationToken.None);
        await context.SaveChangesAsync();

        var pending = await repository.GetPendingMessagesAsync(1, CancellationToken.None);
        var messageId = pending.Single().Id;

        await repository.MarkAsProcessedAsync(messageId, CancellationToken.None);
        await context.SaveChangesAsync();

        var reloaded = await repository.GetPendingMessagesAsync(10, CancellationToken.None);
        reloaded.Should().BeEmpty();

        var stored = await context.OutboxMessages.FindAsync(messageId);
        stored!.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsFailed_ShouldKeepUnprocessedWithError()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new OutboxRepository(context);
        var evt = new LeadCreatedEvent(Guid.NewGuid(), "Lead Teste", Domain.Enums.LeadSource.Phone);

        await repository.AddAsync(evt, CancellationToken.None);
        await context.SaveChangesAsync();

        var pending = await repository.GetPendingMessagesAsync(1, CancellationToken.None);
        var messageId = pending.Single().Id;

        await repository.MarkAsFailedAsync(messageId, "erro", CancellationToken.None);
        await context.SaveChangesAsync();

        var stillPending = await repository.GetPendingMessagesAsync(10, CancellationToken.None);
        stillPending.Should().HaveCount(1);
        stillPending.Single().Error.Should().Be("erro");
        stillPending.Single().ProcessedAt.Should().BeNull();
    }
}
