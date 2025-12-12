package com.gestauto.vehicleevaluation.infra.service.storage;

import io.micrometer.core.instrument.MeterRegistry;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.core.sync.RequestBody;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.model.DeleteObjectRequest;
import software.amazon.awssdk.services.s3.model.GetObjectRequest;
import software.amazon.awssdk.services.s3.model.PutObjectRequest;
import software.amazon.awssdk.services.s3.model.S3Exception;
import software.amazon.awssdk.services.s3.presigner.S3Presigner;
import software.amazon.awssdk.services.s3.presigner.model.GetObjectPresignRequest;
import software.amazon.awssdk.services.s3.presigner.model.PresignedGetObjectRequest;

import java.util.Map;

import java.io.IOException;
import java.io.InputStream;
import java.time.Duration;
import java.util.UUID;

/**
 * Serviço para upload e gerenciamento de imagens em Cloudflare R2
 */
@Service
@Slf4j
public class ImageStorageService {

    private final S3Client s3Client;
    private final S3Presigner s3Presigner;
    private final MeterRegistry meterRegistry;

    @Value("${app.external-apis.cloudflare-r2.bucket-name:vehicle-evaluation-photos}")
    private String bucketName;

    @Value("${app.external-apis.cloudflare-r2.endpoint}")
    private String endpoint;

    public ImageStorageService(S3Client s3Client, S3Presigner s3Presigner, MeterRegistry meterRegistry) {
        this.s3Client = s3Client;
        this.s3Presigner = s3Presigner;
        this.meterRegistry = meterRegistry;
    }

    /**
     * Upload de uma imagem para R2
     *
     * @param imageStream Fluxo de dados da imagem
     * @param fileName Nome do arquivo
     * @param contentType Tipo MIME do arquivo
     * @return URL da imagem armazenada
     */
    public String uploadImage(InputStream imageStream, String fileName, String contentType) {
        long startTime = System.currentTimeMillis();

        try {
            byte[] imageBytes = imageStream.readAllBytes();

            String key = generateS3Key(fileName);

            PutObjectRequest putObjectRequest = PutObjectRequest.builder()
                    .bucket(bucketName)
                    .key(key)
                    .contentType(contentType)
                    .contentLength((long) imageBytes.length)
                    .metadata(Map.of("uploaded-at", String.valueOf(System.currentTimeMillis())))
                    .build();

            s3Client.putObject(putObjectRequest, RequestBody.fromBytes(imageBytes));

            String imageUrl = generateImageUrl(key);
            long duration = System.currentTimeMillis() - startTime;

            meterRegistry.timer("image_storage.upload.duration",
                    "status", "success").record(duration, java.util.concurrent.TimeUnit.MILLISECONDS);
            meterRegistry.counter("image_storage.uploads", "status", "success").increment();

            log.info("Image uploaded successfully: {} ({}ms)", key, duration);
            return imageUrl;

        } catch (IOException e) {
            meterRegistry.counter("image_storage.uploads", "status", "error").increment();
            log.error("Failed to read image stream", e);
            throw new ImageStorageException("Failed to read image stream", "IO_ERROR", e);
        } catch (S3Exception e) {
            meterRegistry.counter("image_storage.uploads", "status", "error").increment();
            log.error("S3/R2 error uploading image: {} - {}", e.awsErrorDetails().errorCode(),
                    e.awsErrorDetails().errorMessage());
            throw new ImageStorageException("Failed to upload image to R2",
                    "S3_ERROR_" + e.awsErrorDetails().errorCode(), e);
        } catch (Exception e) {
            meterRegistry.counter("image_storage.uploads", "status", "error").increment();
            log.error("Unexpected error uploading image", e);
            throw new ImageStorageException("Unexpected error uploading image",
                    "UNEXPECTED_ERROR", e);
        }
    }

    /**
     * Deletar uma imagem do R2
     *
     * @param imageUrl URL da imagem
     * @return true se deletada com sucesso, false caso contrário
     */
    public boolean deleteImage(String imageUrl) {
        try {
            String key = extractKeyFromUrl(imageUrl);

            DeleteObjectRequest deleteObjectRequest = DeleteObjectRequest.builder()
                    .bucket(bucketName)
                    .key(key)
                    .build();

            s3Client.deleteObject(deleteObjectRequest);

            meterRegistry.counter("image_storage.deletes", "status", "success").increment();
            log.info("Image deleted successfully: {}", key);
            return true;

        } catch (S3Exception e) {
            meterRegistry.counter("image_storage.deletes", "status", "error").increment();
            log.error("S3/R2 error deleting image: {}", e.getMessage(), e);
            return false;
        } catch (Exception e) {
            meterRegistry.counter("image_storage.deletes", "status", "error").increment();
            log.error("Unexpected error deleting image", e);
            return false;
        }
    }

    /**
     * Gerar uma URL pré-assinada para download direto
     * Útil para links temporários com expiração
     *
     * @param imageUrl URL da imagem
     * @param expirationMinutes Minutos até expiração do link
     * @return URL pré-assinada
     */
    public String generatePresignedUrl(String imageUrl, int expirationMinutes) {
        try {
            String key = extractKeyFromUrl(imageUrl);

            GetObjectPresignRequest presignRequest = GetObjectPresignRequest.builder()
                    .signatureDuration(Duration.ofMinutes(expirationMinutes))
                    .getObjectRequest(GetObjectRequest.builder()
                            .bucket(bucketName)
                            .key(key)
                            .build())
                    .build();

            PresignedGetObjectRequest presignedRequest = s3Presigner.presignGetObject(presignRequest);
            String presignedUrl = presignedRequest.url().toString();

            log.debug("Presigned URL generated for: {}", key);
            return presignedUrl;

        } catch (Exception e) {
            log.error("Error generating presigned URL", e);
            throw new ImageStorageException("Failed to generate presigned URL",
                    "PRESIGNED_URL_ERROR", e);
        }
    }

    /**
     * Gerar chave S3 para o arquivo
     * Formato: evaluations/{uuid}/{timestamp}-{originalFileName}
     */
    private String generateS3Key(String originalFileName) {
        String timestamp = System.currentTimeMillis() + "";
        String uuid = UUID.randomUUID().toString();
        return "evaluations/" + uuid + "/" + timestamp + "-" + originalFileName;
    }

    /**
     * Gerar URL pública da imagem baseada na chave S3
     */
    private String generateImageUrl(String s3Key) {
        // Assumindo que o endpoint termina com slash
        if (endpoint.endsWith("/")) {
            return endpoint + bucketName + "/" + s3Key;
        } else {
            return endpoint + "/" + bucketName + "/" + s3Key;
        }
    }

    /**
     * Extrair chave S3 da URL da imagem
     */
    private String extractKeyFromUrl(String imageUrl) {
        if (imageUrl.contains(bucketName)) {
            int startIndex = imageUrl.indexOf(bucketName) + bucketName.length() + 1;
            return imageUrl.substring(startIndex);
        }
        throw new IllegalArgumentException("Invalid image URL: " + imageUrl);
    }
}
