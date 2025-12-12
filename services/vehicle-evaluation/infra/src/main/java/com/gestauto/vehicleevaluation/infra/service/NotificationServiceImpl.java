package com.gestauto.vehicleevaluation.infra.service;

import com.gestauto.vehicleevaluation.domain.service.NotificationService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Service;

/**
 * Implementação do serviço de notificações.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class NotificationServiceImpl implements NotificationService {

    private final RabbitTemplate rabbitTemplate;
    // TODO: adicionar EmailService quando disponível

    @Override
    public void notifyEvaluator(String evaluatorId, String subject, String message) {
        log.info("Enviando notificação para avaliador {}: {}", evaluatorId, subject);

        try {
            // 1. Salvar notificação no BD (se houver entidade Notification)
            // TODO: implementar persistência de notificações

            // 2. Enviar email (quando EmailService estiver disponível)
            // emailService.sendEmail(evaluatorId, subject, message);

            // 3. Publicar evento para WebSocket/mobile push via RabbitMQ
            NotificationEvent event = new NotificationEvent(evaluatorId, subject, message);
            rabbitTemplate.convertAndSend("gestauto.notifications", event);

            log.info("Notificação enviada com sucesso para avaliador: {}", evaluatorId);

        } catch (Exception e) {
            log.error("Erro ao enviar notificação para avaliador {}: {}", evaluatorId, e.getMessage());
            // Não lançar exception para não quebrar fluxo de aprovação
        }
    }

    /**
     * Evento de notificação para RabbitMQ.
     */
    public static class NotificationEvent {
        private final String evaluatorId;
        private final String subject;
        private final String message;

        public NotificationEvent(String evaluatorId, String subject, String message) {
            this.evaluatorId = evaluatorId;
            this.subject = subject;
            this.message = message;
        }

        public String getEvaluatorId() { return evaluatorId; }
        public String getSubject() { return subject; }
        public String getMessage() { return message; }
    }
}