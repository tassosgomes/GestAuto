using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> GetBySalesPersonIdAsync(Guid salesPersonId, CancellationToken cancellationToken = default);
    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lead>> ListBySalesPersonAsync(
        Guid salesPersonId,
        IReadOnlyCollection<LeadStatus>? statuses,
        LeadScore? score,
        string? search,
        DateTime? createdFrom,
        DateTime? createdTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lead>> ListAllAsync(
        IReadOnlyCollection<LeadStatus>? statuses,
        LeadScore? score,
        string? search,
        DateTime? createdFrom,
        DateTime? createdTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountBySalesPersonAsync(
        Guid salesPersonId,
        IReadOnlyCollection<LeadStatus>? statuses,
        LeadScore? score,
        string? search,
        DateTime? createdFrom,
        DateTime? createdTo,
        CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(
        IReadOnlyCollection<LeadStatus>? statuses,
        LeadScore? score,
        string? search,
        DateTime? createdFrom,
        DateTime? createdTo,
        CancellationToken cancellationToken = default);
    
    // Dashboard methods
    Task<int> CountByStatusAsync(LeadStatus status, string? salesPersonId, CancellationToken cancellationToken = default);
    Task<int> CountCreatedSinceAsync(DateTime since, string? salesPersonId, CancellationToken cancellationToken = default);
    Task<int> CountByStatusSinceAsync(LeadStatus status, DateTime since, string? salesPersonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lead>> GetHotLeadsAsync(string? salesPersonId, DateTime lastInteractionBefore, int limit, CancellationToken cancellationToken = default);
}