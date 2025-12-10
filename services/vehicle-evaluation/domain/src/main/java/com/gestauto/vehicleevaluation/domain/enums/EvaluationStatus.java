package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Status de uma avaliação de veículo.
 *
 * Esta enumeração representa o ciclo de vida completo de uma avaliação,
 * desde sua criação até a aprovação ou rejeição final.
 */
public enum EvaluationStatus {

    /**
     * Avaliação em rascunho, ainda em preenchimento.
     * Status inicial quando uma avaliação é criada.
     */
    DRAFT("Rascunho"),

    /**
     * Avaliação em andamento, com dados sendo preenchidos.
     * Status intermediário durante o preenchimento.
     */
    IN_PROGRESS("Em Andamento"),

    /**
     * Avaliação submetida para aprovação gerencial.
     * Avaliação completa aguardando decisão do gerente.
     */
    PENDING_APPROVAL("Aguardando Aprovação"),

    /**
     * Avaliação aprovada pelo gerente.
     * Status final positivo, valor definido e laudo gerado.
     */
    APPROVED("Aprovada"),

    /**
     * Avaliação rejeitada pelo gerente.
     * Status final negativo com justificativa.
     */
    REJECTED("Rejeitada"),

    /**
     * Avaliação expirada.
     * Laudo perdeu validade (após 72 horas da aprovação).
     */
    EXPIRED("Expirada"),

    /**
     * Avaliação cancelada pelo avaliador.
     * Cancelamento antes da submissão.
     */
    CANCELLED("Cancelada");

    private final String description;

    EvaluationStatus(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }

    /**
     * Verifica se o status é um status final (não pode mais ser alterado).
     *
     * @return true se for status final
     */
    public boolean isFinal() {
        return this == APPROVED || this == REJECTED || this == CANCELLED;
    }

    /**
     * Verifica se o status permite edição dos dados da avaliação.
     *
     * @return true se permitir edição
     */
    public boolean isEditable() {
        return this == DRAFT || this == IN_PROGRESS;
    }

    /**
     * Verifica se o status permite submissão para aprovação.
     *
     * @return true se permitir submissão
     */
    public boolean canBeSubmitted() {
        return this == DRAFT || this == IN_PROGRESS;
    }

    /**
     * Verifica se o status permite aprovação.
     *
     * @return true se permitir aprovação
     */
    public boolean canBeApproved() {
        return this == PENDING_APPROVAL;
    }

    /**
     * Verifica se o status permite rejeição.
     *
     * @return true se permitir rejeição
     */
    public boolean canBeRejected() {
        return this == PENDING_APPROVAL;
    }

    /**
     * Verifica se o status está ativo (não cancelado ou expirado).
     *
     * @return true se estiver ativo
     */
    public boolean isActive() {
        return this != CANCELLED && this != EXPIRED;
    }
}