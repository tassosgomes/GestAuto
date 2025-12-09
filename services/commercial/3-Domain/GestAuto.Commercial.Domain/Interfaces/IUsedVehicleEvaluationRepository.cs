using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface IUsedVehicleEvaluationRepository
{
    Task<UsedVehicleEvaluation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsedVehicleEvaluation>> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task<UsedVehicleEvaluation> AddAsync(UsedVehicleEvaluation evaluation, CancellationToken cancellationToken = default);
    Task UpdateAsync(UsedVehicleEvaluation evaluation, CancellationToken cancellationToken = default);
}
