package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationPhotoRepository;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadResult;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.util.Map;

@Component
public class AddPhotosHandler implements CommandHandler<AddPhotosCommand, Void> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final ImageStorageService imageStorageService;
    private final EvaluationPhotoRepository photoRepository;

    public AddPhotosHandler(VehicleEvaluationRepository evaluationRepository,
                            ImageStorageService imageStorageService,
                            EvaluationPhotoRepository photoRepository) {
        this.evaluationRepository = evaluationRepository;
        this.imageStorageService = imageStorageService;
        this.photoRepository = photoRepository;
    }

    @Override
    @Transactional
    public Void handle(AddPhotosCommand command) {
        EvaluationId evaluationId = EvaluationId.from(command.evaluationId());
        VehicleEvaluation evaluation = evaluationRepository.findById(evaluationId)
                .orElseThrow(() -> new RuntimeException("Evaluation not found"));

        // Validate photo types
        for (String typeStr : command.photos().keySet()) {
            try {
                PhotoType.valueOf(typeStr);
            } catch (IllegalArgumentException e) {
                throw new RuntimeException("Invalid photo type: " + typeStr);
            }
        }

        // Upload photos
        ImageUploadResult result = imageStorageService.uploadEvaluationPhotos(command.evaluationId(), command.photos());

        if (!result.errors().isEmpty()) {
            throw new RuntimeException("Error uploading photos: " + result.errors());
        }

        // Save metadata
        for (Map.Entry<String, String> entry : result.uploadedUrls().entrySet()) {
            PhotoType type = PhotoType.valueOf(entry.getKey());
            String url = entry.getValue();
            ImageUploadRequest request = command.photos().get(entry.getKey());
            
            // Reconstruct key/path
            String filePath = "evaluations/" + command.evaluationId() + "/" + type.name() + ".jpg";

            EvaluationPhoto photo = EvaluationPhoto.createWithoutThumbnail(
                    evaluationId,
                    type,
                    request.fileName(),
                    filePath,
                    request.size(),
                    request.contentType(),
                    url
            );
            photoRepository.save(photo);
        }

        return null;
    }
}
