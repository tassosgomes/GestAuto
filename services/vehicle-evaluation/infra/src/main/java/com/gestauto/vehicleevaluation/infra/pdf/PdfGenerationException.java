package com.gestauto.vehicleevaluation.infra.pdf;

/**
 * Exceção lançada quando há erro na geração de PDF.
 */
public class PdfGenerationException extends RuntimeException {

    public PdfGenerationException(String message) {
        super(message);
    }

    public PdfGenerationException(String message, Throwable cause) {
        super(message, cause);
    }
}
