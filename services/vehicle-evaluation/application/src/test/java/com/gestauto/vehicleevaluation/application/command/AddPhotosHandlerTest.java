package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationPhotoRepository;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadResult;
import com.gestauto.vehicleevaluation.domain.value.UploadedPhoto;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.io.ByteArrayInputStream;
import java.util.Collections;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class AddPhotosHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @Mock
    private ImageStorageService imageStorageService;

    @Mock
    private EvaluationPhotoRepository photoRepository;

    @InjectMocks
    private AddPhotosHandler handler;

    private UUID evaluationId;
    private AddPhotosCommand command;

    @BeforeEach
    void setUp() {
        evaluationId = UUID.randomUUID();
        ImageUploadRequest request = new ImageUploadRequest(
                new ByteArrayInputStream(new byte[0]),
                "test.jpg",
                "image/jpeg",
                1024
        );
        command = new AddPhotosCommand(evaluationId, Map.of(PhotoType.EXTERIOR_FRONT.name(), request));
    }

    @Test
    void shouldAddPhotosSuccessfully() {
        VehicleEvaluation mockEvaluation = mock(VehicleEvaluation.class);
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.of(mockEvaluation));
        
        UploadedPhoto photo = mock(UploadedPhoto.class);
        ImageUploadResult result = new ImageUploadResult(
                Map.of(PhotoType.EXTERIOR_FRONT.name(), photo),
                Collections.emptyMap()
        );
        when(imageStorageService.uploadEvaluationPhotos(any(), any())).thenReturn(result);

        assertDoesNotThrow(() -> handler.handle(command));

        verify(photoRepository, times(1)).save(any());
    }

    @Test
    void shouldThrowExceptionWhenEvaluationNotFound() {
        when(evaluationRepository.findById(any(EvaluationId.class))).thenReturn(Optional.empty());

        assertThrows(RuntimeException.class, () -> handler.handle(command));
    }
}
