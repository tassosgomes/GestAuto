using Moq;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.UnitTest.Application;

public class GetLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly GetLeadHandler _handler;

    public GetLeadHandlerTests()
    {
        _handler = new GetLeadHandler(_leadRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Lead()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            salesPersonId);

        var query = new GetLeadQuery(leadId, salesPersonId);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.Id);
        Assert.Equal("João Silva", result.Name);
        Assert.Equal("joao@test.com", result.Email);
        _leadRepositoryMock.Verify(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Lead_Not_Found()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var query = new GetLeadQuery(leadId, salesPersonId);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.HandleAsync(query, CancellationToken.None));
    }
}
