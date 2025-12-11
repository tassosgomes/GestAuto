package com.gestauto.vehicleevaluation.application.command;

/**
 * Comando CQRS para calcular a valoração de um veículo.
 *
 * Este comando encapsula os dados necessários para executar o cálculo
 * automático de valoração baseado na tabela FIPE com aplicação de
 * regras de depreciação e margens configuráveis.
 */
public record CalculateValuationCommand(
    /**
     * ID único da avaliação para a qual será calculada a valoração
     */
    String evaluationId,

    /**
     * Percentual de ajuste manual (opcional).
     * Deve estar entre -10% e +10%, requer aprovação gerencial.
     * Se null, não aplica ajuste manual.
     */
    Double manualAdjustmentPercentage
) {

    /**
     * Cria um comando de cálculo de valoração.
     *
     * @param evaluationId ID da avaliação
     * @param manualAdjustmentPercentage ajuste manual em percentual
     */
    public CalculateValuationCommand {
        // Valida ID da avaliação
        if (evaluationId == null || evaluationId.isBlank()) {
            throw new IllegalArgumentException("evaluationId não pode ser nulo ou vazio");
        }

        // Valida ajuste manual se fornecido
        if (manualAdjustmentPercentage != null) {
            if (manualAdjustmentPercentage < -10.0 || manualAdjustmentPercentage > 10.0) {
                throw new IllegalArgumentException(
                    "Ajuste manual deve estar entre -10% e +10%, recebido: " + manualAdjustmentPercentage
                );
            }
        }
    }

    /**
     * Factory method para criar comando sem ajuste manual.
     */
    public static CalculateValuationCommand withoutManualAdjustment(String evaluationId) {
        return new CalculateValuationCommand(evaluationId, null);
    }

    /**
     * Factory method para criar comando com ajuste manual.
     */
    public static CalculateValuationCommand withManualAdjustment(
        String evaluationId,
        Double manualAdjustmentPercentage
    ) {
        return new CalculateValuationCommand(evaluationId, manualAdjustmentPercentage);
    }

    /**
     * Indica se há ajuste manual configurado.
     */
    public boolean hasManualAdjustment() {
        return manualAdjustmentPercentage != null && manualAdjustmentPercentage != 0.0;
    }
}
