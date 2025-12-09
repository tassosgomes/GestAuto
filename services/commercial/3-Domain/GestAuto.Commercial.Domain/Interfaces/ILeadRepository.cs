using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lead>> GetBySalesPersonIdAsync(Guid salesPersonId);
    Task AddAsync(Lead lead);
    Task UpdateAsync(Lead lead);
}