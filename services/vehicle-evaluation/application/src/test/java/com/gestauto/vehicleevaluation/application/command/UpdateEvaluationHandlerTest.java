package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.UpdateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.application.service.FipeService;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.Collections;
import java.util.Optional;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class UpdateEvaluationHandlerTest {

    @Mock
    private VehicleEvaluationRepository repository;

    @Mock
    private FipeService fipeService;

    @Mock
    private DomainEventPublisherService eventPublisher;

    @InjectMocks
    private UpdateEvaluationHandler handler;

    private UpdateEvaluationCommand command;
    private UUID evaluationId;

    @BeforeEach
    void setUp() {
        evaluationId = UUID.randomUUID();
        command = new UpdateEvaluationCommand(
            evaluationId,
            "ABC1234",
            2020,
            50000,
            "Preto",
            "1.0 Flex",
            "FLEX",
            "MANUAL",
            Collections.emptyList(),
            "Observações"
        );
    }

    @Test
    void shouldUpdateEvaluationSuccessfully() throws Exception {
        // Arrange
        VehicleEvaluation evaluation = mock(VehicleEvaluation.class);
        when(repository.findById(any(EvaluationId.class))).thenReturn(Optional.of(evaluation));
        when(evaluation.getStatus()).thenReturn(EvaluationStatus.DRAFT);
        when(evaluation.getObservations()).thenReturn("Old notes");
        when(evaluation.getId()).thenReturn(EvaluationId.from(evaluationId.toString()));
        when(repository.update(any(VehicleEvaluation.class))).thenReturn(evaluation);
        when(evaluation.getDomainEvents()).thenReturn(Collections.emptyList());

        // Act
        handler.handle(command);

        // Assert
        verify(evaluation).setObservations("Observações");
        verify(repository).update(evaluation);
    }
}
