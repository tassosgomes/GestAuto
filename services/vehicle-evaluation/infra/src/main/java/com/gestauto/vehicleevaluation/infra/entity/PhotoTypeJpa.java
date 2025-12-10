package com.gestauto.vehicleevaluation.infra.entity;

/**
 * Enum JPA para mapeamento do tipo de foto.
 */
public enum PhotoTypeJpa {
    FRONT("Frontal"),
    REAR("Traseira"),
    LEFT_SIDE("Lateral Esquerda"),
    RIGHT_SIDE("Lateral Direita"),
    INTERIOR_FRONT("Interior Frente"),
    INTERIOR_REAR("Interior Traseira"),
    DASHBOARD("Painel"),
    ENGINE("Motor"),
    TRUNK("Porta-malas"),
    TIRES("Pneus"),
    ODOMETER("Odômetro"),
    DOCUMENTATION("Documentação"),
    DAMAGE("Dano/Avaria"),
    EXTRACTION("Extração (opcional)");

    private final String description;

    PhotoTypeJpa(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }
}