package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.ValuationResultDto;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.application.service.ValuationConfig;
import com.gestauto.vehicleevaluation.application.service.ValuationService;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.event.ValuationCalculatedEvent;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

/**
 * Handler CQRS para o comando CalculateValuation.
 *
 * Responsável por:
 * - Recuperar a avaliação do repositório
 * - Validar o status da avaliação
 * - Delegar cálculo ao ValuationService
 * - Atualizar dados de valoração na avaliação
 * - Persistir alterações
 * - Publicar evento de domínio
 */
@Component
@Slf4j
@RequiredArgsConstructor
public class CalculateValuationHandler implements CommandHandler<CalculateValuationCommand, ValuationResultDto> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final ValuationService valuationService;
    private final DomainEventPublisherService eventPublisher;

    /**
     * Executa o cálculo de valoração para uma avaliação.
     *
     * @param command comando com dados de cálculo
     * @return resultado detalhado do cálculo
     * @throws IllegalArgumentException se avaliação não encontrada ou em status inválido
     */
    @Override
    @Transactional
    public ValuationResultDto handle(CalculateValuationCommand command) {
        log.info(
            "Iniciando handler CalculateValuation para avaliação: {}",
            command.evaluationId()
        );

        // 1. Buscar avaliação
        VehicleEvaluation evaluation = evaluationRepository.findById(
            EvaluationId.from(command.evaluationId())
        ).orElseThrow(() -> new IllegalArgumentException(
            "Avaliação não encontrada: " + command.evaluationId()
        ));

        log.debug("Avaliação encontrada. Status: {}", evaluation.getStatus());

        // 2. Validar status
        validateEvaluationStatus(evaluation.getStatus());

        // 3. Obter configuração de valoração (padrão por enquanto)
        ValuationConfig config = ValuationConfig.defaultConfig();
        log.debug("Configuração de valoração: {}", config);

        // 4. Calcular valoração
        ValuationResultDto valuationResult;
        
        if (command.hasManualAdjustment()) {
            log.info(
                "Aplicando ajuste manual de {}%",
                command.manualAdjustmentPercentage()
            );
            valuationResult = valuationService.calculateValuationWithManualAdjustment(
                evaluation,
                config,
                command.manualAdjustmentPercentage()
            );
        } else {
            valuationResult = valuationService.calculateValuation(evaluation, config);
        }

        log.info(
            "Valoração calculada com sucesso. Valor sugerido: {} | Valor final: {}",
            valuationResult.getSuggestedValue(),
            valuationResult.getFinalValue()
        );

        // 5. Atualizar avaliação com resultado
        updateEvaluationWithValuationResult(evaluation, valuationResult);

        // 6. Salvar avaliação
        evaluationRepository.save(evaluation);
        log.debug("Avaliação salva com sucesso");

        // 7. Publicar evento de domínio
        publishValuationCalculatedEvent(valuationResult);

        return valuationResult;
    }

    /**
     * Valida se o status da avaliação permite cálculo de valoração.
     */
    private void validateEvaluationStatus(EvaluationStatus status) {
        // Permite cálculo apenas em status apropriados
        if (status != EvaluationStatus.DRAFT && 
            status != EvaluationStatus.IN_PROGRESS &&
            status != EvaluationStatus.PENDING_APPROVAL) {
            throw new IllegalArgumentException(
                String.format(
                    "Não é possível calcular valoração em status '%s'. " +
                    "Status permitidos: DRAFT, IN_PROGRESS, PENDING_APPROVAL",
                    status
                )
            );
        }
    }

    /**
     * Atualiza a avaliação com os resultados do cálculo.
     */
    private void updateEvaluationWithValuationResult(
        VehicleEvaluation evaluation,
        ValuationResultDto valuationResult
    ) {
        // Atualizar preço FIPE
        evaluation.setFipePrice(valuationResult.getFipePrice());

        // Atualizar valor base
        evaluation.setBaseValue(valuationResult.getSuggestedValue());

        // Atualizar valor final
        evaluation.setFinalValue(valuationResult.getFinalValue());
    }

    /**
     * Publica evento de domínio informando que a valoração foi calculada.
     */
    private void publishValuationCalculatedEvent(ValuationResultDto valuationResult) {
        ValuationCalculatedEvent event = new ValuationCalculatedEvent(
            valuationResult.getEvaluationId(),
            valuationResult.getFipePrice(),
            valuationResult.getTotalDepreciation(),
            valuationResult.getSuggestedValue(),
            valuationResult.getFinalValue(),
            valuationResult.getLiquidityPercentage(),
            valuationResult.getManualAdjustmentPercentage() != null
        );

        eventPublisher.publish(event);
        
        log.info(
            "Evento ValuationCalculatedEvent publicado para avaliação: {}",
            valuationResult.getEvaluationId()
        );
    }
}
