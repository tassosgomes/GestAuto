package com.gestauto.vehicleevaluation.infra.entity;

/**
 * Enum JPA para mapeamento do status da avaliação.
 *
 * Versão do enum do domínio adaptada para persistência JPA.
 */
public enum EvaluationStatusJpa {
    DRAFT("Draft"),
    IN_PROGRESS("In Progress"),
    PENDING_APPROVAL("Pending Approval"),
    APPROVED("Approved"),
    REJECTED("Rejected"),
    CANCELLED("Cancelled"),
    EXPIRED("Expired");

    private final String description;

    EvaluationStatusJpa(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }
}