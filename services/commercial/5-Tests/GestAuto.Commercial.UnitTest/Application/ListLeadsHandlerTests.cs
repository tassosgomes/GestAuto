using Moq;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;
using System.Linq;

namespace GestAuto.Commercial.UnitTest.Application;

public class ListLeadsHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly ListLeadsHandler _handler;

    public ListLeadsHandlerTests()
    {
        _handler = new ListLeadsHandler(_leadRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Paged_Leads_For_SalesPerson()
    {
        // Arrange
        var salesPersonId = Guid.NewGuid();
        var leads = new List<Lead>
        {
            Lead.Create("João Silva", new Email("joao@test.com"), new Phone("11999999999"), LeadSource.Instagram, salesPersonId),
            Lead.Create("Maria Santos", new Email("maria@test.com"), new Phone("11988888888"), LeadSource.Google, salesPersonId)
        };

        var query = new ListLeadsQuery(salesPersonId, null, null, null, null, null, 1, 20);

        _leadRepositoryMock.Setup(x => x.ListBySalesPersonAsync(
            salesPersonId, null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        
        _leadRepositoryMock.Setup(x => x.CountBySalesPersonAsync(
            salesPersonId, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        _leadRepositoryMock.Verify(x => x.ListBySalesPersonAsync(
            salesPersonId, null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_All_Leads_When_SalesPersonId_Is_Null()
    {
        // Arrange
        var leads = new List<Lead>
        {
            Lead.Create("João Silva", new Email("joao@test.com"), new Phone("11999999999"), LeadSource.Instagram, Guid.NewGuid()),
            Lead.Create("Maria Santos", new Email("maria@test.com"), new Phone("11988888888"), LeadSource.Google, Guid.NewGuid()),
            Lead.Create("Pedro Costa", new Email("pedro@test.com"), new Phone("11977777777"), LeadSource.Store, Guid.NewGuid())
        };

        var query = new ListLeadsQuery(null, null, null, null, null, null, 1, 20);

        _leadRepositoryMock.Setup(x => x.ListAllAsync(
            null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        
        _leadRepositoryMock.Setup(x => x.CountAllAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        _leadRepositoryMock.Verify(x => x.ListAllAsync(
            null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Filter_By_Status()
    {
        // Arrange
        var salesPersonId = Guid.NewGuid();
        var leads = new List<Lead>
        {
            Lead.Create("João Silva", new Email("joao@test.com"), new Phone("11999999999"), LeadSource.Instagram, salesPersonId)
        };

        var query = new ListLeadsQuery(salesPersonId, "InContact", null, null, null, null, 1, 20);

        var statuses = new[] { LeadStatus.InContact };

        _leadRepositoryMock.Setup(x => x.ListBySalesPersonAsync(
            salesPersonId,
            It.Is<IReadOnlyCollection<LeadStatus>>(value => value.SequenceEqual(statuses)),
            null,
            null,
            null,
            null,
            1,
            20,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        
        _leadRepositoryMock.Setup(x => x.CountBySalesPersonAsync(
            salesPersonId,
            It.Is<IReadOnlyCollection<LeadStatus>>(value => value.SequenceEqual(statuses)),
            null,
            null,
            null,
            null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        _leadRepositoryMock.Verify(x => x.ListBySalesPersonAsync(
            salesPersonId,
            It.Is<IReadOnlyCollection<LeadStatus>>(value => value.SequenceEqual(statuses)),
            null,
            null,
            null,
            null,
            1,
            20,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Filter_By_Score()
    {
        // Arrange
        var salesPersonId = Guid.NewGuid();
        var leads = new List<Lead>
        {
            Lead.Create("João Silva", new Email("joao@test.com"), new Phone("11999999999"), LeadSource.Instagram, salesPersonId)
        };

        var query = new ListLeadsQuery(salesPersonId, null, "Gold", null, null, null, 1, 20);

        _leadRepositoryMock.Setup(x => x.ListBySalesPersonAsync(
            salesPersonId, null, LeadScore.Gold, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        
        _leadRepositoryMock.Setup(x => x.CountBySalesPersonAsync(
            salesPersonId, null, LeadScore.Gold, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        _leadRepositoryMock.Verify(x => x.ListBySalesPersonAsync(
            salesPersonId, null, LeadScore.Gold, null, null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Calculate_Pagination_Properties()
    {
        // Arrange
        var leads = new List<Lead>
        {
            Lead.Create("João Silva", new Email("joao@test.com"), new Phone("11999999999"), LeadSource.Instagram, Guid.NewGuid())
        };

        var query = new ListLeadsQuery(null, null, null, null, null, null, 2, 10);

        _leadRepositoryMock.Setup(x => x.ListAllAsync(
            null, null, null, null, null, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        
        _leadRepositoryMock.Setup(x => x.CountAllAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages); // 25 / 10 = 3
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
}
