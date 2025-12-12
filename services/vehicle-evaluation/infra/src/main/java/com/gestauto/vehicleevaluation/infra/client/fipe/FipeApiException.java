package com.gestauto.vehicleevaluation.infra.client.fipe;

/**
 * Exceção customizada para erros da API FIPE
 */
public class FipeApiException extends RuntimeException {

    private final String errorCode;

    public FipeApiException(String message, String errorCode) {
        super(message);
        this.errorCode = errorCode;
    }

    public FipeApiException(String message, String errorCode, Throwable cause) {
        super(message, cause);
        this.errorCode = errorCode;
    }

    public String getErrorCode() {
        return errorCode;
    }
}
