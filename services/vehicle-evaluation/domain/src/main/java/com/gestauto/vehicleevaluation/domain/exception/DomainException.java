package com.gestauto.vehicleevaluation.domain.exception;

/**
 * Exceção base para todas as exceções de domínio.
 *
 * Esta classe serve como superclasse para todas as exceções
 * que representam violações de regras de negócio no domínio
 * de avaliação de veículos.
 */
public abstract class DomainException extends RuntimeException {

    /**
     * Constrói uma nova exceção de domínio.
     *
     * @param message mensagem de erro
     */
    protected DomainException(String message) {
        super(message);
    }

    /**
     * Constrói uma nova exceção de domínio com causa.
     *
     * @param message mensagem de erro
     * @param cause causa raiz da exceção
     */
    protected DomainException(String message, Throwable cause) {
        super(message, cause);
    }
}