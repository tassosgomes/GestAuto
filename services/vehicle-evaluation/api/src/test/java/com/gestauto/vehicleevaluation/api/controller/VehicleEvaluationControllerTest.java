package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.CreateEvaluationHandler;
import com.gestauto.vehicleevaluation.application.command.GenerateReportHandler;
import com.gestauto.vehicleevaluation.application.command.UpdateChecklistHandler;
import com.gestauto.vehicleevaluation.application.command.UpdateEvaluationHandler;
import com.gestauto.vehicleevaluation.application.dto.CreateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.dto.DocumentsDto;
import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.UpdateChecklistCommand;
import com.gestauto.vehicleevaluation.application.dto.UpdateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationDto;
import com.gestauto.vehicleevaluation.application.query.GetEvaluationHandler;
import com.gestauto.vehicleevaluation.application.query.ListEvaluationsHandler;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.mock.web.MockHttpServletRequest;
import org.springframework.web.context.request.RequestContextHolder;
import org.springframework.web.context.request.ServletRequestAttributes;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.assertj.core.api.Assertions.assertThat;

class VehicleEvaluationControllerTest {

    CreateEvaluationHandler createEvaluationHandler;

    UpdateEvaluationHandler updateEvaluationHandler;

    UpdateChecklistHandler updateChecklistHandler;

    GetEvaluationHandler getEvaluationHandler;

    ListEvaluationsHandler listEvaluationsHandler;

    GenerateReportHandler generateReportHandler;

    VehicleEvaluationController controller;

    @BeforeEach
    void setUp() {
        createEvaluationHandler = mock(CreateEvaluationHandler.class);
        updateEvaluationHandler = mock(UpdateEvaluationHandler.class);
        updateChecklistHandler = mock(UpdateChecklistHandler.class);
        getEvaluationHandler = mock(GetEvaluationHandler.class);
        listEvaluationsHandler = mock(ListEvaluationsHandler.class);
        generateReportHandler = mock(GenerateReportHandler.class);

        controller = new VehicleEvaluationController(
            createEvaluationHandler,
            updateEvaluationHandler,
            updateChecklistHandler,
            getEvaluationHandler,
            listEvaluationsHandler,
            generateReportHandler
        );
    }

    @Test
    void createEvaluationReturns201AndLocation() throws Exception {
        UUID id = UUID.randomUUID();
        when(createEvaluationHandler.handle(any(CreateEvaluationCommand.class))).thenReturn(id);

        CreateEvaluationCommand body = new CreateEvaluationCommand(
            "ABC1234",
            2022,
            45000,
            "Prata",
            null,
            null,
            null,
            List.of(),
            null
        );

        MockHttpServletRequest request = new MockHttpServletRequest("POST", "/api/v1/evaluations");
        RequestContextHolder.setRequestAttributes(new ServletRequestAttributes(request));
        try {
            var response = controller.createEvaluation(body);
            assertThat(response.getStatusCode().value()).isEqualTo(201);
            assertThat(response.getHeaders().getLocation()).isNotNull();
            assertThat(response.getHeaders().getLocation().toString()).contains("/api/v1/evaluations/");
            assertThat(response.getBody()).isEqualTo(id);
        } finally {
            RequestContextHolder.resetRequestAttributes();
        }
    }

    @Test
    void updateEvaluationReturns204() throws Exception {
        UUID id = UUID.randomUUID();

        UpdateEvaluationCommand body = new UpdateEvaluationCommand(
            null,
            "ABC1234",
            2022,
            45000,
            "Prata",
            null,
            null,
            null,
            List.of(),
            null
        );

        var response = controller.updateEvaluation(id, body);
        assertThat(response.getStatusCode().value()).isEqualTo(204);
    }

    @Test
    void getEvaluationReturns200() throws Exception {
        UUID id = UUID.randomUUID();

        when(getEvaluationHandler.handle(any())).thenReturn(minimalVehicleEvaluationDto(id, "token-1", LocalDateTime.now().plusHours(1)));

        var response = controller.getEvaluation(id);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
    }

    @Test
    void listEvaluationsReturns200AndCoversInvalidStatusPath() throws Exception {
        when(listEvaluationsHandler.handle(any())).thenReturn(new PagedResult<>(
            List.of(),
            0,
            20,
            0,
            0,
            true,
            true,
            0
        ));

        var response = controller.listEvaluations(
            null,
            "not-a-real-status",
            null,
            0,
            20,
            "createdAt",
            "DESC"
        );
        assertThat(response.getStatusCode().value()).isEqualTo(200);
    }

    @Test
    void updateChecklistReturns204() throws Exception {
        UUID id = UUID.randomUUID();

        UpdateChecklistCommand body = new UpdateChecklistCommand(
            null,
            null,
            null,
            null,
            null,
            new DocumentsDto(true, null, null, null, null)
        );

        var response = controller.updateChecklist(id, body);
        assertThat(response.getStatusCode().value()).isEqualTo(204);
    }

    @Test
    void generateReportReturns200WithPdfHeaders() throws Exception {
        UUID id = UUID.randomUUID();
        when(generateReportHandler.handle(any())).thenReturn(new byte[] {1, 2, 3});

        var response = controller.generateReport(id);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        assertThat(response.getHeaders().getFirst("Content-Type")).isEqualTo("application/pdf");
        assertThat(response.getHeaders().getFirst("Content-Disposition")).contains("evaluation_report_" + id);
        assertThat(response.getHeaders().getFirst("Cache-Control")).isEqualTo("max-age=3600");
    }

    @Test
    void validateReportReturns400WhenTokenMismatch() throws Exception {
        UUID id = UUID.randomUUID();
        when(getEvaluationHandler.handle(any())).thenReturn(minimalVehicleEvaluationDto(id, "expected-token", LocalDateTime.now().plusHours(1)));

        var response = controller.validateReport(id, "wrong-token");
        assertThat(response.getStatusCode().value()).isEqualTo(400);
        assertThat(response.getBody()).isNotNull();
        assertThat(response.getBody().get("valid")).isEqualTo(false);
    }

    @Test
    void validateReportReturns400WhenExpired() throws Exception {
        UUID id = UUID.randomUUID();
        when(getEvaluationHandler.handle(any())).thenReturn(minimalVehicleEvaluationDto(id, "token-1", LocalDateTime.now().minusMinutes(1)));

        var response = controller.validateReport(id, "token-1");
        assertThat(response.getStatusCode().value()).isEqualTo(400);
        assertThat(response.getBody()).isNotNull();
        assertThat(response.getBody().get("valid")).isEqualTo(false);
    }

    @Test
    void validateReportReturns200WhenValid() throws Exception {
        UUID id = UUID.randomUUID();
        when(getEvaluationHandler.handle(any())).thenReturn(minimalVehicleEvaluationDto(id, "token-1", LocalDateTime.now().plusHours(1)));

        var response = controller.validateReport(id, "token-1");
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        assertThat(response.getBody()).isNotNull();
        assertThat(response.getBody().get("valid")).isEqualTo(true);
        assertThat(response.getBody().get("evaluationId")).isEqualTo(id.toString());
    }

    private static VehicleEvaluationDto minimalVehicleEvaluationDto(UUID id, String validationToken, LocalDateTime validUntil) {
        return new VehicleEvaluationDto(
            id,
            "ABC1234",
            null,
            "Volkswagen",
            "Gol",
            2022,
            45000,
            null,
            null,
            null,
            null,
            List.of(),
            "APPROVED",
            null,
            null,
            null,
            new BigDecimal("45000.00"),
            null,
            null,
            "evaluator-1",
            null,
            LocalDateTime.now(),
            LocalDateTime.now(),
            null,
            null,
            validUntil,
            validationToken,
            List.of(),
            List.of(),
            null
        );
    }
}
