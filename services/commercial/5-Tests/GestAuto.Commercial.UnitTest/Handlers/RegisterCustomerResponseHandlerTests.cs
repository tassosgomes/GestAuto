using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.UnitTest.Handlers;

public class RegisterCustomerResponseHandlerTests
{
    private readonly Mock<IUsedVehicleEvaluationRepository> _evaluationRepository;
    private readonly Mock<IProposalRepository> _proposalRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly RegisterCustomerResponseHandler _handler;

    public RegisterCustomerResponseHandlerTests()
    {
        _evaluationRepository = new Mock<IUsedVehicleEvaluationRepository>();
        _proposalRepository = new Mock<IProposalRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new RegisterCustomerResponseHandler(
            _evaluationRepository.Object,
            _proposalRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenAccepted_ShouldUpdateProposalTradeInValue()
    {
        var evaluationId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var evaluation = CreateCompletedEvaluation(evaluationId, proposalId, 50000m);
        var proposal = CreateProposal(proposalId);

        _evaluationRepository.Setup(x => x.GetByIdAsync(evaluationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluation);
        _proposalRepository.Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        var command = new RegisterCustomerResponseCommand(evaluationId, true, null);

        var response = await _handler.HandleAsync(command, CancellationToken.None);

        response.CustomerAccepted.Should().BeTrue();
        proposal.TradeInValue.Amount.Should().Be(50000m);
        _proposalRepository.Verify(x => x.UpdateAsync(proposal), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNotCompleted_ShouldThrowDomainException()
    {
        var evaluationId = Guid.NewGuid();
        var evaluation = UsedVehicleEvaluation.Request(
            Guid.NewGuid(),
            UsedVehicle.Create("Ford", "Ka", 2019, 30000, new LicensePlate("ABC1234"), "Black", "Good", false),
            Guid.NewGuid());

        _evaluationRepository.Setup(x => x.GetByIdAsync(evaluationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluation);

        var command = new RegisterCustomerResponseCommand(evaluationId, true, null);

        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    private static UsedVehicleEvaluation CreateCompletedEvaluation(Guid evaluationId, Guid proposalId, decimal value)
    {
        var evaluation = UsedVehicleEvaluation.Request(
            proposalId,
            UsedVehicle.Create("Toyota", "Corolla", 2020, 50000, new LicensePlate("ABC1234"), "White", "Good", true),
            Guid.NewGuid());

        typeof(UsedVehicleEvaluation).GetProperty("Id")?.SetValue(evaluation, evaluationId);
        evaluation.MarkAsCompleted(new Money(value));
        return evaluation;
    }

    private static Proposal CreateProposal(Guid proposalId)
    {
        var proposal = Proposal.Create(
            Guid.NewGuid(),
            "Model S",
            "Long Range",
            "Red",
            2024,
            true,
            new Money(300000),
            new Money(0),
            PaymentMethod.Cash);

        typeof(Proposal).GetProperty("Id")?.SetValue(proposal, proposalId);
        return proposal;
    }
}
