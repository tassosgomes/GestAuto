using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra.Repositories;

public class UsedVehicleEvaluationRepository : IUsedVehicleEvaluationRepository
{
    private readonly CommercialDbContext _context;

    public UsedVehicleEvaluationRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<UsedVehicleEvaluation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UsedVehicleEvaluations
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UsedVehicleEvaluation>> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        return await _context.UsedVehicleEvaluations
            .Where(e => e.ProposalId == proposalId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<UsedVehicleEvaluation> Items, int TotalCount)> GetPagedAsync(
        Guid? proposalId,
        EvaluationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UsedVehicleEvaluations.AsQueryable();

        // Aplicar filtros
        if (proposalId.HasValue)
        {
            query = query.Where(e => e.ProposalId == proposalId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        // Contar total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação
        var items = await query
            .OrderByDescending(e => e.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<UsedVehicleEvaluation> AddAsync(UsedVehicleEvaluation evaluation, CancellationToken cancellationToken = default)
    {
        await _context.UsedVehicleEvaluations.AddAsync(evaluation, cancellationToken);
        return evaluation;
    }

    public Task UpdateAsync(UsedVehicleEvaluation evaluation, CancellationToken cancellationToken = default)
    {
        _context.UsedVehicleEvaluations.Update(evaluation);
        return Task.CompletedTask;
    }
}