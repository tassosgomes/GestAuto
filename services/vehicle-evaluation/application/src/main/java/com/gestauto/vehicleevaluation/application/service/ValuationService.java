package com.gestauto.vehicleevaluation.application.service;

import com.gestauto.vehicleevaluation.application.dto.DepreciationDetailDto;
import com.gestauto.vehicleevaluation.application.dto.ValuationResultDto;
import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.value.Money;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.List;
import java.util.stream.Collectors;

/**
 * Serviço de lógica de negócio para cálculo de valoração.
 *
 * Responsável por:
 * - Obter preço FIPE
 * - Calcular depreciações baseadas no checklist
 * - Aplicar margens de segurança e lucro
 * - Calcular valor final sugerido
 * - Validar ajustes manuais
 */
@Service
@Slf4j
@RequiredArgsConstructor
public class ValuationService {

    private final FipeService fipeService;

    /**
     * Calcula a valoração completa de uma avaliação de veículo.
     *
     * @param evaluation avaliação do veículo
     * @param config configuração de valoração (margens e ajustes)
     * @return resultado detalhado do cálculo de valoração
     * @throws IllegalArgumentException se dados requeridos estiverem ausentes
     */
    public ValuationResultDto calculateValuation(
        VehicleEvaluation evaluation,
        ValuationConfig config
    ) {
        log.info(
            "Iniciando cálculo de valoração para avaliação: {}",
            evaluation.getId().getValueAsString()
        );

        // 1. Obter valor FIPE
        Money fipePrice = obtainFipePrice(evaluation);
        log.debug("Preço FIPE obtido: {}", fipePrice);

        // 2. Obter percentual de liquidez
        double liquidityPercentage = calculateLiquidityPercentage(evaluation);
        log.debug("Percentual de liquidez: {}%", liquidityPercentage * 100);

        // 3. Calcular valor base (FIPE × liquidez)
        Money baseValue = calculateBaseValue(fipePrice, liquidityPercentage);
        log.debug("Valor base calculado: {}", baseValue);

        // 4. Calcular depreciações
        List<DepreciationItem> depreciationItems = evaluation.getDepreciationItems();
        Money totalDepreciation = calculateTotalDepreciation(depreciationItems);
        log.debug("Total de depreciações: {}", totalDepreciation);

        // 5. Calcular margens
        Money safetyMargin = calculateMargin(fipePrice, config.getSafetyMarginPercentage());
        Money profitMargin = calculateMargin(fipePrice, config.getProfitMarginPercentage());
        log.debug("Margem de segurança: {}, Margem de lucro: {}", safetyMargin, profitMargin);

        // 6. Calcular valor sugerido
        // suggestedValue = baseValue - totalDepreciation + safetyMargin + profitMargin
        Money suggestedValue = calculateSuggestedValue(
            baseValue,
            totalDepreciation,
            safetyMargin,
            profitMargin
        );
        log.debug("Valor sugerido: {}", suggestedValue);

        // 7. Preparar detalhes de depreciações
        List<DepreciationDetailDto> depreciationDetails = mapDepreciationDetails(depreciationItems);

        // 8. Construir resultado
        ValuationResultDto result = new ValuationResultDto.Builder()
            .evaluationId(evaluation.getId().getValueAsString())
            .fipePrice(fipePrice)
            .totalDepreciation(totalDepreciation)
            .safetyMargin(safetyMargin)
            .profitMargin(profitMargin)
            .suggestedValue(suggestedValue)
            .depreciationDetails(depreciationDetails)
            .liquidityPercentage(liquidityPercentage)
            .finalValue(suggestedValue)
            .build();

        log.info(
            "Cálculo de valoração concluído. Valor sugerido: {}",
            suggestedValue
        );

        return result;
    }

    /**
     * Calcula a valoração com ajuste manual.
     *
     * @param evaluation avaliação do veículo
     * @param config configuração de valoração
     * @param manualAdjustmentPercentage percentual de ajuste manual (-10 a +10)
     * @return resultado do cálculo com ajuste manual aplicado
     */
    public ValuationResultDto calculateValuationWithManualAdjustment(
        VehicleEvaluation evaluation,
        ValuationConfig config,
        Double manualAdjustmentPercentage
    ) {
        // Valida ajuste manual
        if (manualAdjustmentPercentage == null) {
            return calculateValuation(evaluation, config);
        }

        if (Math.abs(manualAdjustmentPercentage) > config.getMaxManualAdjustmentPercentage()) {
            throw new IllegalArgumentException(
                String.format(
                    "Ajuste manual %.1f%% excede o limite de ±%d%%",
                    manualAdjustmentPercentage,
                    config.getMaxManualAdjustmentPercentage()
                )
            );
        }

        // Calcula valoração base
        ValuationResultDto baseResult = calculateValuation(evaluation, config);

        // Aplica ajuste manual
        Money adjustedValue = applyManualAdjustment(
            baseResult.getSuggestedValue(),
            manualAdjustmentPercentage
        );
        Money adjustmentAmount = adjustedValue.subtract(baseResult.getSuggestedValue());

        log.info(
            "Ajuste manual aplicado: {}% = {}",
            manualAdjustmentPercentage,
            adjustmentAmount
        );

        // Retorna resultado com ajuste
        return new ValuationResultDto.Builder()
            .evaluationId(baseResult.getEvaluationId())
            .fipePrice(baseResult.getFipePrice())
            .totalDepreciation(baseResult.getTotalDepreciation())
            .safetyMargin(baseResult.getSafetyMargin())
            .profitMargin(baseResult.getProfitMargin())
            .suggestedValue(baseResult.getSuggestedValue())
            .depreciationDetails(baseResult.getDepreciationDetails())
            .liquidityPercentage(baseResult.getLiquidityPercentage())
            .manualAdjustmentPercentage(manualAdjustmentPercentage)
            .manualAdjustmentAmount(adjustmentAmount)
            .finalValue(adjustedValue)
            .build();
    }

    /**
     * Obtém o preço FIPE do veículo.
     */
    private Money obtainFipePrice(VehicleEvaluation evaluation) {
        return fipeService.getFipePrice(
            evaluation.getVehicleInfo().getBrand(),
            evaluation.getVehicleInfo().getModel(),
            evaluation.getVehicleInfo().getYearModel(),
            evaluation.getVehicleInfo().getFuelType()
        ).orElseThrow(() -> new IllegalArgumentException(
            "Não foi possível obter o preço FIPE para o veículo: " +
            evaluation.getVehicleInfo().getBrand() + " " +
            evaluation.getVehicleInfo().getModel()
        ));
    }

    /**
     * Calcula o percentual de liquidez baseado na marca e modelo.
     */
    private double calculateLiquidityPercentage(VehicleEvaluation evaluation) {
        int vehicleAge = java.time.LocalDate.now().getYear() - 
            evaluation.getVehicleInfo().getYearManufacture();
        
        return fipeService.calculateLiquidityPercentage(
            evaluation.getVehicleInfo().getBrand(),
            evaluation.getVehicleInfo().getModel(),
            vehicleAge
        );
    }

    /**
     * Calcula o valor base: FIPE × liquidez.
     */
    private Money calculateBaseValue(Money fipePrice, double liquidityPercentage) {
        if (liquidityPercentage < 0 || liquidityPercentage > 1) {
            throw new IllegalArgumentException(
                "Percentual de liquidez deve estar entre 0 e 1"
            );
        }
        
        return fipePrice.multiply(BigDecimal.valueOf(liquidityPercentage));
    }

    /**
     * Calcula o total de todas as depreciações.
     */
    private Money calculateTotalDepreciation(List<DepreciationItem> depreciationItems) {
        if (depreciationItems == null || depreciationItems.isEmpty()) {
            return Money.ZERO;
        }

        return depreciationItems.stream()
            .map(DepreciationItem::getDepreciationValue)
            .reduce(Money.ZERO, Money::add);
    }

    /**
     * Calcula uma margem (de segurança ou lucro) baseada em percentual.
     */
    private Money calculateMargin(Money baseValue, Double marginPercentage) {
        if (marginPercentage <= 0) {
            return Money.ZERO;
        }

        BigDecimal percentage = BigDecimal.valueOf(marginPercentage)
            .divide(BigDecimal.valueOf(100), 4, RoundingMode.HALF_UP);
        
        return baseValue.multiply(percentage);
    }

    /**
     * Calcula o valor sugerido final.
     */
    private Money calculateSuggestedValue(
        Money baseValue,
        Money totalDepreciation,
        Money safetyMargin,
        Money profitMargin
    ) {
        return baseValue
            .subtract(totalDepreciation)
            .add(safetyMargin)
            .add(profitMargin);
    }

    /**
     * Aplica ajuste manual ao valor sugerido.
     */
    private Money applyManualAdjustment(
        Money suggestedValue,
        Double adjustmentPercentage
    ) {
        BigDecimal adjustmentFactor = BigDecimal.valueOf(1.0)
            .add(BigDecimal.valueOf(adjustmentPercentage)
                .divide(BigDecimal.valueOf(100), 4, RoundingMode.HALF_UP));
        
        return suggestedValue.multiply(adjustmentFactor);
    }

    /**
     * Converte itens de depreciação para DTOs.
     */
    private List<DepreciationDetailDto> mapDepreciationDetails(
        List<DepreciationItem> depreciationItems
    ) {
        if (depreciationItems == null || depreciationItems.isEmpty()) {
            return List.of();
        }

        return depreciationItems.stream()
            .map(item -> DepreciationDetailDto.of(
                item.getDepreciationId(),
                item.getCategory(),
                item.getDescription(),
                item.getDepreciationValue(),
                item.getJustification()
            ))
            .collect(Collectors.toList());
    }
}
