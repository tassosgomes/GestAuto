package com.gestauto.vehicleevaluation.infra.service;

import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadResult;
import com.gestauto.vehicleevaluation.domain.value.UploadedPhoto;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.core.sync.RequestBody;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.model.PutObjectRequest;
import software.amazon.awssdk.services.s3.model.DeleteObjectRequest;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Service
public class ImageStorageServiceImpl implements ImageStorageService {

    private final S3Client s3Client;
    
    @Value("${app.external-apis.cloudflare-r2.bucket-name}")
    private String bucketName;

    @Value("${app.external-apis.cloudflare-r2.public-url:}")
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
        Map<String, UploadedPhoto> uploadedPhotos = new ConcurrentHashMap<>();
        Map<String, String> errors = new ConcurrentHashMap<>();

        photos.entrySet().parallelStream().forEach(entry -> {
            String photoType = entry.getKey();
            ImageUploadRequest request = entry.getValue();
            
            try {
                if (!request.contentType().equals("image/jpeg") && !request.contentType().equals("image/png")) {
                     throw new IllegalArgumentException("Invalid content type. Only JPEG and PNG are allowed.");
                }

                // Read image to check resolution and generate thumbnail
                byte[] imageBytes = request.content().readAllBytes();
                ByteArrayInputStream bais = new ByteArrayInputStream(imageBytes);
                BufferedImage originalImage = ImageIO.read(bais);
                
                if (originalImage == null) {
                    throw new IllegalArgumentException("Invalid image format");
                }
                
                if (originalImage.getWidth() < 800 || originalImage.getHeight() < 600) {
                     throw new IllegalArgumentException("Image resolution too low. Minimum 800x600 required.");
                }

                // Generate thumbnail
                BufferedImage thumbnail = resizeImage(originalImage, 200, 200);
                ByteArrayOutputStream thumbOs = new ByteArrayOutputStream();
                ImageIO.write(thumbnail, "jpg", thumbOs);
                InputStream thumbStream = new ByteArrayInputStream(thumbOs.toByteArray());
                
                // Upload original
                String fileName = "evaluations/" + evaluationId + "/" + photoType + ".jpg";
                String originalUrl = uploadImage(new ByteArrayInputStream(imageBytes), fileName, "image/jpeg", imageBytes.length);
                
                // Upload thumbnail
                String thumbFileName = "evaluations/" + evaluationId + "/" + photoType + "_thumb.jpg";
                String thumbUrl = uploadImage(thumbStream, thumbFileName, "image/jpeg", thumbOs.size());
                
                uploadedPhotos.put(photoType, new UploadedPhoto(originalUrl, thumbUrl));
                
            } catch (Exception e) {
                errors.put(photoType, e.getMessage());
            }
        });

        return new ImageUploadResult(uploadedPhotos, errors);
    }

    private BufferedImage resizeImage(BufferedImage originalImage, int targetWidth, int targetHeight) {
        java.awt.Image resultingImage = originalImage.getScaledInstance(targetWidth, targetHeight, java.awt.Image.SCALE_SMOOTH);
        BufferedImage outputImage = new BufferedImage(targetWidth, targetHeight, BufferedImage.TYPE_INT_RGB);
        outputImage.getGraphics().drawImage(resultingImage, 0, 0, null);
        return outputImage;
    }
}
