# Relatório de Análise Profunda do Componente: LeadRepository

## 1. Resumo Executivo

O componente `LeadRepository` serve como a camada crítica de acesso a dados para o domínio de Gestão de Leads dentro do Módulo Comercial do GestAuto. Ele implementa o padrão Repository, abstraindo a complexidade do Entity Framework Core e fornecendo uma interface orientada ao domínio para interagir com o banco de dados PostgreSQL.

Este componente é responsável por todo o ciclo de vida (CRUD) de Leads, incluindo lógica complexa de filtragem para listagens de vendedores, métricas de dashboard e uma regra de negócio específica para identificação de "Hot Leads" (Leads Quentes).

**Principais Responsabilidades:**
-   Persistência e recuperação de entidades `Lead`.
-   Execução de consultas complexas com múltiplos filtros (status, vendedor, pontuação, busca).
-   Cálculo de métricas de KPI para o dashboard comercial.
-   Gestão de carregamento antecipado (Eager Loading) de entidades relacionadas (Qualificação, Veículo de Troca, Interações).
-   Implementação de lógica de busca otimizada para telefones (extração de dígitos).

**Estado Atual:**
-   Componente estável e maduro seguindo princípios de Arquitetura Limpa.
-   Interface definida na Camada de Domínio, implementação na Camada de Infraestrutura.
-   Alta centralidade: utilizado por 13 Handlers diferentes.
-   Complexidade moderada devido a métodos de consulta personalizados e regras de negócio incorporadas.
-   Cobertura de testes baixa (apenas 3 testes de integração cobrindo 21% dos métodos).

**Risco:** Baixo a Médio (devido à duplicação de lógica de busca e baixa cobertura de testes).

---

## 2. Identificação do Componente

| Atributo | Valor |
|---|---|
| **Nome** | `LeadRepository` |
| **Namespace** | `GestAuto.Commercial.Infra.Repositories` |
| **Caminho do Arquivo** | `services/commercial/4-Infra/GestAuto.Commercial.Infra/Repositories/LeadRepository.cs` |
| **Interface** | `ILeadRepository` (`GestAuto.Commercial.Domain.Interfaces`) |
| **Tipo** | Repositório de Infraestrutura (Camada 4) |
| **Linguagem** | C# (.NET 8.0) |
| **Tecnologias** | Entity Framework Core, LINQ, PostgreSQL |

---

## 3. Análise Funcional

O repositório fornece 14 métodos distintos categorizados em operações CRUD, Listagem/Filtragem, Contagem e Dashboard.

### 3.1 Operações CRUD Básicas

| Método | Assinatura | Descrição | Regras de Negócio |
|---|---|---|---|
| **AddAsync** | `AddAsync(Lead lead, CancellationToken)` | Persiste um novo lead no banco de dados. | Simples pass-through para o EF Core `AddAsync`. Não valida duplicatas explicitamente. |
| **UpdateAsync** | `UpdateAsync(Lead lead, CancellationToken)` | Atualiza um lead existente. | Marca a entidade como modificada no ChangeTracker. |
| **GetByIdAsync** | `GetByIdAsync(Guid id, CancellationToken)` | Retorna um único lead por ID. | **Regra Crítica**: Utiliza `Include` para carregar `Qualification`, `TradeInVehicle` e `Interactions` (Eager Loading completo). Retorna `null` se não encontrado. |
| **GetBySalesPersonIdAsync** | `GetBySalesPersonIdAsync(Guid id, CancellationToken)` | Retorna leads de um vendedor (obsoleto?). | Raramente usado, apenas busca simples por `SalesPersonId`. |

### 3.2 Listagem e Filtragem

Essa é a parte mais complexa do componente, suportando filtragem dinâmica para a UI.

| Método | Parâmetros | Descrição |
|---|---|---|
| **ListBySalesPersonAsync** | `id`, `statuses`, `score`, `search`, `createdFrom`, `createdTo`, `page`, `pageSize` | Lista paginada para um vendedor específico com filtros opcionais. |
| **ListAllAsync** | `statuses`, `score`, `search`, `page`, `pageSize`, `salesPersonId` | Lista paginada global (admin) com filtros opcionais. |

**Lógica de Busca (Implementada em ambos):**
1.  **Status**: Filtra por lista de enums (se fornecido).
2.  **Score**: Filtra por pontuação (ex: Diamond, Gold).
3.  **Datas**: Filtra por intervalo de `CreatedAt`.
4.  **Busca Textual (Complexa)**:
    -   Extrai dígitos do termo de busca.
    -   Busca parcial (`LIKE`) em: `Name`, `Email`.
    -   Busca parcial em `Phone` usando apenas dígitos extraídos (permite buscar "1199999" e achar "(11) 99999...").
5.  **Ordenação**: Ordena por `Score` (desc) e depois `CreatedAt` (desc).
6.  **Paginação**: Aplica `Skip` e `Take`.

**Lógica de Código (Trecho):**
```csharp
// Extração de dígitos para busca de telefone
var digits = new string(term.Where(char.IsDigit).ToArray());
query = query.Where(l =>
    EF.Functions.Like(l.Name, $"%{term}%") ||
    EF.Functions.Like(l.Email.Value, $"%{term}%") ||
    (!string.IsNullOrEmpty(digits) && EF.Functions.Like(l.Phone.Value, $"%{digits}%")));
```

### 3.3 Dashboard e KPIs

Métodos otimizados para widgets de dashboard.

| Método | Descrição | Otimização |
|---|---|---|
| **CountByStatusAsync** | Conta leads em um status específico. | Usa `CountAsync` (SQL `COUNT`) em vez de carregar entidades. |
| **CountCreatedSinceAsync** | Conta novos leads desde data X. | Filtro simples de data + `CountAsync`. |
| **CountByStatusSinceAsync** | Conta leads em status X desde data Y. | Filtro composto + `CountAsync`. |

### 3.4 Regra de Negócio: Hot Leads

O método `GetHotLeadsAsync` implementa uma regra de negócio específica para identificar oportunidades urgentes.

**Definição de "Hot Lead":**
1.  Status não é `Converted` nem `Lost` (lead ativo).
2.  Score é `Diamond` ou `Gold` (alto potencial).
3.  Vendedor específico (opcional).
4.  **Critério de Negligência**: Nenhuma interação registrada OU última interação foi antes de data X (ex: 48h atrás).

**Implementação Híbrida:**
1.  **Fase Banco**: Busca todos os leads ativos Diamond/Gold do vendedor.
2.  **Fase Memória**: Filtra localmente verificando a coleção `Interactions`.

```csharp
// Busca leads de alto valor
query = query.Where(l => l.Score == LeadScore.Diamond || l.Score == LeadScore.Gold);
// ... carrega para memória ...
var hotLeads = await query.ToListAsync(cancellationToken);
// Filtra por data da última interação em memória
return hotLeads.Where(l => !l.Interactions.Any() ||
      l.Interactions.Max(i => i.InteractionDate) < lastInteractionBefore).ToList();
```

### 3.5 Métodos de Contagem

Existem métodos espelho para `ListBySalesPersonAsync` e `ListAllAsync` que retornam apenas o total (`CountBySalesPersonAsync`, `CountAllAsync`) para suportar a paginação no frontend (total de páginas).

Todos os três métodos suportam filtragem opcional por vendedor através de parâmetro string (anulável). A string é convertida para Guid se presente, permitindo reutilização entre contextos onde `salesPersonId` pode ser Guid (filtragem) ou string? (de parâmetros de query).

Esses métodos são otimizados para retornar apenas a contagem (int) em vez de entidades completas, usando o agregado COUNT no SQL para desempenho.

**Fluxo**:
```
Fluxo CountByStatusAsync:
Entrada: status, salesPersonId?, cancellationToken
  ↓
Construir query: _context.Leads.Where(l => l.Status == status)
  ↓
Aplicar filtro vendedor (se fornecido): l.SalesPersonId == salesPersonGuid
  ↓
Executar: CountAsync(cancellationToken)
  ↓
Retornar int (contagem)
```

---

## 4. Estrutura do Componente

```
services/commercial/4-Infra/GestAuto.Commercial.Infra/Repositories/
├── LeadRepository.cs                          # Implementação Principal (281 linhas)
│   ├── Construtor                             # Injeta CommercialDbContext
│   ├── Operações CRUD                         # AddAsync, UpdateAsync
│   ├── Consultas Básicas                      # GetByIdAsync, GetBySalesPersonIdAsync
│   ├── Métodos de Listagem                    # ListBySalesPersonAsync, ListAllAsync
│   ├── Métodos de Contagem                    # CountBySalesPersonAsync, CountAllAsync
│   └── Métodos de Dashboard                   # CountByStatusAsync, GetHotLeadsAsync, etc.

services/commercial/3-Domain/GestAuto.Commercial.Domain/Interfaces/
├── ILeadRepository.cs                         # Interface de Domínio (52 linhas)
│   ├── Assinaturas CRUD Básicas               # GetByIdAsync, AddAsync, UpdateAsync
│   ├── Assinaturas de Listagem/Filtro         # ListBySalesPersonAsync, ListAllAsync
│   ├── Assinaturas de Contagem                # CountBySalesPersonAsync, CountAllAsync
│   └── Assinaturas de Dashboard               # CountByStatusAsync, GetHotLeadsAsync

services/commercial/4-Infra/GestAuto.Commercial.Infra/
├── CommercialDbContext.cs                     # Contexto de Banco de Dados EF Core
│   ├── DbSet<Lead> Leads                      # Conjunto de entidades Lead
│   └── OnModelCreating                        # Aplica configurações do assembly

services/commercial/4-Infra/GestAuto.Commercial.Infra/EntityConfigurations/
├── LeadConfiguration.cs                       # Mapeamento de entidade EF Core
│   ├── Mapeamento de Tabela: "leads"          # Nome da tabela
│   ├── Mapeamentos de Colunas                 # Id, Name, Email, Phone, etc.
│   ├── Conversores de Value Object            # EmailConverter, PhoneConverter
│   ├── Conversões de Enum                     # Source, Status, Score para strings
│   ├── Tipos Proprietários (Owned types)      # Qualification, TradeInVehicle
│   ├── Propriedades de Navegação              # Coleção Interactions
│   └── Índices                                # SalesPersonId, Status, Score

services/commercial/5-Tests/GestAuto.Commercial.IntegrationTest/Repository/
├── LeadRepositoryTests.cs                     # Testes de Integração (126 linhas)
│   ├── AddAsync_ShouldPersistLead             # Verifica persistência
│   ├── ListBySalesPersonAsync_ShouldFilterBySalesPerson  # Verifica filtragem
│   └── ListBySalesPersonAsync_ShouldFilterByScore       # Verifica filtragem por score
```

---

## 5. Análise de Dependências

### Dependências Internas

```
LeadRepository
├── CommercialDbContext (EF Core)
│   ├── DbSet<Lead> - Conjunto de entidades para queries
│   ├── ChangeTracker - Rastreia mudanças nas entidades
│   └── Database - Gestão de transações
│
├── Domain.Entities.Lead
│   ├── Propriedades: Id, Name, Email, Phone, Status, Score, SalesPersonId
│   ├── Navegação: Qualification (entidade proprietária)
│   ├── Navegação: Interactions (coleção)
│   └── Métodos: Create, Qualify, ChangeStatus (lógica de negócio)
│
├── Domain.Interfaces.ILeadRepository
│   └── Definição de contrato (Camada de Domínio)
│
└── Microsoft.EntityFrameworkCore
    ├── Entity Framework Core ORM
    ├── Métodos de extensão Async
    ├── Include/ThenInclude para eager loading
    └── EF.Functions.Like para operações SQL LIKE
```

### Dependências Externas

```
Bibliotecas Externas (do .csproj):
├── Microsoft.EntityFrameworkCore (v8.0.0)
│   └── Funcionalidade central do EF Core
│
├── Microsoft.EntityFrameworkCore.Relational (v8.0.0)
│   └── Suporte a banco de dados relacional
│
├── Npgsql.EntityFrameworkCore.PostgreSQL (v8.0.0)
│   └── Provedor de banco de dados PostgreSQL
│
└── Microsoft.Extensions.DependencyInjection.Abstractions (v8.0.0)
    └── Suporte a injeção de dependência
```

### Configuração de Injeção de Dependência

```csharp
// Arquivo: InfrastructureServiceExtensions.cs:17
services.AddScoped<ILeadRepository, LeadRepository>();
```

**Tempo de Vida**: Scoped (por requisição HTTP)
**Justificativa**: DbContext também é scoped, garantindo que repositório e contexto compartilhem o mesmo tempo de vida da requisição.

### Padrões de Uso do Entity Framework Core

| Recurso EF Core | Local de Uso | Propósito |
|-----------------|--------------|-----------|
| DbSet<T> | _context.Leads | Conjunto de entidades para consulta |
| Include() | Linhas 20-22, 29, 61-62, 142-143, 260-261 | Eager loading de entidades relacionadas |
| ThenInclude() | Linha 21 | Eager loading aninhado (TradeInVehicle) |
| Where() | Linhas 30, 63, 106, etc. | Expressões de filtro |
| OrderByDescending() | Linhas 31, 88, 169 | Ordenação |
| Skip()/Take() | Linhas 90-91, 171-172 | Paginação |
| FirstOrDefaultAsync() | Linha 23 | Recuperação de entidade única |
| ToListAsync() | Linhas 33, 92, 173, 271 | Materialização de múltiplas entidades |
| CountAsync() | Linhas 128, 222, 235, 250 | Consultas de contagem agregada |
| AddAsync() | Linha 38 | Inserção de entidade |
| Update() | Linha 44 | Modificação de entidade |
| EF.Functions.Like() | Linhas 76-78, 117-119, 157-159 | Pattern matching SQL LIKE |
| HasConversion<string>() | LeadConfiguration.cs:40, 45, 50 | Conversão de Enum para string |
| OwnsOne() | LeadConfiguration.cs:78 | Mapeamento de tipo de entidade proprietária |
| HasMany().WithOne() | LeadConfiguration.cs:135-139 | Mapeamento de navegação de coleção |
| HasIndex() | LeadConfiguration.cs:142-144 | Criação de índice de banco de dados |

---

## 6. Análise de Acoplamento

### Métricas de Acoplamento

| Componente | Acoplamento Aferente (Ca) | Acoplamento Eferente (Ce) | Instabilidade (I = Ce / (Ca + Ce)) |
|------------|---------------------------|---------------------------|------------------------------------|
| **LeadRepository** | 13 | 5 | 0.28 (Baixo - Estável) |
| **ILeadRepository (Interface)** | 0 | 1 | 1.00 (Alto - Não aplicável) |

### Acoplamento Aferente (Ca = 13)
*Componentes que dependem de LeadRepository/ILeadRepository:*

1. **GetLeadHandler** - Recupera lead único por ID
2. **ListLeadsHandler** - Lista leads com filtragem/paginação
3. **CreateLeadHandler** - Cria novos leads
4. **UpdateLeadHandler** - Atualiza leads existentes
5. **ChangeLeadStatusHandler** - Altera status do lead
6. **QualifyLeadHandler** - Adiciona qualificação ao lead
7. **RegisterInteractionHandler** - Registra interações
8. **ListInteractionsHandler** - Lista interações do lead
9. **GetDashboardDataHandler** - KPIs de Dashboard
10. **CloseProposalHandler** - Fecha propostas (atualiza status do lead)
11. **CreateProposalHandler** - Cria propostas a partir de leads
12. **TestDriveCommandHandlers** - Operações de test drive afetando leads
13. **TestDriveQueryHandlers** - Consultas de test drive

### Acoplamento Eferente (Ce = 5)
*Componentes dos quais LeadRepository depende:*

1. **CommercialDbContext** - Contexto de banco de dados EF Core
2. **Lead Entity** - Entidade de domínio sendo persistida
3. **ILeadRepository Interface** - Contrato que implementa
4. **Entity Framework Core** - Framework ORM (Microsoft.EntityFrameworkCore)
5. **LINQ Expressions** - Linguagem de expressão de consulta

### Avaliação de Acoplamento

**Pontos Fortes:**
- Baixa instabilidade (0.28) indica componente estável.
- Depende apenas de abstração (ILeadRepository) da camada de Domínio.
- Sem dependências das camadas de Aplicação ou API (boa direção).
- Dependências concretas do EF Core são preocupações de infraestrutura (aceitável).

**Áreas de Preocupação:**
- Alto acoplamento aferente (13 consumidores) indica alta centralidade de uso.
- Qualquer mudança na interface ILeadRepository afeta 13 classes handler.
- Duplicação de código na lógica de filtragem (ListBySalesPersonAsync, ListAllAsync, CountBySalesPersonAsync, CountAllAsync).
- Duplicação da lógica de busca (extração de dígitos do telefone aparece 4 vezes).

### Direção das Dependências

```
┌─────────────────────────────────────────────────────────────┐
│                     API Layer (1-Services)                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          LeadController                               │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                Application Layer (2-Application)             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          Handlers (13 consumidores)                   │  │
│  │  ├─ GetLeadHandler                                    │  │
│  │  ├─ ListLeadsHandler                                  │  │
│  │  ├─ CreateLeadHandler                                 │  │
│  │  ├─ UpdateLeadHandler                                 │  │
│  │  ├─ ChangeLeadStatusHandler                           │  │
│  │  ├─ QualifyLeadHandler                                │  │
│  │  ├─ RegisterInteractionHandler                        │  │
│  │  ├─ GetDashboardDataHandler                           │  │
│  │  └─ ... (mais 5)                                      │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                  Domain Layer (3-Domain)                     │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          ILeadRepository (Interface)                   │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          Lead Entity (Modelo de Domínio)              │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│              Infrastructure Layer (4-Infra)                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          LeadRepository (Implementação)               │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          CommercialDbContext (EF Core)                │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    PostgreSQL Database                       │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          commercial.leads table                       │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

**Avaliação da Direção de Dependência**: ✅ CORRETO (segue Princípio da Inversão de Dependência)
- Infraestrutura depende de Domínio (interface no Domínio).
- Aplicação depende de Domínio (interface).
- Implementação concreta na Infraestrutura.
- Dependências apontam para dentro em direção à lógica de negócio central.

---

## 7. Endpoints

O LeadRepository em si não expõe endpoints HTTP. Ele é consumido por Application Handlers que são invocados por API Controllers. Abaixo está o mapeamento dos métodos do Repositório para endpoints HTTP:

### Mapeamento Repositório-para-Endpoint

| Método Repositório | Método HTTP | Endpoint | Handler do Controller | Propósito |
|---|---|---|---|---|
| AddAsync | POST | /api/leads | LeadController.CreateLead | Cria novo lead |
| GetByIdAsync | GET | /api/leads/{id} | LeadController.GetLead | Obtém lead único |
| ListBySalesPersonAsync | GET | /api/leads?salesPersonId={id} | LeadController.ListLeads | Lista leads para vendedor |
| ListAllAsync | GET | /api/leads | LeadController.ListLeads | Lista todos os leads (admin) |
| UpdateAsync | PUT | /api/leads/{id} | LeadController.UpdateLead | Atualiza detalhes do lead |
| (via Lead entity) | PATCH | /api/leads/{id}/status | LeadController.ChangeStatus | Altera status do lead |
| (via Lead entity) | POST | /api/leads/{id}/qualify | LeadController.QualifyLead | Adiciona qualificação |
| (via Lead entity) | POST | /api/leads/{id}/interactions | LeadController.RegisterInteraction | Adiciona interação |
| CountByStatusAsync | GET | /api/dashboard | LeadController.GetDashboard | KPIs de Dashboard |
| CountCreatedSinceAsync | GET | /api/dashboard | LeadController.GetDashboard | KPIs de Dashboard |
| CountByStatusSinceAsync | GET | /api/dashboard | LeadController.GetDashboard | KPIs de Dashboard |
| GetHotLeadsAsync | GET | /api/dashboard | LeadController.GetDashboard | Widget de leads quentes |

### Endpoints LeadController (LeadController.cs)

| Método HTTP | Rota | Handler | Uso do Repositório |
|---|---|---|---|
| POST | /api/leads | CreateLead | AddAsync + SaveChanges |
| GET | /api/leads/{id} | GetLead | GetByIdAsync |
| GET | /api/leads | ListLeads | ListBySalesPersonAsync ou ListAllAsync |
| PUT | /api/leads/{id} | UpdateLead | UpdateAsync + SaveChanges |
| PATCH | /api/leads/{id}/status | ChangeStatus | UpdateAsync + SaveChanges |
| POST | /api/leads/{id}/qualify | QualifyLead | UpdateAsync + SaveChanges |
| POST | /api/leads/{id}/interactions | RegisterInteraction | GetByIdAsync + UpdateAsync + SaveChanges |
| GET | /api/leads/{id}/interactions | ListInteractions | GetByIdAsync |
| GET | /api/dashboard | GetDashboard | Count*Async + GetHotLeadsAsync |

**Nota**: LeadRepository não gerencia requisições HTTP diretamente. Ele fornece métodos de acesso a dados que os controllers invocam através de handlers de aplicação.

---

## 8. Pontos de Integração

### Integração com Banco de Dados

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|---|---|---|---|---|---|
| PostgreSQL Database | Data Store | Armazenamento persistente de dados de lead | Protocolo ADO.NET/PostgreSQL | Tabelas relacionais | Retentativas de conexão, exceções de timeout |
| Tabela commercial.leads | Tabela Primária | Registros de entidade Lead | SQL (SELECT/INSERT/UPDATE) | Schema mapeado por entidade | Violações de restrição única |
| Tabela commercial.interactions | Tabela Relacionada | Histórico de interação de lead | SQL (via navegação) | Schema mapeado por entidade | Restrições de chave estrangeira |
| Tabela commercial.proposals | Tabela Relacionada | Propostas associadas (futuro) | SQL (via relacionamentos) | Schema mapeado por entidade | Integridade referencial |

### Integração com Entity Framework Core

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|---|---|---|---|---|---|
| CommercialDbContext | Contexto ORM | Sessão de banco de dados EF Core | In-memory → SQL | Objetos .NET → SQL | DbUpdateException, DbUpdateConcurrencyException |
| Change Tracker | Gestão de Estado | Rastreia modificações de entidade | In-memory | Estados de entidade (Added/Modified/Deleted) | Auto-detecção de mudanças |
| Migrations | Gestão de Schema | Versionamento de schema de banco | DDL SQL | Arquivos de migration | Suporte a rollback |
| Conversores de Value Object | Conversão de Tipo | Persistência de Value Object Email/Phone | In-memory | String ↔ Email/Phone | Exceções de conversão |

### Integração com Eventos de Domínio

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|---|---|---|---|---|---|
| Unit of Work | Coordenação de Transação | Salvar atômico com eventos de domínio | In-memory → Transação | Eventos de Domínio → Mensagens Outbox | Rollback de transação em falha |
| Outbox Pattern | Despacho de Eventos | Publicação confiável de eventos | PostgreSQL | Entidade OutboxMessage | Processamento idempotente |
| LeadCreatedEvent | Evento de Domínio | Notificar na criação de lead | Tabela Outbox → Message Bus | Event payload JSON | Mecanismo de retry |
| LeadStatusChangedEvent | Evento de Domínio | Notificar na mudança de status | Tabela Outbox → Message Bus | Event payload JSON | Mecanismo de retry |
| LeadScoredEvent | Evento de Domínio | Notificar na qualificação | Tabela Outbox → Message Bus | Event payload JSON | Mecanismo de retry |

### Integração com Camada de Aplicação

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|---|---|---|---|---|---|
| Application Handlers | Comando/Consulta | Invocar métodos do repositório | Chamadas de método in-memory | Assinaturas de método .NET | Propagação de NotFoundException |
| Mapeamento DTO | Transformação de Dados | Entidade → DTO de Resposta | In-memory (LINQ) | Objetos DTO | Exceções de mapeamento |
| Validação | Validação de Entrada | Garantir integridade de dados | FluentValidation | Regras de validação | ValidationException |

### Integração com Serviços de Infraestrutura

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|---|---|---|---|---|---|
| Dependency Injection | Service Locator | Fornecer instâncias de repositório | DI Container | Serviços Scoped | Exceções de ativação |
| Logging (implícito) | Observabilidade | Logging de execução de queries | ILogger | Logs estruturados | Agregação de logs |
| Health Checks | Saúde do Sistema | Conectividade de banco de dados | Endpoint HTTP | Status de saúde | Detecção de falha de conexão |

### Detalhes do Schema do Banco de Dados

**Tabela: commercial.leads**

```sql
CREATE TABLE commercial.leads (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    source VARCHAR(50) NOT NULL,           -- Enum: 'Showroom', 'Google', etc.
    status VARCHAR(50) NOT NULL,           -- Enum: 'New', 'InNegotiation', etc.
    score VARCHAR(50) NOT NULL,            -- Enum: 'Diamond', 'Gold', 'Silver', 'Bronze'
    sales_person_id UUID NOT NULL,
    interested_model VARCHAR(100),
    interested_trim VARCHAR(100),
    interested_color VARCHAR(50),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,

    -- Entidade proprietária Qualification (colunas achatadas)
    has_trade_in_vehicle BOOLEAN,
    payment_method VARCHAR(50),            -- Enum: 'Cash', 'Financing', 'TradeIn'
    expected_purchase_date TIMESTAMP,
    interested_in_test_drive BOOLEAN,
    estimated_monthly_income DECIMAL,
    qualification_notes VARCHAR(500),

    -- Entidade aninhada TradeInVehicle (colunas achatadas)
    trade_in_brand VARCHAR(50),
    trade_in_model VARCHAR(100),
    trade_in_year INTEGER,
    trade_in_mileage INTEGER,
    trade_in_license_plate VARCHAR(10),
    trade_in_color VARCHAR(50),
    trade_in_general_condition VARCHAR(50),
    trade_in_has_dealership_service_history BOOLEAN,

    -- Índices
    CONSTRAINT idx_leads_sales_person INDEX (sales_person_id),
    CONSTRAINT idx_leads_status INDEX (status),
    CONSTRAINT idx_leads_score INDEX (score)
);
```

**Tabela: commercial.interactions**

```sql
CREATE TABLE commercial.interactions (
    id UUID PRIMARY KEY,
    lead_id UUID NOT NULL,
    type VARCHAR(50) NOT NULL,             -- Enum de tipo de interação
    description TEXT,
    interaction_date TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,

    CONSTRAINT FK_interactions_leads_lead_id
        FOREIGN KEY (lead_id)
        REFERENCES commercial.leads(id)
        ON DELETE CASCADE
);
```

---

## 9. Padrões de Design e Arquitetura

### Padrões Identificados

| Padrão | Implementação | Localização | Propósito |
|---|---|---|---|
| **Repository Pattern** | Interface `ILeadRepository` + classe `LeadRepository` | 3-Domain/Interfaces/ILeadRepository.cs<br>4-Infra/Repositories/LeadRepository.cs | Abstrair lógica de acesso a dados, desacoplar domínio da infraestrutura |
| **Unit of Work Pattern** | `IUnitOfWork` coordena `SaveChanges` entre repositórios | 4-Infra/UnitOfWork/UnitOfWork.cs | Gerenciar transações, garantir atomicidade de múltiplas operações |
| **Dependency Inversion Principle** | Aplicação depende de `ILeadRepository` (Domínio), não classe concreta | Todos os Handlers na camada Application | Depender de abstrações, inverter direção da dependência |
| **Dependency Injection** | `services.AddScoped<ILeadRepository, LeadRepository>()` | 4-Infra/InfrastructureServiceExtensions.cs:17 | Fornecer acoplamento fraco, testabilidade, gestão de tempo de vida |
| **Specification Pattern (Implícito)** | Métodos de filtragem aceitam parâmetros de critérios | `ListBySalesPersonAsync`, `ListAllAsync` | Encapsular lógica de query, combinações de filtro reutilizáveis |
| **Query Object Pattern** | Métodos com múltiplos parâmetros opcionais formam query objects | `ListBySalesPersonAsync(statuses, score, search, dates, page, pageSize)` | Composição de query flexível sem classes de especificação complexas |
| **Lazy Loading vs Eager Loading** | `Include`/`ThenInclude` explícitos para entidades relacionadas | `GetByIdAsync` (linhas 20-22) | Controlar execução de query, prevenir problema N+1 |
| **Value Object Pattern** | Email e Phone como Value Objects com conversores | `LeadConfiguration.cs:25-36` | Encapsular lógica de domínio, prevenir obsessão por primitivos |
| **Owned Entity Type** | `Qualification` e `TradeInVehicle` como entidades proprietárias | `LeadConfiguration.cs:78-132` | Embutir value objects na tabela pai, encapsulamento de agregação raiz |
| **Outbox Pattern** | Eventos de domínio salvos no Outbox antes do commit | `UnitOfWork.SaveChangesAsync` (linhas 28-45) | Publicação confiável de eventos, semântica exactly-once |
| **CQRS (Parcial)** | Métodos de leitura (List/Get) e escrita (Add/Update) separados | Interface `ILeadRepository` | Otimizar operações de leitura/escrita separadamente |
| **Async/Await Pattern** | Todos os métodos retornam `Task` e usam sufixo `*Async` | Todos os métodos do repositório | I/O não bloqueante, aplicações web escaláveis |
| **Cancellation Token Pattern** | Todos os métodos async aceitam parâmetro `cancellationToken` | Todos os métodos do repositório | Cancelamento de requisição gracioso, limpeza de recursos |

### Decisões Arquiteturais

#### Decisão 1: Padrão Repository Sobre Acesso Direto ao DbContext

**Justificativa**:
- **Abstração**: Oculta detalhes do EF Core da camada de Aplicação.
- **Testabilidade**: Permite mockar `ILeadRepository` em testes unitários.
- **Separação de Preocupações**: Lógica de acesso a dados isolada da lógica de negócio.
- **Flexibilidade Futura**: Poderia trocar EF Core por outro ORM sem mudar handlers.

**Trade-offs**:
- **Prós**: Arquitetura limpa, testável, manutenível.
- **Contras**: Camada de abstração adicional, potencial sobre-abstração para queries simples.

#### Decisão 2: Tipos de Entidade Proprietária (Owned Entity) para Qualificação

**Justificativa**:
- **Padrão Aggregate Root**: Lead possui o ciclo de vida da Qualificação.
- **Semântica Value Object**: Qualificação não tem identidade fora do Lead.
- **Otimização de Schema**: Achatado na tabela leads, sem tabela de qualificação separada.
- **Encapsulamento**: Lógica de Qualificação embutida na entidade Lead.

**Implementação**:
```csharp
// LeadConfiguration.cs:78
builder.OwnsOne(x => x.Qualification, q => { ... });
```

**Trade-offs**:
- **Prós**: Schema simples, sem JOINs, sempre carregado com Lead.
- **Contras**: Não pode consultar Qualificação independentemente, tabela tem muitas colunas.

#### Decisão 3: Eager Loading com Include()

**Justificativa**:
- **Desempenho**: Query única recupera lead + qualificação + interações.
- **Prevenção N+1**: Evita queries separadas para cada propriedade de navegação.
- **Consistência**: Sempre retorna dados completos do lead para a camada de aplicação.

**Implementação**:
```csharp
// LeadRepository.cs:20-23
.Include(l => l.Qualification)
    .ThenInclude(q => q!.TradeInVehicle)
.Include(l => l.Interactions)
```

**Trade-offs**:
- **Prós**: Eficiente para casos de uso típicos, desempenho previsível.
- **Contras**: Sempre carrega dados relacionados mesmo quando não necessários (sem lazy loading).

#### Decisão 4: Armazenamentos de String para Enums

**Justificativa**:
- **Legibilidade**: Banco armazena 'New', 'InNegotiation' em vez de 0, 1, 2.
- **Schema Database-First**: Mais fácil de consultar e debugar diretamente no banco.
- **Evolução de Schema**: Adicionar valores de enum não quebra dados existentes.

**Implementação**:
```csharp
// LeadConfiguration.cs:38-50
builder.Property(x => x.Status)
    .HasConversion<string>();
```

**Trade-offs**:
- **Prós**: Legível para humanos, flexível, sem números mágicos no BD.
- **Contras**: Levemente mais espaço de armazenamento, requer parsing de enum.

#### Decisão 5: Lógica de Busca com Extração de Dígitos

**Justificativa**:
- **Experiência do Usuário**: Usuários buscam números de telefone em vários formatos.
- **Flexibilidade**: "(11) 99999-8888", "11999998888", "11 99999 8888" todos funcionam.
- **Simplicidade**: Sem necessidade de validação rígida de formato de telefone na busca.

**Implementação**:
```csharp
// LeadRepository.cs:73-78
var digits = new string(term.Where(char.IsDigit).ToArray());
query = query.Where(l =>
    EF.Functions.Like(l.Phone.Value, $"%{digits}%"));
```

**Trade-offs**:
- **Prós**: Amigável ao usuário, flexível, implementação simples.
- **Contras**: Não pode buscar por padrões de formatação (ex: "buscar todos com parênteses").

#### Decisão 6: Filtragem Híbrida Em-Memória/Banco para HotLeads

**Justificativa**:
- **Complexidade da Query**: Filtro de data de interação complexo para expressar em SQL.
- **Pragmatismo**: Carregar dataset médio (apenas leads de alto valor) depois filtrar em memória.
- **Desempenho**: Banco filtra por score/status primeiro (reduz tamanho do dataset).

**Implementação**:
```csharp
// LeadRepository.cs:271-279
var hotLeads = await query.ToListAsync(cancellationToken);
return hotLeads
    .Where(l => !l.Interactions.Any() ||
               l.Interactions.Max(i => i.InteractionDate) < lastInteractionBefore)
    .ToList();
```

**Trade-offs**:
- **Prós**: Código simples, legível, aproveita LINQ to Objects.
- **Contras**: Potencial problema de desempenho se existirem muitos leads de alto valor.

---

## 10. Dívida Técnica e Riscos

### Problemas Identificados

| Nível de Risco | Área | Problema | Impacto | Evidência |
|---|---|---|---|---|
| **Médio** | Duplicação de Código | Lógica de busca/filtragem duplicada 4 vezes | Carga de manutenção, risco de replicação de bugs | Linhas 71-79, 112-120, 152-160, 192-200 |
| **Baixo** | Desempenho | HotLeads usa filtragem em memória após query de BD | Potencial problema de desempenho com muitos leads Diamond/Gold | Linhas 271-279 |
| **Baixo** | Otimização de Query | Sem projeção de query (sempre SELECT *) | Transferência de dados desnecessária, uso de memória | Todas as chamadas `ToListAsync()` |
| **Baixo** | Indexação de Banco | Índices compostos faltando para queries comuns | Desempenho de query subótimo em filtros | `LeadConfiguration` tem apenas índices de coluna única |
| **Baixo** | Tratamento de Erro | Sem try-catch nos métodos do repositório | Exceções propagam para handlers (design aceitável) | Todos os métodos |
| **Baixo** | Validação | Sem detecção de duplicata antes de inserir | Potenciais violações de restrição única | `AddAsync` (linha 37) |
| **Médio** | Tratamento de Nulo | Entidade Lead pode ter Qualification nula | Propriedades de navegação devem ser checadas | Linha 21: `q!.`, Interactions sempre carregada |
| **Baixo** | Cobertura de Testes | Apenas 3 testes de integração | Cobertura insuficiente para 14 métodos | `LeadRepositoryTests.cs` (126 linhas) |

### Análise Detalhada

#### Dívida 1: Duplicação de Lógica de Busca/Filtragem

**Localização**: Linhas 71-79, 112-120, 152-160, 192-200

**Problema**:
A extração de dígitos de telefone e a lógica de busca LIKE está duplicada em quatro métodos:
- `ListBySalesPersonAsync` (linhas 71-79)
- `CountBySalesPersonAsync` (linhas 112-120)
- `ListAllAsync` (linhas 152-160)
- `CountAllAsync` (linhas 192-200)

**Impacto**:
- Qualquer correção de bug ou melhoria requer mudanças em 4 lugares.
- Risco de divergência lógica ao longo do tempo.
- Carga de manutenção de código.

**Código Atual**:
```csharp
// Repetido 4 vezes
if (!string.IsNullOrWhiteSpace(search))
{
    var term = search.Trim();
    var digits = new string(term.Where(char.IsDigit).ToArray());
    query = query.Where(l =>
        EF.Functions.Like(l.Name, $"%{term}%") ||
        EF.Functions.Like(l.Email.Value, $"%{term}%") ||
        (!string.IsNullOrEmpty(digits) && EF.Functions.Like(l.Phone.Value, $"%{digits}%")));
}
```

---

#### Dívida 2: Filtragem Em-Memória de HotLeads

**Localização**: Linhas 271-279

**Problema**:
O método `GetHotLeadsAsync` carrega todos os leads Diamond/Gold para a memória (excluindo Converted/Lost), depois filtra por data de interação em memória. Se houver milhares de leads de alto valor, isso pode causar problemas de performance.

**Impacto**:
- Uso de memória escala com o número de leads de alto valor.
- Transferência de dados de banco desnecessária.
- Potencial timeout com grandes datasets.

**Código Atual**:
```csharp
// Carrega todos os leads correspondentes em memória primeiro
var hotLeads = await query.ToListAsync(cancellationToken);

// Depois filtra em memória
return hotLeads
    .Where(l => !l.Interactions.Any() ||
               l.Interactions.Max(i => i.InteractionDate) < lastInteractionBefore)
    .ToList();
```

---

#### Dívida 3: Índices Compostos Faltando

**Localização**: `LeadConfiguration.cs:142-144`

**Problema**:
A configuração da entidade define apenas índices de coluna única (SalesPersonId, Status, Score). Padrões de query comuns filtram por combinações:
- `SalesPersonId` + `Status` + `Score` (`ListBySalesPersonAsync`)
- `Status` + `CreatedAt` (`CountByStatusSinceAsync`)

**Impacto**:
- Banco deve realizar scan de índice ou scan sequencial.
- Desempenho de query subótimo em listas filtradas.
- Potenciais table scans em grandes datasets.

**Índices Atuais**:
```csharp
builder.HasIndex(x => x.SalesPersonId);
builder.HasIndex(x => x.Status);
builder.HasIndex(x => x.Score);
```

**Índices Faltando**:
```sql
-- Não definido mas ajudaria
CREATE INDEX idx_leads_salesperson_status_score ON commercial.leads(sales_person_id, status, score);
CREATE INDEX idx_leads_status_created_at ON commercial.leads(status, created_at DESC);
```

---

#### Dívida 4: Sem Projeção de Query (SELECT *)

**Localização**: Todas as chamadas `ToListAsync()`

**Problema**:
Todos os métodos de query usam `ToListAsync()` que gera `SELECT *`, carregando todas as colunas. Para visualizações de lista (ListLeads), nem todas as colunas são necessárias (ex: detalhes de Qualificação não mostrados no item da lista).

**Impacto**:
- Transferência de dados desnecessária do banco.
- Aumento de uso de memória.
- Consumo de largura de banda de rede.

**Código Atual**:
```csharp
return await query
    .OrderByDescending(l => l.Score)
    .ThenByDescending(l => l.CreatedAt)
    .ToListAsync(cancellationToken); // Seleciona todas as colunas
```

---

#### Dívida 5: Cobertura de Testes Limitada

**Localização**: `LeadRepositoryTests.cs` (126 linhas, 3 testes)

**Problema**:
O repositório tem 14 métodos mas apenas 3 testes de integração:
- `AddAsync_ShouldPersistLead`
- `ListBySalesPersonAsync_ShouldFilterBySalesPerson`
- `ListBySalesPersonAsync_ShouldFilterByScore`

**Cobertura de Teste Faltando**:
- `GetByIdAsync` com Include()
- `UpdateAsync`
- `ListAllAsync`
- `CountBySalesPersonAsync`
- `CountAllAsync`
- `CountByStatusAsync`
- `CountCreatedSinceAsync`
- `CountByStatusSinceAsync`
- `GetHotLeadsAsync`
- Funcionalidade de busca (nome, email, telefone)
- Filtragem de intervalo de data
- Paginação (Skip/Take)
- Lógica de ordenação

**Impacto**:
- Baixa confiança na correção da lógica de busca/filtragem.
- Potenciais regressões em queries de dashboard.
- Nenhuma cobertura para regra de negócio de hot leads.

---

## 11. Análise de Cobertura de Testes

### Estratégia de Teste

**Abordagem de Teste**: Testes de integração com banco de dados PostgreSQL real
**Framework de Teste**: xUnit
**Fixture de Teste**: `PostgresFixture` (contexto de banco compartilhado)
**Isolamento de Teste**: Reset de banco antes de cada teste

### Testes Existentes

| Método de Teste | Método Sob Teste | Cobertura | Qualidade |
|---|---|---|---|
| Use `AddAsync_ShouldPersistLead` | `LeadRepository.AddAsync` | CRUD Básico | ✅ Boa assunção, verifica persistência |
| Use `ListBySalesPersonAsync_ShouldFilterBySalesPerson` | `LeadRepository.ListBySalesPersonAsync` | Filtragem por vendedor | ✅ Bom, verifica isolamento de dados |
| Use `ListBySalesPersonAsync_ShouldFilterByScore` | `LeadRepository.ListBySalesPersonAsync` | Filtragem por score | ✅ Bom, verifica relacionamento com Qualification |

### Análise de Cobertura

| Componente | Métodos | Testes | Cobertura | Qualidade |
|---|---|---|---|---|
| **LeadRepository** | 14 | 3 | 21% | Fraco |
| **Operações CRUD** | 4 | 1 | 25% | Limitado |
| **Métodos de Lista/Filtro** | 4 | 2 | 50% | Moderado |
| **Métodos de Contagem** | 4 | 0 | 0% | Nenhuma |
| **Métodos de Dashboard** | 4 | 0 | 0% | Nenhuma |
| **Lógica de Busca** | 4 métodos | 0 | 0% | Nenhuma |
| **Paginação** | 2 métodos | 0 | 0% | Nenhuma |
| **Ordenação** | 2 métodos | 0 | 0% | Nenhuma |

### Avaliação da Qualidade do Teste

**Pontos Fortes**:
- Testes de integração usam PostgreSQL real (não mockado).
- Testes usam `PostgresFixture` para isolamento de banco.
- `FluentAssertions` fornecem asserções legíveis.
- Testes verificam persistência real no banco.

**Pontos Fracos**:
- Cobertura muito baixa (21%).
- Sem testes para funcionalidade de busca (lógica mais complexa).
- Sem testes para métodos de dashboard (critico para o negócio).
- Sem testes para casos de borda de paginação.
- Sem testes para lógica de ordenação.
- Sem testes para filtragem de intervalo de data.
- Sem testes para regra de negócio hot leads.
- Sem testes de desempenho para grandes datasets.

### Cobertura de Teste Recomendada (Não Implementada)

**Testes Críticos Faltando**:

1.  **Testes de Busca** (Prioridade Máxima):
    -   Busca por nome (match parcial).
    -   Busca por email (match parcial).
    -   Busca por telefone com vários formatos.
    -   Busca sem correspondências.
    -   Busca com termo nulo/vazio.

2.  **Testes de Dashboard**:
    -   `CountByStatusAsync` com/sem vendedor.
    -   `CountCreatedSinceAsync` limite de data.
    -   `CountByStatusSinceAsync` filtros combinados.
    -   Verificação de regra de negócio `GetHotLeadsAsync`.

3.  **Testes de Paginação**:
    -   Cálculo Skip/Take.
    -   Casos de borda de número de página (página 0, negativo, grande).
    -   Casos de borda de tamanho de página (0, negativo, grande).

4.  **Testes de Intervalo de Data**:
    -   `createdFrom` apenas.
    -   `createdTo` apenas.
    -   Ambos from e to.
    -   Sem correspondências no intervalo.

5.  **Testes de Eager Loading**:
    -   `GetByIdAsync` carrega Qualification.
    -   `GetByIdAsync` carrega TradeInVehicle.
    -   `GetByIdAsync` carrega Interactions.
    -   Propriedades de navegação não nulas quando esperado.

6.  **Testes de Ordenação**:
    -   `OrderByDescending(Score)`.
    -   `ThenByDescending(CreatedAt)`.
    -   Ordenação estável com chaves iguais.

### Infraestrutura de Teste

**Fixture de Teste**: `PostgresFixture` (localização não fornecida nos arquivos analisados)

**Responsabilidades**:
- Cria `CommercialDbContext` para testes.
- Reseta banco antes de cada teste.
- Fornece ambiente de teste isolado.
- Limpa após conclusão do teste.

**Exemplo de Setup** (de `LeadRepositoryTests.cs`):
```csharp
[Collection("Postgres")] // Fixture compartilhada
public class LeadRepositoryTests
{
    private readonly PostgresFixture _postgresFixture;

    public LeadRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistLead()
    {
        await _postgresFixture.ResetDatabaseAsync(); // Isolamento
        await using var context = _postgresFixture.CreateContext();
        var repository = new LeadRepository(context);
        // ... implementação do teste
    }
}
```

---

## 12. Avaliação de Valor do Padrão Repository

### O Padrão Repository é Necessário Aqui?

#### Critérios de Análise

| Critério | Avaliação | Pontuação (1-5) |
|---|---|---|
| **Valor de Abstração** | Oculta EF Core da camada de Aplicação, fornece interface de domínio | 4/5 |
| **Testabilidade** | Permite mockar em testes unitários de Handler | 5/5 |
| **Complexidade de Query** | Contém queries de negócio complexas (HotLeads, busca, dashboard) | 4/5 |
| **Reusabilidade** | Métodos reutilizados por 13 handlers diferentes | 5/5 |
| **Flexibilidade Futura** | Poderia trocar ORM sem mudar handlers | 3/5 |
| **Custo de Manutenção** | Camada adicional para manter | 3/5 |
| **Overhead** | Métodos em grande parte pass-through para EF Core (baixa abstração) | 3/5 |

**Pontuação Geral**: 3.9/5 - **Valor Moderado a Alto**

### Valor Fornecido pelo Repositório

**Aspectos Positivos**:

1.  **Interface de Domínio na Camada de Domínio**:
    -   `ILeadRepository` em 3-Domain (não Infraestrutura).
    -   Segue Princípio da Inversão de Dependência.
    -   Infraestrutura depende do Domínio (direção correta).

2.  **Encapsulamento de Query Complexa**:
    -   `GetHotLeadsAsync`: Lógica de negócio para identificar leads quentes.
    -   Lógica de busca: Extração de dígitos de telefone, queries LIKE multi-coluna.
    -   Queries de Dashboard: Agregados para KPIs.
    -   Estes poluiriam handlers se fossem inline.

3.  **Testabilidade**:
    -   Handlers podem mockar `ILeadRepository` para testes unitários.
    -   Repositório tem seus próprios testes de integração com banco real.
    -   Clara separação de preocupações de teste.

4.  **Acesso a Dados Consistente**:
    -   Todas as queries de lead vão através de interface única.
    -   Padrões de eager loading padronizados.
    -   Tratamento de erro consistente.

5.  **Estratégia de Eager Loading**:
    -   Repositório controla estratégia `Include()`.
    -   Camada de aplicação não precisa conhecimento de EF Core.
    -   Previne queries N+1 por padrão.

### Valor Limitado (Crítica)

**Preocupações**:

1.  **Abstração Fina para Queries Simples**:
    ```csharp
    // LeadRepository.cs:37-40
    public async Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(lead, cancellationToken);
        return lead;
    }
    ```
    Isso é apenas um pass-through para EF Core com valor mínimo.

2.  **Acesso Direto ao DbContext Funcionaria**:
    -   Handlers poderiam injetar `CommercialDbContext` diretamente.
    -   EF Core já é uma abstração (camada de acesso a dados).
    -   Repositório adiciona pouco em cima do DbContext para CRUD simples.

3.  **Duplicação com Métodos DbSet**:
    -   `GetByIdAsync` ≈ `DbSet.Find()`
    -   `AddAsync` ≈ `DbSet.AddAsync()`
    -   `UpdateAsync` ≈ `DbSet.Update()`
    -   Métodos de lista ≈ `DbSet.Where().ToListAsync()`

4.  **Testabilidade Sem Repositório**:
    -   Poderia usar `InMemory` DbContext para testes de handler.
    -   Poderia mockar `DbSet<T>` diretamente.
    -   Repositório não é estritamente necessário para testabilidade.

### Design Alternativo: Acesso Direto ao DbContext

**Como Seria**:
```csharp
// Handler com DbContext direto
public class GetLeadHandler
{
    private readonly CommercialDbContext _context;

    public GetLeadHandler(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<LeadResponse> HandleAsync(GetLeadQuery query, ct)
    {
        var lead = await _context.Leads
            .Include(l => l.Qualification)
            .ThenInclude(q => q!.TradeInVehicle)
            .Include(l => l.Interactions)
            .FirstOrDefaultAsync(l => l.Id == query.LeadId, ct);

        // ... resto da lógica
    }
}
```

**Trade-offs**:

| Aspecto | Com Repositório | DbContext Direto |
|---|---|---|
| **Abstração** | Separação limpa, interface de domínio | Handlers dependem de Infraestrutura |
| **Testabilidade** | Mock `ILeadRepository` | Usar InMemory DbContext ou mock DbSet |
| **Queries Complexas** | Encapsuladas no repositório | Espalhadas pelos handlers |
| **Conhecimento EF Core** | Oculto dos handlers | Exposto aos handlers |
| **Camadas** | 4 camadas (Controller → Handler → Repository → DbContext) | 3 camadas (Controller → Handler → DbContext) |
| **Manutenção** | Mais código para manter | Menos código, mas handlers mais complexos |

### Avaliação de Recomendação

**Implementação Atual**: ✅ **Apropriada para este Projeto**

**Justificativa**:
1.  **Domain-Driven Design**: Projeto segue princípios DDD, Repository é padrão.
2.  **Queries de Negócio Complexas**: HotLeads, dashboard, busca justificam abstração.
3.  **Padrões de Time**: Outras entidades (Proposal, TestDrive) também usam repositórios.
4.  **Arquitetura Limpa**: Direção da dependência está correta (Interface de Domínio).
5.  **Estratégia de Teste**: Clara separação entre teste unitário (mockado) e integração (BD real).

**Quando Repositório Seria Desnecessário**:
- Aplicação CRUD simples sem queries de negócio.
- Projeto pequeno com <10 entidades.
- Sem necessidade de abstração de interface de domínio.
- Time prefere acesso DbContext mais simples.

**Quando Repositório é Essencial**:
- Queries de negócio complexas (como HotLeads).
- Necessidade de interface de domínio na camada de Domínio.
- Múltiplas fontes de consulta (SQL, NoSQL, API).
- Projeto grande com muitas entidades.
- Requisito de isolamento de teste.

### Conclusão

O `LeadRepository` fornece **valor moderado a alto** para este projeto. Enquanto métodos CRUD simples são abstrações finas, os métodos de consulta complexos (GetHotLeadsAsync, busca, dashboard KPIs) justificam a existência do padrão. O posicionamento arquitetural correto (interface na camada de Domínio) e a aderência ao Princípio da Inversão de Dependência validam ainda mais sua necessidade.

---

## 13. Resumo e Conclusões

### Pontos Fortes do Componente

1.  **Arquitetura Limpa**:
    -   Interface na camada de Domínio (direção de dependência correta).
    -   Infraestrutura depende de Domínio (Princípio de Inversão de Dependência).
    -   Clara separação de preocupações.

2.  **API Abrangente**:
    -   14 métodos cobrem todas as necessidades de CRUD, filtragem, busca e dashboard.
    -   Padrões async/await consistentes.
    -   Suporte a cancellation token em tudo.

3.  **Considerações de Desempenho**:
    -   Eager loading previne queries N+1.
    -   Geração de SQL eficiente com `Include()`.
    -   Indexação adequada em colunas chave.

4.  **Encapsulamento de Lógica de Negócio**:
    -   Query HotLeads implementa regra de negócio valiosa.
    -   Busca com extração de dígitos fornece busca de telefone amigável.
    -   Cálculos de KPI de dashboard suportam inteligência de negócio.

5.  **Infraestrutura de Teste**:
    -   Testes de integração com PostgreSQL real.
    -   `PostgresFixture` para isolamento de teste.
    -   `FluentAssertions` para testes legíveis.

### Pontos Fracos do Componente

1.  **Duplicação de Código**:
    -   Lógica de busca/filtragem duplicada 4 vezes.
    -   Lógica de extração de dígitos de telefone repetida.
    -   Carga de manutenção para mudanças.

2.  **Cobertura de Testes**:
    -   Apenas 21% de cobertura (3 testes para 14 métodos).
    -   Sem testes para lógica de busca.
    -   Sem testes para métodos de dashboard.
    -   Faltam testes de caso de borda.

3.  **Otimização de Query**:
    -   Sem projeção de query (SELECT *).
    -   Faltam índices compostos para padrões de filtro comuns.
    -   Filtragem em memória em HotLeads poderia ser problemática.

4.  **Funcionalidades Faltando**:
    -   Sem detecção de duplicata antes de inserir.
    -   Sem operações em massa (bulk insert, bulk update).
    -   Sem otimização read-only/query-only (`AsNoTracking`).

### Avaliação Geral

**Classificação de Qualidade**: 7.0/10

**Detalhamento**:
-   **Arquitetura**: 9/10 (Excelente estratificação, princípios DDD).
-   **Funcionalidade**: 8/10 (API abrangente, lógica de negócio).
-   **Qualidade de Código**: 6/10 (Duplicação, algumas ineficiências).
-   **Testes**: 4/10 (Cobertura pobre).
-   **Desempenho**: 7/10 (Bom eager loading, falta otimização).
-   **Manutenibilidade**: 7/10 (Boa estrutura, problemas de duplicação).

### Principais Conclusões

**O Que Este Componente Faz Bem**:
-   Implementa o padrão Repository corretamente com interface de Domínio.
-   Fornece consultas flexíveis com múltiplos critérios de filtro.
-   Lida com lógica de negócio complexa (HotLeads) elegantemente.
-   Mantém separação limpa do EF Core na camada de Aplicação.

**O Que Poderia Ser Melhorado**:
-   Extrair lógica de busca duplicada para método auxiliar privado.
-   Adicionar cobertura de teste abrangente (busca, dashboard, paginação).
-   Considerar projeção de query para visualizações de lista.
-   Adicionar índices de banco compostos para padrões de query comuns.
-   Implementar detecção de duplicata em AddAsync.
-   Considerar `AsNoTracking()` para queries somente leitura.

**Validação Arquitetural**:
O `LeadRepository` é um componente bem projetado que implementa adequadamente o padrão Repository dentro de um contexto de Arquitetura Limpa / Domain-Driven Design. A localização da interface na camada de Domínio e implementação na camada de Infraestrutura demonstra aplicação correta do Princípio da Inversão de Dependência. Embora o padrão adicione overhead de abstração, as queries de negócio complexas e benefícios de testabilidade justificam sua existência neste projeto.

---

## Apêndice A: Localização de Arquivos

### Arquivos Analisados

**Componente Primário**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/4-Infra/GestAuto.Commercial.Infra/Repositories/LeadRepository.cs`
-   **Linhas de Código**: 281
-   **Métodos**: 14
-   **Dependências**: 5 (CommercialDbContext, ILeadRepository, Lead, Microsoft.EntityFrameworkCore, LINQ)

**Contrato de Interface**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/3-Domain/GestAuto.Commercial.Domain/Interfaces/ILeadRepository.cs`
-   **Linhas de Código**: 52
-   **Assinaturas de Método**: 14

**Modelo de Entidade**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/3-Domain/GestAuto.Commercial.Domain/Entities/Lead.cs`
-   **Linhas de Código**: 117
-   **Propriedades**: 11
-   **Métodos**: 7 (Create, Qualify, ChangeStatus, AddInteraction, UpdateInterest, UpdateName, UpdateEmail, UpdatePhone, RegisterInteraction)

**Contexto de Banco de Dados**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/4-Infra/GestAuto.Commercial.Infra/CommercialDbContext.cs`
-   **Linhas de Código**: 34
-   **DbSets**: 10 (Leads, Interactions, Proposals, ProposalItems, TestDrives, UsedVehicleEvaluations, Orders, OutboxMessages, AuditEntries, PaymentMethods)

**Configuração de Entidade**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/4-Infra/GestAuto.Commercial.Infra/EntityConfigurations/LeadConfiguration.cs`
-   **Linhas de Código**: 146
-   **Colunas Configuradas**: 23
-   **Relacionamentos Configurados**: 1 (Coleção Interactions)
-   **Índices Configurados**: 3

**Unit of Work**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/4-Infra/GestAuto.Commercial.Infra/UnitOfWork/UnitOfWork.cs`
-   **Linhas de Código**: 107
-   **Métodos**: 6 (SaveChangesAsync, BeginTransactionAsync, CommitAsync, RollbackAsync, Dispose, CollectDomainEventsFromEntities)

**Injeção de Dependência**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/4-Infra/GestAuto.Commercial.Infra/InfrastructureServiceExtensions.cs`
-   **Linhas de Código**: 34
-   **Registros de Repositório**: 6 (Lead, Proposal, TestDrive, UsedVehicleEvaluation, Order, Outbox)

**Testes de Integração**:
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/5-Tests/GestAuto.Commercial.IntegrationTest/Repository/LeadRepositoryTests.cs`
-   **Linhas de Código**: 126
-   **Métodos de Teste**: 3
-   **Framework de Teste**: xUnit + FluentAssertions

**Exemplos de Handler** (13 consumidores totais):
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/2-Application/GestAuto.Commercial.Application/Handlers/GetLeadHandler.cs` (26 linhas)
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/2-Application/GestAuto.Commercial.Application/Handlers/ListLeadsHandler.cs` (84 linhas)
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/2-Application/GestAuto.Commercial.Application/Handlers/CreateLeadHandler.cs` (46 linhas)
-   `/home/tsgomes/github-tassosgomes/GestAuto/services/commercial/2-Application/GestAuto.Commercial.Application/Handlers/GetDashboardDataHandler.cs` (106 linhas)

---

**Fim do Relatório**

---

**Metadados da Análise**:
-   **Data de Análise**: 23/01/2026
-   **Hora da Análise**: 10:14:34 UTC
-   **Analisador**: Agente Analisador Profundo de Componentes
-   **Escopo**: Apenas componente LeadRepository (Camada de Infraestrutura - Acesso a Dados)
-   **Método**: Análise de código estática com reconhecimento de padrões arquiteturais
-   **Total de Arquivos Analisados**: 10
-   **Total de Linhas de Código Analisadas**: 1.069
-   **Profundidade de Análise**: Análise de implementação completa com extração de regras de negócio
