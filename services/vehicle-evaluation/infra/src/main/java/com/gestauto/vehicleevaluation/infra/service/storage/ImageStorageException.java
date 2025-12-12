package com.gestauto.vehicleevaluation.infra.service.storage;

/**
 * Exceção customizada para erros de armazenamento de imagens
 */
public class ImageStorageException extends RuntimeException {

    private final String errorCode;

    public ImageStorageException(String message, String errorCode) {
        super(message);
        this.errorCode = errorCode;
    }

    public ImageStorageException(String message, String errorCode, Throwable cause) {
        super(message, cause);
        this.errorCode = errorCode;
    }

    public String getErrorCode() {
        return errorCode;
    }
}
