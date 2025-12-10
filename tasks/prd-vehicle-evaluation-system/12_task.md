## markdown

## status: pending

<task_context>
<domain>engine</domain>
<type>testing</type>
<scope>performance</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
</task_context>

# Tarefa 12.0: Testes Abrangentes e Validação

## Visão Geral

Implementar suíte completa de testes unitários, de integração e de performance para garantir qualidade e estabilidade do microserviço, incluindo TestContainers para testes de integração, WireMock para APIs externas, e testes de carga.

<requirements>
- Cobertura de testes > 90%
- TestContainers para PostgreSQL e RabbitMQ
- WireMock para FIPE API
- Testes de carga (JMeter/Gatling)
- Testes de contrato (Pact)
- Testes de mutação (PIT)
- Validação de performance
- Documentação de testes

</requirements>

## Subtarefas

- [ ] 12.1 Configurar TestContainers (PostgreSQL, RabbitMQ)
- [ ] 12.2 Implementar WireMock server para FIPE API
- [ ] 12.3 Criar testes unitários das entidades de domínio
- [ ] 12.4 Implementar testes de handlers (CQRS)
- [ ] 12.5 Criar testes de integração dos controllers
- [ ] 12.6 Implementar testes de repository pattern
- [ ] 12.7 Adicionar testes de carga com JMeter
- [ ] 12.8 Implementar testes de contrato
- [ ] 12.9 Configurar relatórios de cobertura

## Detalhes de Implementação

### Configuração TestContainers

```java
@SpringBootTest
@Testcontainers
@TestMethodOrder(OrderAnnotation.class)
class VehicleEvaluationIntegrationTest {

    @Container
    static PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:15")
        .withDatabaseName("testdb")
        .withUsername("test")
        .withPassword("test")
        .withDatabaseName("vehicle_evaluation_test");

    @Container
    static RabbitMQContainer rabbitmq = new RabbitMQContainer("rabbitmq:3.11")
        .withExchange("gestauto.events", "topic", false);

    @DynamicPropertySource
    static void configureProperties(DynamicPropertyRegistry registry) {
        registry.add("spring.datasource.url", postgres::getJdbcUrl);
        registry.add("spring.datasource.username", postgres::getUsername);
        registry.add("spring.datasource.password", postgres::getPassword);
        registry.add("spring.datasource.hikari.maximum-pool-size", () -> "5");
        registry.add("spring.rabbitmq.host", rabbitmq::getHost);
        registry.add("spring.rabbitmq.port", rabbitmq::getAmqpPort);
        registry.add("spring.rabbitmq.username", rabbitmq::getAdminUsername);
        registry.add("spring.rabbitmq.password", rabbitmq::getAdminPassword);
    }

    @BeforeEach
    void setUp(@Autowired Flyway flyway) {
        // Limpar e migrar database para cada teste
        flyway.clean();
        flyway.migrate();
    }
}
```

### WireMock para FIPE API

```java
@TestConfiguration
public class WireMockConfig {

    @Bean(initMethod = "start", destroyMethod = "stop")
    public WireMockServer wireMockServer() {
        return new WireMockServer(options()
            .port(8089)
            .usingFilesUnderDirectory("src/test/resources/wiremock"));
    }

    @Bean
    @Primary
    public FipeApiClient mockFipeApiClient(WireMockServer wireMockServer) {
        WebClient webClient = WebClient.builder()
            .baseUrl(wireMockServer.baseUrl())
            .build();
        return new FipeApiClient(webClient);
    }
}
```

### Mappings WireMock

```json
{
  "request": {
    "method": "GET",
    "url": "/fipe/api/v1/carros/marcas"
  },
  "response": {
    "status": 200,
    "headers": {
      "Content-Type": "application/json"
    },
    "bodyFileName": "fipe/marcas.json"
  }
}

{
  "request": {
    "method": "GET",
    "urlPattern": "/fipe/api/v1/carros/marcas/(\\d+)/modelos"
  },
  "response": {
    "status": 200,
    "headers": {
      "Content-Type": "application/json"
    },
    "bodyFileName": "fipe/modelos_vw.json"
  }
}
```

### Testes de Entidade de Domínio

```java
@ExtendWith(MockitoExtension.class)
class VehicleEvaluationTest {

    @Test
    @DisplayName("Should create evaluation with valid data")
    void shouldCreateEvaluationWithValidData() {
        // Given
        EvaluatorId evaluatorId = EvaluatorId.from(UUID.randomUUID());
        Plate plate = Plate.from("ABC1234");
        FipeCode fipeCode = FipeCode.from("008001-0");
        VehicleInfo vehicleInfo = new VehicleInfo("Volkswagen", "Gol", "1.0", 2022);
        Money mileage = Money.of(new BigDecimal("50000"));

        // When
        VehicleEvaluation evaluation = VehicleEvaluation.create(
            evaluatorId, plate, fipeCode, vehicleInfo, mileage
        );

        // Then
        assertThat(evaluation.getId()).isNotNull();
        assertThat(evaluation.getEvaluatorId()).isEqualTo(evaluatorId);
        assertThat(evaluation.getStatus()).isEqualTo(EvaluationStatus.DRAFT);
        assertThat(evaluation.getCreatedAt()).isNotNull();
        assertThat(evaluation.getDomainEvents()).hasSize(1);
        assertThat(evaluation.getDomainEvents().get(0))
            .isInstanceOf(EvaluationCreatedEvent.class);
    }

    @Test
    @DisplayName("Should calculate valuation correctly")
    void shouldCalculateValuationCorrectly() {
        // Given
        VehicleEvaluation evaluation = createTestEvaluation();
        Money fipePrice = Money.of(new BigDecimal("50000.00"));

        // When
        evaluation.calculateValuation(fipePrice, createDepreciationRules());

        // Then
        assertThat(evaluation.getFipePrice()).isEqualTo(fipePrice);
        assertThat(evaluation.getDepreciationAmount()).isGreaterThan(Money.ZERO);
        assertThat(evaluation.getSafetyMargin()).isEqualTo(fipePrice.percentage(10));
        assertThat(evaluation.getProfitMargin()).isEqualTo(fipePrice.percentage(15));
        assertThat(evaluation.getSuggestedValue())
            .isEqualTo(fipePrice.subtract(evaluation.getDepreciationAmount())
                .add(evaluation.getSafetyMargin())
                .add(evaluation.getProfitMargin()));
    }

    @Test
    @DisplayName("Should throw exception when approving invalid status")
    void shouldThrowExceptionWhenApprovingInvalidStatus() {
        // Given
        VehicleEvaluation evaluation = createTestEvaluation();
        evaluation.setStatus(EvaluationStatus.DRAFT);

        // When/Then
        assertThatThrownBy(() ->
            evaluation.approve(ReviewerId.from(UUID.randomUUID()), null)
        ).isInstanceOf(BusinessException.class)
         .hasMessage("Cannot approve evaluation in current status");
    }
}
```

### Teste de Integração Completo

```java
@SpringBootTest
@Testcontainers
@Transactional
class VehicleEvaluationE2ETest {

    @Autowired
    private VehicleEvaluationController controller;

    @MockBean
    private FipeService fipeService;

    @MockBean
    private ImageStorageService imageStorageService;

    @MockBean
    private EventPublisher eventPublisher;

    @Test
    @DisplayName("Should process complete evaluation workflow")
    void shouldProcessCompleteEvaluationWorkflow() throws Exception {
        // 1. Mock external services
        when(fipeService.getCurrentPrice(anyString(), anyInt()))
            .thenReturn(Optional.of(new BigDecimal("50000.00")));

        when(imageStorageService.uploadEvaluationPhotos(any(), any()))
            .thenReturn(new ImageUploadResult(Map.of(), Map.of()));

        // 2. Create evaluation
        CreateEvaluationCommand createCmd = new CreateEvaluationCommand(
            "ABC1234", 2022, 50000, "Branco", "1.0", "Flex", "Manual",
            List.of("Ar Condicionado"), null
        );

        ResponseEntity<UUID> createResponse = controller.createEvaluation(createCmd);
        UUID evaluationId = createResponse.getBody();

        // 3. Add photos
        Map<String, MultipartFile> photos = createMockPhotos();
        controller.addPhotos(evaluationId, photos);

        // 4. Update checklist
        EvaluationChecklistDto checklist = createMockChecklist();
        controller.updateChecklist(evaluationId, new UpdateChecklistCommand(evaluationId, checklist));

        // 5. Calculate valuation
        ResponseEntity<Void> calcResponse = controller.calculateValuation(evaluationId);
        assertThat(calcResponse.getStatusCode()).isEqualTo(HttpStatus.OK);

        // 6. Submit for approval
        ResponseEntity<Void> submitResponse = controller.submitForApproval(evaluationId);
        assertThat(submitResponse.getStatusCode()).isEqualTo(HttpStatus.OK);

        // 7. Approve evaluation (as manager)
        ApproveEvaluationCommand approveCmd = new ApproveEvaluationCommand(null);
        ResponseEntity<Void> approveResponse = approvalController.approveEvaluation(evaluationId, approveCmd);
        assertThat(approveResponse.getStatusCode()).isEqualTo(HttpStatus.OK);

        // 8. Verify final state
        VehicleEvaluationDto evaluation = controller.getEvaluation(evaluationId).getBody();
        assertThat(evaluation.getStatus()).isEqualTo("APPROVED");
        assertThat(evaluation.getApprovedValue()).isNotNull();

        // 9. Verify events published
        verify(eventPublisher, atLeast(4)).publishEvent(any(DomainEvent.class));
    }
}
```

### Teste de Carga com JMeter

```xml
<!-- VehicleEvaluationLoadTest.jmx -->
<jmeterTestPlan version="1.2">
  <hashTree>
    <TestPlan guiclass="TestPlanGui" testclass="TestPlan">
      <stringProp name="TestPlan.name">Vehicle Evaluation Load Test</stringProp>
      <boolProp name="TestPlan.functional_mode">false</boolProp>
      <boolProp name="TestPlan.serialize_threadgroups">true</boolProp>
      <elementProp name="TestPlan.user_defined_variables" elementType="Arguments">
        <collectionProp name="Arguments.arguments">
          <elementProp name="HOST" elementType="Argument">
            <stringProp name="Argument.name">HOST</stringProp>
            <stringProp name="Argument.value">localhost</stringProp>
          </elementProp>
          <elementProp name="PORT" elementType="Argument">
            <stringProp name="Argument.name">PORT</stringProp>
            <stringProp name="Argument.value">8080</stringProp>
          </elementProp>
        </collectionProp>
      </elementProp>
    </TestPlan>
    <hashTree>
      <ThreadGroup guiclass="ThreadGroupGui" testclass="ThreadGroup">
        <stringProp name="ThreadGroup.name">Evaluators</stringProp>
        <intProp name="ThreadGroup.num_threads">50</intProp>
        <intProp name="ThreadGroup.ramp_time">10</intProp>
        <boolProp name="ThreadGroup.scheduler">false</boolProp>
        <stringProp name="ThreadGroup.duration"></stringProp>
        <stringProp name="ThreadGroup.delay"></stringProp>
      </ThreadGroup>
      <hashTree>
        <!-- Create evaluation -->
        <HTTPSamplerProxy guiclass="HttpTestSampleGui" testclass="HTTPSamplerProxy">
          <stringProp name="HTTPSampler.domain">${HOST}</stringProp>
          <stringProp name="HTTPSampler.port">${PORT}</stringProp>
          <stringProp name="HTTPSampler.path">/api/v1/evaluations</stringProp>
          <stringProp name="HTTPSampler.method">POST</stringProp>
          <boolProp name="HTTPSampler.follow_redirects">true</boolProp>
          <boolProp name="HTTPSampler.use_keepalive">true</boolProp>
        </HTTPSamplerProxy>
        <!-- ... outros testes ... -->
      </hashTree>
    </hashTree>
  </hashTree>
</jmeterTestPlan>
```

### Configuração de Cobertura

```xml
<plugin>
    <groupId>org.jacoco</groupId>
    <artifactId>jacoco-maven-plugin</artifactId>
    <version>0.8.8</version>
    <executions>
        <execution>
            <id>prepare-agent</id>
            <goals>
                <goal>prepare-agent</goal>
            </goals>
        </execution>
        <execution>
            <id>report</id>
            <phase>test</phase>
            <goals>
                <goal>report</goal>
            </goals>
        </execution>
        <execution>
            <id>check</id>
            <goals>
                <goal>check</goal>
            </goals>
            <configuration>
                <rules>
                    <rule>
                        <element>BUNDLE</element>
                        <limits>
                            <limit>
                                <counter>INSTRUCTION</counter>
                                <value>COVEREDRATIO</value>
                                <minimum>0.90</minimum>
                            </limit>
                        </limits>
                    </rule>
                </rules>
            </configuration>
        </execution>
    </executions>
</plugin>
```

## Critérios de Sucesso

- [x] Cobertura > 90% alcançada
- [x] Todos os testes de integração passando
- [x] Testes de carga simulam 50 usuários simultâneos
- [x] Performance < 2s para 95% das requisições
- [x] WireMock mocks funcionando
- [x] TestContainers configurados
- [x] Relatórios de cobertura gerados
- [x] Testes de contratos implementados

## Sequenciamento

- Bloqueado por: Todas as tarefas anteriores
- Desbloqueia: 13.0
- Paralelizável: Não (última fase de testes)

## Tempo Estimado

- Configuração TestContainers: 6 horas
- Testes unitários: 16 horas
- Testes de integração: 10 horas
- Testes de carga: 8 horas
- Relatórios: 4 horas
- Total: 44 horas