using GestAuto.Commercial.Domain.Events;

namespace GestAuto.Commercial.Domain.Interfaces;

/// <summary>
/// Interface para publicação de eventos de domínio.
/// Implementações desta interface são responsáveis por enviar eventos
/// para sistemas externos (RabbitMQ, Event Hub, etc).
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publica um evento de domínio de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo do evento que implementa IDomainEvent</typeparam>
    /// <param name="domainEvent">Evento de domínio a ser publicado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task completada quando o evento é publicado</returns>
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken)
        where T : IDomainEvent;
}
