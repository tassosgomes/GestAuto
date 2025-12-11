package com.gestauto.vehicleevaluation.domain.service;

import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadResult;
import java.io.InputStream;
import java.util.Map;
import java.util.UUID;

public interface ImageStorageService {
    String uploadImage(InputStream imageStream, String fileName, String contentType, long size);
    boolean deleteImage(String imageUrl);
    ImageUploadResult uploadEvaluationPhotos(UUID evaluationId, Map<String, ImageUploadRequest> photos);
}
