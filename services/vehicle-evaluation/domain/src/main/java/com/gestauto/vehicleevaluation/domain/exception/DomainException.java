package com.gestauto.vehicleevaluation.domain.exception;

/**
 * Exceção base para todas as exceções de domínio.
 *
 * Esta classe serve como superclasse para todas as exceções
 * que representam violações de regras de negócio no domínio
 * de avaliação de veículos.
 */
public class DomainException extends RuntimeException {

    /**
     * Constrói uma nova exceção de domínio.
     *
     * @param message mensagem de erro
     */
    public DomainException(String message) {
        super(message);
    }

    public DomainException(String message, Throwable cause) {
        super(message, cause);
    }
}
