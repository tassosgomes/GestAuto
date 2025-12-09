using Moq;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.UnitTest.Application;

public class ListInteractionsHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly ListInteractionsHandler _handler;

    public ListInteractionsHandlerTests()
    {
        _handler = new ListInteractionsHandler(_leadRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Interactions()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        // Adicionar interações ao lead
        lead.RegisterInteraction(InteractionType.Call, "Primeiro contato", DateTime.Now.AddDays(-2));
        lead.RegisterInteraction(InteractionType.WhatsApp, "Enviou catálogo", DateTime.Now.AddDays(-1));
        lead.RegisterInteraction(InteractionType.Email, "Follow-up", DateTime.Now);

        var query = new ListInteractionsQuery(leadId, 1, 20);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, i => i.Type == "Call");
        Assert.Contains(result, i => i.Type == "WhatsApp");
        Assert.Contains(result, i => i.Type == "Email");
        _leadRepositoryMock.Verify(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Empty_List_When_No_Interactions()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var query = new ListInteractionsQuery(leadId, 1, 20);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Lead_Not_Found()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var query = new ListInteractionsQuery(leadId, 1, 20);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_Should_Order_Interactions_By_Date()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = Lead.Create(
            "João Silva", 
            new Email("joao@test.com"), 
            new Phone("11999999999"), 
            LeadSource.Instagram, 
            Guid.NewGuid());

        var date1 = DateTime.Now.AddDays(-3);
        var date2 = DateTime.Now.AddDays(-2);
        var date3 = DateTime.Now.AddDays(-1);

        lead.RegisterInteraction(InteractionType.Call, "Descrição 1", date1);
        lead.RegisterInteraction(InteractionType.Email, "Descrição 2", date2);
        lead.RegisterInteraction(InteractionType.WhatsApp, "Descrição 3", date3);

        var query = new ListInteractionsQuery(leadId, 1, 20);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        // Verificar ordem (mais recente primeiro)
        Assert.Equal("WhatsApp", result[0].Type);
        Assert.Equal("Email", result[1].Type);
        Assert.Equal("Call", result[2].Type);
    }
}
