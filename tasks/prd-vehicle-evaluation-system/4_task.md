## markdown

## status: pending

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
</task_context>

# Tarefa 4.0: Implementação de Documentação Fotográfica

## Visão Geral

Implementar sistema de upload, validação e armazenamento das 15 fotos obrigatórias por avaliação, com integração com Cloudflare R2 (S3-compatible), validação de tipos específicos de fotos, geração de thumbnails e metadados.

<requirements>
- Upload de exatamente 15 fotos obrigatórias
- Tipos definidos: 4 externas, 4 internas, 3 painel, 2 motor, 2 porta-malas
- Armazenamento em Cloudflare R2 (S3 SDK)
- Geração automática de thumbnails
- Validação de qualidade mínima (resolução, formato)
- Progresso de upload em tempo real
- Substituição individual de fotos antes da finalização
- Metadados (data/hora, dispositivo)

</requirements>

## Subtarefas

- [ ] 4.1 Configurar SDK AWS S3 para Cloudflare R2
- [ ] 4.2 Implementar ImageStorageService com S3
- [ ] 4.3 Criar validação de tipos de foto obrigatórios
- [ ] 4.4 Implementar AddPhotosCommand e Handler
- [ ] 4.5 Implementar RemovePhotoCommand e Handler
- [ ] 4.6 Criar endpoint POST /api/v1/evaluations/{id}/photos
- [ ] 4.7 Implementar geração de thumbnails
- [ ] 4.8 Adicionar validação de arquivos (tamanho, formato)
- [ ] 4.9 Implementar serviço de compressão (opcional)

## Detalhes de Implementação

### Tipos de Fotos Obrigatórias

```java
public enum PhotoType {
    // Externas (4)
    EXTERIOR_FRONT("Frente Externa"),
    EXTERIOR_REAR("Traseira Externa"),
    EXTERIOR_LEFT("Lateral Esquerda"),
    EXTERIOR_RIGHT("Lateral Direita"),

    // Internas (4)
    INTERIOR_FRONT("Painel Frontal"),
    INTERIOR_SEATS("Bancos Dianteiros"),
    INTERIOR_REAR("Bancos Traseiros"),
    INTERIOR_TRUNK("Porta-malas Interno"),

    // Painel (3)
    DASHBOARD_SPEED("Painel - Velocímetro"),
    DASHBOARD_INFO("Painel - Central"),
    DASHBOARD_AC("Painel - Ar Condicionado"),

    // Motor (2)
    ENGINE_BAY("Motor - Vista Superior"),
    ENGINE_DETAILS("Motor - Detalhes"),

    // Porta-malas (2)
    TRUNK_OPEN("Porta-malas Aberto"),
    TRUCK_SPARE("Porta-malas - Estepe");
}
```

### Serviço de Armazenamento

```java
@Service
public class ImageStorageServiceImpl implements ImageStorageService {
    private final S3Client s3Client;
    private final String bucketName;

    @Override
    public String uploadImage(InputStream imageStream, String fileName, String contentType) {
        PutObjectRequest request = PutObjectRequest.builder()
            .bucket(bucketName)
            .key(fileName)
            .contentType(contentType)
            .build();

        s3Client.putObject(request, RequestBody.fromInputStream(imageStream, imageStream.available()));
        return generateUrl(fileName);
    }

    @Override
    public ImageUploadResult uploadEvaluationPhotos(UUID evaluationId, Map<String, MultipartFile> photos) {
        // 1. Validar quantidade e tipos
        // 2. Gerar URLs únicas
        // 3. Upload em paralelo
        // 4. Gerar thumbnails
        // 5. Retornar resultados
    }
}
```

### Handler de Fotos

```java
@Component
public class AddPhotosHandler implements CommandHandler<AddPhotosCommand, Void> {
    private final VehicleEvaluationRepository evaluationRepository;
    private final ImageStorageService imageStorageService;
    private final EvaluationPhotoRepository photoRepository;

    @Override
    @Transactional
    public Void handle(AddPhotosCommand command) {
        // 1. Buscar avaliação
        // 2. Validar limite de 15 fotos
        // 3. Validar tipos obrigatórios
        // 4. Fazer upload para R2
        // 5. Salvar metadados no BD
        // 6. Atualizar avaliação
        // 7. Publicar evento PhotosUploadedEvent
        return null;
    }
}
```

### Validações

- Exatamente 15 fotos por avaliação
- Formatos aceitos: JPEG, PNG (máximo 10MB cada)
- Resolução mínima: 800x600 pixels
- Tipos obrigatórios devem estar presentes
- Fotos podem ser substituídas antes da aprovação

## Critérios de Sucesso

- [x] Upload de 15 fotos funciona simultaneamente
- [x] Validação de tipos obrigatórios funcionando
- [x] Thumbnails gerados automaticamente
- [x] URLs de acesso geradas corretamente
- [x] Progresso de upload visível para usuário
- [x] Fotos armazenadas com estrutura organizeda: evaluations/{id}/photos/
- [x] Substituição de fotos individuais funciona
- [x] Metadados salvos (data/hora, device)
- [x] Integração com R2 funcionando

## Sequenciamento

- Bloqueado por: 2.0 (Domínio e Database)
- Desbloqueia: 5.0, 7.0
- Paralelizável: Sim (com 3.0 e 5.0)

## Tempo Estimado

- Setup R2/S3: 4 horas
- Upload/Download service: 8 horas
- Thumbnails/Validação: 6 horas
- Testes: 4 horas
- Total: 22 horas