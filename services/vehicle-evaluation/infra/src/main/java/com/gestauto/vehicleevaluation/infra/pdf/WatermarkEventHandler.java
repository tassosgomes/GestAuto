package com.gestauto.vehicleevaluation.infra.pdf;

import com.itextpdf.kernel.colors.ColorConstants;
import com.itextpdf.kernel.events.Event;
import com.itextpdf.kernel.events.IEventHandler;
import com.itextpdf.kernel.events.PdfDocumentEvent;
import com.itextpdf.kernel.geom.PageSize;
import com.itextpdf.kernel.geom.Rectangle;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfPage;
import com.itextpdf.layout.Canvas;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.properties.TextAlignment;
import com.itextpdf.layout.properties.VerticalAlignment;

/**
 * Handler para adicionar marca d'água dinâmica ao PDF.
 * 
 * Implementa o padrão IEventHandler do iText para adicionar
 * marca d'água (APROVADO/REPROVADO) em cada página.
 */
public class WatermarkEventHandler implements IEventHandler {

    private static final int WATERMARK_FONT_SIZE = 60;
    private static final double ROTATION_ANGLE_RADIANS = Math.PI / 4; // 45 graus

    private final String text;
    private final float opacity;

    /**
     * Construtor do handler de marca d'água.
     *
     * @param text texto da marca d'água
     * @param opacity opacidade (0-1)
     */
    public WatermarkEventHandler(String text, float opacity) {
        this.text = text;
        this.opacity = opacity;
    }

    @Override
    public void handleEvent(Event event) {
        PdfDocumentEvent docEvent = (PdfDocumentEvent) event;
        PdfPage page = docEvent.getPage();
        PdfDocument pdfDoc = docEvent.getDocument();
        Rectangle pageSize = page.getPageSize();
        
        // Canvas para desenhar na página
        Canvas canvas = new Canvas(page, pageSize);
        
        // Criar parágrafo com o texto da marca d'água
        Paragraph watermark = new Paragraph(text)
                .setFontSize(WATERMARK_FONT_SIZE)
                .setOpacity(opacity)
                .setFont(null) // Use default font
                .setFontColor("APROVADO".equalsIgnoreCase(text) ? 
                    ColorConstants.GREEN : ColorConstants.RED);
        
        // Posicionar no centro da página com rotação
        canvas.showTextAligned(
                watermark,
                (pageSize.getWidth()) / 2,
                (pageSize.getHeight()) / 2,
                TextAlignment.CENTER,
                VerticalAlignment.MIDDLE
        );
        
        canvas.close();
    }
}
