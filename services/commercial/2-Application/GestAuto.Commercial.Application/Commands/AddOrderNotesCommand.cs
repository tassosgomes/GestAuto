using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record AddOrderNotesCommand(Guid OrderId, string Notes) : ICommand<OrderResponse>;