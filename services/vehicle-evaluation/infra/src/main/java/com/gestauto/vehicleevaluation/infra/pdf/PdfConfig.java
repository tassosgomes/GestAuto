package com.gestauto.vehicleevaluation.infra.pdf;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuração de geração de PDF para o sistema.
 */
@Configuration
public class PdfConfig {

    @Bean
    public PdfGenerator pdfGenerator() {
        return new PdfGenerator();
    }

    @Bean
    public QrCodeGenerator qrCodeGenerator() {
        return new QrCodeGenerator(200);
    }
}
