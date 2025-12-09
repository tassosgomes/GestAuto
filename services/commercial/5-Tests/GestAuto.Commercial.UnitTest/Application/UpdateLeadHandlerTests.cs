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

public class UpdateLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateLeadHandler _handler;

    public UpdateLeadHandlerTests()
    {
        _handler = new UpdateLeadHandler(_leadRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Lead_Name()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new UpdateLeadCommand(leadId, "João Pedro Silva", null, null, null, null, null);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("João Pedro Silva", result.Name);
        _leadRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Lead_Email()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new UpdateLeadCommand(leadId, null, "joao.novo@test.com", null, null, null, null);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("joao.novo@test.com", result.Email);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Lead_Phone()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new UpdateLeadCommand(leadId, null, null, "11988888888", null, null, null);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("(11) 98888-8888", result.Phone);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Lead_Interest()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new UpdateLeadCommand(leadId, null, null, null, "Accord", "EX", "Branco");

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Accord", result.InterestedModel);
        Assert.Equal("EX", result.InterestedTrim);
        Assert.Equal("Branco", result.InterestedColor);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Multiple_Fields()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var command = new UpdateLeadCommand(
            leadId, 
            "João Pedro Silva", 
            "joao.novo@test.com", 
            "11988888888", 
            "Civic", 
            "Sport", 
            "Preto");

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("João Pedro Silva", result.Name);
        Assert.Equal("joao.novo@test.com", result.Email);
        Assert.Equal("Civic", result.InterestedModel);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Lead_Not_Found()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var command = new UpdateLeadCommand(leadId, "João Silva", null, null, null, null, null);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.HandleAsync(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
