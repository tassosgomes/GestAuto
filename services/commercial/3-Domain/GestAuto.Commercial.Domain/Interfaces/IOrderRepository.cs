using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByProposalIdAsync(Guid proposalId);
    Task<IEnumerable<Order>> GetByLeadIdAsync(Guid leadId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
