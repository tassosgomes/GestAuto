package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Representa a condição de componentes e sistemas do veículo.
 *
 * Cada nível possui uma penalidade associada usada no cálculo
 * do score de conservação.
 */
public enum Condition {

    EXCELLENT("Excelente", 0),
    GOOD("Bom", 5),
    FAIR("Regular", 10),
    POOR("Ruim", 20);

    private final String description;
    private final int penaltyPoints;

    Condition(String description, int penaltyPoints) {
        this.description = description;
        this.penaltyPoints = penaltyPoints;
    }

    public String getDescription() {
        return description;
    }

    public int getPenaltyPoints() {
        return penaltyPoints;
    }

    /**
     * Converte string para enum de forma segura.
     *
     * @param value string a converter
     * @return enum correspondente
     * @throws IllegalArgumentException se o valor não for válido
     */
    public static Condition fromString(String value) {
        if (value == null || value.trim().isEmpty()) {
            return GOOD; // Default
        }

        try {
            return Condition.valueOf(value.toUpperCase());
        } catch (IllegalArgumentException e) {
            throw new IllegalArgumentException(
                "Invalid condition: " + value + ". Must be EXCELLENT, GOOD, FAIR, or POOR"
            );
        }
    }
}
