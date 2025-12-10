package com.gestauto.vehicleevaluation.infra.mapper;

import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.infra.entity.DepreciationItemJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationChecklistJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationPhotoJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationStatusJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import java.util.List;
import java.util.Objects;
import java.util.UUID;
import java.util.stream.Collectors;

/**
 * Mapper do agregado VehicleEvaluation.
 */
public final class VehicleEvaluationMapper {

    private VehicleEvaluationMapper() {
    }

    public static VehicleEvaluationJpaEntity toEntity(VehicleEvaluation evaluation) {
        Objects.requireNonNull(evaluation, "VehicleEvaluation cannot be null");

        VehicleEvaluationJpaEntity entity = new VehicleEvaluationJpaEntity();
        entity.setId(UUID.fromString(evaluation.getId().getValueAsString()));
        entity.setPlate(evaluation.getPlate().getValue());
        entity.setRenavam(evaluation.getRenavam());
        entity.setEvaluatorId(evaluation.getEvaluatorId());
        entity.setApproverId(evaluation.getApproverId());
        entity.setVehicleInfo(VehicleInfoMapper.toEmbeddable(evaluation.getVehicleInfo()));
        entity.setMileageAmount(MoneyMapper.toAmount(evaluation.getMileage()));
        entity.setMileageCurrency(MoneyMapper.toCurrency(evaluation.getMileage()));
        entity.setStatus(EvaluationStatusJpa.valueOf(evaluation.getStatus().name()));

        Money fipePrice = evaluation.getFipePrice();
        entity.setFipePriceAmount(MoneyMapper.toAmount(fipePrice));
        entity.setFipePriceCurrency(MoneyMapper.toCurrency(fipePrice));

        Money baseValue = evaluation.getBaseValue();
        entity.setBaseValueAmount(MoneyMapper.toAmount(baseValue));
        entity.setBaseValueCurrency(MoneyMapper.toCurrency(baseValue));

        Money finalValue = evaluation.getFinalValue();
        entity.setFinalValueAmount(MoneyMapper.toAmount(finalValue));
        entity.setFinalValueCurrency(MoneyMapper.toCurrency(finalValue));

        Money approvedValue = evaluation.getApprovedValue();
        entity.setApprovedValueAmount(MoneyMapper.toAmount(approvedValue));
        entity.setApprovedValueCurrency(MoneyMapper.toCurrency(approvedValue));

        entity.setObservations(evaluation.getObservations());
        entity.setJustification(evaluation.getJustification());
        entity.setCreatedAt(evaluation.getCreatedAt());
        entity.setUpdatedAt(evaluation.getUpdatedAt());
        entity.setSubmittedAt(evaluation.getSubmittedAt());
        entity.setApprovedAt(evaluation.getApprovedAt());
        entity.setValidUntil(evaluation.getValidUntil());
        entity.setValidationToken(evaluation.getValidationToken());

        entity.getPhotos().clear();
        if (evaluation.getPhotos() != null) {
            for (EvaluationPhoto photo : evaluation.getPhotos()) {
                EvaluationPhotoJpaEntity photoEntity = EvaluationPhotoMapper.toEntity(photo, entity);
                entity.getPhotos().add(photoEntity);
            }
        }

        entity.getDepreciationItems().clear();
        if (evaluation.getDepreciationItems() != null) {
            for (DepreciationItem item : evaluation.getDepreciationItems()) {
                DepreciationItemJpaEntity itemEntity = DepreciationItemMapper.toEntity(item, entity);
                entity.getDepreciationItems().add(itemEntity);
            }
        }

        EvaluationChecklist checklist = evaluation.getChecklist();
        if (checklist != null) {
            EvaluationChecklistJpaEntity checklistEntity = EvaluationChecklistMapper.toEntity(checklist, entity);
            entity.setChecklist(checklistEntity);
        }

        return entity;
    }

    public static VehicleEvaluation toDomain(VehicleEvaluationJpaEntity entity) {
        Objects.requireNonNull(entity, "VehicleEvaluationJpaEntity cannot be null");

        EvaluationId evaluationId = EvaluationId.from(entity.getId());
        Plate plate = Plate.of(entity.getPlate());
        VehicleInfo vehicleInfo = VehicleInfoMapper.toDomain(entity.getVehicleInfo());
        Money mileage = MoneyMapper.fromAmountAndCurrency(entity.getMileageAmount(), entity.getMileageCurrency());

        List<EvaluationPhoto> photos = entity.getPhotos().stream()
            .map(EvaluationPhotoMapper::toDomain)
            .collect(Collectors.toList());

        List<DepreciationItem> depreciationItems = entity.getDepreciationItems().stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());

        EvaluationChecklist checklist = EvaluationChecklistMapper.toDomain(entity.getChecklist());

        return VehicleEvaluation.restore(
            evaluationId,
            plate,
            entity.getRenavam(),
            vehicleInfo,
            mileage,
            EvaluationStatus.valueOf(entity.getStatus().name()),
            MoneyMapper.fromAmountAndCurrency(entity.getFipePriceAmount(), entity.getFipePriceCurrency()),
            MoneyMapper.fromAmountAndCurrency(entity.getBaseValueAmount(), entity.getBaseValueCurrency()),
            MoneyMapper.fromAmountAndCurrency(entity.getFinalValueAmount(), entity.getFinalValueCurrency()),
            MoneyMapper.fromAmountAndCurrency(entity.getApprovedValueAmount(), entity.getApprovedValueCurrency()),
            entity.getObservations(),
            entity.getJustification(),
            entity.getCreatedAt(),
            entity.getUpdatedAt(),
            entity.getSubmittedAt(),
            entity.getApprovedAt(),
            entity.getEvaluatorId(),
            entity.getApproverId(),
            entity.getValidUntil(),
            entity.getValidationToken(),
            photos,
            depreciationItems,
            checklist
        );
    }
}
