package com.gestauto.vehicleevaluation.domain.repository;

import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;

import java.util.List;
import java.util.Optional;

/**
 * Repositório para persistência de itens de depreciação.
 *
 * Interface de domínio que define operações de persistência
 * para itens de depreciação associados a avaliações.
 */
public interface DepreciationItemRepository {

    /**
     * Salva um item de depreciação.
     *
     * @param depreciationItem item a ser salvo
     * @return item salvo com ID gerado
     */
    DepreciationItem save(DepreciationItem depreciationItem);

    /**
     * Remove um item por ID.
     *
     * @param depreciationId ID do item a ser removido
     */
    void deleteById(String depreciationId);

    /**
     * Busca item por ID.
     *
     * @param depreciationId ID do item
     * @return item encontrado ou Optional.empty()
     */
    Optional<DepreciationItem> findById(String depreciationId);

    /**
     * Lista itens por avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return lista de itens da avaliação
     */
    List<DepreciationItem> findByEvaluationId(EvaluationId evaluationId);

    /**
     * Lista itens por categoria.
     *
     * @param category categoria dos itens
     * @return lista de itens da categoria
     */
    List<DepreciationItem> findByCategory(String category);

    /**
     * Lista itens por avaliação e categoria.
     *
     * @param evaluationId ID da avaliação
     * @param category categoria dos itens
     * @return lista de itens filtrados
     */
    List<DepreciationItem> findByEvaluationIdAndCategory(EvaluationId evaluationId, String category);

    /**
     * Lista itens criados por usuário.
     *
     * @param createdBy ID do usuário que criou
     * @return lista de itens criados pelo usuário
     */
    List<DepreciationItem> findByCreatedBy(String createdBy);

    /**
     * Calcula valor total de depreciação por avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return valor total das depreciações
     */
    Money sumDepreciationByEvaluationId(EvaluationId evaluationId);

    /**
     * Conta itens por avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return quantidade de itens da avaliação
     */
    long countByEvaluationId(EvaluationId evaluationId);

    /**
     * Remove todos os itens de uma avaliação.
     *
     * @param evaluationId ID da avaliação
     */
    void deleteAllByEvaluationId(EvaluationId evaluationId);

    /**
     * Lista itens com valor acima de um determinado valor.
     *
     * @param minValue valor mínimo
     * @return lista de itens acima do valor
     */
    List<DepreciationItem> findByDepreciationValueGreaterThan(Money minValue);

    /**
     * Lista itens criados em um período.
     *
     * @param evaluationId ID da avaliação
     * @param startDate data de início
     * @param endDate data de fim
     * @return lista de itens no período
     */
    List<DepreciationItem> findByEvaluationIdAndCreatedAtBetween(
        EvaluationId evaluationId,
        java.time.LocalDateTime startDate,
        java.time.LocalDateTime endDate
    );
}