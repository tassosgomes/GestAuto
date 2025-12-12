package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.exception.*;
import com.gestauto.vehicleevaluation.domain.event.*;
import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Objects;

/**
 * Entidade principal de domínio representando uma avaliação de veículo.
 *
 * Esta é a entidade central do sistema, encapsulando todas as
 * informações e regras de negócio de uma avaliação completa.
 * Implementada sem annotations JPA para manter o domínio puro.
 */
public final class VehicleEvaluation {

    private final EvaluationId id;
    private final Plate plate;
    private final String renavam;
    private final VehicleInfo vehicleInfo;
    private final Money mileage;
    private final List<EvaluationPhoto> photos;
    private final List<DepreciationItem> depreciationItems;
    private EvaluationChecklist checklist;
    private EvaluationStatus status;
    private Money fipePrice;
    private Money baseValue;
    private Money finalValue;
    private Money approvedValue;
    private String observations;
    private String justification;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;
    private LocalDateTime submittedAt;
    private LocalDateTime approvedAt;
    private String evaluatorId;
    private String approverId;
    private LocalDateTime validUntil;
    private String validationToken;
    private final List<DomainEvent> domainEvents;

    private void validate() {
        // Validations are already done in the constructor
    }

    // Construtor privado para factory methods
    private VehicleEvaluation(EvaluationId id, Plate plate, String renavam,
                             VehicleInfo vehicleInfo, Money mileage,
                             String evaluatorId) {
        this.id = Objects.requireNonNull(id, "EvaluationId cannot be null");
        this.plate = Objects.requireNonNull(plate, "Plate cannot be null");
        this.renavam = Objects.requireNonNull(renavam, "Renavam cannot be null");
        this.vehicleInfo = Objects.requireNonNull(vehicleInfo, "VehicleInfo cannot be null");
        this.mileage = Objects.requireNonNull(mileage, "Mileage cannot be null");
        this.evaluatorId = Objects.requireNonNull(evaluatorId, "EvaluatorId cannot be null");

        this.status = EvaluationStatus.DRAFT;
        this.createdAt = LocalDateTime.now();
        this.updatedAt = LocalDateTime.now();
        this.photos = new ArrayList<>();
        this.depreciationItems = new ArrayList<>();
        this.domainEvents = new ArrayList<>();

        // Inicializa campos opcionais com null
        this.checklist = null;
        this.fipePrice = null;
        this.baseValue = null;
        this.finalValue = null;
        this.approvedValue = null;
        this.observations = null;
        this.justification = null;
        this.submittedAt = null;
        this.approvedAt = null;
        this.approverId = null;
        this.validUntil = null;
        this.validationToken = null;

        validate();
    }

    // Construtor auxiliar para reidratação sem validar status/estado
    private VehicleEvaluation(EvaluationId id, Plate plate, String renavam,
                              VehicleInfo vehicleInfo, Money mileage,
                              String evaluatorId, boolean skipValidation) {
        this.id = Objects.requireNonNull(id, "EvaluationId cannot be null");
        this.plate = Objects.requireNonNull(plate, "Plate cannot be null");
        this.renavam = Objects.requireNonNull(renavam, "Renavam cannot be null");
        this.vehicleInfo = Objects.requireNonNull(vehicleInfo, "VehicleInfo cannot be null");
        this.mileage = Objects.requireNonNull(mileage, "Mileage cannot be null");
        this.evaluatorId = Objects.requireNonNull(evaluatorId, "EvaluatorId cannot be null");

        this.photos = new ArrayList<>();
        this.depreciationItems = new ArrayList<>();
        this.domainEvents = new ArrayList<>();
        this.createdAt = LocalDateTime.now();
        this.updatedAt = LocalDateTime.now();
        this.status = EvaluationStatus.DRAFT;
        if (!skipValidation) {
            validate();
        }
    }

    /**
     * Cria uma nova avaliação de veículo.
     *
     * @param plate placa do veículo
     * @param renavam RENAVAM do veículo
     * @param vehicleInfo informações do veículo
     * @param mileage quilometragem atual
     * @param evaluatorId ID do avaliador responsável
     * @return nova instância de VehicleEvaluation
     * @throws IllegalArgumentException se algum dado for inválido
     */
    public static VehicleEvaluation create(Plate plate, String renavam,
                                              VehicleInfo vehicleInfo, Money mileage,
                                              String evaluatorId) {
        EvaluationId id = EvaluationId.generate();
        VehicleEvaluation evaluation = new VehicleEvaluation(id, plate, renavam,
                                                          vehicleInfo, mileage, evaluatorId);

        // Adiciona evento de domínio
        evaluation.addDomainEvent(new EvaluationCreatedEvent(
            id.getValueAsString(),
            evaluatorId,
            plate.getFormatted(),
            vehicleInfo.getBrand(),
            vehicleInfo.getModel()
        ));

        return evaluation;
    }

    /**
     * Reidrata uma avaliação a partir do estado persistido.
     */
    public static VehicleEvaluation restore(EvaluationId id, Plate plate, String renavam,
                                            VehicleInfo vehicleInfo, Money mileage,
                                            EvaluationStatus status,
                                            Money fipePrice, Money baseValue, Money finalValue,
                                            Money approvedValue, String observations, String justification,
                                            LocalDateTime createdAt, LocalDateTime updatedAt,
                                            LocalDateTime submittedAt, LocalDateTime approvedAt,
                                            String evaluatorId, String approverId,
                                            LocalDateTime validUntil, String validationToken,
                                            List<EvaluationPhoto> photos,
                                            List<DepreciationItem> depreciationItems,
                                            EvaluationChecklist checklist) {
        VehicleEvaluation evaluation = new VehicleEvaluation(id, plate, renavam, vehicleInfo, mileage, evaluatorId, true);
        evaluation.status = Objects.requireNonNull(status, "Status cannot be null");
        evaluation.fipePrice = fipePrice;
        evaluation.baseValue = baseValue;
        evaluation.finalValue = finalValue;
        evaluation.approvedValue = approvedValue;
        evaluation.observations = observations;
        evaluation.justification = justification;
        evaluation.createdAt = Objects.requireNonNullElseGet(createdAt, LocalDateTime::now);
        evaluation.updatedAt = Objects.requireNonNullElseGet(updatedAt, LocalDateTime::now);
        evaluation.submittedAt = submittedAt;
        evaluation.approvedAt = approvedAt;
        evaluation.approverId = approverId;
        evaluation.validUntil = validUntil;
        evaluation.validationToken = validationToken;

        evaluation.photos.clear();
        if (photos != null) {
            evaluation.photos.addAll(photos);
        }

        evaluation.depreciationItems.clear();
        if (depreciationItems != null) {
            evaluation.depreciationItems.addAll(depreciationItems);
        }

        evaluation.checklist = checklist;
        return evaluation;
    }

    /**
     * Adiciona uma foto à avaliação.
     *
     * @param photo foto a ser adicionada
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void addPhoto(EvaluationPhoto photo) {
        validateEditable("add photo");

        Objects.requireNonNull(photo, "Photo cannot be null");

        if (!photo.getEvaluationId().equals(this.id)) {
            throw new IllegalArgumentException("Photo evaluationId does not match evaluation ID");
        }

        // Verifica se já existe foto do mesmo tipo
        boolean alreadyExists = photos.stream()
            .anyMatch(p -> p.getPhotoType() == photo.getPhotoType());

        if (alreadyExists) {
            throw new IllegalArgumentException("Photo of type " + photo.getPhotoType() + " already exists");
        }

        photos.add(photo);
        markAsUpdated();
    }

    /**
     * Remove uma foto da avaliação.
     *
     * @param photoId ID da foto a ser removida
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void removePhoto(String photoId) {
        validateEditable("remove photo");

        Objects.requireNonNull(photoId, "PhotoId cannot be null");

        boolean removed = photos.removeIf(photo -> photo.getPhotoId().equals(photoId));

        if (!removed) {
            throw new IllegalArgumentException("Photo with ID " + photoId + " not found");
        }

        markAsUpdated();
    }

    /**
     * Adiciona um item de depreciação à avaliação.
     *
     * @param depreciationItem item de depreciação
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void addDepreciationItem(DepreciationItem depreciationItem) {
        validateEditable("add depreciation item");

        Objects.requireNonNull(depreciationItem, "DepreciationItem cannot be null");

        if (!depreciationItem.getEvaluationId().equals(this.id)) {
            throw new IllegalArgumentException("DepreciationItem evaluationId does not match evaluation ID");
        }

        depreciationItems.add(depreciationItem);
        markAsUpdated();
    }

    /**
     * Atualiza o checklist técnico da avaliação.
     *
     * @param checklist checklist técnico preenchido
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void updateChecklist(EvaluationChecklist checklist) {
        validateEditable("update checklist");

        Objects.requireNonNull(checklist, "Checklist cannot be null");

        if (!checklist.getEvaluationId().equals(this.id)) {
            throw new IllegalArgumentException("Checklist evaluationId does not match evaluation ID");
        }

        this.checklist = checklist;
        markAsUpdated();
    }

    /**
     * Calcula o valor da avaliação com base no preço FIPE.
     *
     * @param fipePrice preço de mercado FIPE
     * @param liquidityPercentage percentual de liquidez (ex: 0.82 para 82%)
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void calculateEvaluation(Money fipePrice, double liquidityPercentage) {
        validateEditable("calculate evaluation");

        Objects.requireNonNull(fipePrice, "FipePrice cannot be null");

        if (liquidityPercentage < 0 || liquidityPercentage > 1) {
            throw new IllegalArgumentException("Liquidity percentage must be between 0 and 1");
        }

        this.fipePrice = fipePrice;
        this.baseValue = fipePrice.percentage(liquidityPercentage);

        // Calcula valor final após depreciações
        Money totalDepreciation = depreciationItems.stream()
            .map(DepreciationItem::getDepreciationValue)
            .reduce(Money.ZERO, Money::add);

        this.finalValue = baseValue.subtract(totalDepreciation);

        markAsUpdated();
    }

    /**
     * Atualiza os valores de preço FIPE e valor base da avaliação.
     *
     * @param fipePrice preço de mercado FIPE
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void setFipePrice(Money fipePrice) {
        validateEditable("set FIPE price");
        Objects.requireNonNull(fipePrice, "FipePrice cannot be null");
        this.fipePrice = fipePrice;
        markAsUpdated();
    }

    /**
     * Define o valor base da avaliação.
     *
     * @param baseValue valor base calculado
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void setBaseValue(Money baseValue) {
        validateEditable("set base value");
        Objects.requireNonNull(baseValue, "BaseValue cannot be null");
        this.baseValue = baseValue;
        markAsUpdated();
    }

    /**
     * Define o valor final da avaliação (após depreciações e ajustes).
     *
     * @param finalValue valor final calculado
     * @throws InvalidEvaluationStatusException se a avaliação não permitir edição
     */
    public void setFinalValue(Money finalValue) {
        validateEditable("set final value");
        Objects.requireNonNull(finalValue, "FinalValue cannot be null");
        this.finalValue = finalValue;
        markAsUpdated();
    }

    /**
     * Submete a avaliação para aprovação.
     *
     * @throws InvalidEvaluationStatusException se a avaliação não puder ser submetida
     * @throws IllegalStateException se dados obrigatórios estiverem faltando
     */
    public void submitForApproval() {
        if (!status.canBeSubmitted()) {
            throw new InvalidEvaluationStatusException(status, "submit for approval");
        }

        validateRequiredDataForSubmission();

        this.status = EvaluationStatus.PENDING_APPROVAL;
        this.submittedAt = LocalDateTime.now();
        markAsUpdated();

        // Adiciona evento de domínio
        addDomainEvent(new EvaluationSubmittedEvent(
            id.getValueAsString(),
            evaluatorId,
            plate.getFormatted(),
            finalValue != null ? finalValue.getAmount().doubleValue() : 0.0,
            fipePrice != null ? fipePrice.getAmount().doubleValue() : 0.0
        ));
    }

    /**
     * Aprova a avaliação.
     *
     * @param approverId ID do gerente que aprovou
     * @param adjustedValue valor ajustado opcional
     * @throws InvalidEvaluationStatusException se a avaliação não puder ser aprovada
     */
    public void approve(String approverId, Money adjustedValue) {
        if (!status.canBeApproved()) {
            throw new InvalidEvaluationStatusException(status, "approve");
        }

        Objects.requireNonNull(approverId, "ApproverId cannot be null");

        this.status = EvaluationStatus.APPROVED;
        this.approverId = approverId;
        this.approvedValue = adjustedValue != null ? adjustedValue : finalValue;
        this.approvedAt = LocalDateTime.now();
        this.validUntil = LocalDateTime.now().plusHours(72);
        this.validationToken = generateValidationToken();

        markAsUpdated();

        // Adiciona evento de domínio
        addDomainEvent(new EvaluationApprovedEvent(
            id.getValueAsString(),
            approverId,
            plate.getFormatted(),
            this.approvedValue.getAmount().doubleValue(),
            this.validationToken,
            this.validUntil
        ));

        // Se valor final foi definido, publicar evento completo
        if (this.approvedValue != null && !this.approvedValue.isZero()) {
            addDomainEvent(new VehicleEvaluationCompletedEvent(
                id.getValueAsString(),
                plate.getFormatted(),
                vehicleInfo.getBrand(),
                vehicleInfo.getModel(),
                vehicleInfo.getYearModel(),
                this.approvedValue.getAmount().doubleValue(),
                this.validUntil,
                buildEvaluationData()
            ));
        }
    }

    /**
     * Rejeita a avaliação.
     *
     * @param approverId ID do gerente que rejeitou
     * @param reason motivo da rejeição
     * @throws InvalidEvaluationStatusException se a avaliação não puder ser rejeitada
     */
    public void reject(String approverId, String reason) {
        if (!status.canBeRejected()) {
            throw new InvalidEvaluationStatusException(status, "reject");
        }

        Objects.requireNonNull(approverId, "ApproverId cannot be null");
        Objects.requireNonNull(reason, "Rejection reason cannot be null");

        if (reason.trim().isEmpty()) {
            throw new IllegalArgumentException("Rejection reason cannot be empty");
        }

        this.status = EvaluationStatus.REJECTED;
        this.approverId = approverId;
        this.justification = reason;
        this.approvedAt = LocalDateTime.now();

        markAsUpdated();

        // Adiciona evento de domínio
        addDomainEvent(new EvaluationRejectedEvent(
            id.getValueAsString(),
            approverId,
            plate.getFormatted(),
            reason
        ));
    }

    /**
     * Cancela a avaliação.
     *
     * @throws InvalidEvaluationStatusException se a avaliação não puder ser cancelada
     */
    public void cancel() {
        if (!status.isEditable()) {
            throw new InvalidEvaluationStatusException(status, "cancel");
        }

        this.status = EvaluationStatus.CANCELLED;
        markAsUpdated();
    }

    /**
     * Verifica se a avaliação pode ser editada.
     *
     * @param operation operação a ser validada
     * @throws InvalidEvaluationStatusException se não permitir edição
     */
    private void validateEditable(String operation) {
        if (!status.isEditable()) {
            throw new InvalidEvaluationStatusException(status, operation);
        }
    }

    /**
     * Valida se os dados obrigatórios para submissão estão presentes.
     *
     * @throws IllegalStateException se dados obrigatórios faltarem
     */
    private void validateRequiredDataForSubmission() {
        if (photos.isEmpty()) {
            throw new IllegalStateException("Cannot submit evaluation without photos");
        }

        if (checklist == null || !checklist.isComplete()) {
            throw new IllegalStateException("Cannot submit evaluation without complete checklist");
        }

        if (finalValue == null || finalValue.isZero()) {
            throw new IllegalStateException("Cannot submit evaluation without calculated final value");
        }
    }

    /**
     * Gera token de validação para laudo aprovado.
     *
     * @return token JWT
     */
    private String generateValidationToken() {
        // Implementação simplificada - em produção usaria biblioteca JWT
        return "TOKEN-" + id.getValueAsString() + "-" + System.currentTimeMillis();
    }

    /**
     * Marca a entidade como atualizada.
     */
    private void markAsUpdated() {
        this.updatedAt = LocalDateTime.now();
    }

    /**
     * Adiciona um evento de domínio à lista de eventos.
     *
     * @param event evento a ser adicionado
     */
    private void addDomainEvent(DomainEvent event) {
        this.domainEvents.add(event);
    }

    /**
     * Retorna os eventos de domínio e limpa a lista.
     *
     * @return lista de eventos
     */
    public List<DomainEvent> getDomainEvents() {
        List<DomainEvent> events = new ArrayList<>(domainEvents);
        domainEvents.clear();
        return events;
    }

    /**
     * Verifica se a avaliação está expirada (aprovada há mais de 72h).
     *
     * @return true se estiver expirada
     */
    public boolean isExpired() {
        return status == EvaluationStatus.APPROVED &&
               validUntil != null &&
               LocalDateTime.now().isAfter(validUntil);
    }

    /**
     * Verifica se a avaliação tem fotos suficientes.
     *
     * @param requiredCount número mínimo de fotos
     * @return true se tiver fotos suficientes
     */
    public boolean hasRequiredPhotos(int requiredCount) {
        return photos.size() >= requiredCount;
    }

    /**
     * Retorna o total de depreciação.
     *
     * @return valor total das depreciações
     */
    public Money getTotalDepreciation() {
        return depreciationItems.stream()
            .map(DepreciationItem::getDepreciationValue)
            .reduce(Money.ZERO, Money::add);
    }

    // Getters
    public EvaluationId getId() {
        return id;
    }

    public Plate getPlate() {
        return plate;
    }

    public String getRenavam() {
        return renavam;
    }

    public VehicleInfo getVehicleInfo() {
        return vehicleInfo;
    }

    public Money getMileage() {
        return mileage;
    }

    public EvaluationStatus getStatus() {
        return status;
    }

    public List<EvaluationPhoto> getPhotos() {
        return Collections.unmodifiableList(photos);
    }

    public List<DepreciationItem> getDepreciationItems() {
        return Collections.unmodifiableList(depreciationItems);
    }

    public EvaluationChecklist getChecklist() {
        return checklist;
    }

    public Money getFipePrice() {
        return fipePrice;
    }

    public Money getBaseValue() {
        return baseValue;
    }

    public Money getFinalValue() {
        return finalValue;
    }

    public Money getApprovedValue() {
        return approvedValue;
    }

    public String getObservations() {
        return observations;
    }

    public String getJustification() {
        return justification;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public LocalDateTime getSubmittedAt() {
        return submittedAt;
    }

    public LocalDateTime getApprovedAt() {
        return approvedAt;
    }

    public String getEvaluatorId() {
        return evaluatorId;
    }

    public String getApproverId() {
        return approverId;
    }

    public LocalDateTime getValidUntil() {
        return validUntil;
    }

    public String getValidationToken() {
        return validationToken;
    }

    // Setters com validação
    public void setObservations(String observations) {
        validateEditable("set observations");
        this.observations = observations;
        markAsUpdated();
    }

    /**
     * Constrói um mapa com os dados principais da avaliação.
     * Usado para publicação de eventos completos.
     *
     * @return mapa com dados da avaliação
     */
    private Map<String, Object> buildEvaluationData() {
        Map<String, Object> data = new HashMap<>();
        data.put("evaluationId", id.getValueAsString());
        data.put("plate", plate.getFormatted());
        data.put("renavam", renavam);
        data.put("brand", vehicleInfo.getBrand());
        data.put("model", vehicleInfo.getModel());
        data.put("year", vehicleInfo.getYearModel());
        data.put("mileage", mileage.getAmount());
        data.put("fipePrice", fipePrice != null ? fipePrice.getAmount() : null);
        data.put("baseValue", baseValue != null ? baseValue.getAmount() : null);
        data.put("finalValue", finalValue != null ? finalValue.getAmount() : null);
        data.put("approvedValue", approvedValue != null ? approvedValue.getAmount() : null);
        data.put("status", status.name());
        data.put("evaluatorId", evaluatorId);
        data.put("approverId", approverId);
        data.put("validationToken", validationToken);
        data.put("validUntil", validUntil != null ? validUntil.toString() : null);
        data.put("photoCount", photos.size());
        data.put("depreciationCount", depreciationItems.size());
        data.put("hasChecklist", checklist != null);
        return data;
    }
}