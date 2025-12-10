package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.infra.entity.EvaluationPhotoJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.PhotoTypeJpa;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface EvaluationPhotoJpaRepository extends JpaRepository<EvaluationPhotoJpaEntity, UUID> {

    List<EvaluationPhotoJpaEntity> findByEvaluationId(UUID evaluationId);

    Optional<EvaluationPhotoJpaEntity> findByEvaluationIdAndPhotoType(UUID evaluationId, PhotoTypeJpa photoType);

    List<EvaluationPhotoJpaEntity> findByPhotoType(PhotoTypeJpa photoType);

    boolean existsByEvaluationIdAndPhotoType(UUID evaluationId, PhotoTypeJpa photoType);

    long countByEvaluationId(UUID evaluationId);

    @Modifying
    @Query("delete from EvaluationPhotoJpaEntity p where p.evaluation.id = :evaluationId")
    void deleteAllByEvaluationId(@Param("evaluationId") UUID evaluationId);

    List<EvaluationPhotoJpaEntity> findByFileSizeGreaterThan(Long minSize);

    @Modifying
    @Query("update EvaluationPhotoJpaEntity p set p.thumbnailUrl = :thumbnailUrl where p.photoId = :photoId")
    void updateThumbnailUrl(@Param("photoId") UUID photoId, @Param("thumbnailUrl") String thumbnailUrl);
}
