using Moq;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.UnitTest.Application;

public class ChangeLeadStatusHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ChangeLeadStatusHandler _handler;

    public ChangeLeadStatusHandlerTests()
    {
        _handler = new ChangeLeadStatusHandler(_leadRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Change_Lead_Status()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new ChangeLeadStatusCommand(leadId, "InContact");

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.Id);
        Assert.Equal("InContact", result.Status);
        _leadRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Lead_Not_Found()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var command = new ChangeLeadStatusCommand(leadId, "InContact");

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.HandleAsync(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("New")]
    [InlineData("InContact")]
    [InlineData("InNegotiation")]
    [InlineData("TestDriveScheduled")]
    [InlineData("ProposalSent")]
    [InlineData("Lost")]
    [InlineData("Converted")]
    public async Task HandleAsync_Should_Accept_All_Valid_Statuses(string status)
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new ChangeLeadStatusCommand(leadId, status);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(status, result.Status);
    }
}
