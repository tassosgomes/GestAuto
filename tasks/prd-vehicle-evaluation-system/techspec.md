# Especificação Técnica - Microserviço de Avaliação de Veículos Seminovos

## Resumo Executivo

Esta especificação técnica detalha a implementação do microserviço de Avaliação de Veículos Seminovos (GestAuto.VehicleEvaluation) seguindo os padrões de arquitetura limpa com DDD já estabelecidos no projeto GestAuto. A solução será implementada como um bounded context independente utilizando **Repository Pattern com Mappers** para manter entidades de domínio puras (sem annotations JPA), CQRS com Spring Events, eventos de domínio para integração assíncrona via RabbitMQ, **PostgreSQL existente** como banco de dados principal e Cloudflare R2 para armazenamento de imagens. O serviço seguirá a estrutura padrão de 5 camadas (1-Services, 2-Application, 3-Domain, 4-Infra, 5-Tests) adaptada para o ecossistema Java.

## Abordagem de Persistência: Repository Pattern com Mappers

A implementação adota o **Repository Pattern com Mappers** para garantir separação completa entre o domínio e a infraestrutura de persistência:

1. **Domínio Puro**: Entidades sem annotations JPA, focadas apenas em regras de negócio
2. **Infraestrutura Separada**: Entidades JPA dedicadas para persistência
3. **Mappers**: Componentes especializados para conversão entre domínio e infraestrutura
4. **Repositories**: Interfaces no domínio com implementações na infraestrutura

Essa abordagem mantém o domínio completamente isolado de preocupações de persistência, facilitando testes e evolução do modelo de domínio.

## Arquitetura do Sistema

### Visão Geral dos Componentes

O microserviço será composto pelos seguintes componentes principais:

- **API Layer**: Endpoints REST com Spring MVC para consumo por interfaces web/mobile
- **Application Layer**: Commands/Queries CQRS com validações e handlers
- **Domain Layer**: Entidades de domínio, value objects, eventos e regras de negócio
- **Infrastructure Layer**: Repositórios, integrações externas (FIPE, R2) e configurações
- **Messaging**: Publicação de eventos para integração com outros bounded contexts

Fluxo principal: Controller → Command → Handler → Domain → Repository → Database + Event Publishing

## Design de Implementação

### Interfaces Principais

#### Interfaces de Domínio

```java
// Repositório principal (INTERFACE NO DOMÍNIO - PURA)
public interface VehicleEvaluationRepository {
    Optional<VehicleEvaluation> findById(EvaluationId id);
    Optional<VehicleEvaluation> findByPlate(Plate plate);
    List<VehicleEvaluation> findByStatus(EvaluationStatus status);
    List<VehicleEvaluation> findByEvaluatorId(EvaluatorId evaluatorId);
    Page<VehicleEvaluation> findPendingApprovals(EvaluationStatus status, Pageable pageable);
    VehicleEvaluation save(VehicleEvaluation evaluation);
    void deleteById(EvaluationId id);
    boolean existsByPlate(Plate plate);
}

// Repositório de fotos
public interface EvaluationPhotoRepository {
    List<EvaluationPhoto> findByEvaluationId(EvaluationId evaluationId);
    EvaluationPhoto save(EvaluationPhoto photo);
    void deleteByEvaluationId(EvaluationId evaluationId);
}

// Repositório de checklist
public interface EvaluationChecklistRepository {
    Optional<EvaluationChecklist> findByEvaluationId(EvaluationId evaluationId);
    EvaluationChecklist save(EvaluationChecklist checklist);
    void deleteByEvaluationId(EvaluationId evaluationId);
}

// Serviço de integração FIPE
@Service
public interface FipeService {
    Optional<FipeVehicleInfo> getVehicleInfo(String fipeCode);
    Optional<BigDecimal> getCurrentPrice(String fipeCode, int year);
    List<FipeBrand> getBrands();
    List<FipeModel> getModelsByBrand(String brandId);
}

// Serviço de armazenamento de imagens
@Service
public interface ImageStorageService {
    String uploadImage(InputStream imageStream, String fileName, String contentType);
    boolean deleteImage(String imageUrl);
    ImageUploadResult uploadEvaluationPhotos(UUID evaluationId, Map<String, InputStream> photos);
}

// Serviço de geração de PDF
@Service
public interface ReportService {
    byte[] generateEvaluationReport(VehicleEvaluation evaluation);
    String getValidationUrl(UUID evaluationId, String accessToken);
}

// Publicador de eventos
@Component
public interface EventPublisher {
    void publishEvent(DomainEvent event);
}
```

### Modelos de Dados

#### Entidades de Domínio Principais (SEUS ANNOTATIONS JPA)

```java
// Entidade principal de DOMÍNIO - PURA
public class VehicleEvaluation {
    private final EvaluationId id;
    private final EvaluatorId evaluatorId;
    private final Plate plate;
    private final FipeCode fipeCode;
    private final VehicleInfo vehicleInfo;
    private final Money mileage;
    private EvaluationStatus status;
    private Money fipePrice;
    private Money depreciationAmount;
    private Money safetyMargin;
    private Money profitMargin;
    private Money suggestedValue;
    private Money approvedValue;
    private final LocalDateTime createdAt;
    private LocalDateTime updatedAt;
    private LocalDateTime submittedAt;
    private LocalDateTime reviewedAt;
    private ReviewerId reviewerId;
    private LocalDateTime validUntil;
    private String validationToken;
    private String rejectionReason;
    private String internalNotes;
    private String externalNotes;

    // Coleções de objetos de domínio
    private final List<EvaluationPhoto> photos;
    private final List<DepreciationItem> depreciations;
    private final List<EvaluationAccessory> accessories;
    private EvaluationChecklist checklist;
    private final List<DomainEvent> domainEvents;

    // Constructor privado
    private VehicleEvaluation(EvaluatorId evaluatorId, Plate plate, FipeCode fipeCode,
                             VehicleInfo vehicleInfo, Money mileage) {
        this.id = EvaluationId.generate();
        this.evaluatorId = evaluatorId;
        this.plate = plate;
        this.fipeCode = fipeCode;
        this.vehicleInfo = vehicleInfo;
        this.mileage = mileage;
        this.status = EvaluationStatus.DRAFT;
        this.createdAt = LocalDateTime.now();
        this.photos = new ArrayList<>();
        this.depreciations = new ArrayList<>();
        this.accessories = new ArrayList<>();
        this.domainEvents = new ArrayList<>();
    }

    // Factory method
    public static VehicleEvaluation create(EvaluatorId evaluatorId, Plate plate, FipeCode fipeCode,
                                          VehicleInfo vehicleInfo, Money mileage) {
        return new VehicleEvaluation(evaluatorId, plate, fipeCode, vehicleInfo, mileage);
    }

    // Métodos de domínio
    public void addPhoto(PhotoType type, String url, String thumbnailUrl) {
        EvaluationPhoto photo = EvaluationPhoto.create(this.id, type, url, thumbnailUrl);
        this.photos.add(photo);
        addDomainEvent(new PhotosUploadedEvent(this.id, type, url));
    }

    public void updateChecklist(EvaluationChecklist checklist) {
        this.checklist = checklist;
        if (checklist.hasBlockingIssues()) {
            throw new BusinessException("Evaluation has blocking issues in checklist");
        }
        this.updatedAt = LocalDateTime.now();
        addDomainEvent(new ChecklistCompletedEvent(this.id, checklist.calculateScore()));
    }

    public void calculateValuation(Money fipePrice, List<DepreciationRule> rules) {
        this.fipePrice = fipePrice;
        Money totalDepreciation = calculateTotalDepreciation(rules);
        this.depreciationAmount = totalDepreciation;
        this.safetyMargin = fipePrice.percentage(10); // 10%
        this.profitMargin = fipePrice.percentage(15); // 15%
        this.suggestedValue = fipePrice.subtract(totalDepreciation)
                                      .add(this.safetyMargin)
                                      .add(this.profitMargin);
        this.updatedAt = LocalDateTime.now();

        addDomainEvent(new ValuationCalculatedEvent(this.id, fipePrice, this.suggestedValue));
    }

    public void submitForApproval() {
        validateCanSubmit();
        this.status = EvaluationStatus.PENDING_APPROVAL;
        this.submittedAt = LocalDateTime.now();
        addDomainEvent(new EvaluationSubmittedEvent(this.id, this.evaluatorId));
    }

    public void approve(ReviewerId reviewerId, Money adjustedValue) {
        if (!canApprove()) {
            throw new BusinessException("Cannot approve evaluation in current status");
        }
        this.status = EvaluationStatus.APPROVED;
        this.reviewerId = reviewerId;
        this.reviewedAt = LocalDateTime.now();
        this.approvedValue = adjustedValue != null ? adjustedValue : this.suggestedValue;
        generateValidationToken();

        addDomainEvent(new EvaluationApprovedEvent(
            this.id, reviewerId, this.approvedValue, this.validationToken, this.validUntil
        ));
    }

    public void reject(ReviewerId reviewerId, String reason) {
        this.status = EvaluationStatus.REJECTED;
        this.reviewerId = reviewerId;
        this.reviewedAt = LocalDateTime.now();
        this.rejectionReason = reason;

        addDomainEvent(new EvaluationRejectedEvent(this.id, reviewerId, reason));
    }

    // Métodos privados
    private void validateCanSubmit() {
        if (photos.isEmpty()) {
            throw new BusinessException("Evaluation must have at least one photo");
        }
        if (checklist == null || !checklist.isComplete()) {
            throw new BusinessException("Checklist must be completed");
        }
    }

    private boolean canApprove() {
        return status == EvaluationStatus.PENDING_APPROVAL;
    }

    private void generateValidationToken() {
        this.validUntil = LocalDateTime.now().plusHours(72);
        this.validationToken = generateJwtToken();
    }

    private String generateJwtToken() {
        // Implementação de geração de token JWT
        return Jwts.builder()
            .setSubject(this.id.getValue().toString())
            .setExpiration(Date.from(validUntil.atZone(ZoneId.systemDefault()).toInstant()))
            .signWith(SignatureAlgorithm.HS256, getValidationSecret())
            .compact();
    }

    private void addDomainEvent(DomainEvent event) {
        this.domainEvents.add(event);
    }

    private Money calculateTotalDepreciation(List<DepreciationRule> rules) {
        return depreciations.stream()
            .map(DepreciationItem::getAmount)
            .reduce(Money.ZERO, Money::add);
    }

    // Getters
    public EvaluationId getId() { return id; }
    public EvaluationStatus getStatus() { return status; }
    public Plate getPlate() { return plate; }
    // Outros getters...
}

// Checklist técnico - DOMÍNIO PURO
public class EvaluationChecklist {
    private final ChecklistId id;
    private final EvaluationId evaluationId;
    private BodyworkChecklist bodywork;
    private TiresChecklist tires;
    private InteriorChecklist interior;
    private MechanicalChecklist mechanical;
    private ElectronicsChecklist electronics;
    private DocumentsChecklist documents;
    private Integer conservationScore;
    private final List<String> criticalIssues;
    private final LocalDateTime createdAt;
    private LocalDateTime updatedAt;

    // Constructor e métodos de domínio
    public static EvaluationChecklist create(EvaluationId evaluationId) {
        return new EvaluationChecklist(ChecklistId.generate(), evaluationId);
    }

    public boolean hasBlockingIssues() {
        return !criticalIssues.isEmpty();
    }

    public boolean isComplete() {
        return bodywork != null && tires != null && interior != null
               && mechanical != null && electronics != null && documents != null;
    }

    public int calculateScore() {
        int score = 100;
        score -= bodywork != null ? bodywork.getPenaltyPoints() : 0;
        score -= tires != null ? tires.getPenaltyPoints() : 0;
        score -= interior != null ? interior.getPenaltyPoints() : 0;
        score -= mechanical != null ? mechanical.getPenaltyPoints() : 0;
        score -= electronics != null ? electronics.getPenaltyPoints() : 0;
        this.conservationScore = Math.max(0, score);
        this.updatedAt = LocalDateTime.now();
        return this.conservationScore;
    }
}

// Value Objects - IMUTÁVEIS
public final class EvaluationId {
    private final UUID value;

    private EvaluationId(UUID value) {
        this.value = Objects.requireNonNull(value);
    }

    public static EvaluationId generate() {
        return new EvaluationId(UUID.randomUUID());
    }

    public static EvaluationId from(UUID value) {
        return new EvaluationId(value);
    }

    public UUID getValue() { return value; }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        EvaluationId that = (EvaluationId) o;
        return Objects.equals(value, that.value);
    }

    @Override
    public int hashCode() {
        return Objects.hash(value);
    }
}

public final class Plate {
    private final String value;

    private Plate(String value) {
        if (!isValid(value)) {
            throw new IllegalArgumentException("Invalid plate format");
        }
        this.value = value.toUpperCase();
    }

    public static Plate from(String value) {
        return new Plate(value);
    }

    private boolean isValid(String value) {
        return value != null && value.matches("[A-Z]{3}[0-9][A-Z0-9][0-9]{2}");
    }

    public String getValue() { return value; }
}

public final class Money {
    public static final Money ZERO = new Money(BigDecimal.ZERO);
    private final BigDecimal amount;
    private final Currency currency;

    private Money(BigDecimal amount, Currency currency) {
        this.amount = amount.setScale(2, RoundingMode.HALF_UP);
        this.currency = currency;
    }

    public static Money of(BigDecimal amount) {
        return new Money(amount, Currency.getInstance("BRL"));
    }

    public static Money of(String amount) {
        return new Money(new BigDecimal(amount), Currency.getInstance("BRL"));
    }

    public Money add(Money other) {
        return new Money(this.amount.add(other.amount), this.currency);
    }

    public Money subtract(Money other) {
        return new Money(this.amount.subtract(other.amount), this.currency);
    }

    public Money multiply(BigDecimal multiplier) {
        return new Money(this.amount.multiply(multiplier), this.currency);
    }

    public Money percentage(int percentage) {
        return multiply(BigDecimal.valueOf(percentage).divide(BigDecimal.valueOf(100)));
    }

    public BigDecimal getValue() { return amount; }
    public Currency getCurrency() { return currency; }
}

// Outros Value Objects
public record EvaluatorId(UUID value) {}
public record ReviewerId(UUID value) {}
public record ChecklistId(UUID value) {}
public record FipeCode(String value) {}
public record VehicleInfo(String brand, String model, String version, int year) {}
public record PhotoType(String value) {}
```

#### Entidades JPA (INFRAESTRUTURA)

```java
// Entidade JPA para persistência
@Entity
@Table(name = "vehicle_evaluations")
public class VehicleEvaluationEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "evaluation_id", unique = true, nullable = false)
    private UUID evaluationId;

    @Column(name = "evaluator_id", nullable = false)
    private UUID evaluatorId;

    @Column(name = "plate", nullable = false, unique = true, length = 7)
    private String plate;

    @Column(name = "fipe_code", nullable = false, length = 20)
    private String fipeCode;

    // Vehicle Info columns
    @Column(name = "brand", nullable = false)
    private String brand;

    @Column(name = "model", nullable = false)
    private String model;

    @Column(name = "version", length = 100)
    private String version;

    @Column(name = "year", nullable = false)
    private Integer year;

    @Column(name = "mileage", nullable = false)
    private Integer mileage;

    @Column(name = "color", length = 50)
    private String color;

    @Column(name = "fuel_type", length = 20)
    private String fuelType;

    @Column(name = "gearbox", length = 20)
    private String gearbox;

    // Status e valores
    @Enumerated(EnumType.STRING)
    @Column(name = "status", nullable = false)
    private EvaluationStatus status;

    @Column(name = "fipe_price", precision = 12, scale = 2)
    private BigDecimal fipePrice;

    @Column(name = "depreciation_amount", precision = 12, scale = 2)
    private BigDecimal depreciationAmount;

    @Column(name = "safety_margin", precision = 12, scale = 2)
    private BigDecimal safetyMargin;

    @Column(name = "profit_margin", precision = 12, scale = 2)
    private BigDecimal profitMargin;

    @Column(name = "suggested_value", precision = 12, scale = 2)
    private BigDecimal suggestedValue;

    @Column(name = "approved_value", precision = 12, scale = 2)
    private BigDecimal approvedValue;

    // Timestamps
    @CreatedDate
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @LastModifiedDate
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;

    @Column(name = "submitted_at")
    private LocalDateTime submittedAt;

    @Column(name = "reviewed_at")
    private LocalDateTime reviewedAt;

    @Column(name = "reviewer_id")
    private UUID reviewerId;

    @Column(name = "rejection_reason", columnDefinition = "TEXT")
    private String rejectionReason;

    @Column(name = "internal_notes", columnDefinition = "TEXT")
    private String internalNotes;

    @Column(name = "external_notes", columnDefinition = "TEXT")
    private String externalNotes;

    @Column(name = "valid_until")
    private LocalDateTime validUntil;

    @Column(name = "validation_token", length = 500)
    private String validationToken;

    // Relacionamentos
    @OneToMany(mappedBy = "evaluation", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private List<EvaluationPhotoEntity> photos = new ArrayList<>();

    @OneToMany(mappedBy = "evaluation", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private List<DepreciationItemEntity> depreciations = new ArrayList<>();

    @OneToMany(mappedBy = "evaluation", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private List<EvaluationAccessoryEntity> accessories = new ArrayList<>();

    @OneToOne(mappedBy = "evaluation", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private EvaluationChecklistEntity checklist;

    // Getters e setters
}

@Entity
@Table(name = "evaluation_photos")
public class EvaluationPhotoEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "photo_id", unique = true, nullable = false)
    private UUID photoId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "evaluation_id", nullable = false)
    private VehicleEvaluationEntity evaluation;

    @Enumerated(EnumType.STRING)
    @Column(name = "type", nullable = false)
    private PhotoType type;

    @Column(name = "url", nullable = false)
    private String url;

    @Column(name = "thumbnail_url")
    private String thumbnailUrl;

    @Column(name = "uploaded_at", nullable = false)
    private LocalDateTime uploadedAt;

    @Column(name = "metadata", columnDefinition = "jsonb")
    private String metadata;
}

@Entity
@Table(name = "evaluation_checklists")
public class EvaluationChecklistEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "checklist_id", unique = true, nullable = false)
    private UUID checklistId;

    @OneToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "evaluation_id", nullable = false)
    private VehicleEvaluationEntity evaluation;

    @Column(name = "bodywork_data", columnDefinition = "jsonb")
    private String bodyworkData;

    @Column(name = "tires_data", columnDefinition = "jsonb")
    private String tiresData;

    @Column(name = "interior_data", columnDefinition = "jsonb")
    private String interiorData;

    @Column(name = "mechanical_data", columnDefinition = "jsonb")
    private String mechanicalData;

    @Column(name = "electronics_data", columnDefinition = "jsonb")
    private String electronicsData;

    @Column(name = "documents_data", columnDefinition = "jsonb")
    private String documentsData;

    @Column(name = "conservation_score")
    private Integer conservationScore;

    @ElementCollection
    @CollectionTable(name = "checklist_critical_issues", joinColumns = @JoinColumn(name = "checklist_id"))
    @Column(name = "issue")
    private List<String> criticalIssues = new ArrayList<>();

    @CreatedDate
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @LastModifiedDate
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
}
```

#### Mappers (INFRAESTRUTURA)

```java
@Component
public class VehicleEvaluationMapper {

    private final ModelMapper modelMapper;
    private final EvaluationPhotoMapper photoMapper;
    private final EvaluationChecklistMapper checklistMapper;
    private final DepreciationItemMapper depreciationMapper;
    private final EvaluationAccessoryMapper accessoryMapper;

    public VehicleEvaluationMapper(ModelMapper modelMapper,
                                  EvaluationPhotoMapper photoMapper,
                                  EvaluationChecklistMapper checklistMapper,
                                  DepreciationItemMapper depreciationMapper,
                                  EvaluationAccessoryMapper accessoryMapper) {
        this.modelMapper = modelMapper;
        this.photoMapper = photoMapper;
        this.checklistMapper = checklistMapper;
        this.depreciationMapper = depreciationMapper;
        this.accessoryMapper = accessoryMapper;

        configureMapper();
    }

    private void configureMapper() {
        // Configurações específicas do ModelMapper
        modelMapper.typeMap(VehicleEvaluation.class, VehicleEvaluationEntity.class)
            .addMapping(src -> src.getId().getValue(), VehicleEvaluationEntity::setEvaluationId)
            .addMapping(src -> src.getEvaluatorId().getValue(), VehicleEvaluationEntity::setEvaluatorId)
            .addMapping(src -> src.getPlate().getValue(), VehicleEvaluationEntity::setPlate)
            .addMapping(src -> src.getFipeCode().getValue(), VehicleEvaluationEntity::setFipeCode);

        modelMapper.typeMap(VehicleEvaluationEntity.class, VehicleEvaluation.class)
            .addMapping(VehicleEvaluationEntity::getEvaluationId, (dest, v) -> dest.setId(EvaluationId.from((UUID) v)))
            .addMapping(VehicleEvaluationEntity::getEvaluatorId, (dest, v) -> dest.setEvaluatorId(new EvaluatorId((UUID) v)))
            .addMapping(VehicleEvaluationEntity::getPlate, (dest, v) -> dest.setPlate(Plate.from((String) v)))
            .addMapping(VehicleEvaluationEntity::getFipeCode, (dest, v) -> dest.setFipeCode(new FipeCode((String) v)));
    }

    public VehicleEvaluationEntity toEntity(VehicleEvaluation domain) {
        if (domain == null) {
            return null;
        }

        VehicleEvaluationEntity entity = modelMapper.map(domain, VehicleEvaluationEntity.class);

        // Mapear relacionamentos
        entity.setPhotos(domain.getPhotos().stream()
            .map(photoMapper::toEntity)
            .peek(photoEntity -> photoEntity.setEvaluation(entity))
            .collect(Collectors.toList()));

        entity.setDepreciations(domain.getDepreciations().stream()
            .map(depreciationMapper::toEntity)
            .peek(depEntity -> depEntity.setEvaluation(entity))
            .collect(Collectors.toList()));

        if (domain.getChecklist() != null) {
            EvaluationChecklistEntity checklistEntity = checklistMapper.toEntity(domain.getChecklist());
            checklistEntity.setEvaluation(entity);
            entity.setChecklist(checklistEntity);
        }

        return entity;
    }

    public VehicleEvaluation toDomain(VehicleEvaluationEntity entity) {
        if (entity == null) {
            return null;
        }

        // Construir VehicleInfo
        VehicleInfo vehicleInfo = new VehicleInfo(
            entity.getBrand(),
            entity.getModel(),
            entity.getVersion(),
            entity.getYear(),
            entity.getFipeCode()
        );

        // Criar instância com factory method
        VehicleEvaluation domain = VehicleEvaluation.create(
            new EvaluatorId(entity.getEvaluatorId()),
            Plate.from(entity.getPlate()),
            new FipeCode(entity.getFipeCode()),
            vehicleInfo,
            Money.of(new BigDecimal(entity.getMileage()))
        );

        // Mapear campos adicionais via reflexão ou setter package-private
        setPrivateField(domain, "status", entity.getStatus());
        setPrivateField(domain, "fipePrice", entity.getFipePrice() != null ? Money.of(entity.getFipePrice()) : null);
        setPrivateField(domain, "depreciationAmount", entity.getDepreciationAmount() != null ? Money.of(entity.getDepreciationAmount()) : null);
        // ... outros campos

        // Mapear coleções
        entity.getPhotos().forEach(photoEntity ->
            domain.getPhotos().add(photoMapper.toDomain(photoEntity)));

        entity.getDepreciations().forEach(depEntity ->
            domain.getDepreciations().add(depreciationMapper.toDomain(depEntity)));

        if (entity.getChecklist() != null) {
            domain.updateChecklist(checklistMapper.toDomain(entity.getChecklist()));
        }

        return domain;
    }

    private void setPrivateField(Object obj, String fieldName, Object value) {
        try {
            Field field = obj.getClass().getDeclaredField(fieldName);
            field.setAccessible(true);
            field.set(obj, value);
        } catch (Exception e) {
            throw new RuntimeException("Failed to set field: " + fieldName, e);
        }
    }
}

@Component
public class EvaluationPhotoMapper {

    public EvaluationPhotoEntity toEntity(EvaluationPhoto domain) {
        if (domain == null) {
            return null;
        }

        EvaluationPhotoEntity entity = new EvaluationPhotoEntity();
        entity.setPhotoId(domain.getId().getValue());
        entity.setType(domain.getType());
        entity.setUrl(domain.getUrl());
        entity.setThumbnailUrl(domain.getThumbnailUrl());
        entity.setUploadedAt(domain.getUploadedAt());
        entity.setMetadata(domain.getMetadata());

        return entity;
    }

    public EvaluationPhoto toDomain(EvaluationPhotoEntity entity) {
        if (entity == null) {
            return null;
        }

        return EvaluationPhoto.restore(
            PhotoId.from(entity.getPhotoId()),
            EvaluationId.from(entity.getEvaluation().getEvaluationId()),
            entity.getType(),
            entity.getUrl(),
            entity.getThumbnailUrl(),
            entity.getUploadedAt(),
            entity.getMetadata()
        );
    }
}
```

#### Implementação dos Repositories (INFRAESTRUTURA)

```java
@Repository
public class VehicleEvaluationRepositoryImpl implements VehicleEvaluationRepository {

    private final VehicleEvaluationJpaRepository jpaRepository;
    private final VehicleEvaluationMapper mapper;

    public VehicleEvaluationRepositoryImpl(VehicleEvaluationJpaRepository jpaRepository,
                                          VehicleEvaluationMapper mapper) {
        this.jpaRepository = jpaRepository;
        this.mapper = mapper;
    }

    @Override
    public Optional<VehicleEvaluation> findById(EvaluationId id) {
        return jpaRepository.findByEvaluationId(id.getValue())
            .map(mapper::toDomain);
    }

    @Override
    public Optional<VehicleEvaluation> findByPlate(Plate plate) {
        return jpaRepository.findByPlate(plate.getValue())
            .map(mapper::toDomain);
    }

    @Override
    public List<VehicleEvaluation> findByStatus(EvaluationStatus status) {
        return jpaRepository.findByStatus(status).stream()
            .map(mapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findByEvaluatorId(EvaluatorId evaluatorId) {
        return jpaRepository.findByEvaluatorId(evaluatorId.getValue()).stream()
            .map(mapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public Page<VehicleEvaluation> findPendingApprovals(EvaluationStatus status, Pageable pageable) {
        Page<VehicleEvaluationEntity> entityPage = jpaRepository.findByStatus(status, pageable);
        return entityPage.map(mapper::toDomain);
    }

    @Override
    public VehicleEvaluation save(VehicleEvaluation evaluation) {
        VehicleEvaluationEntity entity = mapper.toEntity(evaluation);
        VehicleEvaluationEntity saved = jpaRepository.save(entity);
        return mapper.toDomain(saved);
    }

    @Override
    public void deleteById(EvaluationId id) {
        jpaRepository.deleteByEvaluationId(id.getValue());
    }

    @Override
    public boolean existsByPlate(Plate plate) {
        return jpaRepository.existsByPlate(plate.getValue());
    }
}

// Spring Data JPA Repository
@Repository
public interface VehicleEvaluationJpaRepository extends JpaRepository<VehicleEvaluationEntity, Long> {
    Optional<VehicleEvaluationEntity> findByEvaluationId(UUID evaluationId);
    Optional<VehicleEvaluationEntity> findByPlate(String plate);
    List<VehicleEvaluationEntity> findByStatus(EvaluationStatus status);
    List<VehicleEvaluationEntity> findByEvaluatorId(UUID evaluatorId);
    Page<VehicleEvaluationEntity> findByStatus(EvaluationStatus status, Pageable pageable);
    boolean existsByPlate(String plate);
    void deleteByEvaluationId(UUID evaluationId);
}
```

#### Commands/Queries CQRS

```java
// Commands
public record CreateEvaluationCommand(
    String plate,
    int year,
    int mileage,
    String color,
    String version,
    String fuelType,
    String gearbox,
    List<String> accessories,
    String internalNotes
) {}

public record AddPhotosCommand(
    UUID evaluationId,
    Map<String, MultipartFile> photos
) {}

public record UpdateChecklistCommand(
    UUID evaluationId,
    EvaluationChecklistDto checklist
) {}

public record CalculateValuationCommand(
    UUID evaluationId
) {}

public record SubmitForApprovalCommand(
    UUID evaluationId
) {}

public record ApproveEvaluationCommand(
    UUID evaluationId,
    BigDecimal adjustedValue
) {}

public record RejectEvaluationCommand(
    UUID evaluationId,
    String reason
) {}

public record GenerateReportCommand(
    UUID evaluationId
) {}

// Queries
public record GetEvaluationQuery(UUID id) {}

public record GetPendingApprovalsQuery(
    Integer page = 0,
    Integer size = 20,
    String sortBy = "createdAt",
    boolean sortDescending = true
) {}

public record GetEvaluationsByEvaluatorQuery(
    UUID evaluatorId,
    LocalDateTime startDate,
    LocalDateTime endDate,
    EvaluationStatus status
) {}

public record GetEvaluationDashboardQuery(
    LocalDateTime startDate,
    LocalDateTime endDate
) {}
```

#### Eventos de Domínio

```java
// Eventos de Domínio
public abstract class DomainEvent {
    private final UUID id = UUID.randomUUID();
    private final LocalDateTime occurredAt = LocalDateTime.now();
}

public class EvaluationCreatedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final UUID evaluatorId;
    private final String plate;

    // Constructor e getters
}

public class PhotosUploadedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final int photoCount;

    // Constructor e getters
}

public class ChecklistCompletedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final int conservationScore;
    private final List<String> criticalIssues;

    // Constructor e getters
}

public class ValuationCalculatedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final BigDecimal fipePrice;
    private final BigDecimal suggestedValue;

    // Constructor e getters
}

public class EvaluationSubmittedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final UUID evaluatorId;

    // Constructor e getters
}

public class EvaluationApprovedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final UUID reviewerId;
    private final BigDecimal approvedValue;
    private final String validationToken;
    private final LocalDateTime validUntil;

    // Constructor e getters
}

public class EvaluationRejectedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final UUID reviewerId;
    private final String reason;

    // Constructor e getters
}

// Evento para integração com outros bounded contexts
public class VehicleEvaluationCompletedEvent extends DomainEvent {
    private final UUID evaluationId;
    private final String plate;
    private final String brand;
    private final String model;
    private final int year;
    private final BigDecimal approvedValue;
    private final LocalDateTime validUntil;
    private final Map<String, Object> evaluationData;

    // Constructor e getters
}
```

### Endpoints de API

```java
// Auth: @PreAuthorize("hasAnyRole('EVALUATOR', 'MANAGER', 'ADMIN')")
@RestController
@RequestMapping("/api/v1/evaluations")
public class VehicleEvaluationController {

    private final CommandBus commandBus;
    private final QueryBus queryBus;

    // Cria nova avaliação
    @PostMapping
    public ResponseEntity<UUID> createEvaluation(@Valid @RequestBody CreateEvaluationCommand command) {
        UUID evaluationId = commandBus.execute(command);
        return ResponseEntity.created(URI.create("/api/v1/evaluations/" + evaluationId))
                             .body(evaluationId);
    }

    // Adiciona fotos à avaliação
    @PostMapping("/{id}/photos")
    public ResponseEntity<Void> addPhotos(@PathVariable UUID id,
                                         @RequestParam Map<String, MultipartFile> photos) {
        commandBus.execute(new AddPhotosCommand(id, photos));
        return ResponseEntity.accepted().build();
    }

    // Atualiza checklist
    @PutMapping("/{id}/checklist")
    public ResponseEntity<Void> updateChecklist(@PathVariable UUID id,
                                               @Valid @RequestBody UpdateChecklistCommand command) {
        commandBus.execute(command);
        return ResponseEntity.ok().build();
    }

    // Calcula valoração
    @PostMapping("/{id}/calculate")
    public ResponseEntity<Void> calculateValuation(@PathVariable UUID id) {
        commandBus.execute(new CalculateValuationCommand(id));
        return ResponseEntity.ok().build();
    }

    // Submete para aprovação
    @PostMapping("/{id}/submit")
    public ResponseEntity<Void> submitForApproval(@PathVariable UUID id) {
        commandBus.execute(new SubmitForApprovalCommand(id));
        return ResponseEntity.ok().build();
    }

    // Obtém detalhes da avaliação
    @GetMapping("/{id}")
    public ResponseEntity<VehicleEvaluationDto> getEvaluation(@PathVariable UUID id) {
        VehicleEvaluationDto evaluation = queryBus.query(new GetEvaluationQuery(id));
        return ResponseEntity.ok(evaluation);
    }

    // Gera PDF do laudo
    @GetMapping("/{id}/report")
    public ResponseEntity<byte[]> generateReport(@PathVariable UUID id) {
        byte[] report = commandBus.execute(new GenerateReportCommand(id));
        return ResponseEntity.ok()
                .header(HttpHeaders.CONTENT_TYPE, MediaType.APPLICATION_PDF_VALUE)
                .header(HttpHeaders.CONTENT_DISPOSITION, "attachment; filename=evaluation_" + id + ".pdf")
                .body(report);
    }

    // Valida laudo online (público com token)
    @GetMapping("/validate/{token}")
    public ResponseEntity<EvaluationValidationDto> validateEvaluation(@PathVariable String token) {
        EvaluationValidationDto validation = queryBus.query(new ValidateEvaluationQuery(token));
        return ResponseEntity.ok(validation);
    }
}

// Auth: @PreAuthorize("hasAnyRole('MANAGER', 'ADMIN')")
@RestController
@RequestMapping("/api/v1/evaluations/pending")
public class PendingEvaluationsController {

    // Lista avaliações pendentes
    @GetMapping
    public ResponseEntity<PagedResult<VehicleEvaluationSummaryDto>> getPendingApprovals(
            @Valid GetPendingApprovalsQuery query) {
        PagedResult<VehicleEvaluationSummaryDto> result = queryBus.query(query);
        return ResponseEntity.ok(result);
    }

    // Aprova avaliação
    @PostMapping("/{id}/approve")
    public ResponseEntity<Void> approveEvaluation(@PathVariable UUID id,
                                                 @Valid @RequestBody ApproveEvaluationCommand command) {
        commandBus.execute(command);
        return ResponseEntity.ok().build();
    }

    // Rejeita avaliação
    @PostMapping("/{id}/reject")
    public ResponseEntity<Void> rejectEvaluation(@PathVariable UUID id,
                                                 @Valid @RequestBody RejectEvaluationCommand command) {
        commandBus.execute(command);
        return ResponseEntity.ok().build();
    }
}

// Auth: @PreAuthorize("hasRole('ADMIN')")
@RestController
@RequestMapping("/api/v1/evaluations/reports")
public class EvaluationReportsController {

    // Dashboard gerencial
    @GetMapping("/dashboard")
    public ResponseEntity<EvaluationDashboardDto> getDashboard(
            @Valid GetEvaluationDashboardQuery query) {
        EvaluationDashboardDto dashboard = queryBus.query(query);
        return ResponseEntity.ok(dashboard);
    }

    // Relatório detalhado
    @GetMapping("/detailed")
    public ResponseEntity<byte[]> getDetailedReport(
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate) {
        // Implementation
        return ResponseEntity.ok().build();
    }
}
```

## Pontos de Integração

### APIs Externas

1. **API FIPE** (https://parallelum.com.br/fipe/api/v1/)
   - Endpoint: `/carros/marcas` - Lista marcas
   - Endpoint: `/carros/marcas/{marcaId}/modelos` - Modelos por marca
   - Endpoint: `/carros/marcas/{marcaId}/modelos/{modeloId}/anos` - Anos do modelo
   - Endpoint: `/carros/marcas/{marcaId}/modelos/{modeloId}/anos/{anoId}` - Valor específico
   - Autenticação: Não requer
   - Rate Limit: Implementar cache com TTL de 24h

2. **Cloudflare R2 Storage**
   - SDK: AWS S3 SDK (compatível)
   - Configuração: Variáveis de ambiente (Endpoint, AccessKey, SecretKey, BucketName)
   - Estrutura: `evaluations/{evaluationId}/photos/{type}_{timestamp}.jpg`

3. **RabbitMQ**
   - Exchange: `gestauto.events`
   - Routing Key: `vehicle_evaluation.{event_type}`
   - Eventos: Created, Approved, Rejected, Completed

### Configuração de Variáveis de Ambiente

```yaml
# application.yml
spring:
  datasource:
    url: jdbc:postgresql://localhost:5432/gestauto
    username: gestauto
    password: gestauto123
    driver-class-name: org.postgresql.Driver

  jpa:
    hibernate:
      ddl-auto: validate
    show-sql: false
    properties:
      hibernate:
        dialect: org.hibernate.dialect.PostgreSQLDialect
        format_sql: true
        # Usar schema específico para vehicle evaluation
        default_schema: vehicle_evaluation
        # Configurar para usar schema separado
        hibernate.boot.allow_jdbc_metadata_access: false

  redis:
    host: ${REDIS_HOST:localhost}
    port: ${REDIS_PORT:6379}
    timeout: 2000ms

  rabbitmq:
    host: localhost
    port: 5672
    username: gestauto
    password: gestauto123
    virtual-host: ${RABBITMQ_VHOST:/}

  security:
    oauth2:
      resourceserver:
        jwt:
          issuer-uri: ${JWT_ISSUER_URI:https://auth.gestauto.com}

# Configurações customizadas
app:
  cloudflare-r2:
    endpoint: ${CLOUDFLARE_R2_ENDPOINT}
    access-key: ${CLOUDFLARE_R2_ACCESS_KEY}
    secret-key: ${CLOUDFLARE_R2_SECRET_KEY}
    bucket-name: ${CLOUDFLARE_R2_BUCKET:vehicle-photos}

  fipe:
    base-url: ${FIPE_BASE_URL:https://parallelum.com.br/fipe/api/v1}
    cache-expiration-hours: ${FIPE_CACHE_EXPIRATION_HOURS:24}

  jwt:
    secret: ${JWT_SECRET:your-secret-key}
    validation-expiration-hours: 72

  database:
    schema: vehicle_evaluation
```

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|-------------------|-----------------|----------------------------|----------------|
| Proposal Entity | Modificação | Adicionar campo UsedVehicleEvaluationId já existe. Baixo risco. | Nenhuma |
| PostgreSQL | Novo Schema | Novo schema `vehicle_evaluation` no banco existente. Baixo risco. | Criar schema e migrations |
| RabbitMQ | Novo Exchange | Novo exchange `gestauto.events` para eventos de avaliação. Baixo risco. | Configurar novo exchange |
| Redis | Novo Cache | Cache para valores FIPE. Baixo risco. | Configurar Redis |
| Cloudflare R2 | Novo Storage | Armazenamento de fotos. Baixo risco. | Configurar bucket |
| API Gateway | Novo Serviço | Novo serviço no API Gateway. Baixo risco. | Adicionar roteamento |

## Abordagem de Testes

### Testes Unitários

**Componentes principais:**
- Domain Entities (VehicleEvaluation, EvaluationChecklist)
- Commands/Queries Validators
- Business Logic (cálculos de depreciação, validações)
- Value Objects

**Cenários críticos:**
- Cálculo de valoração com diferentes combinações de depreciação
- Validação de checklist completo e identificação de bloqueantes
- Transições de status com validações de negócio
- Cálculo de score de conservação

**Mocks necessários:**
- FipeService: Retornar valores FIPE simulados
- ImageStorageService: Simular upload/download de imagens
- EventPublisher: Verificar publicação de eventos

### Testes de Integração

**Componentes a testar juntos:**
- Controllers → Handlers → Repository → Database
- RabbitMQ message publishing/consuming
- FIPE API integration (com WireMock server)
- Cloudflare R2 integration (com MinIO local)

**Testes em `src/test/java/integration/`:**
- Fluxo completo de criação até aprovação
- Persistência e recuperação de entidades completas
- Publicação e consumo de eventos entre bounded contexts

### Estrutura de Testes

```java
@ExtendWith(MockitoExtension.class)
class VehicleEvaluationTest {

    @Mock
    private FipeService fipeService;

    @Mock
    private ImageStorageService imageStorageService;

    @InjectMocks
    private VehicleEvaluationService service;

    @Test
    @DisplayName("Should calculate valuation correctly")
    void shouldCalculateValuation() {
        // Given
        VehicleEvaluation evaluation = new VehicleEvaluation();
        BigDecimal fipePrice = new BigDecimal("50000.00");
        List<DepreciationRule> rules = List.of(/* rules */);

        // When
        service.calculateValuation(evaluation, fipePrice, rules);

        // Then
        assertThat(evaluation.getSuggestedValue()).isGreaterThan(BigDecimal.ZERO);
        assertThat(evaluation.getDepreciationAmount()).isGreaterThan(BigDecimal.ZERO);
    }
}

@SpringBootTest
@Testcontainers
class VehicleEvaluationIntegrationTest {

    @Container
    static PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:15");

    @Test
    @Transactional
    void shouldCreateAndRetrieveEvaluation() {
        // Integration test implementation
    }
}
```

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **Phase 1 - Foundation**
   - Configurar projeto Maven/Gradle e estrutura base
   - Implementar entidades de domínio core com JPA
   - Configurar banco de dados e migrations com Flyway

2. **Phase 2 - Core Features**
   - Implementar handlers para commands de criação e atualização
   - Implementar integração com API FIPE (RestTemplate/WebClient)
   - Implementar upload de fotos (AWS SDK)

3. **Phase 3 - Business Logic**
   - Implementar checklist e validações
   - Implementar cálculos de depreciação e valoração
   - Implementar regras de aprovação

4. **Phase 4 - Integration**
   - Implementar eventos de domínio com Spring Events
   - Configurar RabbitMQ com Spring AMQP
   - Implementar geração de PDF (iText/PDFBox)

5. **Phase 5 - Polish & Tests**
   - Implementar dashboard e relatórios
   - Adicionar validação de laudo online
   - Implementar todos os testes

### Dependências Técnicas

- ✅ Infraestrutura PostgreSQL já disponível (gestauto/gestauto123)
- ✅ Configuração de RabbitMQ já disponível (gestauto/gestauto123)
- Access keys para Cloudflare R2
- Schema `vehicle_evaluation` deve ser criado no PostgreSQL existente
- Liberação de firewall para API FIPE

## Monitoramento e Observabilidade

### Métricas Prometheus

```java
// Configuração com Micrometer
@Configuration
public class MetricsConfig {

    @Bean
    public TimedAspect timedAspect(MeterRegistry registry) {
        return new TimedAspect(registry);
    }

    @Bean
    public CountedAspect countedAspect(MeterRegistry registry) {
        return new CountedAspect(registry);
    }
}

// Métricas nos handlers
@Timed(value = "vehicle.evaluation.duration", description = "Time taken to process evaluation")
@Counted(value = "vehicle.evaluation.created", description = "Number of evaluations created")
@Service
public class CreateEvaluationHandler {
    // Implementation
}
```

Métricas principais:
- `vehicle_evaluations_created_total`
- `vehicle_evaluations_approved_total`
- `vehicle_evaluations_rejected_total`
- `fipe_api_requests_total`
- `vehicle_evaluation_duration_seconds`
- `fipe_api_request_duration_seconds`
- `photo_upload_duration_seconds`
- `vehicle_evaluations_pending_count`
- `fipe_cache_hit_ratio`
- `photo_storage_used_bytes`

### Logs Principais

```java
@Slf4j
@Service
public class VehicleEvaluationService {

    public VehicleEvaluation createEvaluation(CreateEvaluationCommand command) {
        log.info("Creating evaluation for evaluator {} and plate {}",
                command.evaluatorId(), command.plate());
        // Implementation

        log.info("Evaluation {} created successfully", evaluation.getId());
        return evaluation;
    }

    public void approveEvaluation(UUID evaluationId, BigDecimal value) {
        log.warn("Evaluation {} has critical issues: {}",
                evaluationId, criticalIssues);
        // Implementation
    }
}
```

### Integração Grafana

- Dashboard: "Vehicle Evaluation Service"
- Panels: Evaluations by Status, Approval Rate, Average Time, Photo Upload Stats
- Alertas: Taxa de aprovação < 80%, Falhas no upload > 5%

## Considerações Técnicas

### Decisões Principais

**1. Cache FIPE com Redis**
- Justificativa: API FIPE não tem SLA garantido, evita dependência
- Trade-off: Dados podem ficar desatualizados até 24h
- Alternativa rejeitada: Chamada direta sempre (risco de indisponibilidade)
- Implementação: Spring Cache com Redis

**2. Validação de 72h**
- Justificativa: PRD exige laudo válido por período fixo
- Implementação: Token JWT com exp claim
- Trade-off: Rigidez vs flexibilidade comercial

**3. Fotos obrigatórias como regra de domínio**
- Justificativa: Garante consistência e qualidade
- Implementação: Entity não permite transição sem fotos
- Trade-off: Exige conectividade constante

**4. Event-driven integration**
- Justificativa: Desacopla bounded contexts
- Implementação: RabbitMQ com eventos de domínio
- Trade-off: Complexidade adicional vs resiliência

### Riscos Conhecidos

1. **Dependência API FIPE**: Implementar retry exponencial e cache persistente
2. **Upload de fotos grandes**: Implementar compressão client-side e streaming
3. **Concorrência de avaliações**: Implementar bloqueio por placa com timeout
4. **Performance com muitas fotos**: Implementar lazy loading e CDNs

### Requisitos Especiais

**Performance:**
- < 2s para operações CRUD
- < 30s para geração de PDF
- Upload paralelo de fotos com progress bar
- Cache de FIPE com Redis (TTL 24h)

**Segurança:**
- Criptografia de placas no BD
- Tokens de validação expiráveis
- Rate limiting por usuário
- Sanitização de uploads

**Escalabilidade:**
- Horizontal scaling suportado
- Stateless API design
- Database connection pooling (HikariCP)
- Reactive programming com Spring WebFlux (opcional)

### Conformidade com Padrões

✅ **Segue princípios DDD**: Bounded context claro, entidades ricas puras, agregados bem definidos
✅ **Repository Pattern com Mappers**: Domínio puro separado da infraestrutura de persistência
✅ **Aplica CQRS**: Separação clara de commands/queries com Spring Events
✅ **Usa eventos de domínio**: Decoupling via RabbitMQ
✅ **Implementa validação**: Bean Validation nos DTOs
✅ **Segui test patterns**: Unit tests com JUnit 5, mocks com Mockito
✅ **Logging estruturado**: SLF4J com Logback
✅ **Health checks**: Spring Boot Actuator endpoints
✅ **Configuration management**: Spring Boot configuration
✅ **Error handling**: @ControllerAdvice com exceções customizadas

## Exemplo de Handler com Repository Pattern

```java
@Component
public class CreateEvaluationHandler implements CommandHandler<CreateEvaluationCommand, UUID> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final FipeService fipeService;
    private final DomainEventPublisher eventPublisher;

    public CreateEvaluationHandler(VehicleEvaluationRepository evaluationRepository,
                                  FipeService fipeService,
                                  DomainEventPublisher eventPublisher) {
        this.evaluationRepository = evaluationRepository;
        this.fipeService = fipeService;
        this.eventPublisher = eventPublisher;
    }

    @Override
    @Transactional
    public UUID handle(CreateEvaluationCommand command) {
        // 1. Validar placa única
        Plate plate = Plate.from(command.plate());
        if (evaluationRepository.existsByPlate(plate)) {
            throw new BusinessException("Evaluation already exists for plate: " + plate.getValue());
        }

        // 2. Obter informações FIPE
        Optional<FipeVehicleInfo> fipeInfo = fipeService.getVehicleInfo(command.fipeCode());
        if (fipeInfo.isEmpty()) {
            throw new BusinessException("Invalid FIPE code: " + command.fipeCode());
        }

        // 3. Criar entidade de domínio
        VehicleInfo vehicleInfo = new VehicleInfo(
            fipeInfo.get().brand(),
            fipeInfo.get().model(),
            command.version(),
            command.year(),
            command.fipeCode()
        );

        EvaluatorId evaluatorId = EvaluatorId.from(command.evaluatorId());
        Money mileage = Money.of(new BigDecimal(command.mileage()));
        FipeCode fipeCode = FipeCode.from(command.fipeCode());

        VehicleEvaluation evaluation = VehicleEvaluation.create(
            evaluatorId,
            plate,
            fipeCode,
            vehicleInfo,
            mileage
        );

        // 4. Adicionar acessórios se existirem
        command.accessories().forEach(acc -> {
            EvaluationAccessory accessory = EvaluationAccessory.create(
                evaluation.getId(),
                acc,
                determineAccessoryValue(acc)
            );
            evaluation.addAccessory(accessory);
        });

        // 5. Salvar usando repositório puro
        VehicleEvaluation saved = evaluationRepository.save(evaluation);

        // 6. Publicar eventos de domínio
        evaluation.getDomainEvents().forEach(eventPublisher::publish);

        // 7. Limpar eventos
        evaluation.clearDomainEvents();

        return saved.getId().getValue();
    }

    private Money determineAccessoryValue(String accessory) {
        // Lógica para determinar impacto de acessórios no valor
        return Money.ZERO;
    }
}

// Exemplo de Query Handler
@Component
public class GetEvaluationHandler implements QueryHandler<GetEvaluationQuery, VehicleEvaluationDto> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final VehicleEvaluationMapper mapper;

    public GetEvaluationHandler(VehicleEvaluationRepository evaluationRepository,
                               VehicleEvaluationMapper mapper) {
        this.evaluationRepository = evaluationRepository;
        this.mapper = mapper;
    }

    @Override
    @Transactional(readOnly = true)
    public VehicleEvaluationDto handle(GetEvaluationQuery query) {
        EvaluationId evaluationId = EvaluationId.from(query.id());

        Optional<VehicleEvaluation> evaluation = evaluationRepository.findById(evaluationId);

        if (evaluation.isEmpty()) {
            throw new EntityNotFoundException("Evaluation not found: " + evaluationId.getValue());
        }

        return VehicleEvaluationDto.from(evaluation.get());
    }
}
```

## Estrutura de Arquivos

```
services/
└── vehicle-evaluation/
    ├── pom.xml
    ├── Dockerfile
    ├── src/
    │   ├── main/
    │   │   ├── java/
    │   │   │   └── com/gestauto/vehicleevaluation/
    │   │   │       ├── VehicleEvaluationApplication.java
    │   │   │       ├── config/
    │   │   │       │   ├── DatabaseConfig.java
    │   │   │       │   ├── RabbitMQConfig.java
    │   │   │       │   ├── RedisConfig.java
    │   │   │       │   └── SecurityConfig.java
    │   │   │       ├── 1-service/
    │   │   │       │   ├── controller/
    │   │   │       │   │   ├── VehicleEvaluationController.java
    │   │   │       │   │   ├── PendingEvaluationsController.java
    │   │   │       │   │   └── EvaluationReportsController.java
    │   │   │       │   └── dto/
    │   │   │       │       ├── VehicleEvaluationDto.java
    │   │   │       │       ├── CreateEvaluationCommand.java
    │   │   │       │       └── EvaluationChecklistDto.java
    │   │   │       ├── 2-application/
    │   │   │       │   ├── command/
    │   │   │       │   │   ├── CreateEvaluationCommand.java
    │   │   │       │   │   ├── AddPhotosCommand.java
    │   │   │       │   │   └── UpdateChecklistCommand.java
    │   │   │       │   ├── query/
    │   │   │       │   │   ├── GetEvaluationQuery.java
    │   │   │       │   │   └── GetPendingApprovalsQuery.java
    │   │   │       │   ├── handler/
    │   │   │       │   │   ├── command/
    │   │   │       │   │   │   ├── CreateEvaluationHandler.java
    │   │   │       │   │   │   └── ApproveEvaluationHandler.java
    │   │   │       │   │   └── query/
    │   │   │       │   │       ├── GetEvaluationHandler.java
    │   │   │       │   │       └── GetPendingApprovalsHandler.java
    │   │   │       │   ├── validator/
    │   │   │       │   │   ├── CreateEvaluationValidator.java
    │   │   │       │   │   └── UpdateChecklistValidator.java
    │   │   │       │   └── service/
    │   │   │       │       ├── CommandBus.java
    │   │   │       │       └── QueryBus.java
    │   │   │       ├── 3-domain/
    │   │   │       │   ├── entity/
    │   │   │       │   │   ├── VehicleEvaluation.java
    │   │   │       │   │   ├── EvaluationChecklist.java
    │   │   │       │   │   ├── EvaluationPhoto.java
    │   │   │       │   │   └── DepreciationItem.java
    │   │   │       │   ├── enum/
    │   │   │       │   │   ├── EvaluationStatus.java
    │   │   │       │   │   └── PhotoType.java
    │   │   │       │   ├── event/
    │   │   │       │   │   ├── DomainEvent.java
    │   │   │       │   │   ├── EvaluationCreatedEvent.java
    │   │   │       │   │   └── EvaluationApprovedEvent.java
    │   │   │       │   ├── exception/
    │   │   │       │   │   ├── BusinessException.java
    │   │   │       │   │   └── EntityNotFoundException.java
    │   │   │       │   ├── valueobject/
    │   │   │       │   │   ├── Money.java
    │   │   │       │   │   └── VehicleInfo.java
    │   │   │       │   ├── repository/
    │   │   │       │   │   └── VehicleEvaluationRepository.java
    │   │   │       │   └── service/
    │   │   │       │       ├── DomainEventPublisher.java
    │   │   │       │       └── ValuationService.java
    │   │   │       └── 4-infra/
    │   │   │           ├── persistence/
    │   │   │           │   ├── repository/
    │   │   │           │   │   └── VehicleEvaluationRepositoryImpl.java
    │   │   │           │   └── mapper/
    │   │   │           │       └── VehicleEvaluationMapper.java
    │   │   │           ├── external/
    │   │   │           │   ├── fipe/
    │   │   │           │   │   ├── FipeServiceImpl.java
    │   │   │           │   │   └── FipeApiClient.java
    │   │   │           │   ├── storage/
    │   │   │           │   │   ├── ImageStorageServiceImpl.java
    │   │   │           │   │   └── S3Config.java
    │   │   │           │   └── report/
    │   │   │           │       ├── ReportServiceImpl.java
    │   │   │           │       └── PdfGenerator.java
    │   │   │           ├── messaging/
    │   │   │           │   ├── publisher/
    │   │   │           │   │   └── RabbitMQEventPublisher.java
    │   │   │           │   └── handler/
    │   │   │           │       └── EventHandler.java
    │   │   │           └── security/
    │   │   │               ├── JwtTokenValidator.java
    │   │   │               └── SecurityUtils.java
    │   │   └── resources/
    │   │       ├── application.yml
    │   │       ├── application-prod.yml
    │   │       ├── db/migration/V1__Create_vehicle_evaluations_table.sql
    │   │       └── static/
    │   └── test/
    │       └── java/
    │           └── com/gestauto/vehicleevaluation/
    │               ├── unit/
    │               │   ├── domain/
    │               │   ├── application/
    │               │   └── infra/
    │               └── integration/
    │                   ├── VehicleEvaluationControllerIT.java
    │                   └── FipeServiceIT.java
```

## Instruções de Setup e Configuração

### 1. Preparar o Ambiente

Primeiro, inicie a infraestrutura existente:

```bash
# Na raiz do projeto GestAuto
docker-compose up -d postgres rabbitmq
```

Aguarde até que os serviços estejam saudáveis:

```bash
# Verificar PostgreSQL
docker logs gestauto-postgres | grep "database system is ready to accept connections"

# Verificar RabbitMQ
docker logs gestauto-rabbitmq | grep "Server startup complete"
```

### 2. Criar Schema no PostgreSQL

Conecte-se ao PostgreSQL e crie o schema:

```bash
# Conectar ao container PostgreSQL
docker exec -it gestauto-postgres psql -U gestauto -d gestauto

# Ou conectar de forma local
psql -h localhost -p 5432 -U gestauto -d gestauto
```

Execute o script `setup_vehicle_evaluation_schema.sql`:

```sql
-- Criar schema dedicado
CREATE SCHEMA IF NOT EXISTS vehicle_evaluation;

-- Configurar permissões
GRANT USAGE ON SCHEMA vehicle_evaluation TO gestauto;
GRANT CREATE ON SCHEMA vehicle_evaluation TO gestauto;

-- Configurar search path
ALTER ROLE gestauto SET search_path TO public, vehicle_evaluation;

-- Verificar schema criado
\dn vehicle_evaluation
```

### 3. Configurar RabbitMQ

Acesse o painel de administração do RabbitMQ para criar o exchange:

```bash
# Acessar painel
http://localhost:15672
# Usuário: gestauto
# Senha: gestauto123
```

Criar o exchange manualmente:
- Nome: `gestauto.events`
- Tipo: `topic`
- Durável: Sim

### 4. Executar Aplicação

```bash
# Clonar projeto (se necessário)
cd services/vehicle-evaluation

# Compilar e executar
./mvnw clean install
./mvnw spring-boot:run

# Ou executar com Docker
docker build -t gestauto/vehicle-evaluation .
docker run -p 8080:8080 \
  -e SPRING_DATASOURCE_URL=jdbc:postgresql://host.docker.internal:5432/gestauto \
  -e SPRING_DATASOURCE_USERNAME=gestauto \
  -e SPRING_DATASOURCE_PASSWORD=gestauto123 \
  gestauto/vehicle-evaluation
```

### 5. Executar Migrations

Se as migrations não executarem automaticamente:

```bash
# Executar migrations com Flyway
./mvnw flyway:migrate

# Ou via SQL diretamente no banco
psql -h localhost -p 5432 -U gestauto -d gestauto -f scripts/setup_vehicle_evaluation_schema.sql
```

### 6. Verificar Setup

Acesse os endpoints de verificação:

```bash
# Health check
curl http://localhost:8080/actuator/health

# Verificar schema no PostgreSQL
docker exec -it gestauto-postgres psql -U gestauto -d gestauto -c "\dt vehicle_evaluation.*"

# Listar exchanges RabbitMQ
curl -u gestauto:gestauto123 http://localhost:15672/api/exchanges/%2F
```

## Scripts de Infraestrutura

### Dockerfile

```dockerfile
FROM openjdk:21-jdk-slim

LABEL maintainer="GestAuto Team"

# Install dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /app

# Copy Maven wrapper and pom.xml
COPY mvnw .
COPY .mvn .mvn
COPY pom.xml .

# Download dependencies
RUN ./mvnw dependency:go-offline -B

# Copy source code
COPY src ./src

# Build application
RUN ./mvnw clean package -DskipTests

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/actuator/health || exit 1

# Run application
ENTRYPOINT ["java", "-jar", "target/vehicle-evaluation-*.jar"]
```

### Docker Compose para Desenvolvimento (usando infra existente)

```yaml
# docker-compose.dev.yml - Para desenvolvimento local
version: '3.8'

services:
  vehicle-evaluation-api:
    build: .
    environment:
      - SPRING_PROFILES_ACTIVE=docker
      - SPRING_DATASOURCE_URL=jdbc:postgresql://host.docker.internal:5432/gestauto
      - SPRING_DATASOURCE_USERNAME=gestauto
      - SPRING_DATASOURCE_PASSWORD=gestauto123
      - SPRING_RABBITMQ_HOST=host.docker.internal
      - SPRING_RABBITMQ_PORT=5672
      - SPRING_RABBITMQ_USERNAME=gestauto
      - SPRING_RABBITMQ_PASSWORD=gestauto123
      - CLOUDFLARE_R2_ENDPOINT=${CLOUDFLARE_R2_ENDPOINT}
      - CLOUDFLARE_R2_ACCESS_KEY=${CLOUDFLARE_R2_ACCESS_KEY}
      - CLOUDFLARE_R2_SECRET_KEY=${CLOUDFLARE_R2_SECRET_KEY}
      - CLOUDFLARE_R2_BUCKET_NAME=${CLOUDFLARE_R2_BUCKET_NAME:vehicle-photos}
    ports:
      - "8080:8080"
    extra_hosts:
      - "host.docker.internal:host-gateway"
    networks:
      - gestauto-network

networks:
  gestauto-network:
    driver: bridge
    external: true
```

### Script de Setup do Schema no PostgreSQL

```sql
-- setup_vehicle_evaluation_schema.sql
-- Execute este script no PostgreSQL principal para criar o schema separado

-- Criar schema dedicado para vehicle evaluation
CREATE SCHEMA IF NOT EXISTS vehicle_evaluation;

-- Criar role específica para o serviço (opcional)
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'vehicle_evaluation_service') THEN

      CREATE ROLE vehicle_evaluation_service LOGIN PASSWORD 've_service_2024!';
   END IF;
END
$do$;

-- Conceder permissões ao schema
GRANT USAGE ON SCHEMA vehicle_evaluation TO vehicle_evaluation_service;
GRANT CREATE ON SCHEMA vehicle_evaluation TO vehicle_evaluation_service;

-- Conceder permissões ao usuário gestauto (existente)
GRANT USAGE ON SCHEMA vehicle_evaluation TO gestauto;
GRANT CREATE ON SCHEMA vehicle_evaluation TO gestauto;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA vehicle_evaluation TO gestauto;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA vehicle_evaluation TO gestauto;

-- Configurar search path para o usuário do serviço
ALTER ROLE vehicle_evaluation_service SET search_path TO vehicle_evaluation, public;
ALTER ROLE gestauto SET search_path TO public, vehicle_evaluation;

-- Comentários de documentação
COMMENT ON SCHEMA vehicle_evaluation IS 'Schema for Vehicle Evaluation bounded context';
```

### Migration Inicial (Flyway)

```sql
-- V1__Create_vehicle_evaluations_table.sql
-- Criar tabela no schema vehicle_evaluation
CREATE TABLE IF NOT EXISTS vehicle_evaluation.vehicle_evaluations (
    id BIGSERIAL PRIMARY KEY,
    evaluation_id UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
    evaluator_id UUID NOT NULL,
    plate VARCHAR(7) UNIQUE NOT NULL,
    fipe_code VARCHAR(20) NOT NULL,
    brand VARCHAR(50) NOT NULL,
    model VARCHAR(100) NOT NULL,
    version VARCHAR(100),
    year INTEGER NOT NULL,
    mileage INTEGER NOT NULL,
    color VARCHAR(50),
    fuel_type VARCHAR(20),
    gearbox VARCHAR(20),
    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT',
    fipe_price DECIMAL(12,2),
    depreciation_amount DECIMAL(12,2),
    safety_margin DECIMAL(12,2),
    profit_margin DECIMAL(12,2),
    suggested_value DECIMAL(12,2),
    approved_value DECIMAL(12,2),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    submitted_at TIMESTAMP,
    reviewed_at TIMESTAMP,
    reviewer_id UUID,
    rejection_reason TEXT,
    internal_notes TEXT,
    external_notes TEXT,
    valid_until TIMESTAMP,
    validation_token VARCHAR(500)
);

-- V2__Create_evaluation_photos_table.sql
CREATE TABLE IF NOT EXISTS vehicle_evaluation.evaluation_photos (
    id BIGSERIAL PRIMARY KEY,
    photo_id UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL,
    type VARCHAR(20) NOT NULL,
    url VARCHAR(500) NOT NULL,
    thumbnail_url VARCHAR(500),
    uploaded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB,
    FOREIGN KEY (evaluation_id) REFERENCES vehicle_evaluation.vehicle_evaluations(evaluation_id) ON DELETE CASCADE
);

-- V3__Create_evaluation_checklists_table.sql
CREATE TABLE IF NOT EXISTS vehicle_evaluation.evaluation_checklists (
    id BIGSERIAL PRIMARY KEY,
    checklist_id UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL UNIQUE,
    bodywork_data JSONB NOT NULL DEFAULT '{}',
    tires_data JSONB NOT NULL DEFAULT '{}',
    interior_data JSONB NOT NULL DEFAULT '{}',
    mechanical_data JSONB NOT NULL DEFAULT '{}',
    electronics_data JSONB NOT NULL DEFAULT '{}',
    documents_data JSONB NOT NULL DEFAULT '{}',
    conservation_score INTEGER,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    FOREIGN KEY (evaluation_id) REFERENCES vehicle_evaluation.vehicle_evaluations(evaluation_id) ON DELETE CASCADE
);

-- V4__Create_depreciation_items_table.sql
CREATE TABLE IF NOT EXISTS vehicle_evaluation.depreciation_items (
    id BIGSERIAL PRIMARY KEY,
    depreciation_id UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL,
    category VARCHAR(50) NOT NULL,
    description VARCHAR(200) NOT NULL,
    percentage DECIMAL(5,2) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    FOREIGN KEY (evaluation_id) REFERENCES vehicle_evaluation.vehicle_evaluations(evaluation_id) ON DELETE CASCADE
);

-- V5__Create_evaluation_accessories_table.sql
CREATE TABLE IF NOT EXISTS vehicle_evaluation.evaluation_accessories (
    id BIGSERIAL PRIMARY KEY,
    accessory_id UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL,
    accessory_code VARCHAR(20) NOT NULL,
    description VARCHAR(100) NOT NULL,
    value_impact DECIMAL(5,2) DEFAULT 0,
    FOREIGN KEY (evaluation_id) REFERENCES vehicle_evaluation.vehicle_evaluations(evaluation_id) ON DELETE CASCADE
);

-- V6__Create_checklist_critical_issues_table.sql
CREATE TABLE IF NOT EXISTS vehicle_evaluation.checklist_critical_issues (
    id BIGSERIAL PRIMARY KEY,
    checklist_id UUID NOT NULL,
    issue TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (checklist_id) REFERENCES vehicle_evaluation.evaluation_checklists(checklist_id) ON DELETE CASCADE
);

-- V7__Create_indexes.sql
-- Índices para vehicle_evaluations
CREATE INDEX IF NOT EXISTS idx_evaluations_status ON vehicle_evaluation.vehicle_evaluations(status);
CREATE INDEX IF NOT EXISTS idx_evaluations_evaluator ON vehicle_evaluation.vehicle_evaluations(evaluator_id);
CREATE INDEX IF NOT EXISTS idx_evaluations_plate ON vehicle_evaluation.vehicle_evaluations(plate);
CREATE INDEX IF NOT EXISTS idx_evaluations_created ON vehicle_evaluation.vehicle_evaluations(created_at);
CREATE INDEX IF NOT EXISTS idx_evaluations_fipe_code ON vehicle_evaluation.vehicle_evaluations(fipe_code);
CREATE INDEX IF NOT EXISTS idx_evaluations_evaluation_id ON vehicle_evaluation.vehicle_evaluations(evaluation_id);

-- Índices para evaluation_photos
CREATE INDEX IF NOT EXISTS idx_photos_evaluation ON vehicle_evaluation.evaluation_photos(evaluation_id);
CREATE INDEX IF NOT EXISTS idx_photos_type ON vehicle_evaluation.evaluation_photos(type);
CREATE INDEX IF NOT EXISTS idx_photos_photo_id ON vehicle_evaluation.evaluation_photos(photo_id);

-- Índices para evaluation_checklists
CREATE INDEX IF NOT EXISTS idx_checklists_evaluation_id ON vehicle_evaluation.evaluation_checklists(evaluation_id);
CREATE INDEX IF NOT EXISTS idx_checklists_checklist_id ON vehicle_evaluation.evaluation_checklists(checklist_id);

-- Índices para depreciation_items
CREATE INDEX IF NOT EXISTS idx_depreciations_evaluation ON vehicle_evaluation.depreciation_items(evaluation_id);
CREATE INDEX IF NOT EXISTS idx_depreciations_id ON vehicle_evaluation.depreciation_items(depreciation_id);

-- Índices para evaluation_accessories
CREATE INDEX IF NOT EXISTS idx_accessories_evaluation ON vehicle_evaluation.evaluation_accessories(evaluation_id);
CREATE INDEX IF NOT EXISTS idx_accessories_id ON vehicle_evaluation.evaluation_accessories(accessory_id);

-- Índices para checklist_critical_issues
CREATE INDEX IF NOT EXISTS idx_issues_checklist ON vehicle_evaluation.checklist_critical_issues(checklist_id);

-- V8__Create_sequences.sql
-- Criar sequências para IDs (se necessário)
CREATE SEQUENCE IF NOT EXISTS vehicle_evaluation.vehicle_evaluations_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE IF NOT EXISTS vehicle_evaluation.evaluation_photos_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
```

## Configurações Adicionais

### application-docker.yml (para desenvolvimento)

```yaml
spring:
  profiles:
    active: docker
  jpa:
    show-sql: true
    properties:
      hibernate:
        format_sql: true
        default_schema: vehicle_evaluation
        boot.allow_jdbc_metadata_access: false
  datasource:
    url: jdbc:postgresql://localhost:5432/gestauto
    username: gestauto
    password: gestauto123
    hikari:
      maximum-pool-size: 10
      minimum-idle: 5
      idle-timeout: 300000
  rabbitmq:
    host: localhost
    port: 5672
    username: gestauto
    password: gestauto123
  sql:
    init:
      platform: postgresql

logging:
  level:
    com.gestauto: DEBUG
    org.springframework.transaction: DEBUG
    org.hibernate.SQL: DEBUG
    org.hibernate.type.descriptor.sql.BasicBinder: TRACE

app:
  database:
    schema: vehicle_evaluation
  cloudflare-r2:
    endpoint: ${CLOUDFLARE_R2_ENDPOINT:https://your-account.r2.cloudflarestorage.com}
    access-key: ${CLOUDFLARE_R2_ACCESS_KEY:your-access-key}
    secret-key: ${CLOUDFLARE_R2_SECRET_KEY:your-secret-key}
    bucket-name: ${CLOUDFLARE_R2_BUCKET_NAME:vehicle-photos}
  fipe:
    base-url: ${FIPE_BASE_URL:https://parallelum.com.br/fipe/api/v1}
    cache-expiration-hours: ${FIPE_CACHE_EXPIRATION_HOURS:24}
  jwt:
    secret: ${JWT_SECRET:your-secret-key}
    validation-expiration-hours: 72
```

### pom.xml

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0
         https://maven.apache.org/xsd/maven-4.0.0.xsd">

    <modelVersion>4.0.0</modelVersion>

    <parent>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-parent</artifactId>
        <version>3.2.0</version>
        <relativePath/>
    </parent>

    <groupId>com.gestauto</groupId>
    <artifactId>vehicle-evaluation</artifactId>
    <version>1.0.0</version>
    <name>GestAuto Vehicle Evaluation Service</name>
    <description>Microserviço de avaliação de veículos seminovos</description>

    <properties>
        <java.version>21</java.version>
        <testcontainers.version>1.19.3</testcontainers.version>
        <aws-sdk.version>2.21.29</aws-sdk.version>
        <mapstruct.version>1.5.5.Final</mapstruct.version>
        <lombok.version>1.18.30</lombok.version>
    </properties>

    <dependencies>
        <!-- Spring Boot Starters -->
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-web</artifactId>
        </dependency>

        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-jpa</artifactId>
        </dependency>

        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-redis</artifactId>
        </dependency>

        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-amqp</artifactId>
        </dependency>

        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-oauth2-resource-server</artifactId>
        </dependency>

        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-validation</artifactId>
        </dependency>

        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-actuator</artifactId>
        </dependency>

        <!-- Database -->
        <dependency>
            <groupId>org.postgresql</groupId>
            <artifactId>postgresql</artifactId>
            <scope>runtime</scope>
        </dependency>

        <dependency>
            <groupId>org.flywaydb</groupId>
            <artifactId>flyway-core</artifactId>
        </dependency>

        <!-- AWS SDK -->
        <dependency>
            <groupId>software.amazon.awssdk</groupId>
            <artifactId>s3</artifactId>
            <version>${aws-sdk.version}</version>
        </dependency>

        <!-- JSON Processing -->
        <dependency>
            <groupId>com.fasterxml.jackson.core</groupId>
            <artifactId>jackson-databind</artifactId>
        </dependency>

        <dependency>
            <groupId>com.fasterxml.jackson.datatype</groupId>
            <artifactId>jackson-datatype-jsr310</artifactId>
        </dependency>

        <!-- MapStruct -->
        <dependency>
            <groupId>org.mapstruct</groupId>
            <artifactId>mapstruct</artifactId>
            <version>${mapstruct.version}</version>
        </dependency>

        <!-- JWT -->
        <dependency>
            <groupId>io.jsonwebtoken</groupId>
            <artifactId>jjwt-api</artifactId>
            <version>0.11.5</version>
        </dependency>
        <dependency>
            <groupId>io.jsonwebtoken</groupId>
            <artifactId>jjwt-impl</artifactId>
            <version>0.11.5</version>
            <scope>runtime</scope>
        </dependency>
        <dependency>
            <groupId>io.jsonwebtoken</groupId>
            <artifactId>jjwt-jackson</artifactId>
            <version>0.11.5</version>
            <scope>runtime</scope>
        </dependency>

        <!-- PDF Generation -->
        <dependency>
            <groupId>com.itextpdf</groupId>
            <artifactId>itext7-core</artifactId>
            <version>7.2.5</version>
            <type>pom</type>
        </dependency>

        <!-- Monitoring -->
        <dependency>
            <groupId>io.micrometer</groupId>
            <artifactId>micrometer-registry-prometheus</artifactId>
        </dependency>

        <dependency>
            <groupId>io.micrometer</groupId>
            <artifactId>micrometer-tracing-bridge-brave</artifactId>
        </dependency>

        <!-- Development Tools -->
        <dependency>
            <groupId>org.projectlombok</groupId>
            <artifactId>lombok</artifactId>
            <version>${lombok.version}</version>
            <optional>true</optional>
        </dependency>

        <!-- Test Dependencies -->
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-test</artifactId>
            <scope>test</scope>
        </dependency>

        <dependency>
            <groupId>org.springframework.amqp</groupId>
            <artifactId>spring-rabbit-test</artifactId>
            <scope>test</scope>
        </dependency>

        <dependency>
            <groupId>org.testcontainers</groupId>
            <artifactId>junit-jupiter</artifactId>
            <scope>test</scope>
        </dependency>

        <dependency>
            <groupId>org.testcontainers</groupId>
            <artifactId>postgresql</artifactId>
            <scope>test</scope>
        </dependency>

        <dependency>
            <groupId>org.testcontainers</groupId>
            <artifactId>rabbitmq</artifactId>
            <scope>test</scope>
        </dependency>

        <dependency>
            <groupId>com.squareup.okhttp3</groupId>
            <artifactId>mockwebserver</artifactId>
            <version>4.12.0</version>
            <scope>test</scope>
        </dependency>
    </dependencies>

    <build>
        <plugins>
            <plugin>
                <groupId>org.springframework.boot</groupId>
                <artifactId>spring-boot-maven-plugin</artifactId>
                <configuration>
                    <excludes>
                        <exclude>
                            <groupId>org.projectlombok</groupId>
                            <artifactId>lombok</artifactId>
                        </exclude>
                    </excludes>
                </configuration>
            </plugin>

            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-compiler-plugin</artifactId>
                <configuration>
                    <source>21</source>
                    <target>21</target>
                    <annotationProcessorPaths>
                        <path>
                            <groupId>org.projectlombok</groupId>
                            <artifactId>lombok</artifactId>
                            <version>${lombok.version}</version>
                        </path>
                        <path>
                            <groupId>org.mapstruct</groupId>
                            <artifactId>mapstruct-processor</artifactId>
                            <version>${mapstruct.version}</version>
                        </path>
                    </annotationProcessorPaths>
                </configuration>
            </plugin>

            <plugin>
                <groupId>org.flywaydb</groupId>
                <artifactId>flyway-maven-plugin</artifactId>
                <configuration>
                    <url>jdbc:postgresql://localhost:5432/gestauto</url>
                    <user>gestauto</user>
                    <password>gestauto123</password>
                    <defaultSchema>vehicle_evaluation</defaultSchema>
                    <schemas>
                        <schema>vehicle_evaluation</schema>
                    </schemas>
                    <locations>
                        <location>classpath:db/migration</location>
                    </locations>
                    <validateOnMigrate>true</validateOnMigrate>
                    <cleanDisabled>true</cleanDisabled>
                </configuration>
            </plugin>

            <plugin>
                <groupId>org.jacoco</groupId>
                <artifactId>jacoco-maven-plugin</artifactId>
                <version>0.8.8</version>
                <executions>
                    <execution>
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
                </executions>
            </plugin>
        </plugins>
    </build>

    <profiles>
        <profile>
            <id>docker</id>
            <activation>
                <property>
                    <name>env</name>
                    <value>docker</value>
                </property>
            </activation>
        </profile>
    </profiles>
</project>
```

## Considerações sobre Repository Pattern com Mappers

### Vantagens da Abordagem

1. **Domínio Puro**
   - Entidades de domínio sem contaminação de annotations JPA
   - Foco total em regras de negócio e comportamentos
   - Testabilidade simplificada (sem necessidade de BD)
   - Evolução independente do modelo de domínio

2. **Separation of Concerns**
   - Infraestrutura de persistência isolada do domínio
   - Mappers centralizam a conversão entre modelos
   - Mudanças no schema não afetam o domínio
   - Possibilidade de múltiplas estratégias de persistência

3. **Flexibilidade**
   - Facilidade para trocar de tecnologia (JPA → NoSQL)
   - Otimizações de BD sem impacto no domínio
   - Mapeamento complexo centralizado
   - Suporte a diferentes models para diferentes casos de uso

### Trade-offs

1. **Complexidade Adicional**
   - Mais classes para manter (Entities + Mappers)
   - Boilerplate code para conversões
   - Curva de aprendizado inicial

2. **Performance**
   - Overhead de mapeamento entre objetos
   - Múltiplas cópias de objetos em memória
   - Necessidade de otimizar queries específicas

### Melhores Práticas

1. **Implementação de Mappers**
   - Usar ModelMapper para mapeamentos simples
   - Implementar lógica customizada para casos complexos
   - Manter mapeadores imutáveis e stateless
   - Testar todas as conversões

2. **Gerenciamento de Ciclo de Vida**
   - Atualizar entidades JPA apenas quando necessário
   - Evitar lazy loading fora da camada de infra
   - Limpar eventos após publicação
   - Usar @Transactional nos repositories

3. **Performance**
   - Batch operations para múltiplas entidades
   - Projections para consultas específicas
   - Caching de mapeamentos complexos
   - Monitorar overhead dos mappers

## Conclusão

Esta especificação técnica fornece uma arquitetura robusta e escalável para o microserviço de avaliação de veículos seminovos, seguindo todos os padrões estabelecidos no projeto GestAuto e adaptada para o ecossistema Java/Spring Boot. A implementação proposta atende a todos os requisitos funcionais e não-funcionais definidos no PRD, com especial atenção à performance, segurança e manutenibilidade.

A adoção do **Repository Pattern com Mappers** garante um domínio puro e desacoplado da infraestrutura, facilitando testes, evolução e manutenção do código. Embora adicione alguma complexidade inicial, os benefícios a longo prazo em termos de flexibilidade e separação de responsabilidades justificam a abordagem.

### Principais Decisões de Infraestrutura

1. **Aproveitamento da Infraestrutura Existente**: O serviço utiliza o PostgreSQL e RabbitMQ já configurados no projeto, reduzindo complexidade operacional
2. **Schema Dedicado**: Uso do schema `vehicle_evaluation` para isolamento de dados sem necessidade de novo banco
3. **Credenciais Centralizadas**: Utilização das credenciais padrão (gestauto/gestauto123) já configuradas

### Resumo das Configurações

- **PostgreSQL**: `localhost:5432/gestauto` com schema `vehicle_evaluation`
- **RabbitMQ**: `localhost:5672` com usuário `gestauto`
- **Migrations**: Flyway configurado para o schema específico
- **Docker**: Imagem apenas da aplicação, conectando-se aos serviços existentes

A modularidade da solução permite evolução futura, como a adição de machine learning para precificação ou integração com SNG/Checkauto, sem impacto na arquitetura base.