package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.infra.entity.EvaluationChecklistJpaEntity;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface EvaluationChecklistJpaRepository extends JpaRepository<EvaluationChecklistJpaEntity, UUID> {

    Optional<EvaluationChecklistJpaEntity> findByEvaluationId(UUID evaluationId);

    void deleteByEvaluationId(UUID evaluationId);

    List<EvaluationChecklistJpaEntity> findByConservationScoreLessThan(Integer minScore);

    @Query("select c from EvaluationChecklistJpaEntity c where c.criticalIssues is not empty")
    List<EvaluationChecklistJpaEntity> findWithCriticalIssues();

    List<EvaluationChecklistJpaEntity> findByCreatedAtBetween(LocalDateTime startDate, LocalDateTime endDate);

    boolean existsByEvaluationId(UUID evaluationId);

    List<EvaluationChecklistJpaEntity> findByConservationScoreIsNull();

    @Query("select count(c) from EvaluationChecklistJpaEntity c where c.evaluation.evaluatorId = :evaluatorId and c.conservationScore is not null")
    long countCompletedByEvaluator(@Param("evaluatorId") String evaluatorId);

    @Query("select c from EvaluationChecklistJpaEntity c where c.evaluation.evaluatorId = :evaluatorId")
    List<EvaluationChecklistJpaEntity> findByEvaluatorId(@Param("evaluatorId") String evaluatorId);
}
