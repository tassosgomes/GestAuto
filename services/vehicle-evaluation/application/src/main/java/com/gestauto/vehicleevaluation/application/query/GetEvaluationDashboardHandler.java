package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.BrandStatDto;
import com.gestauto.vehicleevaluation.application.dto.EvaluationDashboardDto;
import com.gestauto.vehicleevaluation.application.dto.EvaluationKpiDto;
import com.gestauto.vehicleevaluation.application.dto.MonthlyStatDto;
import com.gestauto.vehicleevaluation.application.dto.PeriodDto;
import com.gestauto.vehicleevaluation.domain.report.BrandStat;
import com.gestauto.vehicleevaluation.domain.report.EvaluationKpi;
import com.gestauto.vehicleevaluation.domain.report.MonthlyStat;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationDashboardRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.cache.annotation.Cacheable;
import org.springframework.stereotype.Component;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDateTime;
import java.time.YearMonth;
import java.time.temporal.ChronoUnit;
import java.util.List;

@Component
@RequiredArgsConstructor
@Slf4j
public class GetEvaluationDashboardHandler implements QueryHandler<GetEvaluationDashboardQuery, EvaluationDashboardDto> {

    private static final int DEFAULT_MONTHS_RANGE = 12;

    private final EvaluationDashboardRepository dashboardRepository;

    @Override
    @Cacheable(
        value = "evaluation-dashboard",
        cacheManager = "dashboardCacheManager",
        key = "T(String).valueOf(#query.startDate()) + ':' + T(String).valueOf(#query.endDate()) + ':' + T(String).valueOf(#query.evaluatorId()) + ':' + T(String).valueOf(#query.status())"
    )
    public EvaluationDashboardDto handle(GetEvaluationDashboardQuery query) {
        LocalDateTime effectiveEndDate = query.endDate() != null ? query.endDate() : LocalDateTime.now();
        LocalDateTime effectiveStartDate = query.startDate() != null ? query.startDate() : effectiveEndDate.minusMonths(DEFAULT_MONTHS_RANGE);

        if (effectiveStartDate.isAfter(effectiveEndDate)) {
            throw new IllegalArgumentException("startDate must be <= endDate");
        }

        log.info("Loading evaluation dashboard: startDate={}, endDate={}, evaluatorId={}, status={}",
            effectiveStartDate, effectiveEndDate, query.evaluatorId(), query.status());

        EvaluationKpi kpis = dashboardRepository.getKpis(
            effectiveStartDate,
            effectiveEndDate,
            query.evaluatorId(),
            query.status()
        );

        List<MonthlyStat> monthly = dashboardRepository.getMonthlyStats(
            effectiveStartDate,
            effectiveEndDate,
            query.evaluatorId(),
            query.status()
        );

        List<BrandStat> brands = dashboardRepository.getBrandStats(
            effectiveStartDate,
            effectiveEndDate,
            query.evaluatorId(),
            query.status()
        );

        BigDecimal evaluationsPerMonth = calculateEvaluationsPerMonth(
            kpis.totalEvaluations(),
            effectiveStartDate,
            effectiveEndDate
        );

        EvaluationKpiDto kpiDto = new EvaluationKpiDto(
            evaluationsPerMonth,
            defaultZero(kpis.approvalRatePercent()),
            defaultZero(kpis.averageTicket()),
            defaultZero(kpis.averageReviewTimeHours())
        );

        List<MonthlyStatDto> monthlyDtos = monthly.stream()
            .map(m -> new MonthlyStatDto(m.month(), m.evaluationsCount(), m.totalTicket()))
            .toList();

        List<BrandStatDto> brandDtos = brands.stream()
            .map(b -> new BrandStatDto(b.brand(), b.evaluationsCount(), b.averageTicket(), b.approvalRatePercent()))
            .toList();

        return new EvaluationDashboardDto(
            new PeriodDto(effectiveStartDate, effectiveEndDate),
            kpiDto,
            monthlyDtos,
            brandDtos,
            LocalDateTime.now()
        );
    }

    private BigDecimal calculateEvaluationsPerMonth(long total, LocalDateTime startDate, LocalDateTime endDate) {
        YearMonth start = YearMonth.from(startDate);
        YearMonth end = YearMonth.from(endDate);

        long months = ChronoUnit.MONTHS.between(start.atDay(1), end.atDay(1)) + 1;
        if (months <= 0) {
            return BigDecimal.ZERO;
        }

        return BigDecimal.valueOf(total)
            .divide(BigDecimal.valueOf(months), 2, RoundingMode.HALF_UP);
    }

    private BigDecimal defaultZero(BigDecimal value) {
        return value != null ? value : BigDecimal.ZERO;
    }
}
