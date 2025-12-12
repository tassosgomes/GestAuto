package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

/**
 * Evento disparado quando um checklist técnico é completado.
 *
 * Este evento permite que outros bounded contexts reajam à
 * conclusão do checklist, como o sistema de aprovação.
 */
public final class ChecklistCompletedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final UUID evaluationId;
    private final int conservationScore;
    private final boolean hasBlockingIssues;
    private final List<String> criticalIssues;

    public ChecklistCompletedEvent(UUID evaluationId, int conservationScore,
                                   boolean hasBlockingIssues, List<String> criticalIssues) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.conservationScore = conservationScore;
        this.hasBlockingIssues = hasBlockingIssues;
        this.criticalIssues = criticalIssues;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "ChecklistCompleted";
    }

    @Override
    public String getEvaluationId() {
        return evaluationId != null ? evaluationId.toString() : null;
    }

    @Override
    public String getEventData() {
        return String.format(
            "{" +
            "\"conservationScore\":%d," +
            "\"hasBlockingIssues\":%s," +
            "\"criticalIssuesCount\":%d" +
            "}",
            conservationScore,
            hasBlockingIssues,
            criticalIssues != null ? criticalIssues.size() : 0
        );
    }

    public UUID getEvaluationUuid() {
        return evaluationId;
    }

    public int getConservationScore() {
        return conservationScore;
    }

    public boolean isHasBlockingIssues() {
        return hasBlockingIssues;
    }

    public List<String> getCriticalIssues() {
        return criticalIssues;
    }

    @Override
    public String toString() {
        return "ChecklistCompletedEvent{" +
                "occurredAt=" + occurredAt +
                "evaluationId=" + evaluationId +
                ", conservationScore=" + conservationScore +
                ", hasBlockingIssues=" + hasBlockingIssues +
                ", criticalIssuesCount=" + (criticalIssues != null ? criticalIssues.size() : 0) +
                '}';
    }
}
