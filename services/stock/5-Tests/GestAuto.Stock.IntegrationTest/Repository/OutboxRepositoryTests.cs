using FluentAssertions;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Infra.Repositories;
using GestAuto.Stock.Tests.Shared;
using Xunit;

namespace GestAuto.Stock.IntegrationTest.Repository;

[Collection("Postgres")]
public class OutboxRepositoryTests
{
    private readonly PostgresFixture _postgresFixture;

    public OutboxRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [SkippableFact]
    public async Task AddAsync_AndRetrievePending_ShouldWork()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, _postgresFixture.UnavailableReason);

        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new OutboxRepository(context);

        IDomainEvent evt = new VehicleSoldEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());

        await repository.AddAsync(evt, CancellationToken.None);
        await context.SaveChangesAsync();

        var pending = await repository.GetPendingMessagesAsync(10, CancellationToken.None);

        pending.Should().HaveCount(1);
        pending[0].EventType.Should().Contain(nameof(VehicleSoldEvent));
        pending[0].ProcessedAt.Should().BeNull();
        pending[0].Error.Should().BeNull();
    }

    [SkippableFact]
    public async Task MarkAsProcessed_ShouldSetProcessedAt()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, _postgresFixture.UnavailableReason);

        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new OutboxRepository(context);

        await repository.AddAsync(new VehicleSoldEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid()), CancellationToken.None);
        await context.SaveChangesAsync();

        var pending = await repository.GetPendingMessagesAsync(1, CancellationToken.None);
        var messageId = pending.Single().Id;

        await repository.MarkAsProcessedAsync(messageId, CancellationToken.None);
        await context.SaveChangesAsync();

        var reloadedPending = await repository.GetPendingMessagesAsync(10, CancellationToken.None);
        reloadedPending.Should().BeEmpty();

        var stored = await context.OutboxMessages.FindAsync(messageId);
        stored.Should().NotBeNull();
        stored!.ProcessedAt.Should().NotBeNull();
        stored.Error.Should().BeNull();
    }

    [SkippableFact]
    public async Task MarkAsFailed_ShouldKeepUnprocessedWithError()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, _postgresFixture.UnavailableReason);

        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new OutboxRepository(context);

        await repository.AddAsync(new VehicleSoldEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid()), CancellationToken.None);
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
