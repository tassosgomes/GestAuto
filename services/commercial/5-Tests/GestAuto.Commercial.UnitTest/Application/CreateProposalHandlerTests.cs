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

public class CreateProposalHandlerTests
{
    private readonly Mock<IProposalRepository> _proposalRepositoryMock = new();
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateProposalHandler _handler;

    public CreateProposalHandlerTests()
    {
        _handler = new CreateProposalHandler(
            _proposalRepositoryMock.Object,
            _leadRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task HandleAsync_Should_Create_Proposal_And_Return_Response()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var command = new CreateProposalCommand(
            leadId,
            "Civic",
            "EX",
            "Preto",
            2024,
            true,
            150000,
            "Cash",
            null,
            null
        );

        var lead = Lead.Create(
            "João Silva",
            new Email("joao@test.com"),
            new Phone("11999999999"),
            LeadSource.Instagram,
            Guid.NewGuid()
        );

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _proposalRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Proposal>()))
            .Returns(Task.CompletedTask);
        _leadRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.VehicleModel, result.VehicleModel);
        Assert.Equal(command.VehiclePrice, result.VehiclePrice);
        Assert.Equal("AwaitingCustomer", result.Status);
        _proposalRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Proposal>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Lead_Not_Found()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var command = new CreateProposalCommand(
            leadId,
            "Civic",
            "EX",
            "Preto",
            2024,
            true,
            150000,
            "Cash",
            null,
            null
        );

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Lead_Status_To_ProposalSent()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var command = new CreateProposalCommand(
            leadId,
            "Civic",
            "EX",
            "Preto",
            2024,
            true,
            150000,
            "Cash",
            null,
            null
        );

        var lead = Lead.Create(
            "João Silva",
            new Email("joao@test.com"),
            new Phone("11999999999"),
            LeadSource.Instagram,
            Guid.NewGuid()
        );

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _proposalRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Proposal>()))
            .Returns(Task.CompletedTask);
        _leadRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(LeadStatus.ProposalSent, lead.Status);
        _leadRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
