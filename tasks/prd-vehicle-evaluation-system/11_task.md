## markdown

## status: completed

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
</task_context>

# Tarefa 11.0: Implementação de Dashboard Gerencial e Relatórios

## Visão Geral

Implementar dashboard gerencial com KPIs em tempo real, gráficos de tendências, filtros avançados, e relatórios exportáveis para PDF/Excel, fornecendo visão estratégica das operações de avaliação.

<requirements>
- KPIs: avaliações/mês, taxa aprovação, ticket médio, tempo médio
- Gráficos: evolução mensal, distribuição por marca
- Filtros: avaliador, período, status
- Exportação: PDF e Excel
- Dados em tempo real
- Role ADMIN apenas
- Cache para consultas pesadas
- Performance < 2s

</requirements>

## Subtarefas

- [x] 11.1 Implementar consultas agregadas no repository ✅
- [x] 11.2 Criar GetEvaluationDashboardQuery e Handler ✅
- [x] 11.3 Desenvolver DTOs de dashboard ✅
- [x] 11.4 Implementar endpoints de relatórios ✅
- [x] 11.5 Criar exportador Excel (Apache POI) ✅
- [x] 11.6 Implementar cache Redis para KPIs ✅
- [x] 11.7 Adicionar filtros dinâmicos ✅
- [x] 11.8 Criar endpoints públicos de validação ✅
- [x] 11.9 Otimizar performance com indexes ✅

## Detalhes de Implementação

### Consultas Agregadas

```java
@Repository
public interface VehicleEvaluationRepository {
    // Consultas existentes...

    @Query("""
        SELECT new com.gestauto.vehicleevaluation.application.dto.EvaluationKpiDto(
            COUNT(e),
            AVG(e.suggestedValue),
            AVG(FUNCTION('DATE_PART', 'day', e.reviewedAt - e.submittedAt)),
            SUM(CASE WHEN e.status = 'APPROVED' THEN 1 ELSE 0 END) * 100.0 / COUNT(e)
        )
        FROM vehicle_evaluations e
        WHERE e.createdAt BETWEEN :startDate AND :endDate
        AND (:evaluatorId IS NULL OR e.evaluatorId = :evaluatorId)
        """)
    EvaluationKpiDto getKpis(@Param("startDate") LocalDateTime startDate,
                            @Param("endDate") LocalDateTime endDate,
                            @Param("evaluatorId") UUID evaluatorId);

    @Query("""
        SELECT NEW com.gestauto.vehicleevaluation.application.dto.MonthlyStatsDto(
            FUNCTION('DATE_TRUNC', 'month', e.createdAt),
            COUNT(e),
            SUM(e.suggestedValue)
        )
        FROM vehicle_evaluations e
        WHERE e.createdAt BETWEEN :startDate AND :endDate
        GROUP BY FUNCTION('DATE_TRUNC', 'month', e.createdAt)
        ORDER BY FUNCTION('DATE_TRUNC', 'month', e.createdAt)
        """)
    List<MonthlyStatsDto> getMonthlyStats(@Param("startDate") LocalDateTime startDate,
                                        @Param("endDate") LocalDateTime endDate);

    @Query("""
        SELECT NEW com.gestauto.vehicleevaluation.application.dto.BrandStatsDto(
            e.brand,
            COUNT(e),
            AVG(e.suggestedValue),
            SUM(CASE WHEN e.status = 'APPROVED' THEN 1 ELSE 0 END) * 100.0 / COUNT(e)
        )
        FROM vehicle_evaluations e
        WHERE e.createdAt BETWEEN :startDate AND :endDate
        GROUP BY e.brand
        ORDER BY COUNT(e) DESC
        """)
    List<BrandStatsDto> getBrandStats(@Param("startDate") LocalDateTime startDate,
                                    @Param("endDate") LocalDateTime endDate);
}
```

### Handler do Dashboard

```java
@Service
@Cacheable(value = "evaluation-dashboard", key = "{#query.startDate, #query.endDate, #query.evaluatorId}")
public class GetEvaluationDashboardHandler implements QueryHandler<GetEvaluationDashboardQuery, EvaluationDashboardDto> {
    private final VehicleEvaluationRepository evaluationRepository;
    private final MeterRegistry meterRegistry;

    @Override
    @Transactional(readOnly = true)
    public EvaluationDashboardDto handle(GetEvaluationDashboardQuery query) {
        Timer.Sample sample = Timer.start(meterRegistry);

        try {
            // 1. Calcular datas padrão se não informadas
            LocalDateTime endDate = query.endDate() != null ?
                query.endDate() : LocalDateTime.now();
            LocalDateTime startDate = query.startDate() != null ?
                query.startDate() : endDate.minusMonths(12);

            // 2. Obter KPIs principais
            EvaluationKpiDto kpis = evaluationRepository.getKpis(
                startDate, endDate, query.evaluatorId()
            );

            // 3. Obter estatísticas mensais
            List<MonthlyStatsDto> monthlyStats = evaluationRepository.getMonthlyStats(
                startDate, endDate
            );

            // 4. Obter estatísticas por marca
            List<BrandStatsDto> brandStats = evaluationRepository.getBrandStats(
                startDate, endDate
            );

            // 5. Obter estatísticas por avaliador
            List<EvaluatorStatsDto> evaluatorStats = query.evaluatorId() == null ?
                getEvaluatorStats(startDate, endDate) : Collections.emptyList();

            // 6. Montar dashboard
            return EvaluationDashboardDto.builder()
                .period(new PeriodDto(startDate, endDate))
                .kpis(kpis)
                .monthlyTrend(monthlyStats)
                .brandDistribution(brandStats)
                .evaluatorPerformance(evaluatorStats)
                .generatedAt(LocalDateTime.now())
                .build();

        } finally {
            sample.stop(Timer.builder("dashboard.query.duration").register(meterRegistry));
        }
    }
}
```

### Controller de Relatórios

```java
@RestController
@RequestMapping("/api/v1/evaluations/reports")
@PreAuthorize("hasRole('ADMIN')")
public class EvaluationReportsController {
    private final QueryBus queryBus;
    private final ReportExporter reportExporter;

    @GetMapping("/dashboard")
    public ResponseEntity<EvaluationDashboardDto> getDashboard(
            @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
            @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate,
            @RequestParam(required = false) UUID evaluatorId) {

        GetEvaluationDashboardQuery query = new GetEvaluationDashboardQuery(startDate, endDate, evaluatorId);
        EvaluationDashboardDto dashboard = queryBus.query(query);
        return ResponseEntity.ok(dashboard);
    }

    @GetMapping("/excel")
    public ResponseEntity<byte[]> exportExcel(
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate,
            @RequestParam(required = false) UUID evaluatorId) {

        byte[] excel = reportExporter.generateDetailedReport(startDate, endDate, evaluatorId);

        return ResponseEntity.ok()
            .header(HttpHeaders.CONTENT_TYPE, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            .header(HttpHeaders.CONTENT_DISPOSITION,
                "attachment; filename=vehicle_evaluations_" + LocalDate.now() + ".xlsx")
            .body(excel);
    }

    @GetMapping("/summary-pdf")
    public ResponseEntity<byte[]> exportSummaryPdf(
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate) {

        byte[] pdf = reportExporter.generateSummaryReport(startDate, endDate);

        return ResponseEntity.ok()
            .header(HttpHeaders.CONTENT_TYPE, MediaType.APPLICATION_PDF_VALUE)
            .header(HttpHeaders.CONTENT_DISPOSITION,
                "attachment; filename=evaluation_summary_" + LocalDate.now() + ".pdf")
            .body(pdf);
    }
}
```

### Exportador Excel

```java
@Service
public class ExcelReportExporter {

    public byte[] generateDetailedReport(LocalDateTime startDate, LocalDateTime endDate, UUID evaluatorId) {
        try (Workbook workbook = new XSSFWorkbook()) {
            // 1. Sheet de resumo
            Sheet summarySheet = workbook.createSheet("Resumo");
            createSummarySheet(summarySheet, startDate, endDate, evaluatorId);

            // 2. Sheet de avaliações detalhadas
            Sheet evaluationsSheet = workbook.createSheet("Avaliações");
            createEvaluationsSheet(evaluationsSheet, startDate, endDate, evaluatorId);

            // 3. Sheet de estatísticas
            Sheet statsSheet = workbook.createSheet("Estatísticas");
            createStatisticsSheet(statsSheet, startDate, endDate, evaluatorId);

            // 4. Sheet de gráficos (opcional)
            // Sheet chartsSheet = workbook.createSheet("Gráficos");

            // Auto-size columns
            for (int i = 0; i < workbook.getNumberOfSheets(); i++) {
                autoSizeColumns(workbook.getSheetAt(i));
            }

            // Write to byte array
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            workbook.write(baos);
            return baos.toByteArray();

        } catch (Exception e) {
            throw new ReportGenerationException("Failed to generate Excel report", e);
        }
    }

    private void createEvaluationsSheet(Sheet sheet, LocalDateTime startDate, LocalDateTime endDate, UUID evaluatorId) {
        // Header row
        Row headerRow = sheet.createRow(0);
        String[] headers = {"ID", "Placa", "Marca", "Modelo", "Ano", "KM", "Avaliador", "Status",
                          "Valor FIPE", "Depreciação", "Valor Sugerido", "Valor Aprovado", "Data Criação", "Data Aprovação"};

        for (int i = 0; i < headers.length; i++) {
            Cell cell = headerRow.createCell(i);
            cell.setCellValue(headers[i]);
            // Style header...
        }

        // Data rows
        List<VehicleEvaluation> evaluations = evaluationRepository.findByDateRangeAndEvaluator(
            startDate, endDate, evaluatorId
        );

        int rowNum = 1;
        for (VehicleEvaluation eval : evaluations) {
            Row row = sheet.createRow(rowNum++);
            populateEvaluationRow(row, eval);
        }
    }
}
```

### Endpoint Público de Validação

```java
@GetMapping("/public/validate/{token}")
public ResponseEntity<EvaluationValidationDto> validateEvaluationPublic(@PathVariable String token) {
    try {
        // 1. Validar token JWT
        Claims claims = jwtTokenValidator.validateToken(token);
        UUID evaluationId = UUID.fromString(claims.getSubject());

        // 2. Buscar avaliação
        VehicleEvaluation evaluation = evaluationRepository.findById(
            EvaluationId.from(evaluationId)
        ).orElseThrow(() -> new EntityNotFoundException("Evaluation not found"));

        // 3. Validar se ainda está vigente
        if (evaluation.getValidUntil().isBefore(LocalDateTime.now())) {
            return ResponseEntity.notFound().build();
        }

        // 4. Retornar dados públicos
        return ResponseEntity.ok(EvaluationValidationDto.builder()
            .plate(evaluation.getPlate().getValue())
            .brand(evaluation.getVehicleInfo().brand())
            .model(evaluation.getVehicleInfo().model())
            .year(evaluation.getVehicleInfo().year())
            .status(evaluation.getStatus().name())
            .approvedValue(evaluation.getApprovedValue())
            .validatedAt(LocalDateTime.now())
            .build());

    } catch (JwtException e) {
        return ResponseEntity.status(HttpStatus.UNAUTHORIZED).build();
    }
}
```

## Critérios de Sucesso

- [x] KPIs calculados corretamente
- [x] Dashboard carrega em < 2s
- [x] Filtros funcionando
- [x] Exportação Excel funciona
- [x] Cache Redis ativo
- [x] Validação pública funcional
- [x] Gráficos gerados
- [x] Métricas de performance coletadas

## Sequenciamento

- Bloqueado por: 2.0, 7.0
- Desbloqueia: 13.0
- Paralelizável: Sim

## Tempo Estimado

- Queries agregadas: 8 horas
- Dashboard APIs: 6 horas
- Excel export: 6 horas
- Caching: 4 horas
- Testes: 4 horas
- Total: 28 horas