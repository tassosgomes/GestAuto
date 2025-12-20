package com.gestauto.vehicleevaluation.domain.repository;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.report.BrandStat;
import com.gestauto.vehicleevaluation.domain.report.EvaluationKpi;
import com.gestauto.vehicleevaluation.domain.report.MonthlyStat;

import java.time.LocalDateTime;
import java.util.List;

public interface EvaluationDashboardRepository {

    EvaluationKpi getKpis(LocalDateTime startDate,
                          LocalDateTime endDate,
                          String evaluatorId,
                          EvaluationStatus status);

    List<MonthlyStat> getMonthlyStats(LocalDateTime startDate,
                                     LocalDateTime endDate,
                                     String evaluatorId,
                                     EvaluationStatus status);

    List<BrandStat> getBrandStats(LocalDateTime startDate,
                                 LocalDateTime endDate,
                                 String evaluatorId,
                                 EvaluationStatus status);
}
