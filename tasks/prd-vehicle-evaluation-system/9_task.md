## markdown

## status: completed

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
</task_context>

# Tarefa 9.0: Implementação de Geração de Laudos PDF

## Visão Geral

Implementar sistema de geração de laudos PDF completos com marca d'água, fotos organizadas, checklist completo, cálculo detalhado, QR code para validação online, e template profissional personalizado.

<requirements>
- PDF com marca d'águla "APROVADO" ou "REPROVADO"
- Todas as 15 fotos organizadas por categoria
- Cálculo completo com FIPE e deduções
- QR code para validação online (72h)
- Observações do avaliador e gerente
- Template profissional com header/footer
- Performance < 30s por PDF
- Download via endpoint seguro

</requirements>

## Subtarefas

- [x] 9.1 Configurar iText 7 para geração PDF ✅
- [x] 9.2 Criar template do laudo com layout profissional ✅
- [x] 9.3 Implementar geração de QR code ✅
- [x] 9.4 Desenvolver layout de fotos em grid ✅
- [x] 9.5 Implementar seção de cálculo detalhado ✅
- [x] 9.6 Adicionar marca d'água dinâmica ✅
- [x] 9.7 Implementar GenerateReportCommand e Handler ✅
- [x] 9.8 Criar endpoint GET /api/v1/evaluations/{id}/report ✅
- [x] 9.9 Otimizar performance de geração ✅

## Detalhes de Implementação

### Configuração iText

```java
@Configuration
public class PdfConfig {

    @Bean
    public PdfGenerator pdfGenerator() {
        return new PdfGenerator();
    }

    @Bean
    public QrCodeGenerator qrCodeGenerator() {
        return new QrCodeGenerator(new QRCodeWriter(), 200);
    }
}
```

### Gerador de PDF

```java
@Service
public class ReportServiceImpl implements ReportService {
    private final PdfGenerator pdfGenerator;
    private final QrCodeGenerator qrCodeGenerator;
    private final ImageStorageService imageStorageService;
    private final VehicleEvaluationRepository evaluationRepository;

    @Override
    @Timed(value = "pdf.generation.duration", description = "Time taken to generate PDF report")
    public byte[] generateEvaluationReport(VehicleEvaluation evaluation) {
        try (ByteArrayOutputStream baos = new ByteArrayOutputStream()) {
            PdfWriter writer = new PdfWriter(baos);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf, PageSize.A4);

            // Configurar fontes e margens
            PdfFont font = PdfFontFactory.createFont(FontConstants.HELVETICA);
            document.setMargins(36, 36, 36, 36);

            // 1. Header
            addHeader(document, evaluation);

            // 2. Informações básicas
            addVehicleInfo(document, evaluation, font);

            // 3. Fotos
            addPhotosSection(document, evaluation);

            // 4. Checklist
            addChecklistSection(document, evaluation, font);

            // 5. Cálculo de valoração
            addValuationSection(document, evaluation, font);

            // 6. Observações
            addObservationsSection(document, evaluation, font);

            // 7. QR Code e validação
            addValidationSection(document, evaluation);

            // 8. Footer
            addFooter(document);

            document.close();
            return baos.toByteArray();
        } catch (Exception e) {
            throw new ReportGenerationException("Failed to generate PDF report", e);
        }
    }
}
```

### Layout de Fotos

```java
private void addPhotosSection(Document document, VehicleEvaluation evaluation) throws IOException {
    Paragraph photosTitle = new Paragraph("Fotografias do Veículo")
        .setFont(boldFont)
        .setFontSize(16)
        .setMarginBottom(10);

    document.add(photosTitle);

    // Organizar fotos por tipo
    Map<PhotoType, List<EvaluationPhoto>> photosByType = evaluation.getPhotos().stream()
        .collect(Collectors.groupingBy(EvaluationPhoto::getType));

    // Grid 3x5 para 15 fotos
    Table photoTable = new Table(UnitValue.createPercentArray(new float[]{33.3f, 33.3f, 33.3f}));
    photoTable.setWidth(UnitValue.createPercentValue(100));
    photoTable.setBorder(Border.NO_BORDER);

    // Adicionar fotos ordenadas
    List<PhotoType> orderedTypes = Arrays.asList(
        PhotoType.EXTERIOR_FRONT, PhotoType.EXTERIOR_REAR, PhotoType.EXTERIOR_LEFT,
        PhotoType.EXTERIOR_RIGHT, PhotoType.INTERIOR_FRONT, PhotoType.INTERIOR_SEATS,
        PhotoType.INTERIOR_REAR, PhotoType.INTERIOR_TRUNK, PhotoType.DASHBOARD_SPEED,
        PhotoType.DASHBOARD_INFO, PhotoType.DASHBOARD_AC, PhotoType.ENGINE_BAY,
        PhotoType.ENGINE_DETAILS, PhotoType.TRUNK_OPEN, PhotoType.TRUCK_SPARE
    );

    for (int i = 0; i < orderedTypes.size(); i += 3) {
        for (int j = 0; j < 3 && i + j < orderedTypes.size(); j++) {
            PhotoType type = orderedTypes.get(i + j);
            List<EvaluationPhoto> photos = photosByType.get(type);

            if (photos != null && !photos.isEmpty()) {
                ImageData imageData = ImageDataFactory.create(
                    imageStorageService.downloadImage(photos.get(0).getUrl())
                );
                Image photo = new Image(imageData);
                photo.setMaxWidth(UnitValue.createPercentValue(100));
                photo.setMaxHeight(100);

                Cell photoCell = new Cell()
                    .add(photo)
                    .add(new Paragraph(type.getDescription())
                        .setFont(smallFont)
                        .setTextAlignment(TextAlignment.CENTER))
                    .setBorder(Border.NO_BORDER)
                    .setPadding(5);

                photoTable.addCell(photoCell);
            } else {
                photoTable.addCell(new Cell().setBorder(Border.NO_BORDER));
            }
        }
    }

    document.add(photoTable);
    document.add(new Paragraph("\n"));
}
```

### Geração de QR Code

```java
@Service
public class QrCodeGenerator {

    public byte[] generateQrCode(String content) {
        try {
            QRCodeWriter qrCodeWriter = new QRCodeWriter();
            BitMatrix bitMatrix = qrCodeWriter.encode(
                content,
                BarcodeFormat.QR_CODE,
                200,
                200
            );

            ByteArrayOutputStream pngOutputStream = new ByteArrayOutputStream();
            MatrixToImageWriter.writeToStream(bitMatrix, "PNG", pngOutputStream);
            return pngOutputStream.toByteArray();
        } catch (Exception e) {
            throw new QrCodeGenerationException("Failed to generate QR code", e);
        }
    }

    public Image createQrCodeImage(String validationUrl) throws IOException {
        byte[] qrCodeBytes = generateQrCode(validationUrl);
        ImageData imageData = ImageDataFactory.create(qrCodeBytes);
        return new Image(imageData).setWidth(80).setHeight(80);
    }
}
```

### Handler do Relatório

```java
@Component
public class GenerateReportHandler implements CommandHandler<GenerateReportCommand, byte[]> {
    private final VehicleEvaluationRepository evaluationRepository;
    private final ReportService reportService;

    @Override
    @Transactional(readOnly = true)
    public byte[] handle(GenerateReportCommand command) {
        // 1. Buscar avaliação
        VehicleEvaluation evaluation = evaluationRepository.findById(
            EvaluationId.from(command.evaluationId())
        ).orElseThrow(() -> new EntityNotFoundException("Evaluation not found"));

        // 2. Validar status
        if (evaluation.getStatus() == EvaluationStatus.DRAFT) {
            throw new BusinessException("Cannot generate report for evaluation in DRAFT status");
        }

        // 3. Gerar PDF
        return reportService.generateEvaluationReport(evaluation);
    }
}
```

### Endpoint de Download

```java
@GetMapping("/{id}/report")
public ResponseEntity<byte[]> generateReport(@PathVariable UUID id) {
    byte[] report = commandBus.execute(new GenerateReportCommand(id));

    return ResponseEntity.ok()
        .header(HttpHeaders.CONTENT_TYPE, MediaType.APPLICATION_PDF_VALUE)
        .header(HttpHeaders.CONTENT_DISPOSITION,
            "attachment; filename=evaluation_report_" + id + ".pdf")
        .header(HttpHeaders.CACHE_CONTROL, "max-age=3600")
        .body(report);
}
```

## Critérios de Sucesso

- [x] PDF gerado com layout profissional
- [x] Todas as 15 fotos incluídas
- [x] QR code funcional para validação
- [x] Marca d'água "APROVADO/REPROVADO"
- [x] Cálculo detalhado visível
- [x] Performance < 30 segundos
- [x] Download seguro funciona
- [x] PDF válido para 72h
- [x] Otimizado para impressão

## Sequenciamento

- Bloqueado por: 2.0, 4.0, 7.0
- Desbloqueia: 10.0, 13.0
- Paralelizável: Sim (com 8.0 e 10.0)

## Tempo Estimado

- PDF template: 10 horas
- QR Code e validação: 4 horas
- Layout de fotos: 6 horas
- Performance: 4 horas
- Testes: 4 horas
- Total: 28 horas