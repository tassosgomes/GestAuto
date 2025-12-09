using GestAuto.Commercial.Domain.Events;

namespace GestAuto.Commercial.Domain.Interfaces;

/// <summary>
/// Interface para Unit of Work pattern
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Adiciona um evento de domínio à fila de eventos
    /// </summary>
    /// <param name="domainEvent">Evento de domínio a ser adicionado</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Persiste as mudanças no contexto e publica eventos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de entidades afetadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia uma transação
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma a transação atual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Desfaz a transação atual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}