## markdown

## status: completed

<task_context>
<domain>engine</domain>
<type>integration</type>
<scope>middleware</scope>
<complexity>medium</complexity>
<dependencies>temporal</dependencies>
</task_context>

# Tarefa 10.0: Implementação de Eventos de Domínio e RabbitMQ

## Visão Geral

Implementar sistema de eventos de domínio para integração assíncrona com outros bounded contexts através do RabbitMQ, incluindo publicação de eventos críticos, tratamento de falhas, e configuração de exchanges e routing keys.

<requirements>
- Exchange `gestauto.events` do tipo topic
- Eventos: Created, Approved, Rejected, Completed
- Integração com bounded context Commercial
- Schema JSON padronizado para eventos
- Retry com exponential backoff
- Dead letter queue para falhas
- Event idempotentes
- Logging de eventos publicados

</requirements>

## Subtarefas

- [ ] 10.1 Configurar RabbitMQ connection e exchange
- [ ] 10.2 Implementar eventos de domínio
- [ ] 10.3 Criar RabbitMQEventPublisher
- [ ] 10.4 Implementar tratamento de eventos na entidade
- [ ] 10.5 Configurar DLQ e retry policies
- [ ] 10.6 Criar schema JSON para eventos
- [ ] 10.7 Implementar serialização/desserialização
- [ ] 10.8 Adicionar logging estruturado
- [ ] 10.9 Testar integração com bounded contexts

## Detalhes de Implementação

### Configuração RabbitMQ

```java
@Configuration
@EnableRabbit
public class RabbitMQConfig {

    @Value("${spring.rabbitmq.template.exchange}")
    private String exchange;

    @Bean
    public TopicExchange gestautoEventsExchange() {
        return new TopicExchange(exchange, true, false);
    }

    @Bean
    public Queue vehicleEvaluationQueue() {
        return QueueBuilder.durable("vehicle-evaluation.events")
            .withArgument("x-dead-letter-exchange", exchange + ".dlq")
            .withArgument("x-dead-letter-routing-key", "vehicle.evaluation.failed")
            .build();
    }

    @Bean
    public Queue vehicleEvaluationDlq() {
        return QueueBuilder.durable("vehicle-evaluation.events.dlq")
            .build();
    }

    @Bean
    public Binding vehicleEvaluationBinding() {
        return BindingBuilder.bind(vehicleEvaluationQueue())
            .to(gestautoEventsExchange())
            .with("vehicle.evaluation.*");
    }

    @Bean
    public RabbitTemplate rabbitTemplate(ConnectionFactory connectionFactory) {
        RabbitTemplate template = new RabbitTemplate(connectionFactory);
        template.setExchange(exchange);
        template.setMessageConverter(new Jackson2JsonMessageConverter());
        template.setRetryTemplate(retryTemplate());
        return template;
    }

    @Bean
    public RetryTemplate retryTemplate() {
        return RetryTemplate.builder()
            .exponentialBackoff(1000, 2, 10)
            .maxAttempts(3)
            .retryOn(Exception.class)
            .build();
    }
}
```

### Eventos de Domínio

```java
public abstract class DomainEvent {
    private final UUID id = UUID.randomUUID();
    private final LocalDateTime occurredAt = LocalDateTime.now();
    private final String eventType;

    protected DomainEvent(String eventType) {
        this.eventType = eventType;
    }

    // Getters...
}

@JsonTypeInfo(use = JsonTypeInfo.Id.NAME, include = JsonTypeInfo.As.PROPERTY, property = "eventType")
@JsonSubTypes({
    @JsonSubTypes.Type(value = EvaluationCreatedEvent.class, name = "evaluation.created"),
    @JsonSubTypes.Type(value = EvaluationApprovedEvent.class, name = "evaluation.approved"),
    @JsonSubTypes.Type(value = EvaluationRejectedEvent.class, name = "evaluation.rejected"),
    @JsonSubTypes.Type(value = VehicleEvaluationCompletedEvent.class, name = "vehicle.evaluation.completed")
})
public class EvaluationCreatedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final UUID evaluatorId;
    private final String plate;
    private final String brand;
    private final String model;
    private final int year;
    private final Map<String, Object> metadata;

    public EvaluationCreatedEvent(UUID evaluationId, UUID evaluatorId, String plate,
                                String brand, String model, int year) {
        super("evaluation.created");
        this.evaluationId = evaluationId;
        this.evaluatorId = evaluatorId;
        this.plate = plate;
        this.brand = brand;
        this.model = model;
        this.year = year;
        this.metadata = new HashMap<>();
    }

    @JsonCreator
    public EvaluationCreatedEvent(
        @JsonProperty("occurredAt") LocalDateTime occurredAt,
        @JsonProperty("evaluationId") UUID evaluationId,
        @JsonProperty("evaluatorId") UUID evaluatorId,
        @JsonProperty("plate") String plate,
        @JsonProperty("brand") String brand,
        @JsonProperty("model") String model,
        @JsonProperty("year") int year,
        @JsonProperty("metadata") Map<String, Object> metadata
    ) {
        super("evaluation.created");
        this.evaluationId = evaluationId;
        this.evaluatorId = evaluatorId;
        this.plate = plate;
        this.brand = brand;
        this.model = model;
        this.year = year;
        this.metadata = metadata != null ? metadata : new HashMap<>();
    }
}
```

### Publisher de Eventos

```java
@Service
@Slf4j
public class RabbitMQEventPublisher implements EventPublisher {
    private final RabbitTemplate rabbitTemplate;
    private final MeterRegistry meterRegistry;

    @Override
    @Async
    public void publishEvent(DomainEvent event) {
        try {
            String routingKey = getRoutingKey(event);

            rabbitTemplate.convertAndSend(routingKey, event, message -> {
                message.getMessageProperties().setTimestamp(Timestamp.from(event.getOccurredAt().atZone(ZoneId.systemDefault()).toInstant()));
                message.getMessageProperties().setHeader("eventId", event.getId().toString());
                message.getMessageProperties().setHeader("eventType", event.getEventType());
                return message;
            });

            meterRegistry.counter("events.published", "type", event.getEventType()).increment();
            log.info("Event published successfully: {} - {}", event.getEventType(), event.getId());

        } catch (Exception e) {
            meterRegistry.counter("events.failed", "type", event.getEventType()).increment();
            log.error("Failed to publish event: {} - {}", event.getEventType(), event.getId(), e);
            throw new EventPublishingException("Failed to publish event: " + event.getEventType(), e);
        }
    }

    private String getRoutingKey(DomainEvent event) {
        return switch (event.getEventType()) {
            case "evaluation.created" -> "vehicle.evaluation.created";
            case "evaluation.submitted" -> "vehicle.evaluation.submitted";
            case "evaluation.approved" -> "vehicle.evaluation.approved";
            case "evaluation.rejected" -> "vehicle.evaluation.rejected";
            case "vehicle.evaluation.completed" -> "vehicle.evaluation.completed";
            default -> "vehicle.evaluation." + event.getEventType();
        };
    }
}
```

### Eventos na Entidade

```java
public class VehicleEvaluation {
    private final List<DomainEvent> domainEvents = new ArrayList<>();

    public void approve(ReviewerId reviewerId, Money adjustedValue) {
        if (!canApprove()) {
            throw new BusinessException("Cannot approve evaluation in current status");
        }

        this.status = EvaluationStatus.APPROVED;
        this.reviewerId = reviewerId;
        this.reviewedAt = LocalDateTime.now();
        this.approvedValue = adjustedValue != null ? adjustedValue : this.suggestedValue;
        generateValidationToken();

        // Adicionar evento de domínio
        addDomainEvent(new EvaluationApprovedEvent(
            this.id,
            reviewerId.getValue(),
            this.approvedValue.getValue(),
            this.validationToken,
            this.validUntil
        ));

        // Se valor final foi definido, publicar evento completo
        if (this.approvedValue != null) {
            addDomainEvent(new VehicleEvaluationCompletedEvent(
                this.id,
                this.plate.getValue(),
                this.vehicleInfo.brand(),
                this.vehicleInfo.model(),
                this.vehicleInfo.year(),
                this.approvedValue.getValue(),
                this.validUntil,
                buildEvaluationData()
            ));
        }
    }

    private void addDomainEvent(DomainEvent event) {
        this.domainEvents.add(event);
    }

    public List<DomainEvent> getUncommittedEvents() {
        return Collections.unmodifiableList(domainEvents);
    }

    public void markEventsAsCommitted() {
        domainEvents.clear();
    }
}
```

### Listener de Eventos (para outros bounded contexts)

```java
@RabbitListener(queues = "vehicle-evaluation.events")
@Component
public class VehicleEvaluationEventListener {

    @RabbitHandler
    public void handleEvaluationApproved(EvaluationApprovedEvent event) {
        // Integrar com bounded context Commercial
        // Por exemplo: criar proposta para este veículo
        log.info("Processing evaluation approved event: {}", event.getEvaluationId());
        // Integration logic here...
    }

    @RabbitHandler
    public void handleVehicleEvaluationCompleted(VehicleEvaluationCompletedEvent event) {
        // Integrar com sistema de estoque
        log.info("Processing vehicle evaluation completed: {}", event.getPlate());
        // Add vehicle to inventory...
    }

    @RabbitListener(queues = "vehicle-evaluation.events.dlq")
    public void handleFailedEvent(Message failedMessage) {
        log.error("Processing failed event: {}", failedMessage.getMessageProperties().getMessageId());
        // Implementar lógica de reprocessamento manual
    }
}
```

### Configuration

```yaml
spring:
  rabbitmq:
    host: localhost
    port: 5672
    username: gestauto
    password: gestauto123
    virtual-host: /
    template:
      exchange: gestauto.events
      routing-key: vehicle.evaluation
    publisher-confirm-type: correlated
    publisher-returns: true
    listener:
      simple:
        acknowledge-mode: manual
        retry:
          enabled: true
          max-attempts: 3
          initial-interval: 1000
          multiplier: 2.0
```

## Critérios de Sucesso

- [x] Exchange gestauto.events criado
- [x] Eventos publicados corretamente
- [x] Schema JSON padronizado
- [x] DLQ configurada para falhas
- [x] Retry com exponential backoff
- [x] Logs de eventos públicos
- [x] Integração com Commercial
- [x] Eventos idempotentes
- [x] Métricas de publicação

## Sequenciamento

- Bloqueado por: 1.0, 7.0, 9.0
- Desbloqueia: 11.0, 13.0
- Paralelizável: Sim (com 8.0 e 9.0)

## Tempo Estimado

- RabbitMQ setup: 4 horas
- Eventos de domínio: 6 horas
- Publisher implementation: 6 horas
- Testing/Integration: 4 horas
- Total: 20 horas