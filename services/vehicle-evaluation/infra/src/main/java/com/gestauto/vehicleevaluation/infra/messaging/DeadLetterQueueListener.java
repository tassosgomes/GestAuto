package com.gestauto.vehicleevaluation.infra.messaging;

import com.gestauto.vehicleevaluation.domain.event.DomainEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.core.Message;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

/**
 * Listener da Dead Letter Queue (DLQ) para processar eventos falhados.
 *
 * Este componente processa mensagens que falharam após todas as tentativas
 * de retry, permitindo análise, logging detalhado e possível reprocessamento manual.
 */
@Component
public class DeadLetterQueueListener {

    private static final Logger log = LoggerFactory.getLogger(DeadLetterQueueListener.class);

    /**
     * Processa eventos que falharam após todas as tentativas.
     *
     * @param failedMessage mensagem que falhou
     */
    @RabbitListener(queues = "${rabbitmq.queue.vehicle-evaluation.dlq:vehicle-evaluation.events.dlq}")
    public void handleFailedEvent(Message failedMessage) {
        String messageId = failedMessage.getMessageProperties().getMessageId();
        String eventType = (String) failedMessage.getMessageProperties().getHeader("eventType");
        String evaluationId = (String) failedMessage.getMessageProperties().getHeader("evaluationId");
        
        log.error("Processing failed event from DLQ: messageId={}, eventType={}, evaluationId={}", 
                messageId, eventType, evaluationId);
        
        // Log detalhado para análise
        log.error("Failed message headers: {}", failedMessage.getMessageProperties().getHeaders());
        log.error("Failed message body: {}", new String(failedMessage.getBody()));
        
        // Aqui você pode implementar:
        // 1. Persistir em banco de dados para análise posterior
        // 2. Enviar notificação para equipe de operações
        // 3. Criar ticket automático no sistema de suporte
        // 4. Tentar reprocessamento manual após correção
        
        // TODO: Implementar persistência de eventos falhados para reprocessamento
        // TODO: Implementar notificação via e-mail/Slack para equipe de operações
    }
}
