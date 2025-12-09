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
}