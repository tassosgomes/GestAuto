package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.NotificationService;
import com.gestauto.vehicleevaluation.domain.service.ReportService;
import com.gestauto.vehicleevaluation.domain.value.*;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.mockito.junit.jupiter.MockitoSettings;
import org.mockito.quality.Strictness;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.Collections;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.*;

/**
 * Testes unitários para ApproveEvaluationHandler.
 */
@ExtendWith(MockitoExtension.class)
@MockitoSettings(strictness = Strictness.LENIENT)
class ApproveEvaluationHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @Mock
    private ReportService reportService;

    @Mock
    private NotificationService notificationService;

    @Mock
    private ApplicationEventPublisher eventPublisher;

    @Mock
    private SecurityContext securityContext;

    @Mock
    private Authentication authentication;

    @InjectMocks
    private ApproveEvaluationHandler handler;

    private VehicleEvaluation evaluation;
    private UUID evaluationId;

    @BeforeEach
    void setUp() {
        evaluationId = UUID.randomUUID();
        
        // Configurar contexto de segurança
        SecurityContextHolder.setContext(securityContext);
        when(securityContext.getAuthentication()).thenReturn(authentication);
        when(authentication.isAuthenticated()).thenReturn(true);
        when(authentication.getName()).thenReturn("manager-user-id");
        doReturn(java.util.List.of(new SimpleGrantedAuthority("ROLE_MANAGER")))
            .when(authentication)
            .getAuthorities();

        // Criar avaliação mock
        evaluation = mock(VehicleEvaluation.class);
        when(evaluation.getId()).thenReturn(EvaluationId.from(evaluationId.toString()));
        when(evaluation.getStatus()).thenReturn(EvaluationStatus.PENDING_APPROVAL);
        when(evaluation.getFinalValue()).thenReturn(Money.of(new BigDecimal("50000.00")));
        when(evaluation.getEvaluatorId()).thenReturn("evaluator-id");
        when(evaluation.getApprovedValue()).thenReturn(Money.of(new BigDecimal("50000.00")));
        when(evaluation.getApprovedAt()).thenReturn(LocalDateTime.now());
    }

    @Test
    void handle_WithValidCommand_ShouldApproveEvaluation() {
        // Arrange
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(any())).thenReturn(new byte[0]);
        
        // Act
        handler.handle(command);

        // Assert
        verify(evaluation).approve(eq("manager-user-id"), isNull());
        verify(evaluationRepository).save(evaluation);
        verify(reportService).generateEvaluationReport(evaluation);
        verify(notificationService).notifyEvaluator(anyString(), anyString(), anyString());
        verify(eventPublisher).publishEvent(any(EvaluationApprovedEvent.class));
    }

    @Test
    void handle_WithEvaluationNotFound_ShouldThrowException() {
        // Arrange
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.empty());

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(EvaluationNotFoundException.class);
        
        verify(evaluation, never()).approve(anyString(), any());
        verify(evaluationRepository, never()).save(any());
    }

    @Test
    void handle_WithInvalidStatus_ShouldThrowDomainException() {
        // Arrange
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        
        EvaluationStatus mockStatus = mock(EvaluationStatus.class);
        when(evaluation.getStatus()).thenReturn(mockStatus);
        when(mockStatus.canBeApproved()).thenReturn(false);

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(DomainException.class)
            .hasMessageContaining("not pending approval");
        
        verify(evaluation, never()).approve(anyString(), any());
    }

    @Test
    void handle_WithAdjustedValue_ShouldApplyAdjustment() {
        // Arrange
        BigDecimal adjustedValue = new BigDecimal("52000.00");
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, adjustedValue);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(any())).thenReturn(new byte[0]);

        // Act
        handler.handle(command);

        // Assert
        verify(evaluation).approve(eq("manager-user-id"), argThat(money -> 
            money != null && money.getAmount().compareTo(adjustedValue) == 0
        ));
    }

    @Test
    void handle_WithAdjustmentUnder10Percent_ShouldSucceed() {
        // Arrange
        BigDecimal adjustedValue = new BigDecimal("54000.00"); // 8% increase
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, adjustedValue);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(any())).thenReturn(new byte[0]);

        // Act & Assert
        assertThatNoException().isThrownBy(() -> handler.handle(command));
        
        verify(evaluation).approve(anyString(), any());
    }

    @Test
    void handle_WithAdjustmentOver10PercentAndNotAdmin_ShouldThrowException() {
        // Arrange
        BigDecimal adjustedValue = new BigDecimal("60000.00"); // 20% increase
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, adjustedValue);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(DomainException.class)
            .hasMessageContaining("requires admin approval");
        
        verify(evaluation, never()).approve(anyString(), any());
    }

    @Test
    void handle_WithAdjustmentOver10PercentAndAdmin_ShouldSucceed() {
        // Arrange
        doReturn(java.util.List.of(new SimpleGrantedAuthority("ROLE_ADMIN")))
            .when(authentication)
            .getAuthorities();
        
        BigDecimal adjustedValue = new BigDecimal("60000.00"); // 20% increase
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, adjustedValue);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(any())).thenReturn(new byte[0]);

        // Act & Assert
        assertThatNoException().isThrownBy(() -> handler.handle(command));
        
        verify(evaluation).approve(anyString(), any());
    }

    @Test
    void handle_ShouldGeneratePdfReport() {
        // Arrange
        byte[] expectedReport = "PDF content".getBytes();
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(evaluation)).thenReturn(expectedReport);

        // Act
        handler.handle(command);

        // Assert
        verify(reportService).generateEvaluationReport(evaluation);
    }

    @Test
    void handle_ShouldNotifyEvaluator() {
        // Arrange
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(any())).thenReturn(new byte[0]);

        // Act
        handler.handle(command);

        // Assert
        verify(notificationService).notifyEvaluator(
            eq("evaluator-id"),
            eq("Evaluation approved"),
            eq("Your evaluation has been approved")
        );
    }

    @Test
    void handle_ShouldPublishEvaluationApprovedEvent() {
        // Arrange
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(reportService.generateEvaluationReport(any())).thenReturn(new byte[0]);

        // Act
        handler.handle(command);

        // Assert
        verify(eventPublisher).publishEvent(any(EvaluationApprovedEvent.class));
    }

    @Test
    void handle_WithUnauthenticatedUser_ShouldThrowSecurityException() {
        // Arrange
        when(authentication.isAuthenticated()).thenReturn(false);
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(evaluationId, null);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(SecurityException.class)
            .hasMessageContaining("not authenticated");
    }
}
