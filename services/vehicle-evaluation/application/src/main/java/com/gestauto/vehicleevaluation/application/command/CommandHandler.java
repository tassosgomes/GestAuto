package com.gestauto.vehicleevaluation.application.command;

/**
 * Interface genérica para handlers de comando CQRS.
 *
 * @param <C> Tipo do comando
 * @param <R> Tipo do resultado retornado pelo comando
 */
@FunctionalInterface
public interface CommandHandler<C, R> {

    /**
     * Executa o comando e retorna o resultado.
     *
     * @param command comando a ser executado
     * @return resultado da execução do comando
     * @throws Exception caso ocorra erro durante a execução
     */
    R handle(C command) throws Exception;
}