using Xunit;
using Moq;
using FluentAssertions;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.UnitTest.Handlers;

public class RequestEvaluationHandlerTests
{
    private readonly Mock<IUsedVehicleEvaluationRepository> _mockEvaluationRepository;
    private readonly Mock<IProposalRepository> _mockProposalRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly RequestEvaluationHandler _handler;

    public RequestEvaluationHandlerTests()
    {
        _mockEvaluationRepository = new Mock<IUsedVehicleEvaluationRepository>();
        _mockProposalRepository = new Mock<IProposalRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _handler = new RequestEvaluationHandler(
            _mockEvaluationRepository.Object,
            _mockProposalRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProposalExists_ShouldCreateEvaluation()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var proposal = CreateTestProposal(proposalId);
        
        var command = new RequestEvaluationCommand(
            proposalId,
            "Toyota",
            "Corolla",
            2020,
            50000,
            "ABC1234",
            "Branco",
            "Boa",
            true,
            userId
        );

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);

        _mockEvaluationRepository
            .Setup(x => x.AddAsync(It.IsAny<UsedVehicleEvaluation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsedVehicleEvaluation eval, CancellationToken ct) => eval);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProposalId.Should().Be(proposalId);
        result.Vehicle.Brand.Should().Be("Toyota");
        result.Vehicle.Model.Should().Be("Corolla");
        result.Status.Should().Be("Requested");

        _mockEvaluationRepository.Verify(
            x => x.AddAsync(It.IsAny<UsedVehicleEvaluation>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProposalDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var command = new RequestEvaluationCommand(
            proposalId,
            "Toyota",
            "Corolla",
            2020,
            50000,
            "ABC1234",
            "Branco",
            "Boa",
            true,
            userId
        );

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync((Proposal?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain(proposalId.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenBrandIsInvalid_ShouldThrowArgumentException(string invalidBrand)
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var proposal = CreateTestProposal(proposalId);
        
        var command = new RequestEvaluationCommand(
            proposalId,
            invalidBrand,
            "Corolla",
            2020,
            50000,
            "ABC1234",
            "Branco",
            "Boa",
            true,
            userId
        );

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.ParamName.Should().Be("brand");
    }

    private static Proposal CreateTestProposal(Guid proposalId)
    {
        var leadId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        
        // Use reflection to create proposal since constructor is internal/private
        var proposal = (Proposal)Activator.CreateInstance(typeof(Proposal), nonPublic: true)!;
        
        // Set properties via reflection if needed
        typeof(Proposal).GetProperty("Id")?.SetValue(proposal, proposalId);
        
        return proposal;
    }
}