package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.query.GetEvaluationDashboardHandler;
import com.gestauto.vehicleevaluation.domain.service.ManagementReportExporter;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.application.dto.EvaluationDashboardDto;
import java.time.LocalDateTime;
import java.util.List;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.http.HttpHeaders;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.assertj.core.api.Assertions.assertThat;

class EvaluationReportsControllerTest {

    GetEvaluationDashboardHandler dashboardHandler;

    ManagementReportExporter reportExporter;

    EvaluationReportsController controller;

    @BeforeEach
    void setUp() {
        dashboardHandler = mock(GetEvaluationDashboardHandler.class);
        reportExporter = mock(ManagementReportExporter.class);
        controller = new EvaluationReportsController(dashboardHandler, reportExporter);
    }

    @Test
    void dashboardReturns400WhenStartAfterEnd() throws Exception {
        var response = controller.getDashboard(
            LocalDateTime.parse("2025-01-02T00:00:00"),
            LocalDateTime.parse("2025-01-01T00:00:00"),
            null,
            null
        );
        assertThat(response.getStatusCode().value()).isEqualTo(400);
    }

    @Test
    void dashboardReturns200WhenDatesOk() throws Exception {
        when(dashboardHandler.handle(any())).thenReturn(new EvaluationDashboardDto(
            null,
            null,
            List.of(),
            List.of(),
            LocalDateTime.now()
        ));

        var response = controller.getDashboard(
            LocalDateTime.parse("2025-01-01T00:00:00"),
            LocalDateTime.parse("2025-01-02T00:00:00"),
            null,
            null
        );
        assertThat(response.getStatusCode().value()).isEqualTo(200);
    }

    @Test
    void exportExcelReturns400WhenStartAfterEnd() throws Exception {
        var response = controller.exportExcel(
            LocalDateTime.parse("2025-01-02T00:00:00"),
            LocalDateTime.parse("2025-01-01T00:00:00"),
            null,
            null
        );
        assertThat(response.getStatusCode().value()).isEqualTo(400);
    }

    @Test
    void exportExcelReturns200WithHeaders() throws Exception {
        when(reportExporter.exportDetailedExcel(any(), any(), any(), any())).thenReturn(new byte[] {1, 2, 3});

        var response = controller.exportExcel(
            LocalDateTime.parse("2025-01-01T00:00:00"),
            LocalDateTime.parse("2025-01-02T00:00:00"),
            null,
            EvaluationStatus.APPROVED
        );
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        assertThat(response.getHeaders().getFirst(HttpHeaders.CONTENT_TYPE))
            .isEqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        assertThat(response.getHeaders().getFirst(HttpHeaders.CONTENT_DISPOSITION))
            .contains("attachment; filename=");
    }

    @Test
    void exportSummaryPdfReturns400WhenStartAfterEnd() throws Exception {
        var response = controller.exportSummaryPdf(
            LocalDateTime.parse("2025-01-02T00:00:00"),
            LocalDateTime.parse("2025-01-01T00:00:00"),
            null,
            null
        );
        assertThat(response.getStatusCode().value()).isEqualTo(400);
    }

    @Test
    void exportSummaryPdfReturns200WithHeaders() throws Exception {
        when(reportExporter.exportSummaryPdf(any(), any(), any(), any())).thenReturn(new byte[] {1, 2, 3});

        var response = controller.exportSummaryPdf(
            LocalDateTime.parse("2025-01-01T00:00:00"),
            LocalDateTime.parse("2025-01-02T00:00:00"),
            null,
            null
        );
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        assertThat(response.getHeaders().getFirst(HttpHeaders.CONTENT_TYPE)).isEqualTo("application/pdf");
        assertThat(response.getHeaders().getFirst(HttpHeaders.CONTENT_DISPOSITION)).contains("attachment; filename=");
    }
}
