package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.dto.EvaluationDashboardDto;
import com.gestauto.vehicleevaluation.application.query.GetEvaluationDashboardHandler;
import com.gestauto.vehicleevaluation.application.query.GetEvaluationDashboardQuery;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.service.ManagementReportExporter;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.time.LocalDate;
import java.time.LocalDateTime;

@RestController
@RequestMapping("/api/v1/evaluations/reports")
@RequiredArgsConstructor
@Slf4j
@Tag(name = "Relatórios de Avaliações", description = "Dashboard gerencial e exportações")
@SecurityRequirement(name = "bearerAuth")
@PreAuthorize("hasRole('ADMIN')")
public class EvaluationReportsController {

    private static final int DEFAULT_MONTHS_RANGE = 12;

    private final GetEvaluationDashboardHandler dashboardHandler;
    private final ManagementReportExporter reportExporter;

    @GetMapping("/dashboard")
    public ResponseEntity<EvaluationDashboardDto> getDashboard(
        @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
        @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate,
        @RequestParam(required = false) String evaluatorId,
        @RequestParam(required = false) EvaluationStatus status
    ) {
        LocalDateTime effectiveEndDate = endDate != null ? endDate : LocalDateTime.now();
        LocalDateTime effectiveStartDate = startDate != null ? startDate : effectiveEndDate.minusMonths(DEFAULT_MONTHS_RANGE);

        if (effectiveStartDate.isAfter(effectiveEndDate)) {
            return ResponseEntity.badRequest().build();
        }

        EvaluationDashboardDto dashboard = dashboardHandler.handle(
            new GetEvaluationDashboardQuery(effectiveStartDate, effectiveEndDate, evaluatorId, status)
        );
        return ResponseEntity.ok(dashboard);
    }

    @GetMapping("/excel")
    public ResponseEntity<byte[]> exportExcel(
        @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
        @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate,
        @RequestParam(required = false) String evaluatorId,
        @RequestParam(required = false) EvaluationStatus status
    ) {
        if (startDate.isAfter(endDate)) {
            return ResponseEntity.badRequest().build();
        }

        byte[] excel = reportExporter.exportDetailedExcel(startDate, endDate, evaluatorId, status);

        String filename = "vehicle_evaluations_" + LocalDate.now() + ".xlsx";

        return ResponseEntity.ok()
            .header(HttpHeaders.CONTENT_TYPE, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            .header(HttpHeaders.CONTENT_DISPOSITION, "attachment; filename=" + filename)
            .body(excel);
    }

    @GetMapping("/summary-pdf")
    public ResponseEntity<byte[]> exportSummaryPdf(
        @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
        @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate,
        @RequestParam(required = false) String evaluatorId,
        @RequestParam(required = false) EvaluationStatus status
    ) {
        if (startDate.isAfter(endDate)) {
            return ResponseEntity.badRequest().build();
        }

        byte[] pdf = reportExporter.exportSummaryPdf(startDate, endDate, evaluatorId, status);

        String filename = "evaluation_summary_" + LocalDate.now() + ".pdf";

        return ResponseEntity.ok()
            .header(HttpHeaders.CONTENT_TYPE, MediaType.APPLICATION_PDF_VALUE)
            .header(HttpHeaders.CONTENT_DISPOSITION, "attachment; filename=" + filename)
            .body(pdf);
    }
}
