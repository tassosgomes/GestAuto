using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly CommercialDbContext _context;

    public ProposalRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<Proposal?> GetByIdAsync(Guid id)
    {
        return await _context.Proposals
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Proposal proposal)
    {
        await _context.Proposals.AddAsync(proposal);
    }

    public Task UpdateAsync(Proposal proposal)
    {
        _context.Proposals.Update(proposal);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Proposal>> GetByLeadIdAsync(Guid leadId)
    {
        return await _context.Proposals
            .Include(p => p.Items)
            .Where(p => p.LeadId == leadId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Proposal>> ListByLeadAsync(
        Guid leadId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .Include(p => p.Items)
            .Where(p => p.LeadId == leadId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Proposal>> ListByStatusAsync(
        ProposalStatus status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .Include(p => p.Items)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Proposal>> ListValidProposalsAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .Include(p => p.Items)
            .Where(p => p.Status == ProposalStatus.AwaitingCustomer)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}