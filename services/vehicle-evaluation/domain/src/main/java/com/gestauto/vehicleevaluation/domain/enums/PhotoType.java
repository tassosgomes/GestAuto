package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Tipos de fotos obrigatórias para uma avaliação de veículo.
 *
 * Esta enumeração define os 15 tipos de fotos que devem ser capturadas
 * durante uma avaliação, seguindo o padrão estabelecido pelo processo.
 */
public enum PhotoType {

    // Fotos Externas (4)
    /**
     * Vista frontal do veículo.
     */
    FRONT("Frente"),

    /**
     * Vista traseira do veículo.
     */
    REAR("Traseira"),

    /**
     * Lateral esquerda do veículo.
     */
    LEFT_SIDE("Lateral Esquerda"),

    /**
     * Lateral direita do veículo.
     */
    RIGHT_SIDE("Lateral Direita"),

    // Fotos Internas (4)
    /**
     * Interior frontal (bancos e painel).
     */
    INTERIOR_FRONT("Interior Frontal"),

    /**
     * Interior traseiro (bancos traseiros).
     */
    INTERIOR_REAR("Interior Traseiro"),

    /**
     * Detalhes do painel de instrumentos.
     */
    DASHBOARD("Painel de Instrumentos"),

    /**
     * Volante e controles.
     */
    STEERING("Volante e Controles"),

    // Fotos de Motor (3)
    /**
     * Compartimento do motor vista geral.
     */
    ENGINE_GENERAL("Motor - Vista Geral"),

    /**
     * Motor vista lateral direita.
     */
    ENGINE_RIGHT("Motor - Lateral Direita"),

    /**
     * Motor vista lateral esquerda.
     */
    ENGINE_LEFT("Motor - Lateral Esquerda"),

    // Fotos de Porta-malas (2)
    /**
     * Porta-malas aberto.
     */
    TRUNK_OPEN("Porta-malas Aberto"),

    /**
     * Porta-malas fechado.
     */
    TRUNK_CLOSED("Porta-malas Fechado"),

    // Fotos de Documentos (2)
    /**
     * Documento CRLV frontal.
     */
    DOCUMENT_CRLV("CRLV - Frente"),

    /**
     * Documento CRLV verso.
     */
    DOCUMENT_CRLV_BACK("CRLV - Verso");

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
        return this == INTERIOR_FRONT || this == INTERIOR_REAR || this == DASHBOARD || this == STEERING;
    }

    /**
     * Verifica se é uma foto de motor.
     *
     * @return true se for foto de motor
     */
    public boolean isEngine() {
        return this == ENGINE_GENERAL || this == ENGINE_RIGHT || this == ENGINE_LEFT;
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
     * Verifica se é uma foto de documento.
     *
     * @return true se for foto de documento
     */
    public boolean isDocument() {
        return this == DOCUMENT_CRLV || this == DOCUMENT_CRLV_BACK;
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
        return new PhotoType[]{INTERIOR_FRONT, INTERIOR_REAR, DASHBOARD, STEERING};
    }

    public static PhotoType[] getAllEngine() {
        return new PhotoType[]{ENGINE_GENERAL, ENGINE_RIGHT, ENGINE_LEFT};
    }

    public static PhotoType[] getAllTrunk() {
        return new PhotoType[]{TRUNK_OPEN, TRUNK_CLOSED};
    }

    public static PhotoType[] getAllDocument() {
        return new PhotoType[]{DOCUMENT_CRLV, DOCUMENT_CRLV_BACK};
    }

    /**
     * Total de fotos obrigatórias.
     */
    public static final int TOTAL_REQUIRED_PHOTOS = values().length;
}