package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Tipos de fotos obrigatórias para uma avaliação de veículo.
 *
 * Esta enumeração define os 15 tipos de fotos que devem ser capturadas
 * durante uma avaliação, seguindo o padrão estabelecido pelo processo.
 */
public enum PhotoType {
    // Externas (4)
    EXTERIOR_FRONT("Frente Externa"),
    EXTERIOR_REAR("Traseira Externa"),
    EXTERIOR_LEFT("Lateral Esquerda"),
    EXTERIOR_RIGHT("Lateral Direita"),

    // Internas (4)
    INTERIOR_FRONT("Painel Frontal"),
    INTERIOR_SEATS("Bancos Dianteiros"),
    INTERIOR_REAR("Bancos Traseiros"),
    INTERIOR_TRUNK("Porta-malas Interno"),

    // Painel (3)
    DASHBOARD_SPEED("Painel - Velocímetro"),
    DASHBOARD_INFO("Painel - Central"),
    DASHBOARD_AC("Painel - Ar Condicionado"),

    // Motor (2)
    ENGINE_BAY("Motor - Vista Superior"),
    ENGINE_DETAILS("Motor - Detalhes"),

    // Porta-malas (2)
    TRUNK_OPEN("Porta-malas Aberto"),
    TRUCK_SPARE("Porta-malas - Estepe");

    private final String description;

    PhotoType(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }

    public boolean isExternal() {
        return this == EXTERIOR_FRONT || this == EXTERIOR_REAR || this == EXTERIOR_LEFT || this == EXTERIOR_RIGHT;
    }

    public boolean isInternal() {
        return this == INTERIOR_FRONT || this == INTERIOR_SEATS || this == INTERIOR_REAR || this == INTERIOR_TRUNK;
    }

    public boolean isPanel() {
        return this == DASHBOARD_SPEED || this == DASHBOARD_INFO || this == DASHBOARD_AC;
    }

    public boolean isEngine() {
        return this == ENGINE_BAY || this == ENGINE_DETAILS;
    }

    public boolean isTrunk() {
        return this == TRUNK_OPEN || this == TRUCK_SPARE;
    }

    public static final int TOTAL_REQUIRED_PHOTOS = 15;
}