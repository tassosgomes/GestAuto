# Relatório de Análise Arquitetural
## Módulo Comercial GestAuto

**Relatório Gerado:** 2026-01-23-09:09:14
**Caminho do Projeto:** /home/tsgomes/github-tassosgomes/GestAuto/services/commercial
**Estilo de Arquitetura:** Arquitetura Limpa (Clean Architecture) com CQRS
**Framework:** .NET 8.0

---

## 1. Resumo Executivo

O Módulo Comercial GestAuto é um microsserviço que implementa um padrão de Arquitetura Limpa com CQRS (Segregação de Responsabilidade de Comando e Consulta) para gerenciar operações comerciais em um sistema de concessionária de veículos. O módulo lida com leads, propostas, test-drives, avaliações de veículos usados e pedidos através de uma arquitetura bem estruturada de quatro camadas.

**Principais Características Arquiteturais:**
- Arquitetura Limpa com inversão de dependência estrita
- Padrão CQRS com manipuladores de comando e consulta separados
- Design Orientado ao Domínio (DDD) com agregados, objetos de valor e eventos de domínio
- Arquitetura orientada a eventos usando RabbitMQ para comunicação entre serviços
- Padrão Outbox para publicação confiável de eventos
- Autenticação baseada em token via Keycloak JWT
- PostgreSQL como armazenamento de dados primário com Entity Framework Core 8
- Cobertura de testes abrangente (unitários, integração, E2E)

**Stack Tecnológico:**
- .NET 8.0 / ASP.NET Core Web API
- PostgreSQL 15 com Entity Framework Core 8
- RabbitMQ 3.12 para mensagens assíncronas
- Keycloak para autenticação/autorização
- FluentValidation para validação de entrada
- Serilog para log estruturado
- Swagger/OpenAPI para documentação REST
- Saunter para documentação de eventos AsyncAPI

---

## 2. Visão Geral da Arquitetura

### 2.1 Estilo Arquitetural

O módulo implementa princípios de **Arquitetura Limpa** com clara separação de preocupações e inversão de dependência. A arquitetura segue o padrão "Onion" (Cebola) onde as dependências fluem para dentro em direção ao núcleo do domínio.

**Padrão de Arquitetura:** Arquitetura Limpa em Camadas com CQRS
**Padrão de Comunicação:** Síncrono REST + Assíncrono Orientado a Eventos
**Padrão de Acesso a Dados:** Repositório + Unit of Work
**Padrão de Mensageria:** Publicar-Assinar (Publish-Subscribe) com Outbox

### 2.2 Estrutura de Alto Nível

```
┌─────────────────────────────────────────────────────────────┐
│                     Camada de Apresentação                   │
│                  (1-Services/API)                            │
│  Controllers → Middleware → Autenticação → Documentação      │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Camada de Aplicação                       │
│              (2-Application)                                 │
│   Comandos → Consultas → Handlers → Validadores → DTOs       │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                      Camada de Domínio                       │
│                   (3-Domain)                                 │
│  Entidades → Objetos de Valor → Eventos de Domínio → Serviços│
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                   Camada de Infraestrutura                   │
│                    (4-Infra)                                 │
│  DbContext → Repositórios → Mensageria → Migrações           │
└─────────────────────────────────────────────────────────────┘
```

### 2.3 Fluxo de Dependência

**Dependências por Camada:**

1. **Camada de API (Serviços)**
   - Referências: Aplicação, Infraestrutura
   - Propósito: Tratamento de requisições HTTP, autenticação, roteamento

2. **Camada de Aplicação**
   - Referências: Domínio, Infraestrutura
   - Propósito: Orquestração de lógica de negócios, handlers CQRS, validação

3. **Camada de Domínio**
   - Referências: Nenhuma (núcleo)
   - Propósito: Entidades de negócio, lógica de domínio, eventos de domínio

4. **Camada de Infraestrutura**
   - Referências: Domínio
   - Propósito: Persistência de dados, serviços externos, mensageria

**Regra de Dependência:** Todas as dependências apontam para a camada de Domínio. O Domínio tem zero dependências das camadas externas.

---

## 3. Análise de Camadas

### 3.1 Camada de Apresentação (1-Services/API)

**Localização:** `1-Services/GestAuto.Commercial.API/`

**Responsabilidades:**
- Tratamento de requisições/respostas HTTP
- Autenticação e autorização (Keycloak JWT)
- Validação de requisições através do FluentValidation
- Middleware de tratamento de exceções
- Documentação da API (Swagger/OpenAPI)
- Documentação AsyncAPI (Saunter)
- Hospedagem de serviço em segundo plano (OutboxProcessor)

**Componentes Chave:**

| Componente | Tipo | Linhas | Propósito |
|------------|------|--------|-----------|
| LeadController | Controller | 335 | Endpoints de gerenciamento de Leads |
| ProposalController | Controller | 363 | Endpoints de gerenciamento de Propostas |
| TestDriveController | Controller | 282 | Agendamento e gestão de Test-drives |
| EvaluationController | Controller | 174 | Endpoints de avaliação de veículos usados |
| OrderController | Controller | 122 | Endpoints de gerenciamento de Pedidos |
| PaymentMethodsController | Controller | 70 | Configuração de métodos de pagamento |
| DashboardController | Controller | 61 | Agregação de dados para Dashboard |
| ExceptionHandlerMiddleware | Middleware | - | Tratamento global de exceções |
| OutboxProcessorService | BackgroundService | - | Publicação de eventos da outbox |
| SalesPersonFilterService | Service | - | Contexto do usuário e filtro de autorização |

**Autenticação & Autorização:**
- Autenticação JWT Bearer via Keycloak
- Políticas baseadas em função: `SalesPerson`, `Manager`
- Normalização de claims para claims de `roles`
- Suporte para array JSON de roles do Keycloak

**Documentação da API:**
- Swagger UI em `/swagger` (Endpoints REST)
- AsyncAPI UI em `/asyncapi` (documentação de eventos)
- Especificação OpenAPI em `/swagger/v1/swagger.json`
- AsyncAPI YAML em `/asyncapi.yaml`

### 3.2 Camada de Aplicação (2-Application)

**Localização:** `2-Application/GestAuto.Commercial.Application/`

**Responsabilidades:**
- Definições de comando/consulta (padrão CQRS)
- DTOs de Requisição/Resposta
- Orquestração de lógica de negócios
- Validação de entrada via FluentValidation
- Coordenação de eventos de domínio

**Padrão de Arquitetura:** CQRS (Segregação de Responsabilidade de Comando e Consulta)

**Diretórios Chave:**

| Diretório | Propósito | Contagem de Arquivos |
|-----------|-----------|---------------------|
| Commands | Operações de escrita (CQRS) | 17 |
| Queries | Operações de leitura (CQRS) | 12 |
| Handlers | Manipuladores de comando/consulta | 21 |
| DTOs | Objetos de transferência de dados | 15 |
| Validators | Validadores FluentValidation | 12 |
| Interfaces | Contratos de handler e serviço | 4 |

**Implementação CQRS:**

- **Comandos:** Operações que modificam estado (Criar, Atualizar, Deletar)
  - CreateLeadCommand, UpdateLeadCommand, QualifyLeadCommand
  - CreateProposalCommand, AddProposalItemCommand, ApplyDiscountCommand
  - TestDriveCommands (Schedule, Complete, Cancel)
  - RequestEvaluationCommand

- **Consultas:** Operações que leem dados
  - GetLeadQuery, ListLeadsQuery
  - GetProposalQuery, ListProposalsQuery
  - GetDashboardDataQuery
  - TestDriveQueryHandlers

- **Handlers:** Manipuladores separados para comandos e consultas
  - Todos implementam `ICommandHandler<TCommand, TResponse>` ou `IQueryHandler<TQuery, TResponse>`
  - Handlers orquestram a lógica de negócios e coordenam operações de domínio

**Estratégia de Validação:**
- FluentValidation para todos os comandos e consultas
- Validação ocorre antes da execução do handler
- Validadores aplicam regras de negócio e integridade de dados

### 3.3 Camada de Domínio (3-Domain)

**Localização:** `3-Domain/GestAuto.Commercial.Domain/`

**Responsabilidades:**
- Entidades e agregados principais de negócio
- Encapsulamento de lógica de domínio
- Objetos de valor para segurança de tipos
- Eventos de domínio para notificações de negócio
- Serviços de domínio para operações complexas
- Interfaces de repositório (contratos)

**Padrão de Arquitetura:** Design Orientado ao Domínio (DDD)

**Componentes Chave:**

**Entidades (9 agregados):**

| Entidade | Propósito | Raiz de Agregado |
|----------|-----------|------------------|
| Lead | Gestão de leads de clientes | Sim |
| Interaction | Histórico de interações com lead | Não (parte de Lead) |
| Proposal | Gestão de propostas de venda | Sim |
| ProposalItem | Itens de linha em propostas | Não (parte de Proposal) |
| TestDrive | Agendamento de test-drive | Sim |
| UsedVehicleEvaluation | Solicitações de avaliação de veículo usado | Sim |
| Order | Pedidos de venda | Sim |
| PaymentMethodEntity | Configuração de método de pagamento | Sim |
| BaseEntity | Entidade base com ID, timestamps, eventos | Não (classe base) |

**Objetos de Valor (7):**
- Email (endereço de email validado)
- Phone (número de telefone validado)
- Money (valores monetários com moeda)
- LicensePlate (validação de placa de veículo)
- Qualification (dados de qualificação do lead)
- TestDriveChecklist (itens da lista de verificação de test-drive)
- ValueObject (classe base)

**Enums (10):**
- LeadStatus, LeadScore, LeadSource
- ProposalStatus
- TestDriveStatus, FuelLevel
- EvaluationStatus
- OrderStatus, PaymentMethod
- InteractionType

**Eventos de Domínio (9):**
- LeadCreatedEvent, LeadScoredEvent, LeadStatusChangedEvent
- ProposalCreatedEvent, ProposalUpdatedEvent
- SaleClosedEvent
- TestDriveScheduledEvent, TestDriveCompletedEvent
- UsedVehicleEvaluationRequestedEvent

**Serviços de Domínio:**
- LeadScoringService (calcula pontuação do lead baseado na qualificação)

**Interfaces de Repositório (6):**
- ILeadRepository, IProposalRepository, ITestDriveRepository
- IUsedVehicleEvaluationRepository, IOrderRepository
- IUnitOfWork (gestão de transações)
- IEventPublisher (publicação de eventos de domínio)

**Características do Domínio:**
- Modelos de domínio ricos com comportamento
- Setters privados com métodos factory (ex: `Lead.Create()`)
- Eventos de domínio disparados a partir de métodos da entidade
- Invariantes aplicadas dentro das entidades
- Regras de negócio encapsuladas na lógica de domínio

### 3.4 Camada de Infraestrutura (4-Infra)

**Localização:** `4-Infra/GestAuto.Commercial.Infra/`

**Responsabilidades:**
- Persistência de dados com Entity Framework Core
- Implementações de repositório
- Mensageria RabbitMQ (publicar e consumir)
- Migrações de banco de dados
- Verificações de saúde (Health checks)
- Implementação do padrão Outbox

**Componentes Chave:**

**Acesso a Dados:**
- CommercialDbContext (EF Core DbContext)
- EntityConfigurations (Mapeamentos Fluent API)
- Migrations (evolução do esquema do banco de dados)
- Provedor PostgreSQL com Npgsql

**Repositórios (6 implementações):**
- LeadRepository, ProposalRepository, TestDriveRepository
- UsedVehicleEvaluationRepository, OrderRepository
- OutboxRepository (padrão outbox para eventos)

**Mensageria:**
- RabbitMqPublisher (publica eventos de domínio para RabbitMQ)
- RabbitMqConfiguration (definições de exchange e routing key)
- Consumidores (2 consumidores de eventos):
  - UsedVehicleEvaluationRespondedConsumer
  - OrderUpdatedConsumer

**Infraestrutura de Suporte:**
- Implementação de UnitOfWork com gestão de transações
- Verificações de saúde (conectividade RabbitMQ)
- Padrão Outbox para publicação confiável de eventos

**Esquema de Banco de Dados:**
- Nome do esquema: `commercial`
- Tabelas mapeadas para entidades com configurações Fluent API
- Suporte para trilhas de auditoria (AuditEntry)
- Rastreamento de mensagens Outbox (OutboxMessage)

---

## 4. Análise de Componentes Críticos

### 4.1 Inventário de Componentes

**Total de Componentes Identificados:** 87

**Por Camada:**

| Camada | Controllers | Handlers | Repositórios | Entidades | Eventos | Serviços | Total |
|--------|-------------|----------|--------------|-----------|---------|----------|-------|
| API | 7 | - | - | - | - | 2 | 9 |
| Application | - | 21 | - | - | - | - | 21 |
| Domain | - | - | - | 9 | 9 | 1 | 19 |
| Infrastructure | - | - | 6 | - | - | 4 | 10 |
| **Total** | **7** | **21** | **6** | **9** | **9** | **7** | **52** |

**Componentes Adicionais:**
- DTOs: 15
- Validadores: 12
- Comandos: 17
- Consultas: 12
- Objetos de Valor: 7
- Enums: 10
- Middleware: 1
- Serviços em Segundo Plano: 1
- Classes de Configuração: 3

**Total Geral: 135 componentes**

### 4.2 Componentes Principais Detalhados

#### 4.2.1 Sistema de Gestão de Leads

**Componentes:**
- LeadController (API)
- CreateLeadHandler, UpdateLeadHandler, QualifyLeadHandler, ChangeLeadStatusHandler
- Lead entity (Domain)
- LeadRepository (Infrastructure)
- LeadCreatedEvent, LeadScoredEvent, LeadStatusChangedEvent

**Responsabilidades:**
- Gestão do ciclo de vida do Lead (Novo → Em Negociação → Qualificado → Ganho/Perdido)
- Pontuação de leads baseada em dados de qualificação
- Rastreamento de interações
- Atribuição e filtragem de vendedor

#### 4.2.2 Sistema de Gestão de Propostas

**Componentes:**
- ProposalController (API)
- CreateProposalHandler, UpdateProposalHandler, AddProposalItemHandler, RemoveProposalItemHandler
- ApproveDiscountHandler, ApplyDiscountHandler, CloseProposalHandler
- Proposal, ProposalItem entities
- ProposalRepository
- ProposalCreatedEvent, ProposalUpdatedEvent, SaleClosedEvent

**Responsabilidades:**
- Criação e gestão de propostas
- Gestão de itens de linha (veículos, acessórios, serviços)
- Fluxo de trabalho de aplicação e aprovação de descontos
- Fechamento de proposta e finalização de venda

#### 4.2.3 Sistema de Gestão de Test-Drive

**Componentes:**
- TestDriveController (API)
- TestDriveCommandHandlers (Schedule, Complete, Cancel)
- TestDrive entity
- TestDriveRepository
- TestDriveScheduledEvent, TestDriveCompletedEvent

**Responsabilidades:**
- Agendamento de test-drive
- Conclusão com lista de verificação
- Tratamento de cancelamento
- Coordenação de veículo e cliente

#### 4.2.4 Sistema de Avaliação

**Componentes:**
- EvaluationController (API)
- RequestEvaluationHandler, RegisterCustomerResponseHandler
- UsedVehicleEvaluation entity
- UsedVehicleEvaluationRepository
- UsedVehicleEvaluationRequestedEvent
- UsedVehicleEvaluationRespondedConsumer (consome evento externo)

**Responsabilidades:**
- Solicitações de avaliação de veículos usados
- Rastreamento de resposta do cliente
- Integração com módulo de Veículos Usados via eventos

#### 4.2.5 Sistema de Gestão de Pedidos

**Componentes:**
- OrderController (API)
- OrderHandlers
- Order entity
- OrderRepository
- OrderUpdatedConsumer (consome eventos externos)

**Responsabilidades:**
- Criação e gestão de pedidos de venda
- Integração com módulo Financeiro via eventos
- Configuração de método de pagamento

#### 4.2.6 Sistema de Publicação de Eventos

**Componentes:**
- OutboxProcessorService (serviço em segundo plano)
- RabbitMqPublisher (infraestrutura)
- OutboxRepository (persisência)
- UnitOfWork (coordena eventos com transações)

**Responsabilidades:**
- Publicação confiável de eventos com padrão outbox
- Armazenamento transacional de eventos
- Processamento em segundo plano de eventos pendentes
- Integração com RabbitMQ

---

## 5. Relacionamentos e Dependências de Componentes

### 5.1 Análise de Fluxo de Dependência

**Acoplamento Aferente (Ca - Incoming Dependencies):**
Número de componentes que dependem deste componente.

**Acoplamento Eferente (Ce - Outgoing Dependencies):**
Número de componentes dos quais este componente depende.

**Instabilidade (I = Ce / (Ca + Ce)):**
Mede quão facilmente um componente pode mudar. Maior = menos estável (mais dependências).

### 5.2 Dependências em Nível de Camada

**Matriz de Dependência:**

| De \ Para | API | Application | Domain | Infra | External |
|-----------|-----|-------------|--------|-------|----------|
| API | - | ✓ | - | ✓ | ✓ (JWT, RabbitMQ) |
| Application | - | - | ✓ | ✓ | - |
| Domain | - | - | - | - | - |
| Infra | - | - | ✓ | - | ✓ (EF, RabbitMQ) |

**Legenda:**
- ✓ = Tem dependência
- - = Sem dependência

### 5.3 Análise de Dependência de Componentes

**Métricas de Componente de Alto Nível:**

| Componente | Ca | Ce | I (Instabilidade) | Tipo |
|------------|----|----|-------------------|------|
| Camada de Domínio | 3 | 0 | 0.00 | Estável (Core) |
| Camada de Aplicação | 1 | 2 | 0.67 | Moderadamente Estável |
| Camada de Infraestrutura | 2 | 1 | 0.33 | Estável |
| Camada de API | 0 | 3 | 1.00 | Instável (Apresentação) |

**Interpretação:**
- **Camada de Domínio (I=0.00):** Mais estável, sem dependências. Mudanças aqui afetam todas as camadas externas.
- **Camada de Infraestrutura (I=0.33):** Baixa instabilidade, dependências mínimas no Domínio.
- **Camada de Aplicação (I=0.67):** Instabilidade moderada, equilibra a coordenação entre camadas.
- **Camada de API (I=1.00):** Mais instável, pode mudar sem afetar camadas internas.

### 5.4 Padrões de Comunicação

**Comunicação Síncrona (REST API):**
- Cliente → Controlador API → Handler → Domínio → Repositório → Banco de Dados
- Usado para execução de comandos e consultas
- Retorna resposta imediata

**Comunicação Assíncrona (Orientada a Eventos):**
- Entidade de Domínio → Evento de Domínio → UnitOfWork → Outbox
- OutboxProcessor → RabbitMQ Publisher → RabbitMQ Exchange
- Outros serviços consomem eventos do RabbitMQ
- Usado para integração entre serviços

**Integrações Externas:**
- Keycloak (autenticação/autorização)
- RabbitMQ (broker de mensagens)
- PostgreSQL (banco de dados)
- Módulo de Veículos Usados (via eventos)
- Módulo Financeiro (via eventos)

---

## 6. Avaliação Arquitetural

### 6.1 Pontos Fortes

**Arquitetura & Design:**
1. **Implementação de Arquitetura Limpa:** Separação estrita de preocupações com clara inversão de dependência. A camada de domínio tem zero dependências das camadas externas.
2. **Padrão CQRS:** Separação clara entre operações de leitura e escrita melhora manutenção e escalabilidade.
3. **Design Orientado ao Domínio (DDD):** Modelos de domínio ricos com comportamento, objetos de valor e eventos de domínio alinham-se estreitamente com os requisitos de negócio.
4. **Princípios SOLID:** Responsabilidade única, aberto/fechado e princípios de inversão de dependência são aplicados consistentemente.

**Confiabilidade & Resiliência:**
1. **Padrão Outbox:** Publicação confiável de eventos evita perda de mensagens durante falhas.
2. **Consistência Transacional:** Eventos de domínio são armazenados atomicamente com mudanças na entidade.
3. **Processamento em Segundo Plano:** OutboxProcessor roda como serviço hospedado para publicação de eventos.

**Testabilidade:**
1. **Cobertura de Testes Abrangente:** Testes unitários, de integração e E2E presentes.
2. **Injeção de Dependência:** Todos os componentes são injetáveis, facilitando o mocking.
3. **Segregação de Interface:** Interfaces claras para repositórios, handlers e serviços.

**Documentação:**
1. **Swagger/OpenAPI:** Documentação interativa da API REST.
2. **AsyncAPI:** Documentação completa de eventos com esquemas e exemplos.
3. **Comentários de Código:** Documentação XML em APIs públicas.

**Segurança:**
1. **Autenticação JWT:** Integração Keycloak para autenticação baseada em token.
2. **Políticas de Autorização:** Controle de acesso baseado em função (SalesPerson, Manager).
3. **Normalização de Claims:** Lida com vários formatos de claim do Keycloak.

**Observabilidade:**
1. **Log Estruturado:** Serilog para logs estruturados e pesquisáveis.
2. **Health Checks:** Endpoint para monitoramento de saúde do serviço.
3. **Trilha de Auditoria:** Entidade AuditEntry para rastrear alterações.

### 6.2 Preocupações e Considerações

**Complexidade:**
1. **Indireção de Camadas:** Múltiplas camadas adicionam complexidade para operações simples.
2. **Proliferação de Handlers:** 21 handlers para operações CRUD podem ser excessivos para algumas funcionalidades.
3. **Mapeamento de DTOs:** Múltiplos DTOs para entidades similares aumentam a carga de manutenção.

**Performance:**
1. **Processamento Outbox:** Polling em segundo plano pode introduzir latência na entrega de eventos.
2. **Overhead de Repositório:** Padrão de repositório genérico pode adicionar abstração desnecessária sobre o EF Core DbContext.
3. **Coleta de Eventos de Domínio:** Coleta de eventos baseada em reflexão a partir do ChangeTracker em cada SaveChanges.

**Escalabilidade:**
1. **Restrições de Banco de Dados:** Instância única de PostgreSQL limita a escalabilidade horizontal.
2. **Polling de Outbox:** Polling de serviço em segundo plano pode não escalar eficientemente sob alta carga.
3. **API Síncrona:** Endpoints REST podem se tornar gargalos para operações de alto rendimento.

**Acoplamento:**
1. **Acoplamento RabbitMQ:** Dependência direta do RabbitMQ em toda a infraestrutura.
2. **Acoplamento Entity Framework:** Entidades de domínio devem trabalhar com restrições do EF Core (setters privados, etc.).
3. **Acoplamento Keycloak:** Lógica de autenticação fortemente acoplada ao comportamento específico do Keycloak.

**Tratamento de Erros:**
1. **Manipulador de Exceção Global:** Middleware único pode não fornecer respostas de erro granulares.
2. **Falhas na Publicação de Eventos:** Eventos da outbox que falham na publicação exigem intervenção manual ou lógica de retentativa.
3. **Tratamento de Erro do Consumidor:** Visibilidade limitada no tratamento de erros do consumidor e filas de dead-letter.

**Testes:**
1. **Complexidade de Testes de Integração:** Requer PostgreSQL, RabbitMQ para testes de integração.
2. **Manutenção de Testes E2E:** Testes full-stack podem ser frágeis com dependências externas.
3. **Gestão de Dados de Teste:** Dados semente (seed) e limpeza de testes não claramente visíveis.

### 6.3 Indicadores de Dívida Técnica

**Itens de Dívida Atual:**

1. **Bloqueios de Migração de Banco de Dados:** Bloqueios consultivos (advisory locks) impedem migrações concorrentes, mas indicam potenciais problemas de deploy.
2. **Código Comentado:** Health check do RabbitMQ comentado em Program.cs.
3. **Configurações de Desenvolvimento:** `RequireHttpsMetadata = false` para desenvolvimento local.
4. **Política CORS:** Política `AllowAll` é excessivamente permissiva para produção.

**Áreas de Dívida Potencial:**

1. **Padrão Repository:** Repositórios genéricos podem ser uma abstração desnecessária sobre o EF Core.
2. **Contagem de Handlers:** 21 handlers poderiam ser potencialmente consolidados ou simplificados.
3. **Proliferação de DTOs:** Múltiplos DTOs por entidade aumentam a complexidade de mapeamento.
4. **Processador Outbox:** Abordagem baseada em polling poderia ser substituída por um mecanismo mais eficiente.

**Dívida Evolucionária:**

1. **Scripts de Migração:** Múltiplas migrações indicam esquema em evolução.
2. **Uso de Enum:** Enums de status espalhados pelas entidades poderiam ser consolidados.
3. **Adoção de Objetos de Valor:** Nem todos os potenciais objetos de valor estão implementados (ex: Money, LicensePlate).

---

## 7. Stack Tecnológico

### 7.1 Framework & Runtime

| Componente | Versão | Propósito |
|------------|--------|-----------|
| .NET | 8.0 | Framework primário |
| ASP.NET Core | 8.0 | Framework Web API |
| C# | 12.0 | Versão da linguagem |
| Entity Framework Core | 8.0.10 | ORM para acesso a dados |

### 7.2 Dados & Mensageria

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| PostgreSQL | 15+ | Banco de dados primário |
| Npgsql | 8.0.10 | Provedor PostgreSQL para EF Core |
| RabbitMQ | 3.12 | Broker de mensagens |
| RabbitMQ.Client | 6.8.1 | Biblioteca cliente AMQP |

### 7.3 Autenticação & Segurança

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| Keycloak | Mais recente | Gestão de identidade e acesso |
| JWT Bearer | 8.0.10 | Autenticação baseada em token |
| Authorization Policies | - | Controle de acesso baseado em função |

### 7.4 Validação & Documentação

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| FluentValidation | 12.1.1 | Validação de entrada |
| Swashbuckle | 6.6.2 | Documentação Swagger/OpenAPI |
| Saunter | 0.9.0 | Geração de documentação AsyncAPI |

### 7.5 Logs & Diagnóstico

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| Serilog.AspNetCore | 10.0.0 | Log estruturado |
| Serilog.Sinks.Console | 6.1.1 | Log em console |
| Serilog.Formatting.Elasticsearch | 10.0.0 | Integração com Elasticsearch |
| Health Checks | 8.0.0 | Monitoramento de saúde do serviço |

### 7.6 Conteinerização & Deploy

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| Docker | Mais recente | Runtime de contêiner |
| Dockerfile | - | Definição de imagem de contêiner |
| Docker Compose | - | Orquestração multi-contêiner |

### 7.7 Ferramentas de Desenvolvimento

| Tecnologia | Propósito |
|------------|-----------|
| .NET CLI | Comandos de build, teste, execução |
| Entity Framework Core Tools | Migrações de banco de dados |
| MSBuild | Automação de build |

---

## 8. Padrões Arquiteturais Identificados

### 8.1 Padrões Primários

1. **Arquitetura Limpa (Onion Architecture)**
   - Camadas concêntricas com inversão de dependência
   - Domínio no núcleo, infraestrutura no anel externo
   - Dependências apontam para dentro

2. **CQRS (Segregação de Responsabilidade de Comando e Consulta)**
   - Manipuladores de comando e consulta separados
   - DTOs diferentes para comandos vs consultas
   - Otimizado para separação de leitura/escrita

3. **Design Orientado ao Domínio (DDD)**
   - Agregados (Lead, Proposal, TestDrive, Order)
   - Objetos de Valor (Email, Phone, Money)
   - Eventos de Domínio (9 tipos de eventos)
   - Padrão Repository
   - Linguagem Ubíqua

### 8.2 Padrões Secundários

4. **Padrão Repository**
   - Abstração de acesso a dados atrás de interfaces
   - 6 implementações de repositório

5. **Padrão Unit of Work**
   - Coordena múltiplas operações de repositório
   - Consistência transacional
   - Coleta e publicação de eventos de domínio

6. **Padrão Outbox**
   - Publicação confiável de eventos
   - Previne perda de mensagens
   - Tabela outbox transacional

7. **Padrão Factory**
   - Métodos factory estáticos em entidades (ex: `Lead.Create()`)
   - Encapsula lógica de criação de objeto

8. **Padrão Specification (Implícito)**
   - Validadores agem como especificações para comandos
   - FluentValidation aplica regras de negócio

9. **Padrão Background Service**
   - OutboxProcessor como serviço hospedado
   - Processamento outbox de longa duração

10. **Padrão Middleware**
    - ExceptionHandlerMiddleware
    - Pipeline de Autenticação e autorização

---

## 9. Arquitetura de Segurança

### 9.1 Fluxo de Autenticação

```
Cliente → API Gateway → Keycloak (Solicitação de Token)
                    ↓
Cliente → Controlador API → Validação JWT Bearer
                    ↓
               Claims Principal
                    ↓
          Políticas de Autorização
```

### 9.2 Modelo de Autorização

**Funções (SCREAMING_SNAKE_CASE):**
- `SALES_PERSON`: Acesso aos próprios registros
- `SALES_MANAGER`: Gerente com permissões de vendas
- `MANAGER`: Acesso total a todos os registros
- `ADMIN`: Acesso administrativo

**Políticas:**
- `SalesPerson`: Requer SALES_PERSON, SALES_MANAGER, MANAGER, ou ADMIN
- `Manager`: Requer MANAGER, SALES_MANAGER, ou ADMIN

**Filtragem de Dados:**
- SalesPersonFilterService filtra dados baseado na função do usuário
- Vendedores veem apenas seus registros
- Gerentes veem todos os registros

### 9.3 Preocupações de Segurança

1. **Política CORS:** Política `AllowAll` é excessivamente permissiva
2. **HTTPS Desativado:** `RequireHttpsMetadata = false` para desenvolvimento
3. **Normalização de Claims:** Lógica complexa para lidar com formatos de claim do Keycloak
4. **Sem Rate Limiting:** Nenhum rate limiting nos endpoints visível
5. **Sem Sanitização de Entrada:** Confia unicamente na validação

---

## 10. Arquitetura de Integração

### 10.1 Sistemas Externos

**Dependências Upstream (serviços consumidos pelo Commercial):**
- Keycloak (autenticação)
- RabbitMQ (mensageria)
- PostgreSQL (armazenamento de dados)

**Dependências Downstream (serviços consumindo eventos Commercial):**
- Módulo de Veículos Usados (consome solicitações de avaliação)
- Módulo Financeiro (consome atualizações de pedidos)

### 10.2 Contratos de Eventos

**Eventos Publicados (9):**
1. `commercial.lead.created`
2. `commercial.lead.scored`
3. `commercial.lead.status-changed`
4. `commercial.proposal.created`
5. `commercial.proposal.updated`
6. `commercial.sale.closed`
7. `commercial.testdrive.scheduled`
8. `commercial.testdrive.completed`
9. `commercial.used-vehicle.evaluation-requested`

**Eventos Consumidos (2):**
1. `used-vehicles.evaluation.responded`
2. `finance.order.updated`

**Configuração de Exchange:**
- Nome da Exchange: `gestauto.commercial`
- Tipo da Exchange: Topic
- Routing Keys: Definidas por tipo de evento

---

## 11. Arquitetura de Dados

### 11.1 Esquema de Banco de Dados

**Nome do Esquema:** `commercial`

**Tabelas (11):**
- leads
- interactions
- proposals
- proposal_items
- test_drives
- used_vehicle_evaluations
- orders
- payment_methods
- outbox_messages
- audit_entries
- __EFMigrationsHistory (Rastreamento EF Core)

### 11.2 Relacionamentos do Modelo de Dados

**Agregados:**
- Lead (1) → (N) Interactions
- Proposal (1) → (N) ProposalItems
- TestDrive (agregado isolado)
- UsedVehicleEvaluation (agregado isolado)
- Order (agregado isolado)
- PaymentMethodEntity (dados de configuração)

**Relacionamentos Inter-Agregados:**
- Proposal → Lead (referência via LeadId)
- TestDrive → Lead (referência via LeadId)
- Order → Proposal (referência via ProposalId)

### 11.3 Padrões de Persistência

**ORM:** Entity Framework Core 8 com PostgreSQL
**Migrações:** Migrações code-first com configurações Fluent API
**Rastreamento:** ChangeTracker para coleta de eventos de domínio
**Transações:** IDbContextTransaction para operações atômicas

---

## 12. Considerações de Escalabilidade

### 12.1 Características Atuais de Escalabilidade

**Escalabilidade Horizontal:**
- Instâncias de API podem escalar horizontalmente (controladores stateless)
- Serviço em segundo plano (OutboxProcessor) requer eleição de líder para múltiplas instâncias
- Banco de dados é ponto único de restrição (PostgreSQL)

**Escalabilidade Vertical:**
- Operações assíncronas baseadas em thread pool
- Pooling de conexão EF Core
- Gestão de conexão RabbitMQ

### 12.2 Gargalos e Restrições

**Banco de Dados:**
- Instância única de PostgreSQL limita escalabilidade de escrita
- Bloqueios consultivos em migrações impedem deploy concorrente
- Nenhuma estratégia de sharding ou particionamento visível

**Mensageria:**
- OutboxProcessor faz polling no banco de dados continuamente
- Exchange RabbitMQ única para todos os eventos
- Nenhuma configuração de dead-letter queue visível

**API:**
- Modelo de requisição/resposta síncrono
- Nenhuma camada de cache visível
- Sem rate limiting

---

## 13. Arquitetura de Observabilidade

### 13.1 Estratégia de Logs

**Framework:** Serilog log estruturado
**Sinks:** Console, Elasticsearch
**Níveis de Log:** Information (padrão), Warning (Microsoft)

**Pontos de Log:**
- Inicialização da aplicação e migração
- Publicação de eventos (sucesso/falha)
- Ações do controlador (via log embutido)
- Operações de domínio (seletivo)

### 13.2 Monitoramento

**Health Checks:**
- Endpoint: `/health`
- Health check do RabbitMQ (comentado)
- Conectividade do banco de dados (implícito via EF)

**Trilha de Auditoria:**
- Entidade AuditEntry rastreia mudanças
- Timestamps UpdatedAt em entidades
- Eventos de domínio fornecem trilha de auditoria de operações de negócio

### 13.3 Lacunas de Observabilidade

1. Nenhum rastreamento distribuído visível
2. Nenhuma coleta de métricas (Prometheus, etc.)
3. Nenhum monitoramento de performance de aplicação (APM)
4. IDs de correlação estruturada limitados
5. Nenhuma configuração de agregação de logs centralizada visível

---

## 14. Resumo do Inventário de Componentes

### 14.1 Lista Completa de Componentes

**Controladores de API (7):**
1. LeadController
2. ProposalController
3. TestDriveController
4. EvaluationController
5. OrderController
6. PaymentMethodsController
7. DashboardController

**Manipuladores de Comando/Consulta (21):**
1. CreateLeadHandler
2. UpdateLeadHandler
3. QualifyLeadHandler
4. ChangeLeadStatusHandler
5. RegisterInteractionHandler
6. CreateProposalHandler
7. UpdateProposalHandler
8. AddProposalItemHandler
9. RemoveProposalItemHandler
10. ApplyDiscountHandler
11. ApproveDiscountHandler
12. CloseProposalHandler
13. ScheduleTestDriveHandler
14. CompleteTestDriveHandler
15. CancelTestDriveHandler
16. RequestEvaluationHandler
17. RegisterCustomerResponseHandler
18. GetLeadHandler
19. ListLeadsHandler
20. GetDashboardDataHandler
21. ListProposalsHandler

**Repositórios (6):**
1. LeadRepository
2. ProposalRepository
3. TestDriveRepository
4. UsedVehicleEvaluationRepository
5. OrderRepository
6. OutboxRepository

**Entidades de Domínio (9):**
1. Lead
2. Interaction
3. Proposal
4. ProposalItem
5. TestDrive
6. UsedVehicleEvaluation
7. Order
8. PaymentMethodEntity
9. BaseEntity

**Eventos de Domínio (9):**
1. LeadCreatedEvent
2. LeadScoredEvent
3. LeadStatusChangedEvent
4. ProposalCreatedEvent
5. ProposalUpdatedEvent
6. SaleClosedEvent
7. TestDriveScheduledEvent
8. TestDriveCompletedEvent
9. UsedVehicleEvaluationRequestedEvent

**Serviços de Domínio (1):**
1. LeadScoringService

**Serviços de Aplicação (2):**
1. OutboxProcessorService
2. SalesPersonFilterService

**Serviços de Infraestrutura (4):**
1. RabbitMqPublisher
2. RabbitMqConfiguration
3. UnitOfWork
4. InfrastructureServiceExtensions

**Middleware (1):**
1. ExceptionHandlerMiddleware

**Objetos de Valor (7):**
1. Email
2. Phone
3. Money
4. LicensePlate
5. Qualification
6. TestDriveChecklist
7. ValueObject (base)

**Enums (10):**
1. LeadStatus
2. LeadScore
3. LeadSource
4. ProposalStatus
5. TestDriveStatus
6. FuelLevel
7. EvaluationStatus
8. OrderStatus
9. PaymentMethod
10. InteractionType

**Consumidores de Evento (2):**
1. UsedVehicleEvaluationRespondedConsumer
2. OrderUpdatedConsumer

**Validadores (12):**
1. CreateLeadValidator
2. UpdateLeadValidator
3. QualifyLeadValidator
4. RegisterInteractionValidator
5. CreateProposalValidator
6. UpdateProposalValidator
7. AddProposalItemValidator
8. RemoveProposalItemValidator
9. ApplyDiscountValidator
10. ApproveDiscountValidator
11. CloseProposalValidator
12. RequestEvaluationValidator

---

## 15. Conclusão

O Módulo Comercial GestAuto demonstra um microsserviço bem arquitetado seguindo princípios de Arquitetura Limpa e Design Orientado ao Domínio. A base de código mostra clara separação de preocupações, cobertura de testes abrangente e práticas modernas de .NET 8.

**Qualidade da Arquitetura:** Alta
**Manutenibilidade:** Alta
**Testabilidade:** Alta
**Escalabilidade:** Moderada (restrição no banco de dados)
**Segurança:** Boa (baseada em JWT, baseada em função)
**Observabilidade:** Moderada (log presente, métricas/rastreamento limitados)

**Principais Pontos Fortes:**
- Arquitetura Limpa com inversão de dependência adequada
- Padrão CQRS para clara separação leitura/escrita
- Design orientado ao domínio com modelos de domínio ricos
- Publicação confiável de eventos com padrão outbox
- Documentação abrangente (Swagger + AsyncAPI)

**Áreas para Consideração:**
- Abstração de repositório pode ser desnecessária sobre EF Core
- Contagem de handlers poderia ser consolidada
- Polling do Outbox poderia ser otimizado
- Observabilidade limitada além de logs
- Nenhuma estratégia de cache visível
- Banco de dados é ponto único de restrição

**Avaliação Geral:** A arquitetura é sólida e segue as melhores práticas da indústria. O módulo está pronto para produção com considerações para melhorias de escalabilidade e observabilidade em iterações futuras.

---

**Fim do Relatório**

Gerado por: Agente Analisador Arquitetural
Formato do Relatório: Markdown
Timestamp: 2026-01-23-09:09:14