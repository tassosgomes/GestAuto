package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.infra.entity.DepreciationItemJpaEntity;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface DepreciationItemJpaRepository extends JpaRepository<DepreciationItemJpaEntity, UUID> {

    List<DepreciationItemJpaEntity> findByEvaluationId(UUID evaluationId);

    List<DepreciationItemJpaEntity> findByCategory(String category);

    List<DepreciationItemJpaEntity> findByEvaluationIdAndCategory(UUID evaluationId, String category);

    List<DepreciationItemJpaEntity> findByCreatedBy(String createdBy);

    @Query("select coalesce(sum(d.depreciationValueAmount), 0) from DepreciationItemJpaEntity d where d.evaluation.id = :evaluationId")
    BigDecimal sumByEvaluationId(@Param("evaluationId") UUID evaluationId);

    long countByEvaluationId(UUID evaluationId);

    @Modifying
    @Query("delete from DepreciationItemJpaEntity d where d.evaluation.id = :evaluationId")
    void deleteAllByEvaluationId(@Param("evaluationId") UUID evaluationId);

    List<DepreciationItemJpaEntity> findByDepreciationValueAmountGreaterThan(BigDecimal minValue);

    List<DepreciationItemJpaEntity> findByEvaluationIdAndCreatedAtBetween(UUID evaluationId, LocalDateTime startDate, LocalDateTime endDate);

    Optional<DepreciationItemJpaEntity> findByDepreciationId(UUID depreciationId);
}
