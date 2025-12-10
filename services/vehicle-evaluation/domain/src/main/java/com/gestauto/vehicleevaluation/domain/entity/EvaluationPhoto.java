package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import java.time.LocalDateTime;
import java.util.Objects;
import java.util.UUID;

/**
 * Entidade de domínio representando uma foto de uma avaliação de veículo.
 *
 * Esta entidade encapsula as informações de uma foto associada
 * a uma avaliação, mantendo consistência e validações de negócio.
 */
public final class EvaluationPhoto {

    private final String photoId;
    private final EvaluationId evaluationId;
    private final PhotoType photoType;
    private final String fileName;
    private final String filePath;
    private final long fileSize;
    private final String contentType;
    private final LocalDateTime uploadedAt;
    private String thumbnailUrl;
    private final String uploadUrl;

    private EvaluationPhoto(String photoId, EvaluationId evaluationId, PhotoType photoType,
                            String fileName, String filePath, long fileSize, String contentType,
                            String uploadUrl, String thumbnailUrl) {
        this.photoId = Objects.requireNonNull(photoId, "PhotoId cannot be null");
        this.evaluationId = Objects.requireNonNull(evaluationId, "EvaluationId cannot be null");
        this.photoType = Objects.requireNonNull(photoType, "PhotoType cannot be null");
        this.fileName = Objects.requireNonNull(fileName, "FileName cannot be null");
        this.filePath = Objects.requireNonNull(filePath, "FilePath cannot be null");
        this.contentType = Objects.requireNonNull(contentType, "ContentType cannot be null");
        this.uploadUrl = uploadUrl;
        this.thumbnailUrl = thumbnailUrl;
        this.uploadedAt = LocalDateTime.now();
        this.fileSize = fileSize;

        validate();
    }

    private EvaluationPhoto(String photoId, EvaluationId evaluationId, PhotoType photoType,
                            String fileName, String filePath, long fileSize, String contentType,
                            String uploadUrl, String thumbnailUrl, LocalDateTime uploadedAt) {
        this.photoId = Objects.requireNonNull(photoId, "PhotoId cannot be null");
        this.evaluationId = Objects.requireNonNull(evaluationId, "EvaluationId cannot be null");
        this.photoType = Objects.requireNonNull(photoType, "PhotoType cannot be null");
        this.fileName = Objects.requireNonNull(fileName, "FileName cannot be null");
        this.filePath = Objects.requireNonNull(filePath, "FilePath cannot be null");
        this.contentType = Objects.requireNonNull(contentType, "ContentType cannot be null");
        this.uploadUrl = uploadUrl;
        this.thumbnailUrl = thumbnailUrl;
        this.uploadedAt = Objects.requireNonNullElse(uploadedAt, LocalDateTime.now());
        this.fileSize = fileSize;
        validate();
    }

    /**
     * Cria uma nova foto de avaliação.
     *
     * @param evaluationId ID da avaliação
     * @param photoType tipo da foto
     * @param fileName nome original do arquivo
     * @param filePath caminho do arquivo no storage
     * @param fileSize tamanho do arquivo em bytes
     * @param contentType tipo MIME do arquivo
     * @param uploadUrl URL pública da foto
     * @param thumbnailUrl URL do thumbnail
     * @return nova instância de EvaluationPhoto
     * @throws IllegalArgumentException se algum dado for inválido
     */
    public static EvaluationPhoto create(EvaluationId evaluationId, PhotoType photoType,
                                          String fileName, String filePath, long fileSize,
                                          String contentType, String uploadUrl, String thumbnailUrl) {
        String photoId = UUID.randomUUID().toString();
        return new EvaluationPhoto(photoId, evaluationId, photoType, fileName,
                                    filePath, fileSize, contentType, uploadUrl, thumbnailUrl);
    }

    /**
     * Reidrata foto já existente.
     */
    public static EvaluationPhoto restore(String photoId, EvaluationId evaluationId, PhotoType photoType,
                                          String fileName, String filePath, long fileSize,
                                          String contentType, String uploadUrl, String thumbnailUrl,
                                          LocalDateTime uploadedAt) {
        return new EvaluationPhoto(photoId, evaluationId, photoType, fileName,
            filePath, fileSize, contentType, uploadUrl, thumbnailUrl, uploadedAt);
    }

    /**
     * Cria uma nova foto de avaliação sem thumbnail.
     *
     * @param evaluationId ID da avaliação
     * @param photoType tipo da foto
     * @param fileName nome original do arquivo
     * @param filePath caminho do arquivo no storage
     * @param fileSize tamanho do arquivo em bytes
     * @param contentType tipo MIME do arquivo
     * @param uploadUrl URL pública da foto
     * @return nova instância de EvaluationPhoto
     */
    public static EvaluationPhoto createWithoutThumbnail(EvaluationId evaluationId, PhotoType photoType,
                                                          String fileName, String filePath, long fileSize,
                                                          String contentType, String uploadUrl) {
        return create(evaluationId, photoType, fileName, filePath, fileSize,
                     contentType, uploadUrl, null);
    }

    /**
     * Valida os dados da foto.
     *
     * @throws IllegalArgumentException se algum dado for inválido
     */
    private void validate() {
        if (fileName.trim().isEmpty()) {
            throw new IllegalArgumentException("FileName cannot be empty");
        }

        if (filePath.trim().isEmpty()) {
            throw new IllegalArgumentException("FilePath cannot be empty");
        }

        if (fileSize <= 0) {
            throw new IllegalArgumentException("FileSize must be positive");
        }

        if (fileSize > 10 * 1024 * 1024) { // 10MB
            throw new IllegalArgumentException("FileSize cannot exceed 10MB");
        }

        if (!isValidImageContentType(contentType)) {
            throw new IllegalArgumentException("Invalid content type: " + contentType);
        }

        if (uploadUrl != null && uploadUrl.trim().isEmpty()) {
            throw new IllegalArgumentException("UploadUrl cannot be empty if provided");
        }
    }

    /**
     * Verifica se o tipo MIME é válido para imagens.
     *
     * @param contentType tipo MIME
     * @return true se for válido
     */
    private boolean isValidImageContentType(String contentType) {
        return contentType.equals("image/jpeg") ||
               contentType.equals("image/jpg") ||
               contentType.equals("image/png") ||
               contentType.equals("image/webp");
    }

    /**
     * Atualiza o URL do thumbnail.
     *
     * @param thumbnailUrl novo URL do thumbnail
     */
    public void updateThumbnailUrl(String thumbnailUrl) {
        if (thumbnailUrl != null && !thumbnailUrl.trim().isEmpty()) {
            this.thumbnailUrl = thumbnailUrl;
        }
    }

    /**
     * Verifica se a foto é de alta qualidade.
     * Considera alta qualidade se tiver mais de 2MB.
     *
     * @return true se for alta qualidade
     */
    public boolean isHighQuality() {
        return fileSize > 2 * 1024 * 1024; // 2MB
    }

    /**
     * Verifica se o tipo MIME representa JPEG.
     *
     * @return true se for JPEG
     */
    public boolean isJpeg() {
        return contentType.equals("image/jpeg") || contentType.equals("image/jpg");
    }

    /**
     * Verifica se o tipo MIME representa PNG.
     *
     * @return true se for PNG
     */
    public boolean isPng() {
        return contentType.equals("image/png");
    }

    // Getters
    public String getPhotoId() {
        return photoId;
    }

    public EvaluationId getEvaluationId() {
        return evaluationId;
    }

    public PhotoType getPhotoType() {
        return photoType;
    }

    public String getFileName() {
        return fileName;
    }

    public String getFilePath() {
        return filePath;
    }

    public long getFileSize() {
        return fileSize;
    }

    public String getContentType() {
        return contentType;
    }

    public LocalDateTime getUploadedAt() {
        return uploadedAt;
    }

    public String getThumbnailUrl() {
        return thumbnailUrl;
    }

    public String getUploadUrl() {
        return uploadUrl;
    }

    /**
     * Retorna o tamanho do arquivo formatado para exibição.
     *
     * @return tamanho formatado (ex: "2.5 MB")
     */
    public String getFormattedFileSize() {
        double size = fileSize;
        String[] units = {"B", "KB", "MB", "GB"};
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.length - 1) {
            size /= 1024;
            unitIndex++;
        }

        return String.format("%.1f %s", size, units[unitIndex]);
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        EvaluationPhoto that = (EvaluationPhoto) o;
        return photoId.equals(that.photoId);
    }

    @Override
    public int hashCode() {
        return photoId.hashCode();
    }

    @Override
    public String toString() {
        return "EvaluationPhoto{" +
                "photoId='" + photoId + '\'' +
                ", evaluationId=" + evaluationId +
                ", photoType=" + photoType +
                ", fileName='" + fileName + '\'' +
                ", fileSize=" + fileSize +
                ", contentType='" + contentType + '\'' +
                ", uploadUrl='" + uploadUrl + '\'' +
                ", thumbnailUrl='" + thumbnailUrl + '\'' +
                '}';
    }
}