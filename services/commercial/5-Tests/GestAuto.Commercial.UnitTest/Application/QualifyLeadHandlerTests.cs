using Moq;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Services;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.UnitTest.Application;

public class QualifyLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<LeadScoringService> _scoringServiceMock = new();
    private readonly QualifyLeadHandler _handler;

    public QualifyLeadHandlerTests()
    {
        _handler = new QualifyLeadHandler(
            _leadRepositoryMock.Object, 
            _unitOfWorkMock.Object,
            _scoringServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Qualify_Lead_With_TradeIn()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new QualifyLeadCommand(
            leadId,
            true,
            new TradeInVehicleDto("Honda", "Civic", 2020, 30000, "ABC1234", "Preto", "Bom", true),
            "Financing",
            7500m,
            DateTime.Now.AddDays(10),
            true
        );

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.Id);
        Assert.NotNull(result.Qualification);
        Assert.True(result.Qualification.HasTradeInVehicle);
        Assert.NotNull(result.Qualification.TradeInVehicle);
        _leadRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Qualify_Lead_Without_TradeIn()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "Maria Santos", 
            new Email("maria@test.com"), 
            new Phone("11988888888"), 
            LeadSource.Google, 
            Guid.NewGuid());

        var command = new QualifyLeadCommand(
            leadId,
            false,
            null,
            "Cash",
            null,
            DateTime.Now.AddDays(5),
            false
        );

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Qualification);
        Assert.False(result.Qualification.HasTradeInVehicle);
        Assert.Null(result.Qualification.TradeInVehicle);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Lead_Not_Found()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var command = new QualifyLeadCommand(
            leadId,
            false,
            null,
            "Cash",
            null,
            DateTime.Now.AddDays(5),
            false
        );

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.HandleAsync(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
