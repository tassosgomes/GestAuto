package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.NotificationService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;

import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.*;

/**
 * Testes unitários para RejectEvaluationHandler.
 */
@ExtendWith(MockitoExtension.class)
class RejectEvaluationHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @Mock
    private NotificationService notificationService;

    @Mock
    private ApplicationEventPublisher eventPublisher;

    @Mock
    private SecurityContext securityContext;

    @Mock
    private Authentication authentication;

    @InjectMocks
    private RejectEvaluationHandler handler;

    private VehicleEvaluation evaluation;
    private UUID evaluationId;
    private static final String VALID_REASON = "Vehicle condition is worse than reported";

    @BeforeEach
    void setUp() {
        evaluationId = UUID.randomUUID();
        
        // Configurar contexto de segurança
        SecurityContextHolder.setContext(securityContext);
        when(securityContext.getAuthentication()).thenReturn(authentication);
        when(authentication.isAuthenticated()).thenReturn(true);
        when(authentication.getName()).thenReturn("manager-user-id");

        // Criar avaliação mock
        evaluation = mock(VehicleEvaluation.class);
        when(evaluation.getId()).thenReturn(EvaluationId.from(evaluationId.toString()));
        when(evaluation.getStatus()).thenReturn(EvaluationStatus.PENDING_APPROVAL);
        when(evaluation.getEvaluatorId()).thenReturn("evaluator-id");
    }

    @Test
    void handle_WithValidCommand_ShouldRejectEvaluation() {
        // Arrange
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act
        handler.handle(command);

        // Assert
        verify(evaluation).reject(eq("manager-user-id"), eq(VALID_REASON));
        verify(evaluationRepository).save(evaluation);
        verify(notificationService).notifyEvaluator(anyString(), anyString(), anyString());
        verify(eventPublisher).publishEvent(any(EvaluationRejectedEvent.class));
    }

    @Test
    void handle_WithEvaluationNotFound_ShouldThrowException() {
        // Arrange
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.empty());

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(EvaluationNotFoundException.class);
        
        verify(evaluation, never()).reject(anyString(), anyString());
        verify(evaluationRepository, never()).save(any());
    }

    @Test
    void handle_WithInvalidStatus_ShouldThrowDomainException() {
        // Arrange
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        
        EvaluationStatus mockStatus = mock(EvaluationStatus.class);
        when(evaluation.getStatus()).thenReturn(mockStatus);
        when(mockStatus.canBeRejected()).thenReturn(false);

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(DomainException.class)
            .hasMessageContaining("not pending approval");
        
        verify(evaluation, never()).reject(anyString(), anyString());
    }

    @Test
    void handle_ShouldNotifyEvaluatorWithReason() {
        // Arrange
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act
        handler.handle(command);

        // Assert
        verify(notificationService).notifyEvaluator(
            eq("evaluator-id"),
            eq("Evaluation rejected"),
            contains(VALID_REASON)
        );
    }

    @Test
    void handle_ShouldPublishEvaluationRejectedEvent() {
        // Arrange
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act
        handler.handle(command);

        // Assert
        verify(eventPublisher).publishEvent(argThat(event -> 
            event instanceof EvaluationRejectedEvent &&
            ((EvaluationRejectedEvent) event).getReason().equals(VALID_REASON)
        ));
    }

    @Test
    void handle_WithUnauthenticatedUser_ShouldThrowSecurityException() {
        // Arrange
        when(authentication.isAuthenticated()).thenReturn(false);
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(SecurityException.class)
            .hasMessageContaining("not authenticated");
    }

    @Test
    void handle_WithNullAuthentication_ShouldThrowSecurityException() {
        // Arrange
        when(securityContext.getAuthentication()).thenReturn(null);
        RejectEvaluationCommand command = new RejectEvaluationCommand(evaluationId, VALID_REASON);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));

        // Act & Assert
        assertThatThrownBy(() -> handler.handle(command))
            .isInstanceOf(SecurityException.class)
            .hasMessageContaining("not authenticated");
    }

    @Test
    void constructor_WithNullEvaluationId_ShouldThrowException() {
        // Act & Assert
        assertThatThrownBy(() -> new RejectEvaluationCommand(null, VALID_REASON))
            .isInstanceOf(IllegalArgumentException.class)
            .hasMessageContaining("EvaluationId cannot be null");
    }

    @Test
    void constructor_WithNullReason_ShouldThrowException() {
        // Act & Assert
        assertThatThrownBy(() -> new RejectEvaluationCommand(evaluationId, null))
            .isInstanceOf(IllegalArgumentException.class)
            .hasMessageContaining("reason cannot be null");
    }

    @Test
    void constructor_WithEmptyReason_ShouldThrowException() {
        // Act & Assert
        assertThatThrownBy(() -> new RejectEvaluationCommand(evaluationId, ""))
            .isInstanceOf(IllegalArgumentException.class)
            .hasMessageContaining("reason cannot be null or empty");
    }

    @Test
    void constructor_WithReasonTooLong_ShouldThrowException() {
        // Arrange
        String longReason = "a".repeat(501);

        // Act & Assert
        assertThatThrownBy(() -> new RejectEvaluationCommand(evaluationId, longReason))
            .isInstanceOf(IllegalArgumentException.class)
            .hasMessageContaining("cannot exceed 500 characters");
    }
}
