using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Infra.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly CommercialDbContext _context;

    public OrderRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.ProposalId == proposalId, cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Order?> GetByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.ExternalId == externalId, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}