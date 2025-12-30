using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id);
    Task<IEnumerable<Proposal>> GetByLeadIdAsync(Guid leadId);
    Task AddAsync(Proposal proposal);
    Task UpdateAsync(Proposal proposal);
    Task<IReadOnlyList<Proposal>> ListAsync(
        Guid? salesPersonId,
        Guid? leadId,
        ProposalStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        Guid? salesPersonId,
        Guid? leadId,
        ProposalStatus? status,
        CancellationToken cancellationToken = default);
    
    // Dashboard methods
    Task<int> CountByStatusAsync(ProposalStatus status, string? salesPersonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proposal>> GetPendingActionProposalsAsync(string? salesPersonId, int limit, CancellationToken cancellationToken = default);
}