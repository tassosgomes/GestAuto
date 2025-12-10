package com.gestauto.vehicleevaluation.domain.enums;

/**
 * Seções do checklist de avaliação.
 *
 * Agrupa os itens do checklist em categorias lógicas para
 * organização e cálculo de pontuação.
 */
public enum ChecklistSection {

    BODYWORK("Lataria e Pintura"),
    MECHANICAL("Mecânica e Motor"),
    INTERIOR("Acabamento Interno"),
    TIRES("Pneus e Rodas"),
    DOCUMENTATION("Documentação e Chaves"),
    ACCESSORIES("Acessórios e Opcionais");

    private final String description;

    ChecklistSection(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }
}
