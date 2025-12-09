using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id);
    Task<IEnumerable<Proposal>> GetByLeadIdAsync(Guid leadId);
    Task AddAsync(Proposal proposal);
    Task UpdateAsync(Proposal proposal);
}