package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationPhotoRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.infra.entity.PhotoTypeJpa;
import com.gestauto.vehicleevaluation.infra.mapper.EvaluationPhotoMapper;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional
public class EvaluationPhotoRepositoryImpl implements EvaluationPhotoRepository {

    private final EvaluationPhotoJpaRepository jpaRepository;
    private final VehicleEvaluationJpaRepository evaluationJpaRepository;

    public EvaluationPhotoRepositoryImpl(EvaluationPhotoJpaRepository jpaRepository,
                                         VehicleEvaluationJpaRepository evaluationJpaRepository) {
        this.jpaRepository = jpaRepository;
        this.evaluationJpaRepository = evaluationJpaRepository;
    }

    @Override
    public EvaluationPhoto save(EvaluationPhoto photo) {
        var evaluation = evaluationJpaRepository.findById(
            UUID.fromString(photo.getEvaluationId().getValueAsString()))
            .orElseThrow(() -> new IllegalArgumentException("Evaluation not found for photo"));

        var entity = EvaluationPhotoMapper.toEntity(photo, evaluation);
        var saved = jpaRepository.save(entity);
        return EvaluationPhotoMapper.toDomain(saved);
    }

    @Override
    public void deleteById(String photoId) {
        jpaRepository.deleteById(UUID.fromString(photoId));
    }

    @Override
    public Optional<EvaluationPhoto> findById(String photoId) {
        return jpaRepository.findById(UUID.fromString(photoId)).map(EvaluationPhotoMapper::toDomain);
    }

    @Override
    public List<EvaluationPhoto> findByEvaluationId(EvaluationId evaluationId) {
        return jpaRepository.findByEvaluationId(UUID.fromString(evaluationId.getValueAsString())).stream()
            .map(EvaluationPhotoMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public Optional<EvaluationPhoto> findByEvaluationIdAndPhotoType(EvaluationId evaluationId, PhotoType photoType) {
        return jpaRepository.findByEvaluationIdAndPhotoType(
                UUID.fromString(evaluationId.getValueAsString()), PhotoTypeJpa.valueOf(photoType.name()))
            .map(EvaluationPhotoMapper::toDomain);
    }

    @Override
    public List<EvaluationPhoto> findByPhotoType(PhotoType photoType) {
        return jpaRepository.findByPhotoType(PhotoTypeJpa.valueOf(photoType.name())).stream()
            .map(EvaluationPhotoMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public boolean existsByEvaluationIdAndPhotoType(EvaluationId evaluationId, PhotoType photoType) {
        return jpaRepository.existsByEvaluationIdAndPhotoType(
            UUID.fromString(evaluationId.getValueAsString()), PhotoTypeJpa.valueOf(photoType.name()));
    }

    @Override
    public long countByEvaluationId(EvaluationId evaluationId) {
        return jpaRepository.countByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
    }

    @Override
    public void deleteAllByEvaluationId(EvaluationId evaluationId) {
        jpaRepository.deleteAllByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
    }

    @Override
    public List<EvaluationPhoto> findLargePhotos() {
        return jpaRepository.findByFileSizeGreaterThan(2L * 1024 * 1024).stream()
            .map(EvaluationPhotoMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public void updateThumbnailUrl(String photoId, String thumbnailUrl) {
        jpaRepository.updateThumbnailUrl(UUID.fromString(photoId), thumbnailUrl);
    }
}
