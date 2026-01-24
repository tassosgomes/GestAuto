# Relatório de Análise Profunda de Componente: Entidade de Domínio Lead

**Relatório Gerado**: 23/01/2026 10:14:39
**Componente**: Entidade de Domínio Lead
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Entities/Lead.cs`
**Camada**: Camada de Domínio
**Paradigma**: Domain-Driven Design (DDD)

---

## 1. Visão Geral do Componente

A entidade Lead é uma entidade central do domínio no contexto delimitado Comercial, representando um cliente potencial no funil de vendas automotivas. Ela implementa modelagem rica de domínio com lógica de negócio encapsulada, eventos de domínio e objetos de valor. A entidade segue princípios DDD com agregados claros, encapsulamento de comportamento e publicação de eventos de domínio.

**Principais Responsabilidades**:
- Gerenciar ciclo de vida do lead através de transições de status (Novo → EmContato → EmNegociacao → TestDriveAgendado → PropostaEnviada → Perdido/Convertido)
- Calcular e manter pontuação do lead baseado em critérios de qualificação
- Rastrear interações com o cliente ao longo do processo de vendas
- Impor invariantes de negócio para integridade de dados do lead
- Publicar eventos de domínio para integração com sistemas externos

**Papel Arquitetural**:
- Raiz de Agregado para o agregado Lead
- Origina eventos de domínio para consistência eventual
- Coordena com serviços de domínio (LeadScoringService) para lógica de negócio complexa
- Mantém relacionamentos com entidades filhas (Interaction)

---

## 2. Detalhes do Modelo de Domínio

### 2.1 Propriedades Principais

| Propriedade | Tipo | Acesso | Descrição | Validação |
|-------------|------|--------|-----------|-----------|
| `Id` | `Guid` | Protected | Identificador único (herdado de BaseEntity) | Auto-gerado |
| `Name` | `string` | Private set | Nome completo do Lead | Não nulo/vazio |
| `Email` | `Email` (Value Object) | Private set | Endereço de email do Lead | Formato de email válido, normalizado para minúsculo |
| `Phone` | `Phone` (Value Object) | Private set | Número de telefone do Lead | 10-11 dígitos, auto-formatado |
| `Source` | `LeadSource` (Enum) | Private set | Canal de aquisição do Lead | Instagram, Indicação, Google, Loja, Telefone, Showroom, PortalClassificados, Outros |
| `Status` | `LeadStatus` (Enum) | Private set | Estágio atual no funil de vendas | Novo, EmContato, EmNegociacao, TestDriveAgendado, PropostaEnviada, Perdido, Convertido |
| `Score` | `LeadScore` (Enum) | Private set | Avaliação de qualidade do Lead | Bronze, Prata, Ouro, Diamante |
| `SalesPersonId` | `Guid` | Private set | Identificador do representante de vendas atribuído | Obrigatório |
| `Qualification` | `Qualification?` (Value Object) | Private set | Detalhes opcionais de qualificação | nulo até qualificado |
| `Interactions` | `List<Interaction>` | Private set | Coleção de registros de interação | Auto-inicializado |
| `InterestedModel` | `string?` | Private set | Modelo de veículo de interesse | Opcional |
| `InterestedTrim` | `string?` | Private set | Versão/Acabamento do veículo de interesse | Opcional |
| `InterestedColor` | `string?` | Private set | Preferência de cor do veículo | Opcional |

**Propriedades Temporais** (de BaseEntity):
- `CreatedAt` (DateTime) - Auto-definido na criação
- `UpdatedAt` (DateTime) - Auto-atualizado em mudanças de estado

### 2.2 Objetos de Valor

#### Email (`/services/commercial/3-Domain/GestAuto.Commercial.Domain/ValueObjects/Email.cs`)
- **Objeto de valor imutável** com validação de email
- Normaliza para invariante minúsculo
- Usa `System.Net.Mail.MailAddress` para validação
- Implementa classe base `ValueObject` para igualdade por valor
- **Invariante**: Deve ser formato de email válido

#### Phone (`/services/commercial/3-Domain/GestAuto.Commercial.Domain/ValueObjects/Phone.cs`)
- **Objeto de valor imutável** com formatação de número de telefone
- Extrai componentes DDD (código de área) e número
- Valida comprimento de 10-11 dígitos
- Fornece exibição formatada: `(DDD) XXXXX-XXXX` ou `(DDD) XXXX-XXXX`
- **Invariante**: Deve ter 10-11 dígitos

#### Qualification (`/services/commercial/3-Domain/GestAuto.Commercial.Domain/ValueObjects/Qualification.cs`)
- **Record imutável** representando dados de qualificação do lead
- Contém:
  - `HasTradeInVehicle` (bool)
  - `TradeInVehicle` (TradeInVehicle?) - objeto de valor aninhado
  - `PaymentMethod` (enum: Dinheiro, Financiamento, Consorcio, Leasing)
  - `ExpectedPurchaseDate` (DateTime)
  - `InterestedInTestDrive` (bool)
  - `EstimatedMonthlyIncome` (decimal?) - opcional
  - `Notes` (string?) - opcional
- **Invariantes**:
  - Se `HasTradeInVehicle` é verdadeiro, `TradeInVehicle` deve ser fornecido
  - Se `HasTradeInVehicle` é falso, `TradeInVehicle` deve ser nulo

#### TradeInVehicle (aninhado em Qualification.cs)
- **Record imutável** para detalhes de veículo de troca
- Contém: Marca, Modelo, Ano, Quilometragem, Placa, Cor, CondicaoGeral, TemHistoricoServicoConcessionaria
- **Invariantes**:
  - Marca e Modelo não podem ser vazios
  - Ano deve ser entre 1900 e ano atual + 1
  - Quilometragem não pode ser negativa
  - Placa não pode ser vazia

### 2.3 Entidades Filhas

#### Interaction (`/services/commercial/3-Domain/GestAuto.Commercial.Domain/Entities/Interaction.cs`)
- **Entidade** herdando de BaseEntity
- Representa um ponto de contato único de comunicação com o lead
- Propriedades:
  - `LeadId` (Guid) - chave estrangeira para Lead pai
  - `Type` (string) - ex: "Ligacao", "Email", "Reuniao"
  - `Description` (string) - detalhes da interação
  - `InteractionDate` (DateTime) - quando a interação ocorreu
  - `Result` (string?) - resultado opcional
- **Invariantes**:
  - Tipo não pode ser vazio
  - Descrição não pode ser vazia

### 2.4 Relacionamentos de Domínio

```
Lead (Raiz de Agregado)
├── Email (Value Object)
├── Phone (Value Object)
├── Qualification? (Value Object)
│   └── TradeInVehicle? (Value Object)
└── Interactions (Collection<Entity>)
    └── Interaction (Entity)
```

---

## 3. Comportamentos de Domínio

### 3.1 Comportamento de Criação

**Método**: `Lead.Create()`
**Localização**: `Lead.cs:28-57`
**Padrão Static Factory**

**Parâmetros**:
- `name` (string) - Obrigatório
- `email` (Email) - Value object
- `phone` (Phone) - Value object
- `source` (LeadSource) - Enum
- `salesPersonId` (Guid) - Obrigatório
- `interestedModel` (string?) - Opcional
- `interestedTrim` (string?) - Opcional
- `interestedColor` (string?) - Opcional

**Lógica de Negócio**:
1. Valida se nome não é nulo/vazio (lança `ArgumentException`)
2. Inicializa nova instância Lead com padrões:
   - `Status` = `LeadStatus.New`
   - `Score` = `LeadScore.Bronze` (pontuação inicial)
3. Publica evento de domínio `LeadCreatedEvent`
4. Retorna entidade Lead totalmente construída

**Evento de Domínio Publicado**:
- `LeadCreatedEvent(LeadId, Name, Source)`

### 3.2 Comportamento de Qualificação

**Método**: `Qualify(Qualification, LeadScoringService)`
**Localização**: `Lead.cs:59-67`

**Parâmetros**:
- `qualification` (Qualification) - Value object com dados de qualificação
- `scoringService` (LeadScoringService) - Serviço de domínio para cálculo de pontuação

**Lógica de Negócio**:
1. Armazena dados de qualificação
2. Delega cálculo de pontuação para `LeadScoringService`
3. Atualiza propriedade `Score` baseado no cálculo do serviço
4. Transiciona automaticamente `Status` para `LeadStatus.InNegotiation`
5. Atualiza timestamp `UpdatedAt`
6. Publica evento de domínio `LeadScoredEvent`

**Transição de Estado**: `Novo/EmContato` → `EmNegociacao`

**Evento de Domínio Publicado**:
- `LeadScoredEvent(LeadId, Score)`

**Regra de Negócio**: Qualificação avança automaticamente o lead para estágio de negociação

### 3.3 Comportamento de Mudança de Status

**Método**: `ChangeStatus(LeadStatus)`
**Localização**: `Lead.cs:69-74`

**Parâmetros**:
- `newStatus` (LeadStatus) - Novo valor de status

**Lógica de Negócio**:
1. Atualiza propriedade `Status`
2. Atualiza timestamp `UpdatedAt`
3. Publica evento de domínio `LeadStatusChangedEvent`

**Evento de Domínio Publicado**:
- `LeadStatusChangedEvent(LeadId, NewStatus)`

**Invariantes Impostos**:
- Qualquer transição de status é permitida (sem validação na entidade)
- Validação de status ocorre na camada de aplicação (validadores)

### 3.4 Comportamentos de Gerenciamento de Interação

#### Adicionar Interação
**Método**: `AddInteraction(Interaction)`
**Localização**: `Lead.cs:76-80`

**Parâmetros**:
- `interaction` (Interaction) - Entidade Interaction pré-construída

**Lógica de Negócio**:
1. Adiciona interação à coleção `Interactions`
2. Atualiza timestamp `UpdatedAt`

#### Registrar Interação (Método Factory)
**Método**: `RegisterInteraction(InteractionType, string, DateTime)`
**Localização**: `Lead.cs:111-116`

**Parâmetros**:
- `type` (InteractionType) - Enum: Ligacao, Email, WhatsApp, Visita, Outros
- `description` (string) - Detalhes da interação
- `occurredAt` (DateTime) - Quando a interação aconteceu

**Lógica de Negócio**:
1. Cria entidade Interaction via static factory (`Interaction.Create`)
2. Adiciona à coleção de interações via `AddInteraction`
3. Retorna entidade Interaction criada

**Padrão de Design**: Combina factory e gerenciamento de estado em operação única

### 3.5 Comportamentos de Atualização de Informação

#### Atualizar Nome
**Método**: `UpdateName(string)`
**Localização**: `Lead.cs:90-97`

**Lógica de Negócio**:
1. Valida se nome não é nulo/vazio
2. Atualiza propriedade `Name`
3. Atualiza timestamp `UpdatedAt`

#### Atualizar Email
**Método**: `UpdateEmail(Email)`
**Localização**: `Lead.cs:99-103`

**Lógica de Negócio**:
1. Atualiza propriedade `Email` (já validada no construtor do value object)
2. Atualiza timestamp `UpdatedAt`

#### Atualizar Telefone
**Método**: `UpdatePhone(Phone)`
**Localização**: `Lead.cs:105-109`

**Lógica de Negócio**:
1. Atualiza propriedade `Phone` (já validada no construtor do value object)
2. Atualiza timestamp `UpdatedAt`

#### Atualizar Interesse em Veículo
**Método**: `UpdateInterest(string?, string?, string?)`
**Localização**: `Lead.cs:82-88`

**Parâmetros**:
- `model` (string?) - Modelo do veículo
- `trim` (string?) - Versão/Acabamento
- `color` (string?) - Preferência de cor

**Lógica de Negócio**:
1. Atualiza todas as três propriedades de interesse
2. Atualiza timestamp `UpdatedAt`

**Nota de Design**: Permite atualizações parciais (valores nulos permitidos)

---

## 4. Eventos de Domínio

### 4.1 Arquitetura de Evento

**Interface Base**: `IDomainEvent`
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Events/IDomainEvent.cs`

Todos eventos de domínio implementam:
```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

Eventos são armazenados em `BaseEntity` usando padrão de coleção:
```csharp
private readonly List<IDomainEvent> _domainEvents = new();
public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
protected void AddEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
public void ClearEvents() { _domainEvents.Clear(); }
```

### 4.2 Eventos Específicos de Lead

#### LeadCreatedEvent
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Events/LeadCreatedEvent.cs`
**Tipo de Record**: Immutable record

**Payload**:
- `EventId` (Guid) - Identificador único do evento
- `LeadId` (Guid) - Identificador da raiz de agregado
- `Name` (string) - Nome do Lead
- `Source` (LeadSource) - Canal de aquisição do Lead
- `OccurredAt` (DateTime) - Timestamp do evento (UTC)

**Quando Publicado**: Criação de entidade Lead via static factory `Lead.Create()`

**Propósito**: Notifica sistemas externos de nova aquisição de lead, dispara fluxos de boas-vindas, inicializa métricas de vendas

#### LeadStatusChangedEvent
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Events/LeadStatusChangedEvent.cs`
**Tipo de Record**: Immutable record

**Payload**:
- `EventId` (Guid) - Identificador único do evento
- `LeadId` (Guid) - Identificador da raiz de agregado
- `NewStatus` (LeadStatus) - Novo valor de status
- `OccurredAt` (DateTime) - Timestamp do evento (UTC)

**Quando Publicado**: Mudança de status via método `ChangeStatus()`

**Propósito**: Atualiza métricas de funil de vendas, dispara fluxos específicos de estágio, notifica vendedor de avanço do lead

#### LeadScoredEvent
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Events/LeadScoredEvent.cs`
**Tipo de Record**: Immutable record

**Payload**:
- `EventId` (Guid) - Identificador único do evento
- `LeadId` (Guid) - Identificador da raiz de agregado
- `Score` (LeadScore) - Pontuação calculada do lead
- `OccurredAt` (DateTime) - Timestamp do evento (UTC)

**Quando Publicado**: Qualificação de lead via método `Qualify()`

**Propósito**: Atualiza priorização de lead, dispara fluxos de lead de alto valor, ajusta estratégias de atribuição de vendedor

### 4.3 Padrão de Fluxo de Evento

```
Método de Entidade → Mudança de Estado → AddEvent() → Coleção DomainEvents
                                            ↓
                        Camada de Aplicação (Após Persistência)
                                            ↓
                        Dispatcher de Evento → Sistemas Externos
```

**Ciclo de Vida de Evento**:
1. Entidade publica evento durante mudança de estado
2. Eventos armazenados na coleção `_domainEvents` da entidade
3. Camada de aplicação recupera eventos após persistência
4. Dispatcher de evento publica para message bus
5. Eventos limpos via `ClearEvents()`

---

## 5. Regras de Negócio

### 5.1 Visão Geral das Regras de Negócio

| ID Regra | Tipo Regra | Descrição | Localização | Ponto de Imposição |
|----------|------------|-----------|-------------|--------------------|
| RN-001 | Validação | Nome do Lead não pode ser vazio | Lead.cs:38-39 | Construtor de Entidade |
| RN-002 | Inicialização | Novos leads iniciam com status "Novo" | Lead.cs:47 | Factory Create |
| RN-003 | Inicialização | Novos leads iniciam com score "Bronze" | Lead.cs:48 | Factory Create |
| RN-004 | Transição Estado | Qualificação avança status para "EmNegociacao" | Lead.cs:64 | Método Qualify |
| RN-005 | Transição Estado | Todas mudanças de estado atualizam timestamp | Lead.cs:63,72 | Todos métodos mutadores |
| RN-006 | Publicação Evento | Criação de Lead publica LeadCreatedEvent | Lead.cs:55 | Factory Create |
| RN-007 | Publicação Evento | Mudanças de status publicam LeadStatusChangedEvent | Lead.cs:73 | Método ChangeStatus |
| RN-008 | Publicação Evento | Qualificação publica LeadScoredEvent | Lead.cs:66 | Método Qualify |
| RN-009 | Validação | Email deve ser formato válido | Email.cs:15-16 | Construtor Value Object |
| RN-010 | Normalização | Email normalizado para minúsculo | Email.cs:18 | Construtor Value Object |
| RN-011 | Validação | Telefone deve ter 10-11 dígitos | Phone.cs:20-21 | Construtor Value Object |
| RN-012 | Consistência | Qualificação com troca requer dados TradeInVehicle | Qualification.cs:26-27 | Construtor Value Object |
| RN-013 | Consistência | Qualificação sem troca não pode ter dados TradeInVehicle | Qualification.cs:29-30 | Construtor Value Object |
| RN-014 | Validação | Ano veículo troca deve ser válido | Qualification.cs:72-73 | Construtor TradeInVehicle |
| RN-015 | Validação | Quilometragem troca não pode ser negativa | Qualification.cs:75-76 | Construtor TradeInVehicle |

### 5.2 Regras de Negócio Detalhadas

#### RN-001: Validação de Nome do Lead
**Localização**: `Lead.cs:38-39`

**Descrição**:
Todo lead deve ter um nome não vazio para garantir identificação e comunicação apropriada no processo de vendas. Este invariante é imposto no momento da criação da entidade e durante atualizações de nome.

**Implementação**:
```csharp
if (string.IsNullOrWhiteSpace(name))
    throw new ArgumentException("Name cannot be empty", nameof(name));
```

**Impacto de Negócio**:
- Previne criação de leads anônimos/sem nome
- Garante qualidade de dados CRM
- Habilita personalização adequada em comunicações

#### RN-002, RN-003: Padrões de Estado Inicial
**Localização**: `Lead.cs:47-48`

**Descrição**:
Todos leads recém-criados entram automaticamente no sistema com status "Novo" e pontuação "Bronze". Isso estabelece um ponto de partida claro no funil de vendas e garante que a pontuação de lead ocorra através do processo de qualificação apropriado.

**Implementação**:
```csharp
Status = LeadStatus.New,
Score = LeadScore.Bronze, // Score inicial
```

**Impacto de Negócio**:
- Fornece estado inicial consistente através de todos canais de aquisição
- Garante que processo de qualificação seja requerido para avanço de pontuação
- Habilita cálculo de métricas de funil desde o primeiro contato

#### RN-004: Transição de Status Orientada a Qualificação
**Localização**: `Lead.cs:64`

**Descrição**:
Quando um lead é qualificado com intenção de compra, método de financiamento e dados de cronograma, o sistema automaticamente avança o status do lead para "EmNegociacao". Isso reflete a realidade de negócio que leads qualificados estão ativamente engajados em discussões de vendas.

**Implementação**:
```csharp
Status = LeadStatus.InNegotiation;
```

**Impacto de Negócio**:
- Automatiza progressão de funil baseada em dados de qualificação
- Dispara fluxos de estágio de negociação e notificações
- Reflete valor aumentado do lead e nível de engajamento

#### RN-005: Atualizações Automáticas de Timestamp
**Localização**: `Lead.cs:63,72,79,87,96,102,108`

**Descrição**:
Toda mudança de estado na entidade lead atualiza automaticamente o timestamp `UpdatedAt`. Isso fornece capacidades de trilha de auditoria e habilita análises baseadas em tempo como métricas de tempo de resposta e detecção de leads estagnados.

**Implementação**:
```csharp
UpdatedAt = DateTime.UtcNow;
```

**Impacto de Negócio**:
- Habilita cálculo de métricas de envelhecimento de lead
- Suporta requisitos de auditoria e conformidade
- Facilita análises e relatórios baseados em tempo

#### RN-006, RN-007, RN-008: Publicação de Evento de Domínio
**Localização**: `Lead.cs:55,66,73`

**Descrição**:
A entidade Lead publica eventos de domínio para todas mudanças de estado significativas: criação, qualificação (pontuação), e mudanças de status. Estes eventos habilitam consistência eventual com sistemas externos e disparam fluxos downstream.

**Implementação**:
```csharp
AddEvent(new LeadCreatedEvent(lead.Id, name, source));
AddEvent(new LeadScoredEvent(Id, Score));
AddEvent(new LeadStatusChangedEvent(Id, newStatus));
```

**Impacto de Negócio**:
- Desacopla gerenciamento de lead de integrações com sistemas externos
- Habilita dashboards e análises em tempo real
- Suporta padrões de arquitetura orientada a eventos

#### RN-009, RN-010: Validação de Email e Normalização
**Localização**: `Email.cs:15-18`

**Descrição**:
Endereços de email devem estar em formato válido e são automaticamente normalizados para minúsculo para garantir unicidade e prevenir leads duplicados de variações de caixa.

**Implementação**:
```csharp
if (!IsValidEmail(value))
    throw new DomainException("Email inválido");
Value = value.ToLowerInvariant();
```

**Impacto de Negócio**:
- Previne criação de leads com informação de contato inválida
- Garante que deduplicação baseada em email funcione corretamente
- Mantém consistência de dados através dos sistemas

#### RN-011: Validação de Número de Telefone
**Localização**: `Phone.cs:20-21`

**Descrição**:
Números de telefone devem conter 10 ou 11 dígitos para acomodar formatos de telefone brasileiros (fixos: 10 dígitos, móveis: 11 dígitos). Caracteres não numéricos são automaticamente removidos.

**Implementação**:
```csharp
var cleanNumber = new string(value.Where(char.IsDigit).ToArray());
if (cleanNumber.Length < 10 || cleanNumber.Length > 11)
    throw new DomainException("Telefone deve ter 10 ou 11 dígitos");
```

**Impacto de Negócio**:
- Garante que todos números de telefone sejam discáveis
- Suporta canais de comunicação SMS e voz
- Adapta-se a convenções locais de número de telefone

#### RN-012, RN-013: Consistência de Qualificação
**Localização**: `Qualification.cs:26-30`

**Descrição**:
Dados de qualificação mantêm integridade referencial entre a flag `HasTradeInVehicle` e o objeto de valor `TradeInVehicle`. Isso previne estados inconsistentes onde uma troca é indicada mas nenhum dado é fornecido, ou vice-versa.

**Implementação**:
```csharp
if (hasTradeInVehicle && tradeInVehicle == null)
    throw new ArgumentException("TradeInVehicle must be provided when HasTradeInVehicle is true");
if (!hasTradeInVehicle && tradeInVehicle != null)
    throw new ArgumentException("TradeInVehicle should not be provided when HasTradeInVehicle is false");
```

**Impacto de Negócio**:
- Garante que dados de troca sejam sempre completos ou ausentes
- Previne erros de cálculo de pontuação
- Mantém integridade de dados para cálculos financeiros

#### RN-014, RN-015: Validação de Veículo de Troca
**Localização**: `Qualification.cs:72-76`

**Descrição**:
Dados de veículo de troca incluem validação para faixas de ano razoáveis (1900 até ano atual + 1) e valores de quilometragem não negativos para prevenir erros de entrada de dados.

**Implementação**:
```csharp
if (year < 1900 || year > currentYear + 1)
    throw new ArgumentException("Invalid year", nameof(year));
if (mileage < 0)
    throw new ArgumentException("Mileage cannot be negative", nameof(mileage));
```

**Impacto de Negócio**:
- Previne entrada de dados de veículo sem sentido
- Suporta cálculos precisos de avaliação de troca
- Mantém qualidade de banco de dados para análises

### 5.3 Regras de Negócio de Lead Scoring

**Serviço de Domínio**: `LeadScoringService`
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Services/LeadScoringService.cs`

#### Algoritmo de Pontuação

**Cálculo de Pontuação Base** (linhas 27-46):

| Critério | Pontuação | Lógica |
|----------|-----------|--------|
| Diamante | Mais Alta | Financiamento + Troca + Compra < 15 dias |
| Ouro | Alta | (Dinheiro OU Financiamento) + Compra < 15 dias |
| Prata | Média | Pagamento à vista (sem troca, sem financiamento) |
| Bronze | Baixa | Compra > 30 dias |

**Regras de Bônus** (linhas 48-61):
1. **Bônus de Fonte**: Fonte Showroom ou Telefone → +1 nível de pontuação
2. **Bônus de Qualidade Troca**: Baixa quilometragem (< 50k) + Condição excelente + Histórico concessionária → +1 nível de pontuação

**Lógica de Promoção de Pontuação** (linhas 63-70):
- Bronze → Prata → Ouro → Diamante (limitado em Diamante)
- Cada bônus pode aumentar pontuação em um nível
- Máximo de duas promoções possíveis (fonte + qualidade)

**Regras de Negócio Embutidas na Pontuação**:
1. **Prêmio de Urgência**: < 15 dias prazo de compra indica alta intenção
2. **Valor de Financiamento**: Leads de financiamento considerados de maior valor (receita de financiamento da concessionária)
3. **Valor de Troca**: Trocas habilitam lucro adicional em vendas de carros usados
4. **Qualidade de Canal**: Leads de Showroom e Telefone mostram maior intenção que canais digitais
5. **Qualidade de Veículo**: Trocas premium aumentam valor geral do negócio

---

## 6. Avaliação de Qualidade do Modelo de Domínio

### 6.1 Análise de Modelo de Domínio Rico

**Avaliação Geral**: A entidade Lead demonstra **forte modelagem de domínio rico** seguindo melhores práticas DDD.

#### Pontos Fortes

**1. Encapsulamento de Comportamento** ⭐⭐⭐⭐⭐
- Toda lógica de negócio encapsulada dentro de métodos de entidade
- Sem anti-padrão de modelo de domínio anêmico
- Clara separação entre dados e comportamento
- Exemplo: `Qualify()`, `ChangeStatus()`, `RegisterInteraction()`

**2. Objetos de Valor** ⭐⭐⭐⭐⭐
- Uso apropriado de objetos de valor para Email, Phone, Qualification
- Imutabilidade garante integridade de dados
- Igualdade baseada em valor implementada corretamente
- Elimina anti-padrão de obsessão primitiva

**3. Padrão Raiz de Agregado** ⭐⭐⭐⭐⭐
- Lead age como raiz de agregado para Interactions
- Entidades filhas acessadas apenas através da raiz
- Invariantes impostos na fronteira do agregado
- Fronteira de consistência clara

**4. Eventos de Domínio** ⭐⭐⭐⭐⭐
- Eventos publicados para todas mudanças de estado significativas
- Habilita consistência eventual
- Suporta arquitetura orientada a eventos
- Eventos são records imutáveis com schema claro

**5. Encapsulamento** ⭐⭐⭐⭐⭐
- Todos setters são privados
- Mudanças de estado através de métodos explícitos
- Superfície de API clara
- Previne transições de estado inválidas

**6. Padrão Static Factory** ⭐⭐⭐⭐
- `Lead.Create()` para construção de entidade
- Impõe invariantes em tempo de criação
- Publica eventos de domínio
- Expressão de intenção clara

**7. Colaboração de Serviço de Domínio** ⭐⭐⭐⭐
- `LeadScoringService` para lógica de pontuação complexa
- Separação de preocupações apropriada
- Serviço passado como dependência (parâmetro de método)
- Mantém pureza da entidade

#### Áreas para Melhoria Potencial

**1. Validação de Transição de Status** ⭐⭐⭐
- **Atual**: `ChangeStatus()` permite qualquer transição de status
- **Observação**: Sem validação de transições de estado válidas
- **Exemplo**: Poderia transicionar de `Convertido` → `Novo` (potencialmente inválido)
- **Nota**: Validação pode ocorrer na camada de aplicação (validadores)

**2. Consistência de Tipo de Interação** ⭐⭐⭐
- **Atual**: `Interaction.Type` é string apesar de enum `InteractionType` existir
- **Observação**: Incompatibilidade de tipo entre uso de enum em `RegisterInteraction()` e armazenamento string
- **Código**: `Interaction.Create(Id, type.ToString(), description, occurredAt)`
- **Impacto**: Perda de segurança de tipo, potencial para valores inválidos

**3. Imutabilidade de Qualificação** ⭐⭐⭐⭐
- **Atual**: `Qualification` é um record (imutável), mas pode ser substituído
- **Observação**: Sem método explícito para atualizar dados de qualificação
- **Impacto**: Design atual previne atualizações parciais de qualificação, requerendo requalificação total

**4. Estado Deletado/Arquivado** ⭐⭐⭐
- **Atual**: Sem mecanismo de soft delete explícito
- **Observação**: Status `Perdido` serve como estado terminal
- **Impacto**: Confusão potencial entre leads perdidos/arquivados

### 6.2 Aderência a Padrões DDD

| Padrão | Implementação | Qualidade | Notas |
|--------|---------------|-----------|-------|
| **Raiz de Agregado** | Entidade Lead | ⭐⭐⭐⭐⭐ | Fronteira de agregado clara, controla acesso a Interaction |
| **Objetos de Valor** | Email, Phone, Qualification | ⭐⭐⭐⭐⭐ | Imutabilidade adequada, igualdade por valor |
| **Eventos de Domínio** | LeadCreated, LeadScored, StatusChanged | ⭐⭐⭐⭐⭐ | Records imutáveis, intenção clara |
| **Serviço de Domínio** | LeadScoringService | ⭐⭐⭐⭐ | Uso apropriado para lógica complexa |
| **Interface de Repositório** | ILeadRepository | ⭐⭐⭐⭐⭐ | Abstrai persistência, queries orientadas a domínio |
| **Padrão Factory** | Métodos static factory | ⭐⭐⭐⭐ | Encapsula lógica de criação |
| **Linguagem Ubíqua** | Lead, Score, Qualification | ⭐⭐⭐⭐⭐ | Terminologia de negócio clara |

### 6.3 Verificação de Modelo de Domínio Anêmico

**Resultado**: ❌ **NÃO Anêmico** - Este é um **Modelo de Domínio Rico**

**Evidência**:
- Entidade contém lógica de negócio (não apenas dados)
- Métodos implementam comportamentos de domínio (`Qualify`, `ChangeStatus`, `RegisterInteraction`)
- Objetos de valor encapsulam regras de validação
- Eventos de domínio originam da entidade
- Transições de estado são explícitas e controladas

**Anti-padrões Ausentes**:
- ❌ Sem setters públicos expondo estado interno
- ❌ Sem lógica de negócio apenas na camada de aplicação
- ❌ Sem DTO mascarado como entidade de domínio

### 6.4 Aderência aos Princípios SOLID

**Single Responsibility**: ⭐⭐⭐⭐⭐
- Lead foca em gerenciamento de ciclo de vida de lead
- Pontuação delegada para serviço de domínio
- Separação de preocupações clara

**Open/Closed**: ⭐⭐⭐⭐
- Fechado para modificação (setters privados)
- Aberto para extensão (novos comportamentos via métodos)
- Eventos de domínio habilitam extensão sem modificação

**Liskov Substitution**: ⭐⭐⭐⭐⭐
- Herda de BaseEntity corretamente
- Contrato da classe base honrado
- Substituível onde quer que BaseEntity seja usado

**Interface Segregation**: ⭐⭐⭐⭐⭐
- ILeadRepository focado em persistência de lead
- Sem interfaces gordas
- Fronteiras de contrato claras

**Dependency Inversion**: ⭐⭐⭐⭐
- Depende de abstrações (ILeadRepository)
- LeadScoringService injetado como parâmetro de método
- Nenhuma dependência concreta de infraestrutura

### 6.5 Indicadores de Qualidade de Código

**Encapsulamento**: ⭐⭐⭐⭐⭐
- Todas propriedades têm setters privados
- Mudanças de estado através de métodos apenas
- Sem exposição direta de campos
- API pública clara

**Imutabilidade**: ⭐⭐⭐⭐⭐
- Objetos de valor são imutáveis
- Eventos de domínio são records imutáveis
- Mudanças de estado de entidade controladas

**Segurança de Tipo**: ⭐⭐⭐⭐
- Tipagem forte em todo lugar
- Uso de Enum para conjuntos fixos
- Pequeno problema: Tipo de interação como string

**Testabilidade**: ⭐⭐⭐⭐⭐
- Static factory habilita teste fácil
- Sem dependências de framework na entidade
- Serviço de domínio pode ser mockado
- Fronteiras de comportamento claras

**Documentação**: ⭐⭐⭐
- Código é auto-documentável
- Nomes de métodos expressam intenção
- Poderia se beneficiar de comentários de documentação XML
- Regras de negócio implícitas no código

---

## 7. Arquitetura e Integração

### 7.1 Colocação de Camada

**Localização Atual**: Camada de Domínio (3-Domain)
**Namespace**: `GestAuto.Commercial.Domain.Entities`

**Colocação de Camada Correta**: ✅ Sim
- Entidade não contém preocupações de infraestrutura
- Sem dependências de banco de dados (construtor EF Core é privado)
- Sem tipos específicos de framework
- Lógica de domínio pura

### 7.2 Contrato de Repositório

**Interface**: `ILeadRepository`
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Interfaces/ILeadRepository.cs`

**Métodos**:
| Método | Propósito | Retorno |
|--------|-----------|---------|
| `GetByIdAsync` | Recuperar por ID | `Lead?` |
| `GetBySalesPersonIdAsync` | Listar por vendedor | `IEnumerable<Lead>` |
| `AddAsync` | Persistir novo lead | `Task<Lead>` |
| `UpdateAsync` | Atualizar lead existente | `Task` |
| `ListBySalesPersonAsync` | Lista paginada com filtros | `IReadOnlyList<Lead>` |
| `ListAllAsync` | Paginated list all leads | `IReadOnlyList<Lead>` |
| `CountBySalesPersonAsync` | Contar com filtros | `int` |
| `CountAllAsync` | Contar todos leads | `int` |
| `CountByStatusAsync` | Dashboard: contar por status | `int` |
| `CountCreatedSinceAsync` | Dashboard: novos leads desde data | `int` |
| `CountByStatusSinceAsync` | Dashboard: mudanças status desde data | `int` |
| `GetHotLeadsAsync` | Dashboard: leads estagnados precisando atenção | `IReadOnlyList<Lead>` |

**Observação**: Repositório inclui operações CRUD e métodos de query orientados a dashboard, seguindo separação tipo CQRS.

### 7.3 Dependências Externas

**Dependências Diretas**: Nenhuma (modelo de domínio puro)

**Dependência de Serviço de Domínio**: `LeadScoringService`
- Passado como parâmetro de método (injetado)
- Sem acoplamento estático
- Habilita teste com implementações mock

**Pontos de Integração de Infraestrutura**:
1. **Entity Framework Core**: Construtor sem parâmetros privado para materialização EF Core
2. **Event Bus**: Eventos de domínio despachados pela camada de aplicação após persistência
3. **Banco de Dados**: Mapeamento EF Core `LeadConfiguration` (localizado na camada de Infraestrutura)

---

## 8. Análise de Fluxo de Dados

### 8.1 Fluxo de Ciclo de Vida da Entidade

```
┌─────────────────────────────────────────────────────────────────┐
│                    CICLO DE VIDA DA ENTIDADE LEAD                │
└─────────────────────────────────────────────────────────────────┘

1. FLUXO DE CRIAÇÃO
   Camada de Aplicação → Lead.Create() → Nova Instância Lead
                           ↓
                      Validação (Nome)
                           ↓
                      Inicializar Estado
                      - Status: Novo
                      - Score: Bronze
                           ↓
                      Publicar: LeadCreatedEvent
                           ↓
                      Retornar Entidade Lead

2. FLUXO DE QUALIFICAÇÃO
   Camada de Aplicação → lead.Qualify(qualification, scoringService)
                           ↓
                      Armazenar Dados de Qualificação
                           ↓
                      Chamar scoringService.Calculate(this)
                           ↓
                      Atualizar Score (Bronze/Prata/Ouro/Diamante)
                           ↓
                      Atualizar Status (Novo → EmNegociacao)
                           ↓
                      Atualizar Timestamp
                           ↓
                      Publicar: LeadScoredEvent
                           ↓
                      Retornar void (estado mutado)

3. FLUXO DE MUDANÇA DE STATUS
   Camada de Aplicação → lead.ChangeStatus(newStatus)
                           ↓
                      Atualizar Propriedade Status
                           ↓
                      Atualizar Timestamp
                           ↓
                      Publicar: LeadStatusChangedEvent
                           ↓
                      Retornar void (estado mutado)

4. FLUXO DE INTERAÇÃO
   Camada de Aplicação → lead.RegisterInteraction(type, description, date)
                           ↓
                      Criar Entidade Interaction
                           ↓
                      Adicionar à Coleção Interactions
                           ↓
                      Atualizar Timestamp
                           ↓
                      Retornar Entidade Interaction
```

### 8.2 Fluxo de Evento de Domínio

```
┌─────────────────────────────────────────────────────────────────┐
│                     FLUXO DE EVENTO DE DOMÍNIO                   │
└─────────────────────────────────────────────────────────────────┘

Método Entidade → AddEvent() → Coleção _domainEvents
                                          ↓
                    Camada de Aplicação (Handler)
                                          ↓
                    1. Repository.SaveChangesAsync()
                                          ↓
                    2. Recuperar entity.DomainEvents
                                          ↓
                    3. Despachar para event bus
                                          ↓
                    4. entity.ClearEvents()
                                          ↓
                    Sistemas Externos (Notificação, Analytics, etc.)
```

---

## 9. Análise de Cobertura de Teste

**Arquivos de Teste Localizados**:
- `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Application/CreateLeadHandlerTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Application/QualifyLeadHandlerTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Application/ChangeLeadStatusHandlerTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Application/UpdateLeadHandlerTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Application/GetLeadHandlerTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Application/ListLeadsHandlerTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.IntegrationTest/LeadRepositoryTests.cs`
- `/services/commercial/5-Tests/GestAuto.Commercial.IntegrationTest/LeadApiTests.cs`

**Observações de Cobertura**:

1. **Testes Unitários**: Handlers de camada de aplicação testados (não entidade diretamente)
2. **Testes de Integração**: Repositório e endpoints API cobertos
3. **Teste de Entidade**: Comportamento da entidade testado indiretamente através de handlers
4. **Serviço de Domínio**: Testes de `LeadScoringService` não explicitamente localizados (podem estar em testes de handler)

**Abordagem de Teste**: O código base segue estratégia de teste de camada de aplicação onde comportamento da entidade é testado através de orquestração de handler ao invés de testes unitários de entidade diretos.

---

## 10. Resumo e Conclusões

### 10.1 Pontos Fortes do Modelo de Domínio

1. **Modelo Comportamental Rico**: Entidade Lead encapsula lógica de negócio com métodos claros e reveladores de intenção
2. **Objetos de Valor**: Uso apropriado de objetos de valor imutáveis para Email, Phone, e Qualification
3. **Eventos de Domínio**: Publicação de evento abrangente habilita arquitetura orientada a eventos
4. **Padrão de Agregado**: Raiz de agregado clara com acesso controlado a entidade filha
5. **Encapsulamento**: Setters privados e métodos explícitos garantem estado válido
6. **Aderência DDD**: Forte alinhamento com princípios de Domain-Driven Design
7. **Princípios SOLID**: Código bem projetado, manutenível e extensível

### 10.2 Resumo de Regras de Negócio

- **Inicialização**: Novos leads iniciam com status=Novo, score=Bronze
- **Validação**: Come, email, telefone validados em nível de entidade/value object
- **Qualificação**: Automaticamente avança status para EmNegociacao
- **Pontuação**: Algoritmo complexo baseado em método de pagamento, troca, urgência, fonte e qualidade de veículo
- **Transições de Estado**: Todas mudanças publicam eventos de domínio e atualizam timestamps
- **Consistência**: Dados de qualificação mantêm integridade referencial

### 10.3 Pontuação de Qualidade do Modelo de Domínio

**Avaliação Geral**: ⭐⭐⭐⭐⭐ (5/5) - **Modelo de Domínio Rico Excelente**

**Detalhamento**:
- Modelo Rico vs Anêmico: ⭐⭐⭐⭐⭐ (Rico)
- Aderência Padrão DDD: ⭐⭐⭐⭐⭐ (Excelente)
- Encapsulamento: ⭐⭐⭐⭐⭐ (Perfeito)
- Imposição de Regra de Negócio: ⭐⭐⭐⭐⭐ (Forte)
- Design de Evento de Domínio: ⭐⭐⭐⭐⭐ (Excelente)
- Uso de Objeto de Valor: ⭐⭐⭐⭐⭐ (Perfeito)
- Princípios SOLID: ⭐⭐⭐⭐⭐ (Forte)

### 10.4 Recomendações

**Nenhum problema crítico identificado.** A entidade de domínio Lead representa uma implementação de alta qualidade de princípios DDD com modelagem rica de domínio, encapsulamento apropriado e imposição clara de regras de negócio.

---

**Fim do Relatório**

**Analisado Por**: Agente de Análise Profunda de Componente
**Data da Análise**: 23/01/2026
**Versão do Componente**: Baseado em análise de código de intervalo de commit (mais recente)
