package com.gestauto.vehicleevaluation.infra.pdf;

import com.google.zxing.BarcodeFormat;
import com.google.zxing.MultiFormatWriter;
import com.google.zxing.client.j2se.MatrixToImageWriter;
import com.google.zxing.common.BitMatrix;
import com.itextpdf.io.image.ImageData;
import com.itextpdf.io.image.ImageDataFactory;
import com.itextpdf.layout.element.Image;
import lombok.extern.slf4j.Slf4j;

import java.io.ByteArrayOutputStream;
import java.util.UUID;

/**
 * Gerador de QR Code para validação de laudos.
 * 
 * Utiliza a biblioteca ZXing (Zebra Crossing) para gerar QR codes
 * que contêm URLs de validação do laudo com token de acesso temporário.
 */
@Slf4j
public class QrCodeGenerator {

    private static final int WIDTH = 200;
    private static final int HEIGHT = 200;
    private final int size;

    /**
     * Construtor do gerador de QR code.
     *
     * @param size tamanho do QR code em pixels
     */
    public QrCodeGenerator(int size) {
        this.size = size;
    }

    /**
     * Gera QR code a partir do conteúdo texto.
     *
     * @param content conteúdo a ser codificado no QR code
     * @return bytes da imagem PNG do QR code
     * @throws QrCodeGenerationException se houver erro na geração
     */
    public byte[] generateQrCode(String content) {
        try {
            MultiFormatWriter writer = new MultiFormatWriter();
            BitMatrix bitMatrix = writer.encode(
                    content,
                    BarcodeFormat.QR_CODE,
                    size,
                    size
            );

            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            MatrixToImageWriter.writeToStream(bitMatrix, "PNG", outputStream);
            return outputStream.toByteArray();
        } catch (Exception e) {
            log.error("Erro ao gerar QR code para conteúdo: {}", content, e);
            throw new QrCodeGenerationException("Falha ao gerar QR code", e);
        }
    }

    /**
     * Gera um QR code em formato de imagem iText.
     *
     * @param validationUrl URL de validação do laudo
     * @return imagem do QR code configurada para inserção em PDF
     * @throws QrCodeGenerationException se houver erro na geração
     */
    public Image createQrCodeImage(String validationUrl) {
        try {
            byte[] qrCodeBytes = generateQrCode(validationUrl);
            ImageData imageData = ImageDataFactory.create(qrCodeBytes);
            return new Image(imageData)
                    .setWidth(80)
                    .setHeight(80);
        } catch (Exception e) {
            log.error("Erro ao criar imagem QR code", e);
            throw new QrCodeGenerationException("Falha ao criar imagem QR code", e);
        }
    }

    /**
     * Gera um token único para validação do laudo.
     * 
     * @return token único (UUID)
     */
    public String generateValidationToken() {
        return UUID.randomUUID().toString();
    }
}
