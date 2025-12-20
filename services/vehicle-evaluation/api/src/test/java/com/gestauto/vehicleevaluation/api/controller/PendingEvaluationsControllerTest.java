package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.ApproveEvaluationHandler;
import com.gestauto.vehicleevaluation.application.command.RejectEvaluationHandler;
import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.PendingEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.application.query.GetPendingApprovalsHandler;
import java.util.List;
import java.util.UUID;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.assertj.core.api.Assertions.assertThat;

class PendingEvaluationsControllerTest {

    GetPendingApprovalsHandler getPendingApprovalsHandler;

    ApproveEvaluationHandler approveEvaluationHandler;

    RejectEvaluationHandler rejectEvaluationHandler;

    PendingEvaluationsController controller;

    @BeforeEach
    void setUp() {
        getPendingApprovalsHandler = mock(GetPendingApprovalsHandler.class);
        approveEvaluationHandler = mock(ApproveEvaluationHandler.class);
        rejectEvaluationHandler = mock(RejectEvaluationHandler.class);
        controller = new PendingEvaluationsController(getPendingApprovalsHandler, approveEvaluationHandler, rejectEvaluationHandler);
    }

    @Test
    void getPendingApprovalsReturns200() throws Exception {
        when(getPendingApprovalsHandler.handle(any())).thenReturn(new PagedResult<>(
            List.of(),
            0,
            20,
            0,
            0,
            true,
            true,
            0
        ));

        var response = controller.getPendingApprovals(new com.gestauto.vehicleevaluation.application.query.GetPendingApprovalsQuery(null, null, null, null));
        assertThat(response.getStatusCode().value()).isEqualTo(200);
    }

    @Test
    void approveReturns200() throws Exception {
        UUID id = UUID.randomUUID();
        var body = new PendingEvaluationsController.ApproveEvaluationRequest(new java.math.BigDecimal("45000.00"));

        var response = controller.approveEvaluation(id, body);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        verify(approveEvaluationHandler).handle(any());
    }

    @Test
    void rejectReturns200WhenReasonTooShort() throws Exception {
        // Bean validation lives at the MVC layer; this test class focuses on controller flow only.
        UUID id = UUID.randomUUID();
        var body = new PendingEvaluationsController.RejectEvaluationRequest("short");

        var response = controller.rejectEvaluation(id, body);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        verify(rejectEvaluationHandler).handle(any());
    }

    @Test
    void rejectReturns200WhenValid() throws Exception {
        UUID id = UUID.randomUUID();
        var body = new PendingEvaluationsController.RejectEvaluationRequest("Justificativa válida para rejeição");

        var response = controller.rejectEvaluation(id, body);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        verify(rejectEvaluationHandler).handle(any());
    }
}
