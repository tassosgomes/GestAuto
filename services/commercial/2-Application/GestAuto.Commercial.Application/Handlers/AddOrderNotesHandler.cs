using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class AddOrderNotesHandler : ICommandHandler<AddOrderNotesCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddOrderNotesHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponse> HandleAsync(
        AddOrderNotesCommand command, 
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Pedido {command.OrderId} não encontrado");

        order.AddNotes(command.Notes);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderResponse.FromEntity(order);
    }
}