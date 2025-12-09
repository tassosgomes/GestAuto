using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> GetBySalesPersonIdAsync(Guid salesPersonId, CancellationToken cancellationToken = default);
    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lead>> ListBySalesPersonAsync(Guid salesPersonId, LeadStatus? status, LeadScore? score, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lead>> ListAllAsync(LeadStatus? status, LeadScore? score, int page, int pageSize, CancellationToken cancellationToken = default);
}