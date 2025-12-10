package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.CreateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.application.service.FipeService;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.Collections;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class CreateEvaluationHandlerTest {

    @Mock
    private VehicleEvaluationRepository repository;

    @Mock
    private FipeService fipeService;

    @Mock
    private DomainEventPublisherService eventPublisher;

    @InjectMocks
    private CreateEvaluationHandler handler;

    private CreateEvaluationCommand command;

    @BeforeEach
    void setUp() {
        command = new CreateEvaluationCommand(
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
    void shouldCreateEvaluationSuccessfully() throws Exception {
        // Arrange
        when(repository.existsByPlateAndStatus(any(Plate.class), any(EvaluationStatus.class)))
            .thenReturn(false);
        
        VehicleInfo vehicleInfo = VehicleInfo.of("Fiat", "Uno", "1.0 Flex", 2020, 2020, "Preto", FuelType.FLEX);
        when(fipeService.getVehicleInfoByPlate(anyString()))
            .thenReturn(Optional.of(vehicleInfo));

        when(repository.save(any(VehicleEvaluation.class)))
            .thenAnswer(invocation -> {
                VehicleEvaluation evaluation = invocation.getArgument(0);
                return evaluation;
            });

        // Act
        UUID result = handler.handle(command);

        // Assert
        assertNotNull(result);
        verify(repository).save(any(VehicleEvaluation.class));
        verify(eventPublisher).publishBatch(anyList());
    }
}
