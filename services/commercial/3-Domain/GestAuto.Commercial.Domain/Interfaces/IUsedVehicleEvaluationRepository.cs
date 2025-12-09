using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface IUsedVehicleEvaluationRepository
{
    Task<UsedVehicleEvaluation?> GetByIdAsync(Guid id);
    Task<UsedVehicleEvaluation?> GetByProposalIdAsync(Guid proposalId);
    Task AddAsync(UsedVehicleEvaluation evaluation);
    Task UpdateAsync(UsedVehicleEvaluation evaluation);
}
