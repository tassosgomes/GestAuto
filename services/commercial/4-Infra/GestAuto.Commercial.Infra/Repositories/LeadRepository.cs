using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly CommercialDbContext _context;

    public LeadRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Qualification)
                .ThenInclude(q => q!.TradeInVehicle)
            .Include(l => l.Interactions)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetBySalesPersonIdAsync(Guid salesPersonId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Qualification)
            .Where(l => l.SalesPersonId == salesPersonId)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(lead, cancellationToken);
        return lead;
    }

    public Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Update(lead);
        return Task.CompletedTask;
    }

    // Extended methods for task specifications
    public async Task<IReadOnlyList<Lead>> ListBySalesPersonAsync(
        Guid salesPersonId, 
        LeadStatus? status,
        LeadScore? score,
        int page, 
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads
            .Include(l => l.Qualification)
            .Where(l => l.SalesPersonId == salesPersonId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (score.HasValue)
            query = query.Where(l => l.Score == score.Value);

        return await query
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountBySalesPersonAsync(
        Guid salesPersonId,
        LeadStatus? status,
        LeadScore? score,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.Where(l => l.SalesPersonId == salesPersonId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (score.HasValue)
            query = query.Where(l => l.Score == score.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lead>> ListAllAsync(
        LeadStatus? status,
        LeadScore? score,
        int page, 
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.Include(l => l.Qualification).AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (score.HasValue)
            query = query.Where(l => l.Score == score.Value);

        return await query
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(
        LeadStatus? status,
        LeadScore? score,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (score.HasValue)
            query = query.Where(l => l.Score == score.Value);

        return await query.CountAsync(cancellationToken);
    }

    // Dashboard methods
    public async Task<int> CountByStatusAsync(
        LeadStatus status,
        string? salesPersonId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.Where(l => l.Status == status);

        if (!string.IsNullOrEmpty(salesPersonId) && Guid.TryParse(salesPersonId, out var salesPersonGuid))
            query = query.Where(l => l.SalesPersonId == salesPersonGuid);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountCreatedSinceAsync(
        DateTime since,
        string? salesPersonId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.Where(l => l.CreatedAt >= since);

        if (!string.IsNullOrEmpty(salesPersonId) && Guid.TryParse(salesPersonId, out var salesPersonGuid))
            query = query.Where(l => l.SalesPersonId == salesPersonGuid);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountByStatusSinceAsync(
        LeadStatus status,
        DateTime since,
        string? salesPersonId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads
            .Where(l => l.Status == status && l.CreatedAt >= since);

        if (!string.IsNullOrEmpty(salesPersonId) && Guid.TryParse(salesPersonId, out var salesPersonGuid))
            query = query.Where(l => l.SalesPersonId == salesPersonGuid);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lead>> GetHotLeadsAsync(
        string? salesPersonId,
        DateTime lastInteractionBefore,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads
            .Include(l => l.Qualification)
            .Include(l => l.Interactions)
            .Where(l =>
                (l.Score == LeadScore.Diamond || l.Score == LeadScore.Gold) &&
                l.Status != LeadStatus.Converted &&
                l.Status != LeadStatus.Lost);

        if (!string.IsNullOrEmpty(salesPersonId) && Guid.TryParse(salesPersonId, out var salesPersonGuid))
            query = query.Where(l => l.SalesPersonId == salesPersonGuid);

        // Filter leads without recent interactions
        var hotLeads = await query.ToListAsync(cancellationToken);

        return hotLeads
            .Where(l => !l.Interactions.Any() ||
                       l.Interactions.Max(i => i.InteractionDate) < lastInteractionBefore)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToList();
    }
}