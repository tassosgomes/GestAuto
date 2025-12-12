package com.gestauto.vehicleevaluation.infra.service;

import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.service.ImageStorageService;
import com.gestauto.vehicleevaluation.domain.service.ReportService;
import com.gestauto.vehicleevaluation.infra.pdf.PdfGenerationException;
import com.gestauto.vehicleevaluation.infra.pdf.PdfGenerator;
import com.gestauto.vehicleevaluation.infra.pdf.QrCodeGenerator;
import com.itextpdf.io.image.ImageData;
import com.itextpdf.io.image.ImageDataFactory;
import com.itextpdf.kernel.colors.ColorConstants;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Cell;
import com.itextpdf.layout.element.Image;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.element.Table;
import com.itextpdf.layout.properties.BorderCollapsePropertyValue;
import com.itextpdf.layout.properties.HorizontalAlignment;
import com.itextpdf.layout.properties.TextAlignment;
import com.itextpdf.layout.properties.UnitValue;
import io.micrometer.core.instrument.Timer;
import io.micrometer.core.instrument.MeterRegistry;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

/**
 * Implementação do serviço de geração de relatórios PDF de avaliações.
 * 
 * Gera laudos profissionais em PDF com:
 * - Informações completas do veículo e avaliação
 * - Todas as 15 fotos organizadas em grid
 * - Checklist técnico detalhado
 * - Cálculo de valoração com FIPE
 * - QR code para validação online
 * - Marca d'água dinâmica (APROVADO/REPROVADO)
 * - Performance otimizada < 30 segundos
 */
@Service
@Slf4j
@RequiredArgsConstructor
public class ReportServiceImpl implements ReportService {

    private final PdfGenerator pdfGenerator;
    private final QrCodeGenerator qrCodeGenerator;
    private final ImageStorageService imageStorageService;
    private final Optional<MeterRegistry> meterRegistry;

    @Value("${app.base-url:https://gestauto.com}")
    private String baseUrl;

    private static final DateTimeFormatter DATE_FORMATTER = DateTimeFormatter.ofPattern("dd/MM/yyyy HH:mm");
    private static final DateTimeFormatter DATE_ONLY_FORMATTER = DateTimeFormatter.ofPattern("dd/MM/yyyy");

    private static final List<PhotoType> PHOTO_ORDER = Arrays.asList(
            PhotoType.EXTERIOR_FRONT,
            PhotoType.EXTERIOR_REAR,
            PhotoType.EXTERIOR_LEFT,
            PhotoType.EXTERIOR_RIGHT,
            PhotoType.INTERIOR_FRONT,
            PhotoType.INTERIOR_SEATS,
            PhotoType.INTERIOR_REAR,
            PhotoType.INTERIOR_TRUNK,
            PhotoType.DASHBOARD_SPEED,
            PhotoType.DASHBOARD_INFO,
            PhotoType.DASHBOARD_AC,
            PhotoType.ENGINE_BAY,
            PhotoType.ENGINE_DETAILS,
            PhotoType.TRUNK_OPEN,
            PhotoType.TRUNK_SPARE
    );

    @Override
    public byte[] generateEvaluationReport(VehicleEvaluation evaluation) {
        Timer.Sample sample = meterRegistry.isPresent() ? 
                Timer.start(meterRegistry.get()) : null;
        
        try {
            log.info("Iniciando geração de relatório PDF para avaliação: {}", evaluation.getId());

            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfWriter writer = new PdfWriter(baos);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            // Adicionar marca d'água baseada no status
            String watermarkText = evaluation.getStatus() == EvaluationStatus.APPROVED ? 
                    "APROVADO" : "REPROVADO";
            pdfGenerator.addWatermark(pdf, watermarkText, 0.1f);

            // Adicionar seções do PDF
            addHeader(document, evaluation);
            addVehicleInfo(document, evaluation);
            addPhotosSection(document, evaluation);
            addChecklistSection(document, evaluation);
            addValuationSection(document, evaluation);
            addObservationsSection(document, evaluation);
            addValidationSection(document, evaluation);
            addFooter(document);

            document.close();
            
            byte[] result = baos.toByteArray();
            log.info("Relatório PDF gerado com sucesso. Tamanho: {} bytes", result.length);

            if (sample != null) {
                sample.stop(Timer.builder("pdf.generation.duration")
                        .description("Tempo de geração do PDF de avaliação")
                        .register(meterRegistry.get()));
            }

            return result;
        } catch (Exception e) {
            log.error("Erro ao gerar relatório PDF", e);
            throw new PdfGenerationException("Falha ao gerar PDF de avaliação", e);
        }
    }

    private void addHeader(Document document, VehicleEvaluation evaluation) {
        // Título
        document.add(pdfGenerator.createTitle("LAUDO DE AVALIAÇÃO DE VEÍCULO"));
        
        // Informações gerais
        Table headerTable = pdfGenerator.createTable(3);
        headerTable.setWidth(UnitValue.createPercentValue(100));

        addCell(headerTable, "ID da Avaliação:", evaluation.getId().toString(), TextAlignment.LEFT);
        addCell(headerTable, "Data do Laudo:", LocalDateTime.now().format(DATE_FORMATTER), TextAlignment.CENTER);
        addCell(headerTable, "Status:", evaluation.getStatus().toString(), TextAlignment.RIGHT);

        document.add(headerTable);
        document.add(new Paragraph("\n"));
    }

    private void addVehicleInfo(Document document, VehicleEvaluation evaluation) {
        document.add(pdfGenerator.createSubtitle("1. INFORMAÇÕES DO VEÍCULO"));

        Table vehicleTable = pdfGenerator.createTable(2);
        vehicleTable.setWidth(UnitValue.createPercentValue(100));

        addCell(vehicleTable, "Placa:", evaluation.getPlate().getFormatted(), TextAlignment.LEFT);
        addCell(vehicleTable, "RENAVAM:", evaluation.getRenavam(), TextAlignment.LEFT);
        
        addCell(vehicleTable, "Marca:", evaluation.getVehicleInfo().getBrand(), TextAlignment.LEFT);
        addCell(vehicleTable, "Modelo:", evaluation.getVehicleInfo().getModel(), TextAlignment.LEFT);
        
        addCell(vehicleTable, "Ano:", String.valueOf(evaluation.getVehicleInfo().getYear()), TextAlignment.LEFT);
        addCell(vehicleTable, "Cor:", evaluation.getVehicleInfo().getColor(), TextAlignment.LEFT);
        
        addCell(vehicleTable, "Quilometragem:", String.format("%,d km", 
                evaluation.getMileage().getAmount().longValue()), TextAlignment.LEFT);
        addCell(vehicleTable, "Combustível:", evaluation.getVehicleInfo().getFuelType().toString(), 
                TextAlignment.LEFT);

        document.add(vehicleTable);
        document.add(new Paragraph("\n"));
    }

    private void addPhotosSection(Document document, VehicleEvaluation evaluation) {
        document.add(pdfGenerator.createSubtitle("2. FOTOGRAFIAS DO VEÍCULO"));

        // Organizar fotos por tipo
        Map<PhotoType, List<EvaluationPhoto>> photosByType = evaluation.getPhotos().stream()
                .collect(Collectors.groupingBy(EvaluationPhoto::getPhotoType));

        // Grid 3 colunas x 5 linhas para 15 fotos
        Table photoTable = new Table(UnitValue.createPercentArray(new float[]{33.3f, 33.3f, 33.3f}));
        photoTable.setWidth(UnitValue.createPercentValue(100));
        photoTable.setBorderCollapse(BorderCollapsePropertyValue.COLLAPSE);

        for (PhotoType photoType : PHOTO_ORDER) {
            List<EvaluationPhoto> photos = photosByType.get(photoType);
            Cell photoCell = new Cell();

            if (photos != null && !photos.isEmpty()) {
                try {
                    EvaluationPhoto photo = photos.get(0);
                    byte[] imageBytes = imageStorageService.downloadImage(photo.getUploadUrl());
                    ImageData imageData = ImageDataFactory.create(imageBytes);
                    Image image = new Image(imageData);
                    image.setMaxWidth(UnitValue.createPercentValue(100));
                    image.setMaxHeight(100);

                    photoCell.add(image);
                    photoCell.add(pdfGenerator.createSmallText(photoType.getDescription()));
                } catch (Exception e) {
                    log.warn("Erro ao incluir foto {}: {}", photoType, e.getMessage());
                    photoCell.add(pdfGenerator.createSmallText("Foto não disponível"));
                }
            } else {
                photoCell.add(pdfGenerator.createSmallText(photoType.getDescription()));
            }

            photoCell.setTextAlignment(TextAlignment.CENTER);
            photoCell.setPadding(5);
            photoCell.setHeight(120);
            photoTable.addCell(photoCell);
        }

        document.add(photoTable);
        document.add(new Paragraph("\n"));
    }

    private void addChecklistSection(Document document, VehicleEvaluation evaluation) {
        document.add(pdfGenerator.createSubtitle("3. CHECKLIST TÉCNICO"));

        if (evaluation.getChecklist() == null) {
            document.add(pdfGenerator.createBodyText("Checklist não preenchido"));
            document.add(new Paragraph("\n"));
            return;
        }

        EvaluationChecklist checklist = evaluation.getChecklist();
        Table checklistTable = pdfGenerator.createTable(2);
        checklistTable.setWidth(UnitValue.createPercentValue(100));

        // Lataria e Pintura
        addSection(checklistTable, "LATARIA E PINTURA");
        addChecklistItem(checklistTable, "Condição da Lataria:", checklist.getBodyCondition());
        addChecklistItem(checklistTable, "Condição da Pintura:", checklist.getPaintCondition());
        addChecklistItem(checklistTable, "Presença de Ferrugem:", checklist.isRustPresence() ? "Sim" : "Não");
        addChecklistItem(checklistTable, "Arranhões Profundos:", checklist.isDeepScratches() ? "Sim" : "Não");

        // Mecânica
        addSection(checklistTable, "MECÂNICA");
        addChecklistItem(checklistTable, "Condição do Motor:", checklist.getEngineCondition());
        addChecklistItem(checklistTable, "Condição da Transmissão:", checklist.getTransmissionCondition());
        addChecklistItem(checklistTable, "Vazamento de Óleo:", checklist.isOilLeaks() ? "Sim" : "Não");
        addChecklistItem(checklistTable, "Vazamento de Água:", checklist.isWaterLeaks() ? "Sim" : "Não");

        // Pneus
        addSection(checklistTable, "PNEUS");
        addChecklistItem(checklistTable, "Condição dos Pneus:", checklist.getTiresCondition());
        addChecklistItem(checklistTable, "Desgaste Irregular:", checklist.isUnevenWear() ? "Sim" : "Não");

        // Documentação
        addSection(checklistTable, "DOCUMENTAÇÃO");
        addChecklistItem(checklistTable, "CRVL Presente:", checklist.isCrvlPresent() ? "Sim" : "Não");
        addChecklistItem(checklistTable, "Manual Presente:", checklist.isManualPresent() ? "Sim" : "Não");
        addChecklistItem(checklistTable, "Chave Reserva:", checklist.isSpareKeyPresent() ? "Sim" : "Não");

        // Score de Conservação
        if (checklist.getConservationScore() != null) {
            addChecklistItem(checklistTable, "Score de Conservação:", 
                    String.format("%d/100", checklist.getConservationScore()));
        }

        document.add(checklistTable);
        document.add(new Paragraph("\n"));
    }

    private void addValuationSection(Document document, VehicleEvaluation evaluation) {
        document.add(pdfGenerator.createSubtitle("4. CÁLCULO DE VALORAÇÃO"));

        Table valuationTable = pdfGenerator.createTable(2);
        valuationTable.setWidth(UnitValue.createPercentValue(100));

        // Valores principais
        if (evaluation.getFipePrice() != null) {
            addMoneyCell(valuationTable, "Preço FIPE:", evaluation.getFipePrice());
        }
        if (evaluation.getBaseValue() != null) {
            addMoneyCell(valuationTable, "Valor Base:", evaluation.getBaseValue());
        }

        // Depreciações
        document.add(pdfGenerator.createBodyText("Itens de Depreciação:"));
        if (!evaluation.getDepreciationItems().isEmpty()) {
            Table depreciationTable = pdfGenerator.createTable(3);
            depreciationTable.setWidth(UnitValue.createPercentValue(100));

            addCell(depreciationTable, "Descrição", "", TextAlignment.LEFT);
            addCell(depreciationTable, "Percentual", "", TextAlignment.CENTER);
            addCell(depreciationTable, "Valor", "", TextAlignment.RIGHT);

            for (DepreciationItem item : evaluation.getDepreciationItems()) {
                addCell(depreciationTable, item.getDescription(), "", TextAlignment.LEFT);
                addCell(depreciationTable, 
                        String.format("%.1f%%", item.getPercentage().doubleValue()), "", 
                        TextAlignment.CENTER);
                addMoneyCell(depreciationTable, "", item.getDepreciationValue());
            }

            document.add(depreciationTable);
        } else {
            document.add(pdfGenerator.createSmallText("Sem itens de depreciação"));
        }

        // Valor final
        document.add(new Paragraph("\n"));
        if (evaluation.getFinalValue() != null) {
            addMoneyCell(valuationTable, "Valor Final:", evaluation.getFinalValue());
        }
        if (evaluation.getApprovedValue() != null) {
            addMoneyCell(valuationTable, "Valor Aprovado:", evaluation.getApprovedValue());
        }

        document.add(valuationTable);
        document.add(new Paragraph("\n"));
    }

    private void addObservationsSection(Document document, VehicleEvaluation evaluation) {
        document.add(pdfGenerator.createSubtitle("5. OBSERVAÇÕES"));

        if (evaluation.getObservations() != null && !evaluation.getObservations().isEmpty()) {
            document.add(pdfGenerator.createBodyText("Avaliador: " + evaluation.getObservations()));
        } else {
            document.add(pdfGenerator.createSmallText("Sem observações"));
        }

        if (evaluation.getJustification() != null && !evaluation.getJustification().isEmpty()) {
            document.add(new Paragraph("\n"));
            document.add(pdfGenerator.createBodyText("Gerente: " + evaluation.getJustification()));
        }

        document.add(new Paragraph("\n"));
    }

    private void addValidationSection(Document document, VehicleEvaluation evaluation) {
        document.add(pdfGenerator.createSubtitle("6. VALIDAÇÃO DO LAUDO"));

        // QR Code
        try {
            String validationUrl = getValidationUrl(
                    UUID.fromString(evaluation.getId().toString()),
                    evaluation.getValidationToken()
            );

            Image qrCode = qrCodeGenerator.createQrCodeImage(validationUrl);
            Table qrTable = pdfGenerator.createTable(2);
            qrTable.setWidth(UnitValue.createPercentValue(100));

            Cell qrCell = new Cell();
            qrCell.add(qrCode);
            qrTable.addCell(qrCell);

            Cell infoCell = new Cell();
            infoCell.add(pdfGenerator.createSmallText("QR Code para Validação Online"));
            infoCell.add(pdfGenerator.createSmallText("Escaneie para validar este laudo"));
            infoCell.add(new Paragraph("\n"));
            infoCell.add(pdfGenerator.createSmallText("URL: " + validationUrl));
            if (evaluation.getValidUntil() != null) {
                infoCell.add(pdfGenerator.createSmallText(
                        "Válido até: " + evaluation.getValidUntil().format(DATE_FORMATTER)));
            }
            qrTable.addCell(infoCell);

            document.add(qrTable);
        } catch (Exception e) {
            log.warn("Erro ao gerar QR code", e);
            document.add(pdfGenerator.createSmallText("Erro ao gerar QR code"));
        }

        document.add(new Paragraph("\n"));
    }

    private void addFooter(Document document) {
        document.add(new Paragraph("\n"));
        Paragraph footer = new Paragraph()
                .add("_________________________________________\n")
                .add("Assinatura do Avaliador")
                .add("\n\n")
                .add("_________________________________________\n")
                .add("Assinatura do Gerente")
                .setFontSize(10)
                .setTextAlignment(TextAlignment.CENTER);
        
        document.add(footer);
    }

    @Override
    public String getValidationUrl(UUID evaluationId, String accessToken) {
        return String.format("%s/api/v1/evaluations/%s/validate?token=%s", 
                baseUrl, evaluationId, accessToken);
    }

    // Métodos auxiliares

    private void addCell(Table table, String label, String value, TextAlignment alignment) {
        Cell labelCell = new Cell();
        labelCell.add(pdfGenerator.createSmallText(label));
        labelCell.setTextAlignment(TextAlignment.LEFT);
        labelCell.setBold();
        table.addCell(labelCell);

        Cell valueCell = new Cell();
        valueCell.add(pdfGenerator.createSmallText(value));
        valueCell.setTextAlignment(alignment);
        table.addCell(valueCell);
    }

    private void addMoneyCell(Table table, String label, Object value) {
        if (label != null && !label.isEmpty()) {
            Cell labelCell = new Cell();
            labelCell.add(pdfGenerator.createSmallText(label));
            labelCell.setTextAlignment(TextAlignment.LEFT);
            labelCell.setBold();
            table.addCell(labelCell);

            Cell valueCell = new Cell();
            if (value != null) {
                valueCell.add(pdfGenerator.createSmallText(formatMoney(value)));
            }
            valueCell.setTextAlignment(TextAlignment.RIGHT);
            table.addCell(valueCell);
        }
    }

    private void addChecklistItem(Table table, String label, String value) {
        Cell labelCell = new Cell();
        labelCell.add(pdfGenerator.createSmallText(label));
        labelCell.setTextAlignment(TextAlignment.LEFT);
        labelCell.setBold();
        table.addCell(labelCell);

        Cell valueCell = new Cell();
        valueCell.add(pdfGenerator.createSmallText(value != null ? value : "-"));
        valueCell.setTextAlignment(TextAlignment.LEFT);
        table.addCell(valueCell);
    }

    private void addSection(Table table, String sectionName) {
        Cell sectionCell = new Cell(1, 2);
        sectionCell.add(pdfGenerator.createBodyText(sectionName));
        sectionCell.setBackgroundColor(ColorConstants.LIGHT_GRAY);
        table.addCell(sectionCell);
    }

    private String formatMoney(Object value) {
        if (value == null) return "R$ 0,00";

        BigDecimal amount;
        if (value instanceof com.gestauto.vehicleevaluation.domain.value.Money) {
            amount = ((com.gestauto.vehicleevaluation.domain.value.Money) value).getAmount();
        } else if (value instanceof BigDecimal) {
            amount = (BigDecimal) value;
        } else {
            return "R$ 0,00";
        }

        return String.format("R$ %,.2f", amount.setScale(2, RoundingMode.HALF_UP));
    }
}