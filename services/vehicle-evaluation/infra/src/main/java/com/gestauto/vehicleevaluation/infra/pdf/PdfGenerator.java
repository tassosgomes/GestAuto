package com.gestauto.vehicleevaluation.infra.pdf;

import com.itextpdf.kernel.colors.ColorConstants;
import com.itextpdf.kernel.events.Event;
import com.itextpdf.kernel.events.IEventHandler;
import com.itextpdf.kernel.events.PdfDocumentEvent;
import com.itextpdf.kernel.geom.PageSize;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.element.Table;
import com.itextpdf.layout.properties.TextAlignment;
import com.itextpdf.layout.properties.UnitValue;
import lombok.extern.slf4j.Slf4j;

import java.io.ByteArrayOutputStream;

/**
 * Gerador de PDF para relatórios de avaliação de veículos.
 * 
 * Implementa padrões profissionais com:
 * - Headers e footers customizados
 * - Margens adequadas
 * - Fontes e formatação profissional
 * - Marca d'água dinâmica
 */
@Slf4j
public class PdfGenerator {

    // Configurações de página
    private static final float TOP_MARGIN = 54f;
    private static final float BOTTOM_MARGIN = 54f;
    private static final float LEFT_MARGIN = 36f;
    private static final float RIGHT_MARGIN = 36f;

    /**
     * Cria um novo documento PDF com configuração padrão.
     *
     * @return ByteArrayOutputStream com o PDF
     */
    public ByteArrayOutputStream createDocument() {
        try {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfWriter writer = new PdfWriter(baos);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf, PageSize.A4);
            
            // Configurar margens
            document.setMargins(TOP_MARGIN, RIGHT_MARGIN, BOTTOM_MARGIN, LEFT_MARGIN);
            
            return baos;
        } catch (Exception e) {
            log.error("Erro ao criar documento PDF", e);
            throw new PdfGenerationException("Falha ao criar documento PDF", e);
        }
    }

    /**
     * Adiciona marca d'água ao documento PDF.
     *
     * @param pdf documento PDF
     * @param text texto da marca d'água (ex: "APROVADO", "REPROVADO")
     * @param opacity opacidade da marca (0-1)
     */
    public void addWatermark(PdfDocument pdf, String text, float opacity) {
        try {
            pdf.addEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkEventHandler(text, opacity));
        } catch (Exception e) {
            log.error("Erro ao adicionar marca d'água", e);
            throw new PdfGenerationException("Falha ao adicionar marca d'água", e);
        }
    }

    /**
     * Cria uma tabela básica para layout.
     *
     * @param columnCount número de colunas
     * @return tabela configurada
     */
    public Table createTable(int columnCount) {
        return new Table(UnitValue.createPercentArray(new float[columnCount]));
    }

    /**
     * Cria um parágrafo com título formatado.
     *
     * @param text texto do título
     * @return parágrafo formatado
     */
    public Paragraph createTitle(String text) {
        return new Paragraph(text)
                .setFontSize(20)
                .setBold()
                .setTextAlignment(TextAlignment.CENTER)
                .setMarginBottom(12);
    }

    /**
     * Cria um parágrafo com subtítulo formatado.
     *
     * @param text texto do subtítulo
     * @return parágrafo formatado
     */
    public Paragraph createSubtitle(String text) {
        return new Paragraph(text)
                .setFontSize(14)
                .setBold()
                .setMarginBottom(10)
                .setMarginTop(10);
    }

    /**
     * Cria um parágrafo com texto de corpo formatado.
     *
     * @param text texto do corpo
     * @return parágrafo formatado
     */
    public Paragraph createBodyText(String text) {
        return new Paragraph(text)
                .setFontSize(11)
                .setMarginBottom(8);
    }

    /**
     * Cria um parágrafo com texto em pequeno formatado.
     *
     * @param text texto pequeno
     * @return parágrafo formatado
     */
    public Paragraph createSmallText(String text) {
        return new Paragraph(text)
                .setFontSize(9)
                .setMarginBottom(4);
    }
}
