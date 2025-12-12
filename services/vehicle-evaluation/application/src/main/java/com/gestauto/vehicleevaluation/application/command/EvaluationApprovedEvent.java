package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.value.Money;

import java.time.LocalDateTime;

/**
 * Evento de domínio publicado quando uma avaliação é aprovada.
 */
public class EvaluationApprovedEvent {

    private final String evaluationId;
    private final String approverId;
    private final Money approvedValue;
    private final LocalDateTime approvedAt;
    private final LocalDateTime occurredAt;

    public EvaluationApprovedEvent(String evaluationId, String approverId, Money approvedValue, LocalDateTime approvedAt) {
        this.evaluationId = evaluationId;
        this.approverId = approverId;
        this.approvedValue = approvedValue;
        this.approvedAt = approvedAt;
        this.occurredAt = LocalDateTime.now();
    }

    public String getEvaluationId() {
        return evaluationId;
    }

    public String getApproverId() {
        return approverId;
    }

    public Money getApprovedValue() {
        return approvedValue;
    }

    public LocalDateTime getApprovedAt() {
        return approvedAt;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String toString() {
        return "EvaluationApprovedEvent{" +
                "evaluationId='" + evaluationId + '\'' +
                ", approverId='" + approverId + '\'' +
                ", approvedValue=" + approvedValue +
                ", approvedAt=" + approvedAt +
                ", occurredAt=" + occurredAt +
                '}';
    }
}
