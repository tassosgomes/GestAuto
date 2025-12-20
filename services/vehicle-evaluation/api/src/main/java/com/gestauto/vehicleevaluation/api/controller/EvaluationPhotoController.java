package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.AddPhotosCommand;
import com.gestauto.vehicleevaluation.application.command.CommandHandler;
import com.gestauto.vehicleevaluation.application.command.RemovePhotoCommand;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/evaluations/{id}/photos")
public class EvaluationPhotoController {

    private final CommandHandler<AddPhotosCommand, Void> addPhotosHandler;
    private final CommandHandler<RemovePhotoCommand, Void> removePhotoHandler;

    public EvaluationPhotoController(CommandHandler<AddPhotosCommand, Void> addPhotosHandler,
                                     CommandHandler<RemovePhotoCommand, Void> removePhotoHandler) {
        this.addPhotosHandler = addPhotosHandler;
        this.removePhotoHandler = removePhotoHandler;
    }

    @PostMapping
    public ResponseEntity<Void> uploadPhotos(@PathVariable("id") UUID id, @RequestParam Map<String, MultipartFile> files) throws Exception {
        Map<String, ImageUploadRequest> photos = new HashMap<>();

        for (Map.Entry<String, MultipartFile> entry : files.entrySet()) {
            MultipartFile file = entry.getValue();
            try {
                photos.put(entry.getKey(), new ImageUploadRequest(
                        file.getInputStream(),
                        file.getOriginalFilename(),
                        file.getContentType(),
                        file.getSize()
                ));
            } catch (IOException e) {
                throw new RuntimeException("Error reading file", e);
            }
        }

        AddPhotosCommand command = new AddPhotosCommand(id, photos);
        addPhotosHandler.handle(command);

        return ResponseEntity.ok().build();
    }

    @DeleteMapping("/{type}")
    public ResponseEntity<Void> removePhoto(@PathVariable("id") UUID id, @PathVariable("type") String type) throws Exception {
        RemovePhotoCommand command = new RemovePhotoCommand(id, type);
        removePhotoHandler.handle(command);
        return ResponseEntity.noContent().build();
    }
}
