package com.gestauto.vehicleevaluation.domain.repository;

import com.gestauto.vehicleevaluation.domain.event.DomainEvent;

import java.time.LocalDateTime;
import java.util.List;

/**
 * Repositório para persistência de eventos de domínio.
 *
 * Interface de domínio que define operações de persistência
 * para eventos emitidos pelas agregações de domínio.
 */
public interface DomainEventRepository {

    /**
     * Salva um evento de domínio.
     *
     * @param event evento a ser salvo
     */
    void save(DomainEvent event);

    /**
     * Lista eventos pendentes de processamento.
     *
     * @param limit limite máximo de eventos
     * @return lista de eventos não processados
     */
    List<DomainEvent> findPendingEvents(int limit);

    /**
     * Marca evento como processado.
     *
     * @param eventId ID do evento
     * @param processedAt data do processamento
     */
    void markAsProcessed(String eventId, LocalDateTime processedAt);

    /**
     * Lista eventos por tipo.
     *
     * @param eventType tipo do evento
     * @param limit limite máximo de eventos
     * @return lista de eventos do tipo especificado
     */
    List<DomainEvent> findByEventType(String eventType, int limit);

    /**
     * Lista eventos por agregação.
     *
     * @param aggregateId ID da agregação
     * @param limit limite máximo de eventos
     * @return lista de eventos da agregação
     */
    List<DomainEvent> findByAggregateId(String aggregateId, int limit);

    /**
     * Lista eventos emitidos em um período.
     *
     * @param startDate data de início
     * @param endDate data de fim
     * @return lista de eventos no período
     */
    List<DomainEvent> findByOccurredAtBetween(LocalDateTime startDate, LocalDateTime endDate);

    /**
     * Conta eventos pendentes de processamento.
     *
     * @return quantidade de eventos pendentes
     */
    long countPendingEvents();

    /**
     * Remove eventos antigos já processados.
     *
     * @param olderThan data limite para remoção
     * @return quantidade de eventos removidos
     */
    long deleteProcessedEventsOlderThan(LocalDateTime olderThan);
}