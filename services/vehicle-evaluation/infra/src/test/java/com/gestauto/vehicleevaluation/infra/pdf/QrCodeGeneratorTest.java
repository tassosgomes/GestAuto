package com.gestauto.vehicleevaluation.infra.pdf;

import com.itextpdf.layout.element.Image;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Testes unitários para o QrCodeGenerator.
 */
@DisplayName("QrCodeGenerator Tests")
class QrCodeGeneratorTest {

    private QrCodeGenerator qrCodeGenerator;

    @BeforeEach
    void setUp() {
        qrCodeGenerator = new QrCodeGenerator(200);
    }

    @Test
    @DisplayName("deve gerar QR code com conteúdo válido")
    void testGenerateQrCode() {
        String content = "https://gestauto.com/validate/123e4567-e89b-12d3-a456-426614174000?token=abc123";
        
        byte[] qrCode = qrCodeGenerator.generateQrCode(content);
        
        assertNotNull(qrCode, "QR code não deve ser nulo");
        assertTrue(qrCode.length > 0, "QR code não deve estar vazio");
    }

    @Test
    @DisplayName("deve gerar token único para validação")
    void testGenerateValidationToken() {
        String token1 = qrCodeGenerator.generateValidationToken();
        String token2 = qrCodeGenerator.generateValidationToken();
        
        assertNotNull(token1, "Token não deve ser nulo");
        assertNotNull(token2, "Token não deve ser nulo");
        assertNotEquals(token1, token2, "Tokens devem ser únicos");
        assertTrue(token1.matches("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"), 
                  "Token deve ser UUID válido");
    }

    @Test
    @DisplayName("deve lançar exceção com conteúdo vazio")
    void testGenerateQrCodeWithEmptyContent() {
        assertThrows(QrCodeGenerationException.class, () -> {
            qrCodeGenerator.generateQrCode("");
        }, "Deve lançar exceção para conteúdo vazio");
    }

    @Test
    @DisplayName("deve lançar exceção com conteúdo nulo")
    void testGenerateQrCodeWithNullContent() {
        assertThrows(Exception.class, () -> {
            qrCodeGenerator.generateQrCode(null);
        }, "Deve lançar exceção para conteúdo nulo");
    }

    @Test
    @DisplayName("deve criar imagem QR code para inserção em PDF")
    void testCreateQrCodeImage() {
        String validationUrl = "https://gestauto.com/validate/123e4567-e89b-12d3-a456-426614174000?token=abc123";
        
        Image qrImage = qrCodeGenerator.createQrCodeImage(validationUrl);
        
        assertNotNull(qrImage, "Imagem QR code não deve ser nula");
    }

    @Test
    @DisplayName("deve gerar QR code com diferentes tamanhos")
    void testGenerateQrCodeDifferentSizes() {
        QrCodeGenerator smallQr = new QrCodeGenerator(100);
        QrCodeGenerator largeQr = new QrCodeGenerator(400);
        
        String content = "https://test.com/validate?token=123";
        
        byte[] smallQrCode = smallQr.generateQrCode(content);
        byte[] largeQrCode = largeQr.generateQrCode(content);
        
        assertNotNull(smallQrCode, "Small QR code não deve ser nulo");
        assertNotNull(largeQrCode, "Large QR code não deve ser nulo");
        assertTrue(smallQrCode.length > 0 && largeQrCode.length > 0, 
                  "QR codes não devem estar vazios");
    }
}
