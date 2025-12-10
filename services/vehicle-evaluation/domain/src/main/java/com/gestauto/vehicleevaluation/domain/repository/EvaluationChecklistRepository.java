package com.gestauto.vehicleevaluation.domain.repository;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;

import java.util.List;
import java.util.Optional;

/**
 * Repositório para persistência de checklists de avaliação.
 *
 * Interface de domínio que define operações de persistência
 * para checklists técnicos associados a avaliações.
 */
public interface EvaluationChecklistRepository {

    /**
     * Salva um checklist de avaliação.
     *
     * @param checklist checklist a ser salvo
     * @return checklist salvo
     */
    EvaluationChecklist save(EvaluationChecklist checklist);

    /**
     * Atualiza um checklist existente.
     *
     * @param checklist checklist a ser atualizado
     * @return checklist atualizado
     */
    EvaluationChecklist update(EvaluationChecklist checklist);

    /**
     * Busca checklist por ID da avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return checklist encontrado ou Optional.empty()
     */
    Optional<EvaluationChecklist> findByEvaluationId(EvaluationId evaluationId);

    /**
     * Remove checklist por ID da avaliação.
     *
     * @param evaluationId ID da avaliação
     */
    void deleteByEvaluationId(EvaluationId evaluationId);

    /**
     * Lista checklists incompletos.
     *
     * @return lista de checklists que não foram concluídos
     */
    List<EvaluationChecklist> findIncomplete();

    /**
     * Lista checklists com pontuação geral baixa.
     *
     * @param minScore pontuação mínima
     * @return lista de checklists com pontuação abaixo do mínimo
     */
    List<EvaluationChecklist> findByOverallScoreLessThan(double minScore);

    /**
     * Lista checklists com problemas críticos.
     *
     * @return lista de checklists com itens críticos marcados
     */
    List<EvaluationChecklist> findWithCriticalIssues();

    /**
     * Verifica se existe checklist para a avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return true se existir checklist
     */
    boolean existsByEvaluationId(EvaluationId evaluationId);

    /**
     * Conta checklists completos por avaliador.
     *
     * @param evaluatorId ID do avaliador
     * @return quantidade de checklists completos
     */
    long countCompletedByEvaluator(String evaluatorId);

    /**
     * Lista checklists criados em um período.
     *
     * @param startDate data de início
     * @param endDate data de fim
     * @return lista de checklists no período
     */
    List<EvaluationChecklist> findByCreatedAtBetween(
        java.time.LocalDateTime startDate,
        java.time.LocalDateTime endDate
    );

    /**
     * Busca checklists por avaliador.
     *
     * @param evaluatorId ID do avaliador
     * @return lista de checklists do avaliador
     */
    List<EvaluationChecklist> findByEvaluatorId(String evaluatorId);
}