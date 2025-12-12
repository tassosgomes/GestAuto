package com.gestauto.vehicleevaluation.application.command;

import java.time.LocalDateTime;

/**
 * Evento de domínio publicado quando uma avaliação é rejeitada.
 */
public class EvaluationRejectedEvent {

    private final String evaluationId;
    private final String approverId;
    private final String reason;
    private final LocalDateTime rejectedAt;
    private final LocalDateTime occurredAt;

    public EvaluationRejectedEvent(String evaluationId, String approverId, String reason, LocalDateTime rejectedAt) {
        this.evaluationId = evaluationId;
        this.approverId = approverId;
        this.reason = reason;
        this.rejectedAt = rejectedAt;
        this.occurredAt = LocalDateTime.now();
    }

    public String getEvaluationId() {
        return evaluationId;
    }

    public String getApproverId() {
        return approverId;
    }

    public String getReason() {
        return reason;
    }

    public LocalDateTime getRejectedAt() {
        return rejectedAt;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String toString() {
        return "EvaluationRejectedEvent{" +
                "evaluationId='" + evaluationId + '\'' +
                ", approverId='" + approverId + '\'' +
                ", reason='" + reason + '\'' +
                ", rejectedAt=" + rejectedAt +
                ", occurredAt=" + occurredAt +
                '}';
    }
}
