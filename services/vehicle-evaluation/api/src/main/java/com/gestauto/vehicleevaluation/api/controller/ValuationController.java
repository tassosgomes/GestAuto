package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.CalculateValuationCommand;
import com.gestauto.vehicleevaluation.application.command.CalculateValuationHandler;
import com.gestauto.vehicleevaluation.application.dto.ValuationResultDto;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

/**
 * Controller REST para operações de valoração de veículos.
 *
 * Expõe endpoints para cálculo automático de valoração baseado na
 * tabela FIPE com aplicação de regras de depreciação e margens.
 */
@RestController
@RequestMapping("/api/v1/evaluations")
@RequiredArgsConstructor
@Slf4j
public class ValuationController {

    private final CalculateValuationHandler calculateValuationHandler;

    /**
     * POST /api/v1/evaluations/{id}/calculate
     *
     * Calcula a valoração automática para uma avaliação de veículo.
     *
     * @param evaluationId ID da avaliação para cálculo
     * @param request dados de cálculo (ajuste manual opcional)
     * @return resultado detalhado da valoração
     */
    @PostMapping("/{id}/calculate")
    public ResponseEntity<ValuationResultDto> calculateValuation(
        @PathVariable("id") String evaluationId,
        @RequestBody(required = false) CalculateValuationRequest request
    ) {
        log.info("Recebido requisição para calcular valoração da avaliação: {}", evaluationId);

        try {
            // Criar comando
            CalculateValuationCommand command;
            
            if (request != null && request.getManualAdjustmentPercentage() != null) {
                command = new CalculateValuationCommand(
                    evaluationId,
                    request.getManualAdjustmentPercentage()
                );
                log.debug(
                    "Comando criado com ajuste manual: {}%",
                    request.getManualAdjustmentPercentage()
                );
            } else {
                command = CalculateValuationCommand.withoutManualAdjustment(evaluationId);
                log.debug("Comando criado sem ajuste manual");
            }

            // Executar comando
            ValuationResultDto result = calculateValuationHandler.handle(command);

            log.info(
                "Valoração calculada com sucesso. Valor sugerido: {}",
                result.getSuggestedValue()
            );

            return ResponseEntity.ok(result);

        } catch (IllegalArgumentException e) {
            log.warn("Erro ao calcular valoração: {}", e.getMessage());
            return ResponseEntity.badRequest().build();
        } catch (Exception e) {
            log.error("Erro inesperado ao calcular valoração", e);
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).build();
        }
    }

    /**
     * DTO para requisição de cálculo de valoração.
     */
    @lombok.Data
    @lombok.NoArgsConstructor
    @lombok.AllArgsConstructor
    public static class CalculateValuationRequest {
        /**
         * Percentual de ajuste manual (-10 a +10).
         * Opcional. Se não fornecido, utiliza cálculo automático.
         */
        private Double manualAdjustmentPercentage;
    }
}
