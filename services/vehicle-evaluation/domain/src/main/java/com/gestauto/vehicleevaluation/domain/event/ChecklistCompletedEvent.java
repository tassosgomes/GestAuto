package com.gestauto.vehicleevaluation.domain.event;

import java.util.List;
import java.util.UUID;

/**
 * Evento disparado quando um checklist técnico é completado.
 *
 * Este evento permite que outros bounded contexts reajam à
 * conclusão do checklist, como o sistema de aprovação.
 */
public class ChecklistCompletedEvent extends DomainEvent {

    private final UUID evaluationId;
    private final int conservationScore;
    private final boolean hasBlockingIssues;
    private final List<String> criticalIssues;

    public ChecklistCompletedEvent(UUID evaluationId, int conservationScore,
                                   boolean hasBlockingIssues, List<String> criticalIssues) {
        super();
        this.evaluationId = evaluationId;
        this.conservationScore = conservationScore;
        this.hasBlockingIssues = hasBlockingIssues;
        this.criticalIssues = criticalIssues;
    }

    public UUID getEvaluationId() {
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
                "evaluationId=" + evaluationId +
                ", conservationScore=" + conservationScore +
                ", hasBlockingIssues=" + hasBlockingIssues +
                ", criticalIssuesCount=" + (criticalIssues != null ? criticalIssues.size() : 0) +
                '}';
    }
}
