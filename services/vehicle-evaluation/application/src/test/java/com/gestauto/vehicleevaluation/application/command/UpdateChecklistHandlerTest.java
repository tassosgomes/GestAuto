package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.*;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.exception.InvalidEvaluationStatusException;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationChecklistRepository;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Captor;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
@DisplayName("UpdateChecklistHandler Tests")
class UpdateChecklistHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @Mock
    private EvaluationChecklistRepository checklistRepository;

    @Mock
    private DomainEventPublisherService eventPublisher;

    @InjectMocks
    private UpdateChecklistHandler handler;

    @Captor
    private ArgumentCaptor<EvaluationChecklist> checklistCaptor;

    private UUID evaluationId;
    private VehicleEvaluation mockEvaluation;
    private UpdateChecklistCommand command;

    @BeforeEach
    void setUp() {
        evaluationId = UUID.randomUUID();
        
        // Create mock evaluation in DRAFT status
        mockEvaluation = mock(VehicleEvaluation.class);
        when(mockEvaluation.getStatus()).thenReturn(EvaluationStatus.DRAFT);
    }

    @Test
    @DisplayName("Should update checklist successfully for DRAFT evaluation")
    void shouldUpdateChecklistSuccessfullyForDraftEvaluation() throws Exception {
        // Arrange
        BodyworkDto bodywork = new BodyworkDto(
                "GOOD", "GOOD", false, false,
                false, false, false, 0,
                0, 0, 0, false, "No issues"
        );
        
        DocumentsDto documents = new DocumentsDto(
                true, true, true, true, "All present"
        );

        command = new UpdateChecklistCommand(
                evaluationId, bodywork, null, null, null, documents
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));
        when(checklistRepository.findByEvaluationId(any(EvaluationId.class)))
                .thenReturn(Optional.empty());
        when(checklistRepository.save(any(EvaluationChecklist.class)))
                .thenAnswer(invocation -> invocation.getArgument(0));
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
                .thenReturn(mockEvaluation);

        // Act
        handler.handle(command);

        // Assert
        verify(evaluationRepository).findById(any(EvaluationId.class));
        verify(checklistRepository).save(checklistCaptor.capture());
        verify(evaluationRepository).save(mockEvaluation);
        verify(eventPublisher).publishEvent(any());

        EvaluationChecklist savedChecklist = checklistCaptor.getValue();
        assertEquals("GOOD", savedChecklist.getBodyCondition());
        assertTrue(savedChecklist.isCrvlPresent());
    }

    @Test
    @DisplayName("Should throw exception when evaluation not found")
    void shouldThrowExceptionWhenEvaluationNotFound() {
        // Arrange
        command = new UpdateChecklistCommand(
                evaluationId, null, null, null, null,
                new DocumentsDto(true, false, false, false, null)
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.empty());

        // Act & Assert
        assertThrows(EvaluationNotFoundException.class,
                    () -> handler.handle(command));
        
        verify(checklistRepository, never()).save(any());
        verify(eventPublisher, never()).publishEvent(any());
    }

    @Test
    @DisplayName("Should reject update if evaluation is APPROVED")
    void shouldRejectUpdateIfEvaluationIsApproved() {
        // Arrange
        when(mockEvaluation.getStatus()).thenReturn(EvaluationStatus.APPROVED);
        
        command = new UpdateChecklistCommand(
                evaluationId, null, null, null, null,
                new DocumentsDto(true, false, false, false, null)
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));

        // Act & Assert
        assertThrows(InvalidEvaluationStatusException.class,
                    () -> handler.handle(command));
        
        verify(checklistRepository, never()).save(any());
        verify(eventPublisher, never()).publishEvent(any());
    }

    @Test
    @DisplayName("Should allow update for IN_PROGRESS evaluation")
    void shouldAllowUpdateForInProgressEvaluation() throws Exception {
        // Arrange
        when(mockEvaluation.getStatus()).thenReturn(EvaluationStatus.IN_PROGRESS);
        
        command = new UpdateChecklistCommand(
                evaluationId, null, null, null, null,
                new DocumentsDto(true, false, false, false, null)
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));
        when(checklistRepository.findByEvaluationId(any(EvaluationId.class)))
                .thenReturn(Optional.empty());
        when(checklistRepository.save(any(EvaluationChecklist.class)))
                .thenAnswer(invocation -> invocation.getArgument(0));
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
                .thenReturn(mockEvaluation);

        // Act
        assertDoesNotThrow(() -> handler.handle(command));

        // Assert
        verify(checklistRepository).save(any(EvaluationChecklist.class));
        verify(eventPublisher).publishEvent(any());
    }

    @Test
    @DisplayName("Should update existing checklist")
    void shouldUpdateExistingChecklist() throws Exception {
        // Arrange
        EvaluationChecklist existingChecklist = EvaluationChecklist.create(
                new EvaluationId(evaluationId)
        );
        existingChecklist.setBodyCondition("FAIR");

        command = new UpdateChecklistCommand(
                evaluationId,
                new BodyworkDto("GOOD", "GOOD", false, false, false,
                               false, false, 0, 0, 0, 0, false, null),
                null, null, null,
                new DocumentsDto(true, false, false, false, null)
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));
        when(checklistRepository.findByEvaluationId(any(EvaluationId.class)))
                .thenReturn(Optional.of(existingChecklist));
        when(checklistRepository.save(any(EvaluationChecklist.class)))
                .thenAnswer(invocation -> invocation.getArgument(0));
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
                .thenReturn(mockEvaluation);

        // Act
        handler.handle(command);

        // Assert
        verify(checklistRepository).save(checklistCaptor.capture());
        EvaluationChecklist updatedChecklist = checklistCaptor.getValue();
        assertEquals("GOOD", updatedChecklist.getBodyCondition());
    }

    @Test
    @DisplayName("Should calculate conservation score automatically")
    void shouldCalculateConservationScoreAutomatically() throws Exception {
        // Arrange
        command = new UpdateChecklistCommand(
                evaluationId,
                new BodyworkDto("EXCELLENT", "EXCELLENT", false, false, false,
                               false, false, 0, 0, 0, 0, false, null),
                new MechanicalDto("EXCELLENT", "EXCELLENT", "EXCELLENT", "EXCELLENT",
                                 false, false, true, "EXCELLENT", null),
                new TiresDto("EXCELLENT", false, false, null),
                new InteriorDto("EXCELLENT", "EXCELLENT", "EXCELLENT",
                               false, false, false, null),
                new DocumentsDto(true, true, true, true, null)
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));
        when(checklistRepository.findByEvaluationId(any(EvaluationId.class)))
                .thenReturn(Optional.empty());
        when(checklistRepository.save(any(EvaluationChecklist.class)))
                .thenAnswer(invocation -> invocation.getArgument(0));
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
                .thenReturn(mockEvaluation);

        // Act
        handler.handle(command);

        // Assert
        verify(checklistRepository).save(checklistCaptor.capture());
        EvaluationChecklist savedChecklist = checklistCaptor.getValue();
        assertNotNull(savedChecklist.getConservationScore());
        assertEquals(100, savedChecklist.getConservationScore());
    }

    @Test
    @DisplayName("Should publish ChecklistCompletedEvent")
    void shouldPublishChecklistCompletedEvent() throws Exception {
        // Arrange
        command = new UpdateChecklistCommand(
                evaluationId, null, null, null, null,
                new DocumentsDto(true, false, false, false, null)
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));
        when(checklistRepository.findByEvaluationId(any(EvaluationId.class)))
                .thenReturn(Optional.empty());
        when(checklistRepository.save(any(EvaluationChecklist.class)))
                .thenAnswer(invocation -> invocation.getArgument(0));
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
                .thenReturn(mockEvaluation);

        // Act
        handler.handle(command);

        // Assert
        verify(eventPublisher, times(1)).publishEvent(any());
    }

    @Test
    @DisplayName("Should map all DTO sections to checklist")
    void shouldMapAllDtoSectionsToChecklist() throws Exception {
        // Arrange
        command = new UpdateChecklistCommand(
                evaluationId,
                new BodyworkDto("FAIR", "GOOD", true, true, false,
                               true, false, 2, 1, 0, 1, false, "Minor rust"),
                new MechanicalDto("GOOD", "FAIR", "GOOD", "EXCELLENT",
                                 true, false, true, "GOOD", "Oil leak minor"),
                new TiresDto("FAIR", true, false, "Uneven wear detected"),
                new InteriorDto("GOOD", "FAIR", "GOOD",
                               true, false, true, "Minor seat damage"),
                new DocumentsDto(true, true, false, true, "Missing spare key")
        );

        when(evaluationRepository.findById(any(EvaluationId.class)))
                .thenReturn(Optional.of(mockEvaluation));
        when(checklistRepository.findByEvaluationId(any(EvaluationId.class)))
                .thenReturn(Optional.empty());
        when(checklistRepository.save(any(EvaluationChecklist.class)))
                .thenAnswer(invocation -> invocation.getArgument(0));
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
                .thenReturn(mockEvaluation);

        // Act
        handler.handle(command);

        // Assert
        verify(checklistRepository).save(checklistCaptor.capture());
        EvaluationChecklist savedChecklist = checklistCaptor.getValue();
        
        // Verify all sections mapped
        assertEquals("FAIR", savedChecklist.getBodyCondition());
        assertTrue(savedChecklist.isRustPresence());
        assertEquals(2, savedChecklist.getDoorRepairs());
        assertEquals("GOOD", savedChecklist.getEngineCondition());
        assertTrue(savedChecklist.isOilLeaks());
        assertEquals("FAIR", savedChecklist.getTiresCondition());
        assertTrue(savedChecklist.isUnevenWear());
        assertEquals("GOOD", savedChecklist.getSeatsCondition());
        assertTrue(savedChecklist.isSeatDamage());
        assertTrue(savedChecklist.isCrvlPresent());
        assertFalse(savedChecklist.isSpareKeyPresent());
    }
}
