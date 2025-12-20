package com.gestauto.vehicleevaluation.infra.service;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.report.BrandStat;
import com.gestauto.vehicleevaluation.domain.report.EvaluationKpi;
import com.gestauto.vehicleevaluation.domain.report.MonthlyStat;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationDashboardRepository;
import com.gestauto.vehicleevaluation.domain.service.ManagementReportExporter;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationStatusJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import com.gestauto.vehicleevaluation.infra.pdf.PdfGenerationException;
import com.gestauto.vehicleevaluation.infra.repository.VehicleEvaluationJpaRepository;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.element.Table;
import com.itextpdf.layout.properties.UnitValue;
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.Timer;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;
import org.apache.poi.ss.usermodel.Workbook;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.springframework.stereotype.Service;

import java.io.ByteArrayOutputStream;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.Optional;

@Service
@RequiredArgsConstructor
@Slf4j
public class ManagementReportExporterImpl implements ManagementReportExporter {

    private final VehicleEvaluationJpaRepository jpaRepository;
    private final EvaluationDashboardRepository dashboardRepository;
    private final Optional<MeterRegistry> meterRegistry;

    private static final DateTimeFormatter DATE_TIME_FORMATTER = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");

    @Override
    public byte[] exportDetailedExcel(LocalDateTime startDate,
                                     LocalDateTime endDate,
                                     String evaluatorId,
                                     EvaluationStatus status) {

        Timer.Sample sample = meterRegistry.isPresent() ? Timer.start(meterRegistry.get()) : null;
        try {
            log.info("Generating Excel management report: startDate={}, endDate={}, evaluatorId={}, status={}",
                startDate, endDate, evaluatorId, status);

            EvaluationStatusJpa statusJpa = status != null ? EvaluationStatusJpa.valueOf(status.name()) : null;

            List<VehicleEvaluationJpaEntity> evaluations = jpaRepository.findForManagementReport(
                startDate,
                endDate,
                evaluatorId,
                statusJpa
            );

            EvaluationKpi kpis = dashboardRepository.getKpis(startDate, endDate, evaluatorId, status);
            List<MonthlyStat> monthly = dashboardRepository.getMonthlyStats(startDate, endDate, evaluatorId, status);
            List<BrandStat> brands = dashboardRepository.getBrandStats(startDate, endDate, evaluatorId, status);

            try (Workbook workbook = new XSSFWorkbook()) {
                createSummarySheet(workbook, startDate, endDate, evaluatorId, status, kpis);
                createMonthlySheet(workbook, monthly);
                createBrandSheet(workbook, brands);
                createEvaluationsSheet(workbook, evaluations);

                autosizeAllColumns(workbook);

                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                workbook.write(baos);
                return baos.toByteArray();
            }
        } catch (Exception e) {
            log.error("Failed to generate Excel management report", e);
            throw new IllegalStateException("Failed to generate Excel management report", e);
        } finally {
            if (sample != null) {
                sample.stop(Timer.builder("reports.excel.generation.duration")
                    .description("Tempo de geração de relatório Excel")
                    .register(meterRegistry.get()));
            }
        }
    }

    @Override
    public byte[] exportSummaryPdf(LocalDateTime startDate,
                                  LocalDateTime endDate,
                                  String evaluatorId,
                                  EvaluationStatus status) {

        Timer.Sample sample = meterRegistry.isPresent() ? Timer.start(meterRegistry.get()) : null;
        try {
            log.info("Generating PDF summary report: startDate={}, endDate={}, evaluatorId={}, status={}",
                startDate, endDate, evaluatorId, status);

            EvaluationKpi kpis = dashboardRepository.getKpis(startDate, endDate, evaluatorId, status);
            List<MonthlyStat> monthly = dashboardRepository.getMonthlyStats(startDate, endDate, evaluatorId, status);
            List<BrandStat> brands = dashboardRepository.getBrandStats(startDate, endDate, evaluatorId, status);

            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfWriter writer = new PdfWriter(baos);
            PdfDocument pdfDocument = new PdfDocument(writer);
            Document document = new Document(pdfDocument);

            document.add(new Paragraph("Vehicle Evaluation - Management Summary").setFontSize(16));
            document.add(new Paragraph(String.format("Period: %s to %s",
                startDate.format(DATE_TIME_FORMATTER),
                endDate.format(DATE_TIME_FORMATTER))));

            if (evaluatorId != null) {
                document.add(new Paragraph("Evaluator: " + evaluatorId));
            }
            if (status != null) {
                document.add(new Paragraph("Status filter: " + status));
            }

            document.add(new Paragraph("\nKPIs").setFontSize(12));
            Table kpiTable = new Table(UnitValue.createPercentArray(new float[]{50, 50}))
                .useAllAvailableWidth();
            kpiTable.addCell("Total evaluations");
            kpiTable.addCell(String.valueOf(kpis.totalEvaluations()));
            kpiTable.addCell("Approval rate (%)");
            kpiTable.addCell(valueOrZero(kpis.approvalRatePercent()).toString());
            kpiTable.addCell("Average ticket");
            kpiTable.addCell(valueOrZero(kpis.averageTicket()).toString());
            kpiTable.addCell("Average review time (hours)");
            kpiTable.addCell(valueOrZero(kpis.averageReviewTimeHours()).toString());
            document.add(kpiTable);

            document.add(new Paragraph("\nMonthly trend").setFontSize(12));
            Table monthlyTable = new Table(UnitValue.createPercentArray(new float[]{40, 30, 30}))
                .useAllAvailableWidth();
            monthlyTable.addCell("Month");
            monthlyTable.addCell("Evaluations");
            monthlyTable.addCell("Total ticket");
            for (MonthlyStat stat : monthly) {
                monthlyTable.addCell(stat.month().toString());
                monthlyTable.addCell(String.valueOf(stat.evaluationsCount()));
                monthlyTable.addCell(valueOrZero(stat.totalTicket()).toString());
            }
            document.add(monthlyTable);

            document.add(new Paragraph("\nBrand distribution").setFontSize(12));
            Table brandTable = new Table(UnitValue.createPercentArray(new float[]{40, 20, 20, 20}))
                .useAllAvailableWidth();
            brandTable.addCell("Brand");
            brandTable.addCell("Evaluations");
            brandTable.addCell("Avg ticket");
            brandTable.addCell("Approval rate (%)");
            for (BrandStat stat : brands) {
                brandTable.addCell(stat.brand());
                brandTable.addCell(String.valueOf(stat.evaluationsCount()));
                brandTable.addCell(valueOrZero(stat.averageTicket()).toString());
                brandTable.addCell(valueOrZero(stat.approvalRatePercent()).toString());
            }
            document.add(brandTable);

            document.close();
            return baos.toByteArray();

        } catch (Exception e) {
            log.error("Failed to generate PDF summary report", e);
            throw new PdfGenerationException("Failed to generate PDF summary report", e);
        } finally {
            if (sample != null) {
                sample.stop(Timer.builder("reports.pdf.summary.generation.duration")
                    .description("Tempo de geração de relatório PDF resumido")
                    .register(meterRegistry.get()));
            }
        }
    }

    private void createSummarySheet(Workbook workbook,
                                    LocalDateTime startDate,
                                    LocalDateTime endDate,
                                    String evaluatorId,
                                    EvaluationStatus status,
                                    EvaluationKpi kpis) {
        Sheet sheet = workbook.createSheet("Summary");
        int rowNum = 0;

        Row title = sheet.createRow(rowNum++);
        title.createCell(0).setCellValue("Vehicle Evaluation - Management Report");

        Row periodRow = sheet.createRow(rowNum++);
        periodRow.createCell(0).setCellValue("Start");
        periodRow.createCell(1).setCellValue(startDate.format(DATE_TIME_FORMATTER));
        Row periodRow2 = sheet.createRow(rowNum++);
        periodRow2.createCell(0).setCellValue("End");
        periodRow2.createCell(1).setCellValue(endDate.format(DATE_TIME_FORMATTER));

        if (evaluatorId != null) {
            Row evaluatorRow = sheet.createRow(rowNum++);
            evaluatorRow.createCell(0).setCellValue("Evaluator");
            evaluatorRow.createCell(1).setCellValue(evaluatorId);
        }

        if (status != null) {
            Row statusRow = sheet.createRow(rowNum++);
            statusRow.createCell(0).setCellValue("Status filter");
            statusRow.createCell(1).setCellValue(status.name());
        }

        rowNum++;

        Row header = sheet.createRow(rowNum++);
        header.createCell(0).setCellValue("KPI");
        header.createCell(1).setCellValue("Value");

        Row r1 = sheet.createRow(rowNum++);
        r1.createCell(0).setCellValue("Total evaluations");
        r1.createCell(1).setCellValue(kpis.totalEvaluations());

        Row r2 = sheet.createRow(rowNum++);
        r2.createCell(0).setCellValue("Approval rate (%)");
        r2.createCell(1).setCellValue(valueOrZero(kpis.approvalRatePercent()).doubleValue());

        Row r3 = sheet.createRow(rowNum++);
        r3.createCell(0).setCellValue("Average ticket");
        r3.createCell(1).setCellValue(valueOrZero(kpis.averageTicket()).doubleValue());

        Row r4 = sheet.createRow(rowNum++);
        r4.createCell(0).setCellValue("Average review time (hours)");
        r4.createCell(1).setCellValue(valueOrZero(kpis.averageReviewTimeHours()).doubleValue());
    }

    private void createMonthlySheet(Workbook workbook, List<MonthlyStat> monthly) {
        Sheet sheet = workbook.createSheet("Monthly");
        int rowNum = 0;

        Row header = sheet.createRow(rowNum++);
        header.createCell(0).setCellValue("Month");
        header.createCell(1).setCellValue("Evaluations");
        header.createCell(2).setCellValue("Total ticket");

        for (MonthlyStat stat : monthly) {
            Row row = sheet.createRow(rowNum++);
            row.createCell(0).setCellValue(stat.month().toString());
            row.createCell(1).setCellValue(stat.evaluationsCount());
            row.createCell(2).setCellValue(valueOrZero(stat.totalTicket()).doubleValue());
        }
    }

    private void createBrandSheet(Workbook workbook, List<BrandStat> brands) {
        Sheet sheet = workbook.createSheet("Brands");
        int rowNum = 0;

        Row header = sheet.createRow(rowNum++);
        header.createCell(0).setCellValue("Brand");
        header.createCell(1).setCellValue("Evaluations");
        header.createCell(2).setCellValue("Avg ticket");
        header.createCell(3).setCellValue("Approval rate (%)");

        for (BrandStat stat : brands) {
            Row row = sheet.createRow(rowNum++);
            row.createCell(0).setCellValue(stat.brand());
            row.createCell(1).setCellValue(stat.evaluationsCount());
            row.createCell(2).setCellValue(valueOrZero(stat.averageTicket()).doubleValue());
            row.createCell(3).setCellValue(valueOrZero(stat.approvalRatePercent()).doubleValue());
        }
    }

    private void createEvaluationsSheet(Workbook workbook, List<VehicleEvaluationJpaEntity> evaluations) {
        Sheet sheet = workbook.createSheet("Evaluations");
        int rowNum = 0;

        String[] headers = {
            "ID",
            "Plate",
            "Brand",
            "Model",
            "Year",
            "Mileage",
            "Evaluator",
            "Status",
            "FIPE",
            "Base value",
            "Final value",
            "Approved value",
            "Created at",
            "Submitted at",
            "Approved at"
        };

        Row headerRow = sheet.createRow(rowNum++);
        for (int i = 0; i < headers.length; i++) {
            Cell cell = headerRow.createCell(i);
            cell.setCellValue(headers[i]);
        }

        for (VehicleEvaluationJpaEntity eval : evaluations) {
            Row row = sheet.createRow(rowNum++);
            int col = 0;

            row.createCell(col++).setCellValue(eval.getId() != null ? eval.getId().toString() : "");
            row.createCell(col++).setCellValue(eval.getPlate() != null ? eval.getPlate() : "");
            row.createCell(col++).setCellValue(eval.getVehicleInfo() != null ? nullSafe(eval.getVehicleInfo().getBrand()) : "");
            row.createCell(col++).setCellValue(eval.getVehicleInfo() != null ? nullSafe(eval.getVehicleInfo().getModel()) : "");
            row.createCell(col++).setCellValue(eval.getVehicleInfo() != null && eval.getVehicleInfo().getYearModel() != null ? eval.getVehicleInfo().getYearModel() : 0);
            row.createCell(col++).setCellValue(eval.getMileageAmount() != null ? eval.getMileageAmount().doubleValue() : 0d);
            row.createCell(col++).setCellValue(nullSafe(eval.getEvaluatorId()));
            row.createCell(col++).setCellValue(eval.getStatus() != null ? eval.getStatus().name() : "");
            row.createCell(col++).setCellValue(toDouble(eval.getFipePriceAmount()));
            row.createCell(col++).setCellValue(toDouble(eval.getBaseValueAmount()));
            row.createCell(col++).setCellValue(toDouble(eval.getFinalValueAmount()));
            row.createCell(col++).setCellValue(toDouble(eval.getApprovedValueAmount()));
            row.createCell(col++).setCellValue(eval.getCreatedAt() != null ? eval.getCreatedAt().toString() : "");
            row.createCell(col++).setCellValue(eval.getSubmittedAt() != null ? eval.getSubmittedAt().toString() : "");
            row.createCell(col++).setCellValue(eval.getApprovedAt() != null ? eval.getApprovedAt().toString() : "");
        }
    }

    private void autosizeAllColumns(Workbook workbook) {
        for (int sheetIndex = 0; sheetIndex < workbook.getNumberOfSheets(); sheetIndex++) {
            Sheet sheet = workbook.getSheetAt(sheetIndex);
            if (sheet.getPhysicalNumberOfRows() == 0) {
                continue;
            }
            int maxColumns = sheet.getRow(0).getPhysicalNumberOfCells();
            for (int col = 0; col < maxColumns; col++) {
                sheet.autoSizeColumn(col);
            }
        }
    }

    private BigDecimal valueOrZero(BigDecimal value) {
        return value != null ? value : BigDecimal.ZERO;
    }

    private double toDouble(BigDecimal value) {
        return value != null ? value.doubleValue() : 0d;
    }

    private String nullSafe(String value) {
        return value != null ? value : "";
    }
}
