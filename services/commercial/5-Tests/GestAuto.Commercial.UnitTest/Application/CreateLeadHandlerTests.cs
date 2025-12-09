using Moq;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.UnitTest.Application;

public class CreateLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateLeadHandler _handler;

    public CreateLeadHandlerTests()
    {
        _handler = new CreateLeadHandler(_leadRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Create_Lead_And_Return_Response()
    {
        // Arrange
        var command = new CreateLeadCommand(
            "João Silva",
            "joao@test.com",
            "11999999999",
            "instagram",
            Guid.NewGuid(),
            "Civic",
            "EX",
            "Preto"
        );

        var lead = Lead.Create("João Silva", new Email("joao@test.com"), new Phone("11999999999"), LeadSource.Instagram, command.SalesPersonId);

        _leadRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("João Silva", result.Name);
        Assert.Equal("joao@test.com", result.Email);
        _leadRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}