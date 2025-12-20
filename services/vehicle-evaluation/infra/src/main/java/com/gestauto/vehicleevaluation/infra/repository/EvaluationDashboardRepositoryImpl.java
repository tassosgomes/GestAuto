package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.report.BrandStat;
import com.gestauto.vehicleevaluation.domain.report.EvaluationKpi;
import com.gestauto.vehicleevaluation.domain.report.MonthlyStat;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationDashboardRepository;
import com.gestauto.vehicleevaluation.infra.repository.projection.BrandStatProjection;
import com.gestauto.vehicleevaluation.infra.repository.projection.EvaluationKpiProjection;
import com.gestauto.vehicleevaluation.infra.repository.projection.MonthlyStatProjection;
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.Timer;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Repository
@RequiredArgsConstructor
public class EvaluationDashboardRepositoryImpl implements EvaluationDashboardRepository {

    private final VehicleEvaluationJpaRepository jpaRepository;
    private final Optional<MeterRegistry> meterRegistry;

    @Override
    public EvaluationKpi getKpis(LocalDateTime startDate,
                                 LocalDateTime endDate,
                                 String evaluatorId,
                                 EvaluationStatus status) {

        Timer.Sample sample = meterRegistry.isPresent() ? Timer.start(meterRegistry.get()) : null;
        try {
            String statusValue = status != null ? status.name() : null;
            EvaluationKpiProjection projection = jpaRepository.getDashboardKpis(startDate, endDate, evaluatorId, statusValue);

            return new EvaluationKpi(
                projection.getTotalEvaluations() != null ? projection.getTotalEvaluations() : 0L,
                projection.getApprovalRatePercent(),
                projection.getAverageTicket(),
                projection.getAverageReviewTimeHours()
            );
        } finally {
            if (sample != null) {
                sample.stop(Timer.builder("dashboard.kpis.query.duration")
                    .description("Tempo de consulta dos KPIs do dashboard")
                    .register(meterRegistry.get()));
            }
        }
    }

    @Override
    public List<MonthlyStat> getMonthlyStats(LocalDateTime startDate,
                                            LocalDateTime endDate,
                                            String evaluatorId,
                                            EvaluationStatus status) {

        Timer.Sample sample = meterRegistry.isPresent() ? Timer.start(meterRegistry.get()) : null;
        try {
            String statusValue = status != null ? status.name() : null;
            List<MonthlyStatProjection> projections = jpaRepository.getDashboardMonthlyStats(startDate, endDate, evaluatorId, statusValue);

            return projections.stream()
                .map(p -> new MonthlyStat(
                    p.getMonth(),
                    p.getEvaluationsCount() != null ? p.getEvaluationsCount() : 0L,
                    p.getTotalTicket()
                ))
                .toList();
        } finally {
            if (sample != null) {
                sample.stop(Timer.builder("dashboard.monthly.query.duration")
                    .description("Tempo de consulta das estatísticas mensais do dashboard")
                    .register(meterRegistry.get()));
            }
        }
    }

    @Override
    public List<BrandStat> getBrandStats(LocalDateTime startDate,
                                        LocalDateTime endDate,
                                        String evaluatorId,
                                        EvaluationStatus status) {

        Timer.Sample sample = meterRegistry.isPresent() ? Timer.start(meterRegistry.get()) : null;
        try {
            String statusValue = status != null ? status.name() : null;
            List<BrandStatProjection> projections = jpaRepository.getDashboardBrandStats(startDate, endDate, evaluatorId, statusValue);

            return projections.stream()
                .map(p -> new BrandStat(
                    p.getBrand(),
                    p.getEvaluationsCount() != null ? p.getEvaluationsCount() : 0L,
                    p.getAverageTicket(),
                    p.getApprovalRatePercent()
                ))
                .toList();
        } finally {
            if (sample != null) {
                sample.stop(Timer.builder("dashboard.brand.query.duration")
                    .description("Tempo de consulta das estatísticas por marca do dashboard")
                    .register(meterRegistry.get()));
            }
        }
    }
}
