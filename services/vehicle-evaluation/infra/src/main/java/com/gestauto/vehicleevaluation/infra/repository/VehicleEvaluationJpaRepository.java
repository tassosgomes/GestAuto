package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.infra.entity.EvaluationStatusJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import com.gestauto.vehicleevaluation.infra.repository.projection.BrandStatProjection;
import com.gestauto.vehicleevaluation.infra.repository.projection.EvaluationKpiProjection;
import com.gestauto.vehicleevaluation.infra.repository.projection.MonthlyStatProjection;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
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

    Optional<VehicleEvaluationJpaEntity> findByValidationToken(String validationToken);

    @Query("""
        select v from VehicleEvaluationJpaEntity v
        where v.createdAt between :startDate and :endDate
          and (:evaluatorId is null or v.evaluatorId = :evaluatorId)
          and (:status is null or v.status = :status)
        order by v.createdAt desc
        """)
    List<VehicleEvaluationJpaEntity> findForManagementReport(@Param("startDate") LocalDateTime startDate,
                                                            @Param("endDate") LocalDateTime endDate,
                                                            @Param("evaluatorId") String evaluatorId,
                                                            @Param("status") EvaluationStatusJpa status);

    @Query(value = """
        select
            count(*) as totalEvaluations,
            coalesce(
                sum(case when status = 'APPROVED' then 1 else 0 end) * 100.0 / nullif(count(*), 0),
                0
            ) as approvalRatePercent,
            coalesce(
                avg(coalesce(approved_value_amount, final_value_amount, base_value_amount)),
                0
            ) as averageTicket,
            coalesce(
                avg(extract(epoch from (approved_at - submitted_at)) / 3600.0),
                0
            ) as averageReviewTimeHours
        from vehicle_evaluation.vehicle_evaluations
        where created_at between :startDate and :endDate
          and (:evaluatorId is null or evaluator_id = :evaluatorId)
          and (:status is null or status = :status)
        """, nativeQuery = true)
    EvaluationKpiProjection getDashboardKpis(@Param("startDate") LocalDateTime startDate,
                                            @Param("endDate") LocalDateTime endDate,
                                            @Param("evaluatorId") String evaluatorId,
                                            @Param("status") String status);

    @Query(value = """
        select
            date_trunc('month', created_at)::date as month,
            count(*) as evaluationsCount,
            coalesce(sum(coalesce(approved_value_amount, final_value_amount, base_value_amount)), 0) as totalTicket
        from vehicle_evaluation.vehicle_evaluations
        where created_at between :startDate and :endDate
          and (:evaluatorId is null or evaluator_id = :evaluatorId)
          and (:status is null or status = :status)
        group by month
        order by month
        """, nativeQuery = true)
    List<MonthlyStatProjection> getDashboardMonthlyStats(@Param("startDate") LocalDateTime startDate,
                                                        @Param("endDate") LocalDateTime endDate,
                                                        @Param("evaluatorId") String evaluatorId,
                                                        @Param("status") String status);

    @Query(value = """
        select
            brand as brand,
            count(*) as evaluationsCount,
            coalesce(avg(coalesce(approved_value_amount, final_value_amount, base_value_amount)), 0) as averageTicket,
            coalesce(
                sum(case when status = 'APPROVED' then 1 else 0 end) * 100.0 / nullif(count(*), 0),
                0
            ) as approvalRatePercent
        from vehicle_evaluation.vehicle_evaluations
        where created_at between :startDate and :endDate
          and (:evaluatorId is null or evaluator_id = :evaluatorId)
          and (:status is null or status = :status)
        group by brand
        order by evaluationsCount desc
        """, nativeQuery = true)
    List<BrandStatProjection> getDashboardBrandStats(@Param("startDate") LocalDateTime startDate,
                                                    @Param("endDate") LocalDateTime endDate,
                                                    @Param("evaluatorId") String evaluatorId,
                                                    @Param("status") String status);

    @Query("select v from VehicleEvaluationJpaEntity v where v.status = :status and v.validUntil <= :limit")
    List<VehicleEvaluationJpaEntity> findExpiring(@Param("status") EvaluationStatusJpa status,
                                                   @Param("limit") LocalDateTime limit);

    List<VehicleEvaluationJpaEntity> findByFinalValueAmountBetween(BigDecimal min, BigDecimal max);

    boolean existsByPlateAndStatus(String plate, EvaluationStatusJpa status);

    long countByStatus(EvaluationStatusJpa status);
}
