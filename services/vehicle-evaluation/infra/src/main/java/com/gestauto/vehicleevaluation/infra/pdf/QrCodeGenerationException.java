package com.gestauto.vehicleevaluation.infra.pdf;

/**
 * Exceção lançada quando há erro na geração de QR Code.
 */
public class QrCodeGenerationException extends RuntimeException {

    public QrCodeGenerationException(String message) {
        super(message);
    }

    public QrCodeGenerationException(String message, Throwable cause) {
        super(message, cause);
    }
}
