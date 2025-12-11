package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationPhotoRepository;
import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Component
public class RemovePhotoHandler implements CommandHandler<RemovePhotoCommand, Void> {

    private final EvaluationPhotoRepository photoRepository;
    private final ImageStorageService imageStorageService;

    public RemovePhotoHandler(EvaluationPhotoRepository photoRepository, ImageStorageService imageStorageService) {
        this.photoRepository = photoRepository;
        this.imageStorageService = imageStorageService;
    }

    @Override
    @Transactional
    public Void handle(RemovePhotoCommand command) {
        EvaluationId evaluationId = EvaluationId.from(command.evaluationId());
        PhotoType type = PhotoType.valueOf(command.photoType());

        List<EvaluationPhoto> photos = photoRepository.findByEvaluationId(evaluationId);
        EvaluationPhoto photoToRemove = photos.stream()
                .filter(p -> p.getPhotoType() == type)
                .findFirst()
                .orElseThrow(() -> new RuntimeException("Photo not found"));

        // Delete from storage
        imageStorageService.deleteImage(photoToRemove.getUploadUrl());

        // Delete from DB
        photoRepository.deleteById(photoToRemove.getPhotoId());

        return null;
    }
}
