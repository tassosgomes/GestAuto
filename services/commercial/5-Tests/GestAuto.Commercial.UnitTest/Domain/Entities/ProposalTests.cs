using FluentAssertions;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.UnitTest.Domain.Entities;

public class ProposalTests
{
    [Fact]
    public void Create_ShouldCreateProposalWithCorrectInitialState()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleModel = "Corolla";
        var vehicleTrim = "XEi";
        var vehicleColor = "Branco";
        var vehicleYear = 2024;
        var isReadyDelivery = true;
        var vehiclePrice = new Money(100000);
        var tradeInValue = new Money(50000);
        var paymentMethod = PaymentMethod.Financiamento;
        var downPayment = new Money(20000);
        var installments = 48;

        // Act
        var proposal = Proposal.Create(
            leadId, vehicleModel, vehicleTrim, vehicleColor, vehicleYear,
            isReadyDelivery, vehiclePrice, tradeInValue, paymentMethod,
            downPayment, installments);

        // Assert
        proposal.LeadId.Should().Be(leadId);
        proposal.VehicleModel.Should().Be(vehicleModel);
        proposal.Status.Should().Be(ProposalStatus.AwaitingCustomer);
        proposal.VehiclePrice.Should().Be(vehiclePrice);
        proposal.TradeInValue.Should().Be(tradeInValue);
        proposal.PaymentMethod.Should().Be(paymentMethod);
        proposal.DownPayment.Should().Be(downPayment);
        proposal.Installments.Should().Be(installments);
        proposal.Items.Should().BeEmpty();
        proposal.TotalValue.Amount.Should().Be(50000); // 100000 - 20000 (discount) - 50000 (trade-in) + 0 (items) = 50000
        proposal.DomainEvents.Should().ContainSingle(e => e is ProposalCreatedEvent);
    }

    [Fact]
    public void ApplyDiscount_LessThan5Percent_ShouldApplyWithoutApproval()
    {
        // Arrange
        var proposal = CreateTestProposal();
        var discount = new Money(4000); // 4% of 100000

        // Act
        proposal.ApplyDiscount(discount, "Cliente pediu desconto", Guid.NewGuid());

        // Assert
        proposal.DiscountAmount.Should().Be(discount);
        proposal.Status.Should().Be(ProposalStatus.AwaitingCustomer);
        proposal.DomainEvents.Should().Contain(e => e is ProposalUpdatedEvent);
    }

    [Fact]
    public void ApplyDiscount_MoreThan5Percent_ShouldRequireApproval()
    {
        // Arrange
        var proposal = CreateTestProposal();
        var discount = new Money(6000); // 6% of 100000

        // Act
        proposal.ApplyDiscount(discount, "Cliente pediu desconto", Guid.NewGuid());

        // Assert
        proposal.DiscountAmount.Should().Be(discount);
        proposal.Status.Should().Be(ProposalStatus.AwaitingDiscountApproval);
    }

    [Fact]
    public void ApproveDiscount_WhenAwaitingApproval_ShouldApproveAndChangeStatus()
    {
        // Arrange
        var proposal = CreateTestProposal();
        proposal.ApplyDiscount(new Money(6000), "Cliente pediu desconto", Guid.NewGuid());
        var managerId = Guid.NewGuid();

        // Act
        proposal.ApproveDiscount(managerId);

        // Assert
        proposal.DiscountApproverId.Should().Be(managerId);
        proposal.Status.Should().Be(ProposalStatus.AwaitingCustomer);
        proposal.DomainEvents.Should().Contain(e => e is ProposalUpdatedEvent);
    }

    [Fact]
    public void ApproveDiscount_WhenNotAwaitingApproval_ShouldThrowException()
    {
        // Arrange
        var proposal = CreateTestProposal();

        // Act & Assert
        var act = () => proposal.ApproveDiscount(Guid.NewGuid());
        act.Should().Throw<DomainException>()
            .WithMessage("Proposta não está aguardando aprovação de desconto");
    }

    [Fact]
    public void Close_ShouldChangeStatusToClosedAndEmitEvent()
    {
        // Arrange
        var proposal = CreateTestProposal();
        var salesPersonId = Guid.NewGuid();

        // Act
        proposal.Close(salesPersonId);

        // Assert
        proposal.Status.Should().Be(ProposalStatus.Closed);
        proposal.DomainEvents.Should().Contain(e => e is SaleClosedEvent);
    }

    [Fact]
    public void Close_WithPendingDiscountApproval_ShouldThrowException()
    {
        // Arrange
        var proposal = CreateTestProposal();
        proposal.ApplyDiscount(new Money(6000), "Cliente pediu desconto", Guid.NewGuid());

        // Act & Assert
        var act = () => proposal.Close(Guid.NewGuid());
        act.Should().Throw<DomainException>()
            .WithMessage("Não é possível fechar proposta com desconto pendente de aprovação");
    }

    private static Proposal CreateTestProposal()
    {
        return Proposal.Create(
            Guid.NewGuid(), "Corolla", "XEi", "Branco", 2024, true,
            new Money(100000), new Money(0), PaymentMethod.AVista);
    }
}