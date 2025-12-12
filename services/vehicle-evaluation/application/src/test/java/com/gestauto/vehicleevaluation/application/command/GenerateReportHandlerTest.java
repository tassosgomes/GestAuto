package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.ReportService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.math.BigDecimal;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

/**
 * Testes unitários para o GenerateReportHandler.
 */
@ExtendWith(MockitoExtension.class)
@DisplayName("GenerateReportHandler Tests")
class GenerateReportHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @Mock
    private ReportService reportService;

    private GenerateReportHandler handler;
    private VehicleEvaluation evaluation;
    private UUID evaluationId;

    @BeforeEach
    void setUp() {
        handler = new GenerateReportHandler(evaluationRepository, reportService);
        evaluationId = UUID.randomUUID();

        // Criar avaliação de teste
        evaluation = createTestEvaluation();
    }

    private VehicleEvaluation createTestEvaluation() {
        return VehicleEvaluation.create(
                Plate.from("ABC1234"),
                "12345678901234",
                VehicleInfo.create(
                        "Toyota",
                        "Corolla",
                        2023,
                        "Branco",
                        FuelType.GASOLINE,
                        "XXX"
                ),
                Money.of(BigDecimal.valueOf(50000)),
                "EVALUATOR-001"
        );
    }

    @Test
    @DisplayName("deve gerar relatório para avaliação aprovada")
    void testGenerateReportForApprovedEvaluation() throws Exception {
        // Aprovar avaliação
        evaluation.approve("REVIEWER-001", Money.of(BigDecimal.valueOf(45000)));
        
        // Mock repository
        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(evaluation));
        
        // Mock report service
        byte[] mockPdf = "MOCK_PDF_CONTENT".getBytes();
        when(reportService.generateEvaluationReport(evaluation))
                .thenReturn(mockPdf);

        // Execute handler
        GenerateReportCommand command = new GenerateReportCommand(evaluationId);
        byte[] result = handler.handle(command);

        // Assertions
        assertNotNull(result, "Resultado não deve ser nulo");
        assertEquals(mockPdf, result, "PDF gerado deve ser o esperado");
        verify(evaluationRepository, times(1)).findById(any(EvaluationId.class));
        verify(reportService, times(1)).generateEvaluationReport(evaluation);
    }

    @Test
    @DisplayName("deve lançar exceção quando avaliação está em DRAFT")
    void testThrowExceptionForDraftEvaluation() {
        // Avaliação já começa em DRAFT
        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(evaluation));

        // Execute handler
        GenerateReportCommand command = new GenerateReportCommand(evaluationId);

        // Assertions
        assertThrows(IllegalStateException.class, () -> {
            handler.handle(command);
        }, "Deve lançar exceção para avaliação em DRAFT");
    }

    @Test
    @DisplayName("deve lançar exceção quando avaliação não encontrada")
    void testThrowExceptionWhenEvaluationNotFound() {
        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.empty());

        GenerateReportCommand command = new GenerateReportCommand(evaluationId);

        assertThrows(EvaluationNotFoundException.class, () -> {
            handler.handle(command);
        }, "Deve lançar exceção quando avaliação não encontrada");
    }

    @Test
    @DisplayName("deve validar dados mínimos antes de gerar")
    void testValidateMinimalData() throws Exception {
        // Aprovação sem calcular valores
        evaluation.approve("REVIEWER-001", null);
        
        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(evaluation));

        GenerateReportCommand command = new GenerateReportCommand(evaluationId);

        // Deve lançar exceção por falta de valores
        assertThrows(IllegalStateException.class, () -> {
            handler.handle(command);
        }, "Deve validar dados mínimos (FIPE price ou final value)");
    }

    @Test
    @DisplayName("deve gerar relatório com avaliação reprovada")
    void testGenerateReportForRejectedEvaluation() throws Exception {
        // Rejeitar avaliação
        evaluation.reject("REVIEWER-001", "Veículo com defeitos graves");
        
        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(evaluation));
        
        byte[] mockPdf = "MOCK_PDF_CONTENT".getBytes();
        when(reportService.generateEvaluationReport(evaluation))
                .thenReturn(mockPdf);

        GenerateReportCommand command = new GenerateReportCommand(evaluationId);
        byte[] result = handler.handle(command);

        assertNotNull(result, "Resultado não deve ser nulo mesmo para rejeição");
        verify(reportService, times(1)).generateEvaluationReport(evaluation);
    }

    @Test
    @DisplayName("deve aceitar avaliação sem fotos (aviso apenas)")
    void testGenerateReportWithoutPhotos() throws Exception {
        evaluation.approve("REVIEWER-001", Money.of(BigDecimal.valueOf(45000)));
        // Avaliação sem fotos adicionadas
        assertTrue(evaluation.getPhotos().isEmpty(), "Avaliação sem fotos");

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(evaluation));
        
        byte[] mockPdf = "MOCK_PDF_CONTENT".getBytes();
        when(reportService.generateEvaluationReport(evaluation))
                .thenReturn(mockPdf);

        GenerateReportCommand command = new GenerateReportCommand(evaluationId);
        byte[] result = handler.handle(command);

        assertNotNull(result, "Deve gerar PDF mesmo sem fotos");
        verify(reportService, times(1)).generateEvaluationReport(evaluation);
    }

    @Test
    @DisplayName("deve usar transação read-only (via mock)")
    void testUsesReadOnlyTransaction() throws Exception {
        evaluation.approve("REVIEWER-001", Money.of(BigDecimal.valueOf(45000)));
        
        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(evaluation));
        
        byte[] mockPdf = "MOCK_PDF_CONTENT".getBytes();
        when(reportService.generateEvaluationReport(evaluation))
                .thenReturn(mockPdf);

        GenerateReportCommand command = new GenerateReportCommand(evaluationId);
        handler.handle(command);

        // Verificar que apenas read foi feito (findById), não save
        verify(evaluationRepository, times(1)).findById(any(EvaluationId.class));
        verify(evaluationRepository, never()).save(any());
    }
}
