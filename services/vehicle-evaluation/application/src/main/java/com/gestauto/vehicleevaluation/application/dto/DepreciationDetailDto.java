package com.gestauto.vehicleevaluation.application.dto;

import com.gestauto.vehicleevaluation.domain.value.Money;
import java.util.Objects;

/**
 * DTO que representa um detalhe individual de depreciação.
 *
 * Fornece informações sobre cada item de depreciação aplicado
 * ao cálculo da valoração.
 */
public class DepreciationDetailDto {
    private final String depreciationId;
    private final String category;
    private final String description;
    private final Money depreciationValue;
    private final String justification;

    private DepreciationDetailDto(
        String depreciationId,
        String category,
        String description,
        Money depreciationValue,
        String justification
    ) {
        this.depreciationId = Objects.requireNonNull(depreciationId, "depreciationId não pode ser nulo");
        this.category = Objects.requireNonNull(category, "category não pode ser nulo");
        this.description = Objects.requireNonNull(description, "description não pode ser nulo");
        this.depreciationValue = Objects.requireNonNull(depreciationValue, "depreciationValue não pode ser nulo");
        this.justification = justification;
    }

    public String getDepreciationId() {
        return depreciationId;
    }

    public String getCategory() {
        return category;
    }

    public String getDescription() {
        return description;
    }

    public Money getDepreciationValue() {
        return depreciationValue;
    }

    public String getJustification() {
        return justification;
    }

    /**
     * Factory method para criar DepreciationDetailDto.
     */
    public static DepreciationDetailDto of(
        String depreciationId,
        String category,
        String description,
        Money depreciationValue,
        String justification
    ) {
        return new DepreciationDetailDto(depreciationId, category, description, depreciationValue, justification);
    }

    @Override
    public String toString() {
        return "DepreciationDetailDto{" +
                "depreciationId='" + depreciationId + '\'' +
                ", category='" + category + '\'' +
                ", description='" + description + '\'' +
                ", depreciationValue=" + depreciationValue +
                ", justification='" + justification + '\'' +
                '}';
    }
}
