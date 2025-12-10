package com.gestauto.vehicleevaluation.infra.entity;

/**
 * Enum JPA para mapeamento do tipo de foto.
 */
public enum PhotoTypeJpa {
    FRONT("Frente"),
    REAR("Traseira"),
    LEFT_SIDE("Lateral Esquerda"),
    RIGHT_SIDE("Lateral Direita"),
    INTERIOR_FRONT("Interior Frontal"),
    INTERIOR_REAR("Interior Traseiro"),
    DRIVER_SIDE("Banco do Motorista"),
    PASSENGER_SIDE("Banco do Passageiro"),
    DASHBOARD_OVERVIEW("Painel - Visão Geral"),
    DASHBOARD_CLUSTER("Painel - Cluster"),
    ODOMETER("Odômetro"),
    ENGINE_OVERVIEW("Motor - Visão Geral"),
    ENGINE_DETAIL("Motor - Detalhe"),
    TRUNK_OPEN("Porta-malas Aberto"),
    TRUNK_CLOSED("Porta-malas Fechado");

    private final String description;

    PhotoTypeJpa(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }
}