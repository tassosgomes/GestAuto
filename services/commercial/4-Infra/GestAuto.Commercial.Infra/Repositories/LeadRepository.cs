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

    public async Task<Lead?> GetByIdAsync(Guid id)
    {
        return await _context.Leads
            .Include(l => l.Qualification)
            .Include(l => l.Interactions)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Lead>> GetBySalesPersonIdAsync(Guid salesPersonId)
    {
        return await _context.Leads
            .Include(l => l.Qualification)
            .Where(l => l.SalesPersonId == salesPersonId)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Lead lead)
    {
        await _context.Leads.AddAsync(lead);
    }

    public Task UpdateAsync(Lead lead)
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
}