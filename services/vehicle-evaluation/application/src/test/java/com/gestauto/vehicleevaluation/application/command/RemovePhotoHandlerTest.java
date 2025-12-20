package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationPhotoRepository;
import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.Test;

import java.util.List;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.*;

class RemovePhotoHandlerTest {

    @Test
    void handle_deletesOriginalAndThumbnail_whenPresent() {
        EvaluationPhotoRepository repo = mock(EvaluationPhotoRepository.class);
        ImageStorageService storage = mock(ImageStorageService.class);
        RemovePhotoHandler handler = new RemovePhotoHandler(repo, storage);

        UUID evaluationUuid = UUID.randomUUID();
        EvaluationId evaluationId = EvaluationId.from(evaluationUuid);

        EvaluationPhoto photo = EvaluationPhoto.create(
                evaluationId,
            PhotoType.EXTERIOR_FRONT,
                "front.jpg",
                "/path/front.jpg",
                1234,
                "image/jpeg",
                "https://cdn.example/front.jpg",
                "https://cdn.example/front_thumb.jpg"
        );

        when(repo.findByEvaluationId(evaluationId)).thenReturn(List.of(photo));

        handler.handle(new RemovePhotoCommand(evaluationUuid, "EXTERIOR_FRONT"));

        verify(storage).deleteImage("https://cdn.example/front.jpg");
        verify(storage).deleteImage("https://cdn.example/front_thumb.jpg");
        verify(repo).deleteById(photo.getPhotoId());
    }

    @Test
    void handle_deletesOnlyOriginal_whenThumbnailIsNull() {
        EvaluationPhotoRepository repo = mock(EvaluationPhotoRepository.class);
        ImageStorageService storage = mock(ImageStorageService.class);
        RemovePhotoHandler handler = new RemovePhotoHandler(repo, storage);

        UUID evaluationUuid = UUID.randomUUID();
        EvaluationId evaluationId = EvaluationId.from(evaluationUuid);

        EvaluationPhoto photo = EvaluationPhoto.createWithoutThumbnail(
                evaluationId,
            PhotoType.EXTERIOR_REAR,
                "rear.png",
                "/path/rear.png",
                2222,
                "image/png",
                "https://cdn.example/rear.png"
        );

        when(repo.findByEvaluationId(evaluationId)).thenReturn(List.of(photo));

        handler.handle(new RemovePhotoCommand(evaluationUuid, "EXTERIOR_REAR"));

        verify(storage).deleteImage("https://cdn.example/rear.png");
        verify(repo).deleteById(photo.getPhotoId());
        verify(storage, times(1)).deleteImage(anyString());
    }

    @Test
    void handle_whenPhotoNotFound_throws() {
        EvaluationPhotoRepository repo = mock(EvaluationPhotoRepository.class);
        ImageStorageService storage = mock(ImageStorageService.class);
        RemovePhotoHandler handler = new RemovePhotoHandler(repo, storage);

        UUID evaluationUuid = UUID.randomUUID();
        EvaluationId evaluationId = EvaluationId.from(evaluationUuid);

        when(repo.findByEvaluationId(evaluationId)).thenReturn(List.of());

        assertThatThrownBy(() -> handler.handle(new RemovePhotoCommand(evaluationUuid, "EXTERIOR_FRONT")))
                .isInstanceOf(RuntimeException.class)
                .hasMessageContaining("Photo not found");
    }
}
