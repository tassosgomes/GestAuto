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

public class CloseProposalHandlerTests
{
    private readonly Mock<IProposalRepository> _proposalRepositoryMock = new();
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CloseProposalHandler _handler;

    public CloseProposalHandlerTests()
    {
        _handler = new CloseProposalHandler(
            _proposalRepositoryMock.Object,
            _leadRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task HandleAsync_Should_Close_Proposal_And_Update_Lead_Status()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();

        var proposal = Proposal.Create(
            leadId,
            "Civic",
            "EX",
            "Preto",
            2024,
            true,
            new Money(100000),
            Money.Zero,
            PaymentMethod.Cash
        );

        var lead = Lead.Create(
            "João Silva",
            new Email("joao@test.com"),
            new Phone("11999999999"),
            LeadSource.Instagram,
            salesPersonId
        );

        var command = new CloseProposalCommand(proposalId, salesPersonId);

        _proposalRepositoryMock.Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);
        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _proposalRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Proposal>()))
            .Returns(Task.CompletedTask);
        _leadRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Closed", result.Status);
        Assert.Equal(LeadStatus.Converted, lead.Status);
        _proposalRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Proposal>()), Times.Once);
        _leadRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Proposal_Not_Found()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var command = new CloseProposalCommand(proposalId, salesPersonId);

        _proposalRepositoryMock.Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync((Proposal)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Update_Lead_If_Lead_Not_Found()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();

        var proposal = Proposal.Create(
            leadId,
            "Civic",
            "EX",
            "Preto",
            2024,
            true,
            new Money(100000),
            Money.Zero,
            PaymentMethod.Cash
        );

        var command = new CloseProposalCommand(proposalId, salesPersonId);

        _proposalRepositoryMock.Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);
        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead)null!);
        _proposalRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Proposal>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Closed", result.Status);
        _leadRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
