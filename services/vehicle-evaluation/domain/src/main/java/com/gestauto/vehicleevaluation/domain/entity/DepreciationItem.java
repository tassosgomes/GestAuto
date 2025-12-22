package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import java.time.LocalDateTime;
import java.util.Objects;
import java.util.UUID;

/**
 * Entidade de domínio representando um item de depreciação de uma avaliação.
 *
 * Esta entidade encapsula as informações sobre depreciações aplicadas
 * a um veículo, como danos na lataria, problemas mecânicos, etc.
 */
public final class DepreciationItem {

    private final String depreciationId;
    private final EvaluationId evaluationId;
    private final String category;
    private final String description;
    private final Money depreciationValue;
    private final String justification;
    private final LocalDateTime createdAt;
    private final String createdBy;

    private DepreciationItem(String depreciationId, EvaluationId evaluationId,
                             String category, String description, Money depreciationValue,
                             String justification, String createdBy) {
        this.depreciationId = Objects.requireNonNull(depreciationId, "DepreciationId cannot be null");
        this.evaluationId = Objects.requireNonNull(evaluationId, "EvaluationId cannot be null");
        this.category = Objects.requireNonNull(category, "Category cannot be null");
        this.description = Objects.requireNonNull(description, "Description cannot be null");
        this.depreciationValue = Objects.requireNonNull(depreciationValue, "DepreciationValue cannot be null");
        this.justification = justification;
        this.createdBy = Objects.requireNonNull(createdBy, "CreatedBy cannot be null");
        this.createdAt = LocalDateTime.now();

        validate();
    }

    private DepreciationItem(String depreciationId, EvaluationId evaluationId,
                             String category, String description, Money depreciationValue,
                             String justification, String createdBy, LocalDateTime createdAt) {
        this.depreciationId = Objects.requireNonNull(depreciationId, "DepreciationId cannot be null");
        this.evaluationId = Objects.requireNonNull(evaluationId, "EvaluationId cannot be null");
        this.category = Objects.requireNonNull(category, "Category cannot be null");
        this.description = Objects.requireNonNull(description, "Description cannot be null");
        this.depreciationValue = Objects.requireNonNull(depreciationValue, "DepreciationValue cannot be null");
        this.justification = justification;
        this.createdBy = Objects.requireNonNull(createdBy, "CreatedBy cannot be null");
        this.createdAt = Objects.requireNonNullElseGet(createdAt, LocalDateTime::now);
        validate();
    }

    /**
     * Cria um novo item de depreciação.
     *
     * @param evaluationId ID da avaliação
     * @param category categoria da depreciação (BODY, PAINT, MECHANICAL, etc)
     * @param description descrição detalhada do problema
     * @param depreciationValue valor da depreciação
     * @param justification justificativa do avaliador
     * @param createdBy usuário que criou o item
     * @return nova instância de DepreciationItem
     * @throws IllegalArgumentException se algum dado for inválido
     */
    public static DepreciationItem create(EvaluationId evaluationId, String category,
                                           String description, Money depreciationValue,
                                           String justification, String createdBy) {
        String depreciationId = UUID.randomUUID().toString();
        return new DepreciationItem(depreciationId, evaluationId, category, description,
                                      depreciationValue, justification, createdBy);
    }

    /**
     * Reidrata item de depreciação persistido.
     */
    public static DepreciationItem restore(String depreciationId, EvaluationId evaluationId,
                                           String category, String description, Money depreciationValue,
                                           String justification, String createdBy, LocalDateTime createdAt) {
        return new DepreciationItem(depreciationId, evaluationId, category, description,
            depreciationValue, justification, createdBy, createdAt);
    }

    /**
     * Valida os dados da depreciação.
     *
     * @throws IllegalArgumentException se algum dado for inválido
     */
    private void validate() {
        if (category.trim().isEmpty()) {
            throw new IllegalArgumentException("Category cannot be empty");
        }

        if (description.trim().isEmpty()) {
            throw new IllegalArgumentException("Description cannot be empty");
        }

        if (depreciationValue.isZero() || depreciationValue.isNegative()) {
            throw new IllegalArgumentException("DepreciationValue must be positive");
        }

        if (!isValidCategory(category)) {
            throw new IllegalArgumentException("Invalid category: " + category);
        }

        if (createdBy.trim().isEmpty()) {
            throw new IllegalArgumentException("CreatedBy cannot be empty");
        }
    }

    /**
     * Verifica se a categoria é válida.
     *
     * @param category categoria a validar
     * @return true se for válida
     */
    private boolean isValidCategory(String category) {
        return category.equals("BODY") ||
               category.equals("PAINT") ||
               category.equals("MECHANICAL") ||
               category.equals("TIRES") ||
               category.equals("INTERIOR") ||
               category.equals("DOCUMENTATION") ||
               category.equals("OTHER");
    }

    /**
     * Verifica se é uma depreciação de lataria.
     *
     * @return true se for de lataria
     */
    public boolean isBodyDepreciation() {
        return "BODY".equals(category);
    }

    /**
     * Verifica se é uma depreciação de pintura.
     *
     * @return true se for de pintura
     */
    public boolean isPaintDepreciation() {
        return "PAINT".equals(category);
    }

    /**
     * Verifica se é uma depreciação mecânica.
     *
     * @return true se for mecânica
     */
    public boolean isMechanicalDepreciation() {
        return "MECHANICAL".equals(category);
    }

    /**
     * Verifica se é uma depreciação de pneus.
     *
     * @return true se for de pneus
     */
    public boolean isTiresDepreciation() {
        return "TIRES".equals(category);
    }

    /**
     * Verifica se é uma depreciação de interior.
     *
     * @return true se for de interior
     */
    public boolean isInteriorDepreciation() {
        return "INTERIOR".equals(category);
    }

    /**
     * Verifica se é uma depreciação relacionada a documentos.
     *
     * @return true se for de documentação
     */
    public boolean isDocumentationDepreciation() {
        return "DOCUMENTATION".equals(category);
    }

    // Getters
    public String getDepreciationId() {
        return depreciationId;
    }

    public EvaluationId getEvaluationId() {
        return evaluationId;
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

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public String getCreatedBy() {
        return createdBy;
    }

    /**
     * Retorna o valor formatado para exibição.
     *
     * @return valor formatado
     */
    public String getFormattedValue() {
        return depreciationValue.toBrazilianFormat();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        DepreciationItem that = (DepreciationItem) o;
        return depreciationId.equals(that.depreciationId);
    }

    @Override
    public int hashCode() {
        return depreciationId.hashCode();
    }

    @Override
    public String toString() {
        return "DepreciationItem{" +
                "depreciationId='" + depreciationId + '\'' +
                ", evaluationId=" + evaluationId +
                ", category='" + category + '\'' +
                ", description='" + description + '\'' +
                ", depreciationValue=" + depreciationValue +
                ", justification='" + justification + '\'' +
                ", createdBy='" + createdBy + '\'' +
                '}';
    }
}