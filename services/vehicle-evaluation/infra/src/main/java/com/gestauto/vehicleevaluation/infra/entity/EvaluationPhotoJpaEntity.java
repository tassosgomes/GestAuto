package com.gestauto.vehicleevaluation.infra.entity;

import jakarta.persistence.*;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Entidade JPA para persistência de fotos de avaliação.
 */
@Entity
@Table(name = "evaluation_photos", schema = "vehicle_evaluation")
public class EvaluationPhotoJpaEntity {

    @Id
    @Column(name = "photo_id", columnDefinition = "UUID")
    private UUID photoId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "evaluation_id", nullable = false)
    private VehicleEvaluationJpaEntity evaluation;

    @Enumerated(EnumType.STRING)
    @Column(name = "photo_type", nullable = false, length = 30)
    private PhotoTypeJpa photoType;

    @Column(name = "file_name", nullable = false, length = 255)
    private String fileName;

    @Column(name = "file_path", nullable = false, length = 500)
    private String filePath;

    @Column(name = "file_size", nullable = false)
    private Long fileSize;

    @Column(name = "content_type", nullable = false, length = 100)
    private String contentType;

    @Column(name = "uploaded_at", nullable = false)
    private LocalDateTime uploadedAt;

    @Column(name = "thumbnail_url", length = 500)
    private String thumbnailUrl;

    @Column(name = "upload_url", length = 500)
    private String uploadUrl;

    // Construtores
    public EvaluationPhotoJpaEntity() {
        this.uploadedAt = LocalDateTime.now();
    }

    // Getters e Setters
    public UUID getPhotoId() {
        return photoId;
    }

    public void setPhotoId(UUID photoId) {
        this.photoId = photoId;
    }

    public VehicleEvaluationJpaEntity getEvaluation() {
        return evaluation;
    }

    public void setEvaluation(VehicleEvaluationJpaEntity evaluation) {
        this.evaluation = evaluation;
    }

    public PhotoTypeJpa getPhotoType() {
        return photoType;
    }

    public void setPhotoType(PhotoTypeJpa photoType) {
        this.photoType = photoType;
    }

    public String getFileName() {
        return fileName;
    }

    public void setFileName(String fileName) {
        this.fileName = fileName;
    }

    public String getFilePath() {
        return filePath;
    }

    public void setFilePath(String filePath) {
        this.filePath = filePath;
    }

    public Long getFileSize() {
        return fileSize;
    }

    public void setFileSize(Long fileSize) {
        this.fileSize = fileSize;
    }

    public String getContentType() {
        return contentType;
    }

    public void setContentType(String contentType) {
        this.contentType = contentType;
    }

    public LocalDateTime getUploadedAt() {
        return uploadedAt;
    }

    public void setUploadedAt(LocalDateTime uploadedAt) {
        this.uploadedAt = uploadedAt;
    }

    public String getThumbnailUrl() {
        return thumbnailUrl;
    }

    public void setThumbnailUrl(String thumbnailUrl) {
        this.thumbnailUrl = thumbnailUrl;
    }

    public String getUploadUrl() {
        return uploadUrl;
    }

    public void setUploadUrl(String uploadUrl) {
        this.uploadUrl = uploadUrl;
    }
}