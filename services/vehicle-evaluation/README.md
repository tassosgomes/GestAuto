# GestAuto - Vehicle Evaluation Service

Microserviço de avaliação de veículos seminovos do sistema GestAuto.

## Visão Geral

Este microserviço é responsável por gerenciar o processo completo de avaliação de veículos usados, incluindo:

- Registro de informações do veículo
- Upload e gestão de fotos
- Checklist técnico de avaliação
- Cálculo automático de valor baseado em FIPE
- Sistema de aprovação gerencial
- Geração de laudos em PDF

## Arquitetura

O serviço segue os padrões de arquitetura do GestAuto:

- **Domain-Driven Design (DDD)** com separação clara de responsabilidades
- **Repository Pattern** com domínio puro (sem annotations JPA)
- **CQRS** com Commands/Queries separados
- **Clean Architecture** com 5 camadas

### Estrutura de Módulos

```
vehicle-evaluation/
├── domain/          # Entidades de domínio puras, Value Objects, regras de negócio
├── application/     # Use cases, Commands, Queries, DTOs, Mappers
├── api/            # Controllers REST, configuração, classe principal
└── infra/          # Repositories JPA, migrations, adapters externos
```

## Stack Tecnológico

- **Java 21** com **Spring Boot 3.2**
- **PostgreSQL** (schema `vehicle_evaluation`)
- **RabbitMQ** para eventos de integração
- **Redis** para cache
- **Cloudflare R2** (S3-compatible) para armazenamento de fotos
- **Flyway** para migrations
- **JUnit 5 + TestContainers** para testes

## Setup Local

### Pré-requisitos

- Java 21+
- Maven 3.8+
- Docker e Docker Compose
- PostgreSQL e RabbitMQ (via docker-compose do GestAuto)

### Passos

1. **Iniciar infraestrutura existente**:
   ```bash
   cd /home/tsgomes/github-tassosgomes/GestAuto
   docker-compose up -d
   ```

2. **Criar schema no PostgreSQL**:
   ```sql
   CREATE SCHEMA IF NOT EXISTS vehicle_evaluation;
   ```

3. **Executar o serviço**:
   ```bash
   cd services/vehicle-evaluation
   ./mvnw spring-boot:run -Dspring-boot.run.profiles=dev
   ```

### Com Docker

1. **Subir serviço completo**:
   ```bash
   cd services/vehicle-evaluation
   docker-compose -f docker-compose.dev.yml up -d
   ```

## Configuração

### Variáveis de Ambiente

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `CLOUDFLARE_R2_ENDPOINT` | Endpoint do Cloudflare R2 | - |
| `CLOUDFLARE_R2_BUCKET` | Bucket para fotos | `vehicle-evaluation-photos` |
| `CLOUDFLARE_R2_ACCESS_KEY` | Access key | - |
| `CLOUDFLARE_R2_SECRET_KEY` | Secret key | - |
| `JWT_SECRET` | Secret para tokens JWT | - |

### Perfis

- **default**: Desenvolvimento local
- **dev**: Desenvolvimento com logs detalhados
- **docker**: Execução em container Docker
- **prod**: Produção (otimizações de performance)

## API Documentation

Após iniciar o serviço, acesse:

- **Swagger UI**: http://localhost:8081/api/swagger-ui.html
- **OpenAPI JSON**: http://localhost:8081/api/v3/api-docs
- **Health Check**: http://localhost:8081/api/actuator/health

## Principais Endpoints

- `POST /api/evaluations` - Criar nova avaliação
- `GET /api/evaluations/{id}` - Buscar avaliação
- `PUT /api/evaluations/{id}/submit` - Submeter para aprovação
- `PUT /api/evaluations/{id}/approve` - Aprovar avaliação
- `PUT /api/evaluations/{id}/reject` - Rejeitar avaliação
- `GET /api/evaluations` - Listar avaliações (com filtros)
- `POST /api/evaluations/{id}/photos` - Upload de fotos
- `GET /api/evaluations/{id}/report` - Download do laudo PDF

## Processo de Avaliação

1. **Criação**: Avaliador registra informações básicas do veículo
2. **Fotos**: Upload de 15 fotos obrigatórias (frentes, laterais, interior, etc)
3. **Checklist**: Preenchimento detalhado do estado mecânico e estético
4. **Cálculo**: Sistema calcula valor base (FIPE % liquidez) - depreciações
5. **Submissão**: Avaliação enviada para aprovação gerencial
6. **Decisão**: Gerente aprova ou rejeita com justificativa
7. **Laudo**: Geração automática do PDF com validade de 72h

## Testes

### Executar todos os testes
```bash
./mvnw test
```

### Testes + cobertura (JaCoCo >= 90%)
```bash
./mvnw verify
```

### Testes de integração (requer Docker)

Os testes `*IT.java` rodam via `maven-failsafe-plugin` durante `verify`.

### Testes de contrato (Pact)

- Pacts gerados em: `infra/target/pacts/`

### Testes de mutação (PIT)

```bash
./mvnw -Pmutation verify
```

## Deploy

### Build do JAR
```bash
./mvnw clean package -DskipTests
```

### Build da imagem Docker
```bash
docker build -t gestauto/vehicle-evaluation:latest .
```

## Monitoramento

- **Actuator**: http://localhost:8081/api/actuator
- **Metrics**: http://localhost:8081/api/actuator/metrics
- **Prometheus**: http://localhost:8081/api/actuator/prometheus

## Logs

- **Console**: Configurado com structured logging
- **File**: `/var/log/vehicle-evaluation/application.log` (em produção)
- **Levels**: Adjustáveis por perfil (dev: DEBUG, prod: INFO)

## Integrações

### com Módulo Commercial

- **Evento**: `VehicleEvaluationCompleted`
- **Exchange**: `gestauto.events`
- **Routing Key**: `vehicle.evaluation.completed`

### API FIPE

- **Base URL**: https://parallelum.com.br/fipe/api/v1/
- **Cache TTL**: 24 horas em Redis
- **Rate Limit**: Configurado para evitar timeouts

## Roadmap

### v1.0.0 (Current)
- [x] Setup do projeto e infraestrutura
- [x] Schema do banco de dados
- [x] Configuração básica
- [ ] Entidades de domínio
- [ ] Repositories e mappers
- [ ] Endpoints REST
- [ ] Integração FIPE
- [ ] Upload de fotos
- [ ] Geração de PDF

### v1.1.0 (Planned)
- [ ] Dashboard gerencial
- [ ] Relatórios e analytics
- [ ] Integração com módulo Estoque
- [ ] Mobile app para avaliadores

## Contribuição

1. Fork o projeto
2. Crie branch para feature (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -m 'Add nova funcionalidade'`)
4. Push para branch (`git push origin feature/nova-funcionalidade`)
5. Abra Pull Request

## Suporte

- **Issues**: GitHub Issues do projeto
- **Documentação**: Confluence da GestAuto
- **Time**: time@gestauto.com.br