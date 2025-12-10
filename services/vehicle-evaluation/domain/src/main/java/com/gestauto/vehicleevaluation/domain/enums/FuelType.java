package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Tipos de combustível suportados pelo sistema.
 */
public enum FuelType {

    /**
     * Gasolina comum.
     */
    GASOLINE("Gasolina"),

    /**
     * Gasolina aditivada.
     */
    GASOLINE_ADITIVATED("Gasolina Aditivada"),

    /**
     * Etanol.
     */
    ETHANOL("Etanol"),

    /**
     * Diesel.
     */
    DIESEL("Diesel"),

    /**
     * Flex (Gasolina/Etanol).
     */
    FLEX("Flex"),

    /**
     * Gás Natural Veicular (GNV).
     */
    NATURAL_GAS("GNV"),

    /**
     * Elétrico.
     */
    ELECTRIC("Elétrico"),

    /**
     * Híbrido.
     */
    HYBRID("Híbrido");

    private final String description;

    FuelType(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }

    /**
     * Verifica se é um combustível fóssil tradicional.
     *
     * @return true se for combustível fóssil
     */
    public boolean isFossilFuel() {
        return this == GASOLINE || this == GASOLINE_ADITIVATED ||
               this == DIESEL || this == NATURAL_GAS;
    }

    /**
     * Verifica se é um combustível renovável.
     *
     * @return true se for renovável
     */
    public boolean isRenewable() {
        return this == ETHANOL || this == ELECTRIC;
    }

    /**
     * Verifica se é veículo flex (aceita múltiplos combustíveis).
     *
     * @return true se for flex
     */
    public boolean isFlexible() {
        return this == FLEX || this == HYBRID;
    }
}