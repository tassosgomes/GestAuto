package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.infra.entity.EvaluationStatusJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface VehicleEvaluationJpaRepository extends JpaRepository<VehicleEvaluationJpaEntity, UUID> {

    List<VehicleEvaluationJpaEntity> findByPlate(String plate);

    List<VehicleEvaluationJpaEntity> findByStatus(EvaluationStatusJpa status);

    List<VehicleEvaluationJpaEntity> findByStatus(EvaluationStatusJpa status, Pageable pageable);

    List<VehicleEvaluationJpaEntity> findByEvaluatorId(String evaluatorId);

    List<VehicleEvaluationJpaEntity> findByCreatedAtBetween(LocalDateTime startDate, LocalDateTime endDate);

    List<VehicleEvaluationJpaEntity> findByStatusAndEvaluatorId(EvaluationStatusJpa status, String evaluatorId);

    List<VehicleEvaluationJpaEntity> findByStatusOrderByCreatedAtAsc(EvaluationStatusJpa status, Pageable pageable);

    @Query("select v from VehicleEvaluationJpaEntity v where v.status = :status and v.validUntil <= :limit")
    List<VehicleEvaluationJpaEntity> findExpiring(@Param("status") EvaluationStatusJpa status,
                                                   @Param("limit") LocalDateTime limit);

    List<VehicleEvaluationJpaEntity> findByFinalValueAmountBetween(BigDecimal min, BigDecimal max);

    boolean existsByPlateAndStatus(String plate, EvaluationStatusJpa status);

    long countByStatus(EvaluationStatusJpa status);
}
