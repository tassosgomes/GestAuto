package com.gestauto.vehicleevaluation.domain.exception;

/**
 * Exceção lançada quando uma avaliação não é encontrada.
 *
 * Esta exceção representa o erro de negócio que ocorre
 * ao tentar acessar uma avaliação que não existe no sistema.
 */
public class EvaluationNotFoundException extends DomainException {

    private final String evaluationId;

    /**
     * Constrói uma nova exceção de avaliação não encontrada.
     *
     * @param evaluationId ID da avaliação não encontrada
     */
    public EvaluationNotFoundException(String evaluationId) {
        super("Evaluation not found: " + evaluationId);
        this.evaluationId = evaluationId;
    }

    /**
     * Retorna o ID da avaliação não encontrada.
     *
     * @return ID da avaliação
     */
    public String getEvaluationId() {
        return evaluationId;
    }
}