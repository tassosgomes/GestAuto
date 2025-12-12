package com.gestauto.vehicleevaluation.domain.service;

/**
 * Serviço para envio de notificações aos usuários.
 */
public interface NotificationService {

    /**
     * Notifica avaliador sobre atualização da avaliação.
     *
     * @param evaluatorId ID do avaliador
     * @param subject assunto da notificação
     * @param message mensagem da notificação
     */
    void notifyEvaluator(String evaluatorId, String subject, String message);
}