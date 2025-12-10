package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Tipos de fotos obrigatórias para uma avaliação de veículo.
 *
 * Esta enumeração define os 15 tipos de fotos que devem ser capturadas
 * durante uma avaliação, seguindo o padrão estabelecido pelo processo.
 */
public enum PhotoType {

    // Fotos Externas (4)
    FRONT("Frente"),
    REAR("Traseira"),
    LEFT_SIDE("Lateral Esquerda"),
    RIGHT_SIDE("Lateral Direita"),

    // Fotos Internas (4)
    INTERIOR_FRONT("Interior Frontal"),
    INTERIOR_REAR("Interior Traseiro"),
    DRIVER_SIDE("Banco do Motorista"),
    PASSENGER_SIDE("Banco do Passageiro"),

    // Fotos de Painel (3)
    DASHBOARD_OVERVIEW("Painel - Visão Geral"),
    DASHBOARD_CLUSTER("Painel - Cluster/Velocímetro"),
    ODOMETER("Odômetro"),

    // Fotos de Motor (2)
    ENGINE_OVERVIEW("Motor - Visão Geral"),
    ENGINE_DETAIL("Motor - Detalhe"),

    // Fotos de Porta-malas (2)
    TRUNK_OPEN("Porta-malas Aberto"),
    TRUNK_CLOSED("Porta-malas Fechado");

    private final String description;

    PhotoType(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }

    /**
     * Verifica se é uma foto externa.
     *
     * @return true se for foto externa
     */
    public boolean isExternal() {
        return this == FRONT || this == REAR || this == LEFT_SIDE || this == RIGHT_SIDE;
    }

    /**
     * Verifica se é uma foto interna.
     *
     * @return true se for foto interna
     */
    public boolean isInternal() {
        return this == INTERIOR_FRONT || this == INTERIOR_REAR ||
               this == DRIVER_SIDE || this == PASSENGER_SIDE ||
               this == DASHBOARD_OVERVIEW || this == DASHBOARD_CLUSTER || this == ODOMETER;
    }

    /**
     * Verifica se é uma foto de motor.
     *
     * @return true se for foto de motor
     */
    public boolean isEngine() {
        return this == ENGINE_OVERVIEW || this == ENGINE_DETAIL;
    }

    /**
     * Verifica se é uma foto de porta-malas.
     *
     * @return true se for foto de porta-malas
     */
    public boolean isTrunk() {
        return this == TRUNK_OPEN || this == TRUNK_CLOSED;
    }

    /**
     * Retorna todas as categorias de fotos.
     *
     * @return array com todas as categorias
     */
    public static PhotoType[] getAllExternal() {
        return new PhotoType[]{FRONT, REAR, LEFT_SIDE, RIGHT_SIDE};
    }

    public static PhotoType[] getAllInternal() {
        return new PhotoType[]{INTERIOR_FRONT, INTERIOR_REAR, DRIVER_SIDE, PASSENGER_SIDE};
    }

    public static PhotoType[] getAllPanel() {
        return new PhotoType[]{DASHBOARD_OVERVIEW, DASHBOARD_CLUSTER, ODOMETER};
    }

    public static PhotoType[] getAllEngine() {
        return new PhotoType[]{ENGINE_OVERVIEW, ENGINE_DETAIL};
    }

    public static PhotoType[] getAllTrunk() {
        return new PhotoType[]{TRUNK_OPEN, TRUNK_CLOSED};
    }

    /**
     * Total de fotos obrigatórias.
     */
    public static final int TOTAL_REQUIRED_PHOTOS = values().length;
}