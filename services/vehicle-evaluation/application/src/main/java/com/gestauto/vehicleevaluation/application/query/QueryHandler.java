package com.gestauto.vehicleevaluation.application.query;

/**
 * Interface genérica para handlers de consulta CQRS.
 *
 * @param <Q> Tipo da consulta
 * @param <R> Tipo do resultado retornado pela consulta
 */
@FunctionalInterface
public interface QueryHandler<Q, R> {

    /**
     * Executa a consulta e retorna o resultado.
     *
     * @param query consulta a ser executada
     * @return resultado da consulta
     * @throws Exception caso ocorra erro durante a execução
     */
    R handle(Q query) throws Exception;
}