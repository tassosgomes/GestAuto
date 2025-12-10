package com.gestauto.vehicleevaluation.domain.exception;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;

/**
 * Exceção lançada quando uma operação é inválida para o status atual da avaliação.
 *
 * Esta exceção representa violações de fluxo de status que não
 * seguem as regras de negócio estabelecidas.
 */
public class InvalidEvaluationStatusException extends DomainException {

    private final EvaluationStatus currentStatus;
    private final String operation;

    /**
     * Constrói uma nova exceção de status inválido.
     *
     * @param currentStatus status atual da avaliação
     * @param operation operação que foi tentada
     */
    public InvalidEvaluationStatusException(EvaluationStatus currentStatus, String operation) {
        super(String.format("Cannot %s evaluation in status %s", operation, currentStatus.getDescription()));
        this.currentStatus = currentStatus;
        this.operation = operation;
    }

    /**
     * Constrói uma nova exceção de status inválido com mensagem customizada.
     *
     * @param currentStatus status atual da avaliação
     * @param operation operação que foi tentada
     * @param message mensagem detalhada
     */
    public InvalidEvaluationStatusException(EvaluationStatus currentStatus, String operation, String message) {
        super(message);
        this.currentStatus = currentStatus;
        this.operation = operation;
    }

    /**
     * Retorna o status atual da avaliação.
     *
     * @return status atual
     */
    public EvaluationStatus getCurrentStatus() {
        return currentStatus;
    }

    /**
     * Retorna a operação que foi tentada.
     *
     * @return operação
     */
    public String getOperation() {
        return operation;
    }
}