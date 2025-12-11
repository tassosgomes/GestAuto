package com.gestauto.vehicleevaluation.infra.entity;

import jakarta.persistence.*;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.type.SqlTypes;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Entidade JPA para persistência de VehicleEvaluation.
 *
 * Esta entidade representa a tabela no banco de dados e
 * contém as annotations JPA necessárias para o mapeamento.
 */
@Entity
@Table(name = "vehicle_evaluations", schema = "vehicle_evaluation")
public class VehicleEvaluationJpaEntity {

    @Id
    @Column(name = "id", columnDefinition = "UUID")
    private UUID id;

    @Column(name = "plate", nullable = false, length = 8)
    private String plate;

    @Column(name = "renavam", nullable = false, length = 11)
    private String renavam;

    @Embedded
    private VehicleInfoEmbeddable vehicleInfo;

    @Column(name = "mileage_amount", nullable = false, precision = 19, scale = 2)
    private BigDecimal mileageAmount;

    @Column(name = "mileage_currency", nullable = false, length = 3, columnDefinition = "CHAR(3)")
    @JdbcTypeCode(SqlTypes.CHAR)
    private String mileageCurrency = "BRL";

    @Enumerated(EnumType.STRING)
    @Column(name = "status", nullable = false, length = 20)
    private EvaluationStatusJpa status;

    @Column(name = "fipe_price_amount", precision = 19, scale = 2)
    private BigDecimal fipePriceAmount;

    @Column(name = "fipe_price_currency", length = 3, columnDefinition = "CHAR(3)")
    @JdbcTypeCode(SqlTypes.CHAR)
    private String fipePriceCurrency;

    @Column(name = "base_value_amount", precision = 19, scale = 2)
    private BigDecimal baseValueAmount;

    @Column(name = "base_value_currency", length = 3, columnDefinition = "CHAR(3)")
    @JdbcTypeCode(SqlTypes.CHAR)
    private String baseValueCurrency;

    @Column(name = "final_value_amount", precision = 19, scale = 2)
    private BigDecimal finalValueAmount;

    @Column(name = "final_value_currency", length = 3, columnDefinition = "CHAR(3)")
    @JdbcTypeCode(SqlTypes.CHAR)
    private String finalValueCurrency;

    @Column(name = "approved_value_amount", precision = 19, scale = 2)
    private BigDecimal approvedValueAmount;

    @Column(name = "approved_value_currency", length = 3, columnDefinition = "CHAR(3)")
    @JdbcTypeCode(SqlTypes.CHAR)
    private String approvedValueCurrency;

    @Column(name = "observations", columnDefinition = "TEXT")
    private String observations;

    @Column(name = "justification", columnDefinition = "TEXT")
    private String justification;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    @Column(name = "submitted_at")
    private LocalDateTime submittedAt;

    @Column(name = "approved_at")
    private LocalDateTime approvedAt;

    @Column(name = "evaluator_id", nullable = false, length = 50)
    private String evaluatorId;

    @Column(name = "approver_id", length = 50)
    private String approverId;

    @Column(name = "valid_until")
    private LocalDateTime validUntil;

    @Column(name = "validation_token", length = 100)
    private String validationToken;

    @OneToMany(mappedBy = "evaluation", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<EvaluationPhotoJpaEntity> photos = new ArrayList<>();

    @OneToMany(mappedBy = "evaluation", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<DepreciationItemJpaEntity> depreciationItems = new ArrayList<>();

    @OneToOne(mappedBy = "evaluation", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private EvaluationChecklistJpaEntity checklist;

    // Construtores
    public VehicleEvaluationJpaEntity() {
        this.createdAt = LocalDateTime.now();
        this.updatedAt = LocalDateTime.now();
    }

    // Getters e Setters
    public UUID getId() {
        return id;
    }

    public void setId(UUID id) {
        this.id = id;
    }

    public String getPlate() {
        return plate;
    }

    public void setPlate(String plate) {
        this.plate = plate;
    }

    public String getRenavam() {
        return renavam;
    }

    public void setRenavam(String renavam) {
        this.renavam = renavam;
    }

    public VehicleInfoEmbeddable getVehicleInfo() {
        return vehicleInfo;
    }

    public void setVehicleInfo(VehicleInfoEmbeddable vehicleInfo) {
        this.vehicleInfo = vehicleInfo;
    }

    public BigDecimal getMileageAmount() {
        return mileageAmount;
    }

    public void setMileageAmount(BigDecimal mileageAmount) {
        this.mileageAmount = mileageAmount;
    }

    public String getMileageCurrency() {
        return mileageCurrency;
    }

    public void setMileageCurrency(String mileageCurrency) {
        this.mileageCurrency = mileageCurrency;
    }

    public EvaluationStatusJpa getStatus() {
        return status;
    }

    public void setStatus(EvaluationStatusJpa status) {
        this.status = status;
    }

    public BigDecimal getFipePriceAmount() {
        return fipePriceAmount;
    }

    public void setFipePriceAmount(BigDecimal fipePriceAmount) {
        this.fipePriceAmount = fipePriceAmount;
    }

    public String getFipePriceCurrency() {
        return fipePriceCurrency;
    }

    public void setFipePriceCurrency(String fipePriceCurrency) {
        this.fipePriceCurrency = fipePriceCurrency;
    }

    public BigDecimal getBaseValueAmount() {
        return baseValueAmount;
    }

    public void setBaseValueAmount(BigDecimal baseValueAmount) {
        this.baseValueAmount = baseValueAmount;
    }

    public String getBaseValueCurrency() {
        return baseValueCurrency;
    }

    public void setBaseValueCurrency(String baseValueCurrency) {
        this.baseValueCurrency = baseValueCurrency;
    }

    public BigDecimal getFinalValueAmount() {
        return finalValueAmount;
    }

    public void setFinalValueAmount(BigDecimal finalValueAmount) {
        this.finalValueAmount = finalValueAmount;
    }

    public String getFinalValueCurrency() {
        return finalValueCurrency;
    }

    public void setFinalValueCurrency(String finalValueCurrency) {
        this.finalValueCurrency = finalValueCurrency;
    }

    public BigDecimal getApprovedValueAmount() {
        return approvedValueAmount;
    }

    public void setApprovedValueAmount(BigDecimal approvedValueAmount) {
        this.approvedValueAmount = approvedValueAmount;
    }

    public String getApprovedValueCurrency() {
        return approvedValueCurrency;
    }

    public void setApprovedValueCurrency(String approvedValueCurrency) {
        this.approvedValueCurrency = approvedValueCurrency;
    }

    public String getObservations() {
        return observations;
    }

    public void setObservations(String observations) {
        this.observations = observations;
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

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(LocalDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }

    public LocalDateTime getSubmittedAt() {
        return submittedAt;
    }

    public void setSubmittedAt(LocalDateTime submittedAt) {
        this.submittedAt = submittedAt;
    }

    public LocalDateTime getApprovedAt() {
        return approvedAt;
    }

    public void setApprovedAt(LocalDateTime approvedAt) {
        this.approvedAt = approvedAt;
    }

    public String getEvaluatorId() {
        return evaluatorId;
    }

    public void setEvaluatorId(String evaluatorId) {
        this.evaluatorId = evaluatorId;
    }

    public String getApproverId() {
        return approverId;
    }

    public void setApproverId(String approverId) {
        this.approverId = approverId;
    }

    public LocalDateTime getValidUntil() {
        return validUntil;
    }

    public void setValidUntil(LocalDateTime validUntil) {
        this.validUntil = validUntil;
    }

    public String getValidationToken() {
        return validationToken;
    }

    public void setValidationToken(String validationToken) {
        this.validationToken = validationToken;
    }

    public List<EvaluationPhotoJpaEntity> getPhotos() {
        return photos;
    }

    public void setPhotos(List<EvaluationPhotoJpaEntity> photos) {
        this.photos = photos;
    }

    public List<DepreciationItemJpaEntity> getDepreciationItems() {
        return depreciationItems;
    }

    public void setDepreciationItems(List<DepreciationItemJpaEntity> depreciationItems) {
        this.depreciationItems = depreciationItems;
    }

    public EvaluationChecklistJpaEntity getChecklist() {
        return checklist;
    }

    public void setChecklist(EvaluationChecklistJpaEntity checklist) {
        this.checklist = checklist;
    }

    @PreUpdate
    protected void onUpdate() {
        this.updatedAt = LocalDateTime.now();
    }
}