package com.gestauto.vehicleevaluation.domain.repository;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.value.Money;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

/**
 * Repositório para persistência de entidades VehicleEvaluation.
 *
 * Interface de domínio que define operações de persistência
 * sem depender de tecnologias específicas de infraestrutura.
 */
public interface VehicleEvaluationRepository {

    /**
     * Salva uma avaliação de veículo.
     *
     * @param evaluation avaliação a ser salva
     * @return avaliação salva com ID gerado
     */
    VehicleEvaluation save(VehicleEvaluation evaluation);

    /**
     * Atualiza uma avaliação existente.
     *
     * @param evaluation avaliação a ser atualizada
     * @return avaliação atualizada
     */
    VehicleEvaluation update(VehicleEvaluation evaluation);

    /**
     * Busca avaliação por ID.
     *
     * @param id ID da avaliação
     * @return avaliação encontrada ou Optional.empty()
     */
    Optional<VehicleEvaluation> findById(EvaluationId id);

    /**
     * Busca avaliação por token de validação.
     *
     * Usado para validação pública do laudo (QR code).
     *
     * @param validationToken token de validação
     * @return avaliação encontrada ou Optional.empty()
     */
    Optional<VehicleEvaluation> findByValidationToken(String validationToken);

    /**
     * Busca avaliação por placa do veículo.
     *
     * @param plate placa do veículo
     * @return lista de avaliações encontradas
     */
    List<VehicleEvaluation> findByPlate(Plate plate);

    /**
     * Lista avaliações por status.
     *
     * @param status status das avaliações
     * @return lista de avaliações com o status especificado
     */
    List<VehicleEvaluation> findByStatus(EvaluationStatus status);

    /**
     * Lista avaliações por avaliador.
     *
     * @param evaluatorId ID do avaliador
     * @return lista de avaliações do avaliador
     */
    List<VehicleEvaluation> findByEvaluator(String evaluatorId);

    /**
     * Lista avaliações criadas em um período.
     *
     * @param startDate data de início
     * @param endDate data de fim
     * @return lista de avaliações no período
     */
    List<VehicleEvaluation> findByCreatedAtBetween(LocalDateTime startDate, LocalDateTime endDate);

    /**
     * Lista avaliações por status e avaliador.
     *
     * @param status status das avaliações
     * @param evaluatorId ID do avaliador
     * @return lista de avaliações filtradas
     */
    List<VehicleEvaluation> findByStatusAndEvaluator(EvaluationStatus status, String evaluatorId);

    /**
     * Lista avaliações que precisam ser aprovadas.
     *
     * @param limit limite máximo de resultados
     * @return lista de avaliações pendentes de aprovação
     */
    List<VehicleEvaluation> findPendingApproval(int limit);

    /**
     * Lista avaliações pendentes de aprovação com paginação.
     *
     * @param status status das avaliações (PENDING_APPROVAL)
     * @param page número da página (0-based)
     * @param size tamanho da página
     * @return lista de avaliações pendentes paginadas
     */
    List<VehicleEvaluation> findPendingApprovals(EvaluationStatus status, int page, int size);

    /**
     * Lista avaliações aprovadas prestes a expirar.
     *
     * @param hoursUntilExpiration horas até expiração
     * @return lista de avaliações próximas da expiração
     */
    List<VehicleEvaluation> findExpiringSoon(int hoursUntilExpiration);

    /**
     * Busca avaliações por faixa de valor final.
     *
     * @param minValue valor mínimo
     * @param maxValue valor máximo
     * @return lista de avaliações na faixa de valor
     */
    List<VehicleEvaluation> findByFinalValueBetween(Money minValue, Money maxValue);

    /**
     * Verifica se existe avaliação para a placa no status especificado.
     *
     * @param plate placa do veículo
     * @param status status a verificar
     * @return true se existir avaliação ativa
     */
    boolean existsByPlateAndStatus(Plate plate, EvaluationStatus status);

    /**
     * Conta avaliações por status.
     *
     * @param status status das avaliações
     * @return quantidade de avaliações
     */
    long countByStatus(EvaluationStatus status);

    /**
     * Lista todas as avaliações com paginação.
     *
     * @param page número da página (0-based)
     * @param size tamanho da página
     * @return lista de avaliações paginadas
     */
    List<VehicleEvaluation> findAll(int page, int size);

    /**
     * Remove uma avaliação por ID.
     *
     * @param id ID da avaliação a ser removida
     */
    void deleteById(EvaluationId id);

    /**
     * Verifica se existe avaliação com o ID especificado.
     *
     * @param id ID da avaliação
     * @return true se existir
     */
    boolean existsById(EvaluationId id);
}