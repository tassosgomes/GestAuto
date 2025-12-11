package com.gestauto.vehicleevaluation.infra.entity;

import jakarta.persistence.*;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.type.SqlTypes;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Entidade JPA para persistência de itens de depreciação.
 */
@Entity
@Table(name = "depreciation_items", schema = "vehicle_evaluation")
public class DepreciationItemJpaEntity {

    @Id
    @Column(name = "depreciation_id", columnDefinition = "UUID")
    private UUID depreciationId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "evaluation_id", nullable = false)
    private VehicleEvaluationJpaEntity evaluation;

    @Column(name = "category", nullable = false, length = 30)
    private String category;

    @Column(name = "description", nullable = false, columnDefinition = "TEXT")
    private String description;

    @Column(name = "depreciation_value_amount", nullable = false, precision = 19, scale = 2)
    private BigDecimal depreciationValueAmount;

    @Column(name = "depreciation_value_currency", nullable = false, length = 3, columnDefinition = "CHAR(3)")
    @JdbcTypeCode(SqlTypes.CHAR)
    private String depreciationValueCurrency = "BRL";

    @Column(name = "justification", columnDefinition = "TEXT")
    private String justification;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "created_by", nullable = false, length = 50)
    private String createdBy;

    // Construtores
    public DepreciationItemJpaEntity() {
        this.createdAt = LocalDateTime.now();
    }

    // Getters e Setters
    public UUID getDepreciationId() {
        return depreciationId;
    }

    public void setDepreciationId(UUID depreciationId) {
        this.depreciationId = depreciationId;
    }

    public VehicleEvaluationJpaEntity getEvaluation() {
        return evaluation;
    }

    public void setEvaluation(VehicleEvaluationJpaEntity evaluation) {
        this.evaluation = evaluation;
    }

    public String getCategory() {
        return category;
    }

    public void setCategory(String category) {
        this.category = category;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public BigDecimal getDepreciationValueAmount() {
        return depreciationValueAmount;
    }

    public void setDepreciationValueAmount(BigDecimal depreciationValueAmount) {
        this.depreciationValueAmount = depreciationValueAmount;
    }

    public String getDepreciationValueCurrency() {
        return depreciationValueCurrency;
    }

    public void setDepreciationValueCurrency(String depreciationValueCurrency) {
        this.depreciationValueCurrency = depreciationValueCurrency;
    }

    public String getJustification() {
        return justification;
    }

    public void setJustification(String justification) {
        this.justification = justification;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public String getCreatedBy() {
        return createdBy;
    }

    public void setCreatedBy(String createdBy) {
        this.createdBy = createdBy;
    }
}