package com.gestauto.vehicleevaluation.infra.messaging;

import com.gestauto.vehicleevaluation.domain.event.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.core.Message;
import org.springframework.amqp.rabbit.annotation.RabbitHandler;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

/**
 * Listener de eventos de domínio do RabbitMQ.
 *
 * Este componente demonstra como consumir eventos publicados
 * na queue de avaliação de veículos. Em produção, outros bounded contexts
 * (como Commercial) teriam seus próprios listeners.
 */
@Component
@RabbitListener(queues = "${rabbitmq.queue.vehicle-evaluation:vehicle-evaluation.events}")
public class VehicleEvaluationEventListener {

    private static final Logger log = LoggerFactory.getLogger(VehicleEvaluationEventListener.class);

    /**
     * Processa eventos de criação de avaliação.
     */
    @RabbitHandler
    public void handleEvaluationCreated(EvaluationCreatedEvent event) {
        log.info("Processing EvaluationCreated event: evaluationId={}, plate={}, brand={}, model={}", 
                event.getEvaluationId(), event.getPlate(), event.getBrand(), event.getModel());
        
        // Lógica de processamento aqui
        // Por exemplo: notificar sistema de estoque sobre nova avaliação
    }

    /**
     * Processa eventos de submissão de avaliação.
     */
    @RabbitHandler
    public void handleEvaluationSubmitted(EvaluationSubmittedEvent event) {
        log.info("Processing EvaluationSubmitted event: evaluationId={}, plate={}, suggestedValue={}", 
                event.getEvaluationId(), event.getPlate(), event.getSuggestedValue());
        
        // Lógica de processamento aqui
        // Por exemplo: notificar gerentes sobre avaliação pendente
    }

    /**
     * Processa eventos de aprovação de avaliação.
     */
    @RabbitHandler
    public void handleEvaluationApproved(EvaluationApprovedEvent event) {
        log.info("Processing EvaluationApproved event: evaluationId={}, plate={}, approvedValue={}, validUntil={}", 
                event.getEvaluationId(), event.getPlate(), event.getApprovedValue(), event.getValidUntil());
        
        // Lógica de processamento aqui
        // Por exemplo: integrar com bounded context Commercial para criar proposta
        // ou adicionar veículo ao inventário
    }

    /**
     * Processa eventos de rejeição de avaliação.
     */
    @RabbitHandler
    public void handleEvaluationRejected(EvaluationRejectedEvent event) {
        log.info("Processing EvaluationRejected event: evaluationId={}, plate={}, reason={}", 
                event.getEvaluationId(), event.getPlate(), event.getRejectionReason());
        
        // Lógica de processamento aqui
        // Por exemplo: notificar avaliador sobre rejeição
    }

    /**
     * Processa eventos de conclusão de avaliação de veículo.
     */
    @RabbitHandler
    public void handleVehicleEvaluationCompleted(VehicleEvaluationCompletedEvent event) {
        log.info("Processing VehicleEvaluationCompleted event: evaluationId={}, plate={}, brand={} {} {}, finalValue={}", 
                event.getEvaluationId(), event.getPlate(), event.getBrand(), event.getModel(), 
                event.getYear(), event.getFinalValue());
        
        // Lógica de processamento aqui
        // Por exemplo: adicionar veículo ao sistema de estoque com todas as informações
        // ou enviar para sistema de precificação
    }

    /**
     * Processa eventos de checklist completado.
     */
    @RabbitHandler
    public void handleChecklistCompleted(ChecklistCompletedEvent event) {
        log.info("Processing ChecklistCompleted event: evaluationId={}", event.getEvaluationId());
        
        // Lógica de processamento aqui
    }

    /**
     * Processa eventos de cálculo de avaliação.
     */
    @RabbitHandler
    public void handleValuationCalculated(ValuationCalculatedEvent event) {
        log.info("Processing ValuationCalculated event: evaluationId={}", event.getEvaluationId());
        
        // Lógica de processamento aqui
    }

    /**
     * Handler padrão para eventos não mapeados.
     */
    @RabbitHandler(isDefault = true)
    public void handleUnknownEvent(Object event) {
        log.warn("Received unknown event type: {}", event.getClass().getName());
    }
}
