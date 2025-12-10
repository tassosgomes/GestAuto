package com.gestauto.vehicleevaluation.infra.mapper;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationPhotoJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.PhotoTypeJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import java.util.UUID;

/**
 * Mapper para conversão de fotos de avaliação.
 */
public final class EvaluationPhotoMapper {

    private EvaluationPhotoMapper() {
    }

    public static EvaluationPhotoJpaEntity toEntity(EvaluationPhoto photo, VehicleEvaluationJpaEntity evaluationJpa) {
        if (photo == null) {
            return null;
        }
        EvaluationPhotoJpaEntity entity = new EvaluationPhotoJpaEntity();
        entity.setPhotoId(UUID.fromString(photo.getPhotoId()));
        entity.setEvaluation(evaluationJpa);
        entity.setPhotoType(PhotoTypeJpa.valueOf(photo.getPhotoType().name()));
        entity.setFileName(photo.getFileName());
        entity.setFilePath(photo.getFilePath());
        entity.setFileSize(photo.getFileSize());
        entity.setContentType(photo.getContentType());
        entity.setUploadUrl(photo.getUploadUrl());
        entity.setThumbnailUrl(photo.getThumbnailUrl());
        entity.setUploadedAt(photo.getUploadedAt());
        return entity;
    }

    public static EvaluationPhoto toDomain(EvaluationPhotoJpaEntity entity) {
        if (entity == null) {
            return null;
        }
        return EvaluationPhoto.restore(
            entity.getPhotoId().toString(),
            EvaluationId.from(entity.getEvaluation().getId()),
            PhotoType.valueOf(entity.getPhotoType().name()),
            entity.getFileName(),
            entity.getFilePath(),
            entity.getFileSize(),
            entity.getContentType(),
            entity.getUploadUrl(),
            entity.getThumbnailUrl(),
            entity.getUploadedAt()
        );
    }
}
