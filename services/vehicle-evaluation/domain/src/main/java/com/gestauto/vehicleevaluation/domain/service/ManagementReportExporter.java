package com.gestauto.vehicleevaluation.domain.service;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;

import java.time.LocalDateTime;

public interface ManagementReportExporter {

    byte[] exportDetailedExcel(LocalDateTime startDate,
                              LocalDateTime endDate,
                              String evaluatorId,
                              EvaluationStatus status);

    byte[] exportSummaryPdf(LocalDateTime startDate,
                            LocalDateTime endDate,
                            String evaluatorId,
                            EvaluationStatus status);
}
