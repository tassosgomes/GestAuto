package com.gestauto.vehicleevaluation.infra.service;

import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadResult;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.core.sync.RequestBody;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.model.PutObjectRequest;
import software.amazon.awssdk.services.s3.model.DeleteObjectRequest;

import java.io.IOException;
import java.io.InputStream;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Service
public class ImageStorageServiceImpl implements ImageStorageService {

    private final S3Client s3Client;
    
    @Value("${cloudflare.r2.bucket-name}")
    private String bucketName;

    @Value("${cloudflare.r2.public-url}")
    private String publicUrl;

    public ImageStorageServiceImpl(S3Client s3Client) {
        this.s3Client = s3Client;
    }

    @Override
    public String uploadImage(InputStream imageStream, String fileName, String contentType, long size) {
        try {
            PutObjectRequest request = PutObjectRequest.builder()
                    .bucket(bucketName)
                    .key(fileName)
                    .contentType(contentType)
                    .build();

            s3Client.putObject(request, RequestBody.fromInputStream(imageStream, size));
            return publicUrl + "/" + fileName;
        } catch (Exception e) {
            throw new RuntimeException("Failed to upload image", e);
        }
    }

    @Override
    public boolean deleteImage(String imageUrl) {
        try {
            String fileName = imageUrl.replace(publicUrl + "/", "");
            DeleteObjectRequest request = DeleteObjectRequest.builder()
                    .bucket(bucketName)
                    .key(fileName)
                    .build();
            s3Client.deleteObject(request);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    @Override
    public ImageUploadResult uploadEvaluationPhotos(UUID evaluationId, Map<String, ImageUploadRequest> photos) {
        Map<String, String> uploadedUrls = new ConcurrentHashMap<>();
        Map<String, String> errors = new ConcurrentHashMap<>();

        photos.entrySet().parallelStream().forEach(entry -> {
            String photoType = entry.getKey();
            ImageUploadRequest request = entry.getValue();
            String fileName = "evaluations/" + evaluationId + "/" + photoType + ".jpg"; // Ideally use extension from request

            try {
                String url = uploadImage(request.content(), fileName, request.contentType(), request.size());
                uploadedUrls.put(photoType, url);
            } catch (Exception e) {
                errors.put(photoType, e.getMessage());
            }
        });

        return new ImageUploadResult(uploadedUrls, errors);
    }
}
