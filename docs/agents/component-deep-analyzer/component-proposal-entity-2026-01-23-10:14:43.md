# Relatório de Análise Profunda do Componente: Entidade de Domínio Proposal

**Nome do Componente**: Entidade de Domínio Proposal
**Tipo do Componente**: Entidade de Domínio (Camada de Domínio)
**Localização**: `/services/commercial/3-Domain/GestAuto.Commercial.Domain/Entities/Proposal.cs`
**Data da Análise**: 23/01/2026
**Tipo de Análise**: Análise de Modelo de Domínio
**Linhas do Componente**: 205 linhas

---

## 1. Visão Geral do Componente

### Propósito
A entidade `Proposal` representa uma proposta comercial de venda no domínio de concessionária de veículos. Ela encapsula toda a lógica de negócio para criar, gerenciar e fechar propostas de venda de veículos, incluindo informações do veículo, precificação, descontos, métodos de pagamento, veículos de troca (trade-in) e itens adicionais.

### Localização na Camada de Domínio
A entidade `Proposal` é um agregado raiz na Camada de Domínio, seguindo os princípios de Domain-Driven Design (DDD). Ela reside no namespace `GestAuto.Commercial.Domain.Entities` e serve como um modelo de domínio central para o contexto delimitado comercial.

### Responsabilidades Principais
- Gerenciar o ciclo de vida da proposta desde a criação até o fechamento
- Lidar com o fluxo de trabalho de aplicação e aprovação de descontos
- Calcular o valor total da proposta, incluindo veículo, itens, descontos e trade-in
- Manter a máquina de estados do status da proposta
- Publicar eventos de domínio para mudanças no ciclo de vida da proposta
- Gerenciar itens adicionais (acessórios, serviços) adicionados às propostas

### Relacionamentos de Domínio
- **Lead**: Relacionamento muitos-para-um (Propostas pertencem a Leads)
- **ProposalItem**: Agregação um-para-muitos (Proposta contém Itens)
- **UsedVehicleEvaluation**: Associação opcional (para cenários de troca)

---

## 2. Detalhes do Modelo de Domínio

### Propriedades da Entidade

A entidade `Proposal` contém os seguintes grupos de propriedades:

#### Identidade Central
- `Id` (Guid): Identificador único, herdado de BaseEntity
- `LeadId` (Guid): Referência de chave estrangeira para o Lead associado
- `Status` (ProposalStatus): Estado atual no ciclo de vida da proposta
- `CreatedAt` (DateTime): Timestamp de criação
- `UpdatedAt` (DateTime): Timestamp da última modificação

#### Informações do Veículo
- `VehicleModel` (string): Nome do modelo do veículo (ex: "Corolla")
- `VehicleTrim` (string): Versão/acabamento do veículo (ex: "XEi")
- `VehicleColor` (string): Cor do veículo
- `VehicleYear` (int): Ano de fabricação
- `IsReadyDelivery` (bool): Indica disponibilidade para pronta entrega

#### Informações Financeiras (Value Objects)
- `VehiclePrice` (Money): Preço base do veículo (Value Object Money)
- `DiscountAmount` (Money): Valor do desconto aplicado
- `DiscountReason` (string?): Justificativa para o desconto
- `DiscountApproverId` (Guid?): Gerente que aprovou o desconto (>5%)
- `TradeInValue` (Money): Valor atribuído ao veículo de troca do cliente

#### Configuração de Pagamento
- `PaymentMethod` (PaymentMethod): Enum (Dinheiro, Financiamento, Consórcio, Leasing)
- `DownPayment` (Money?): Valor de entrada (opcional)
- `Installments` (int?): Número de parcelas (opcional)

#### Entidades Agregadas
- `Items` (List<ProposalItem>): Coleção de itens/serviços adicionais

#### Referências Externas
- `UsedVehicleEvaluationId` (Guid?): Referência à avaliação de troca

#### Propriedade Derivada
- `TotalValue` (Money, calculado): Valor final da proposta = (PreçoVeículo - Desconto + TotalItens - ValorTroca)

### Value Objects Utilizados

#### Value Object Money
Localização: `ValueObjects/Money.cs`

O Value Object Money fornece:
- Valores monetários imutáveis com suporte a moeda
- Operadores aritméticos (+, -, *, >, <)
- Validação para valores não negativos
- Validação de consistência de moeda
- Arredondamento de precisão para 2 casas decimais

**Decisão de Design Chave**: Usar o Value Object Money em vez de decimal previne a obsessão por primitivos e garante consistência de moeda em todos os cálculos financeiros.

#### Entidade ProposalItem
Localização: `Entities/ProposalItem.cs`

Embora tecnicamente uma entidade, `ProposalItem` se comporta como um Value Object dentro do agregado Proposal:
- Propriedades: Id, Description, Price (Money), IsOptional
- Método factory estático: `Create(description, price, isOptional)`
- Validação: Descrição não pode ser vazia
- Sem métodos de ciclo de vida (gerenciado pela raiz do agregado Proposal)

### Enums

#### ProposalStatus
Localização: `Enums/ProposalStatus.cs`

```csharp
public enum ProposalStatus
{
    Draft = 1,
    InNegotiation = 2,
    AwaitingUsedVehicleEvaluation = 3,
    AwaitingDiscountApproval = 4,
    AwaitingCustomer = 5,
    Approved = 6,
    Closed = 7,
    Lost = 8
}
```

O enum representa a máquina de estados da proposta com 8 estados distintos, permitindo rastreamento claro do ciclo de vida e aplicação de regras de negócio baseadas no status atual.

#### PaymentMethod
Localização: `Enums/PaymentMethod.cs`

```csharp
public enum PaymentMethod
{
    Cash = 1,
    Financing = 2,
    Consortium = 3,
    Leasing = 4
}
```

Define os quatro métodos de pagamento disponíveis no domínio da concessionária.

---

## 3. Comportamentos de Domínio

### Método Factory Estático

#### `Create(...)`
Localização: `Proposal.cs:43-81`

**Propósito**: Cria uma nova instância de Proposal com estado inicial validado

**Parâmetros**:
- `leadId` (Guid): Identificador do lead associado
- `vehicleModel`, `vehicleTrim`, `vehicleColor` (string): Detalhes do veículo
- `vehicleYear` (int): Ano de fabricação
- `isReadyDelivery` (bool): Flag de disponibilidade de entrega
- `vehiclePrice` (Money): Preço base do veículo
- `tradeInValue` (Money): Valor inicial de troca (tipicamente zero)
- `paymentMethod` (PaymentMethod): Enum de método de pagamento
- `downPayment` (Money?, opcional): Valor de entrada
- `installments` (int?, opcional): Número de parcelas

**Validações**:
- Modelo do veículo não pode ser vazio
- Preço do veículo deve ser positivo (> 0)

**Estado Inicial**:
- Status: `ProposalStatus.AwaitingCustomer`
- DiscountAmount: `Money.Zero`
- Evento de Domínio: `ProposalCreatedEvent` publicado

**Padrão de Design**: Método factory estático impõe invariantes e garante a criação válida da entidade, prevenindo estados inválidos do objeto.

### Métodos de Transição de Estado

#### `SetAwaitingEvaluation(Guid evaluationId)`
Localização: `Proposal.cs:83-89`

**Propósito**: Transiciona a proposta para aguardar avaliação de seminovo

**Comportamento**:
- Define Status para `AwaitingUsedVehicleEvaluation`
- Armazena referência da avaliação
- Publica `ProposalUpdatedEvent` com descrição "Avaliação de seminovo solicitada"
- Atualiza timestamp `UpdatedAt`

**Pré-condições**: Nenhuma verificada explicitamente

#### `ApplyEvaluationResult(Money evaluatedValue)`
Localização: `Proposal.cs:91-97`

**Propósito**: Aplica resultado da avaliação e retorna proposta para o cliente

**Comportamento**:
- Atualiza `TradeInValue` com valor avaliado
- Define Status de volta para `AwaitingCustomer`
- Publica `ProposalUpdatedEvent` com descrição "Avaliação de seminovo concluída"
- Atualiza timestamp `UpdatedAt`

**Pré-condições**: Nenhuma verificada explicitamente

#### `SetTradeInValue(Money tradeInValue)`
Localização: `Proposal.cs:99-104`

**Propósito**: Atualiza diretamente o valor de troca sem mudança de estado

**Comportamento**:
- Atualiza `TradeInValue`
- Publica `ProposalUpdatedEvent` com descrição "Valor de seminovo confirmado"
- Atualiza timestamp `UpdatedAt`

**Caso de Uso**: Confirmação do valor de troca após avaliação

### Métodos de Gestão de Desconto

#### `ApplyDiscount(Money amount, string reason, Guid salesPersonId)`
Localização: `Proposal.cs:106-121`

**Propósito**: Aplica desconto à proposta com fluxo de aprovação condicional

**Lógica de Negócio**:
1. Calcula porcentagem de desconto: `(amount / VehiclePrice) * 100`
2. Aplica desconto e motivo
3. Se desconto > 5%: Altera status para `AwaitingDiscountApproval`
4. Se desconto <= 5%: Status permanece `AwaitingCustomer`

**Limite de Aprovação**: 5% do preço do veículo aciona requisito de aprovação do gerente

**Efeitos Colaterais**:
- Atualiza `DiscountAmount`
- Atualiza `DiscountReason`
- Atualiza `DiscountApproverId` para null (pendente de aprovação)
- Altera status se desconto > 5%
- Publica `ProposalUpdatedEvent` com descrição "Desconto aplicado"
- Atualiza timestamp `UpdatedAt`

**Observação de Design**: O parâmetro `salesPersonId` é aceito mas não armazenado ou usado, sugerindo implementação incompleta ou recurso futuro de trilha de auditoria.

#### `ApproveDiscount(Guid managerId)`
Localização: `Proposal.cs:123-133`

**Propósito**: Aprova desconto que excedeu limite de 5%

**Validações**:
- Status atual deve ser `AwaitingDiscountApproval`
- Lança `DomainException` se proposta não estiver aguardando aprovação

**Comportamento**:
- Armazena `managerId` em `DiscountApproverId`
- Altera status para `AwaitingCustomer`
- Publica `ProposalUpdatedEvent` com descrição "Desconto aprovado"
- Atualiza timestamp `UpdatedAt`

**Invariante**: Apenas propostas no status `AwaitingDiscountApproval` podem ser aprovadas

### Método de Fechamento de Proposta

#### `Close(Guid salesPersonId)`
Localização: `Proposal.cs:135-143`

**Propósito**: Finaliza a proposta como uma venda bem-sucedida

**Validações** (via `ValidateCanClose()`):
- Não pode fechar com aprovação de desconto pendente (status `AwaitingDiscountApproval`)
- Desconto não pode exceder 10% do preço do veículo

**Comportamento**:
- Altera status para `Closed`
- Publica `SaleClosedEvent` com ProposalId, LeadId e TotalValue
- Atualiza timestamp `UpdatedAt`

**Regra de Negócio**: Desconto máximo de 10% aplicável no momento do fechamento

**Observação de Design**: O parâmetro `salesPersonId` é aceito mas não armazenado, sugerindo implementação incompleta de trilha de auditoria.

### Métodos de Atualização de Informação

#### `UpdateVehicleInfo(...)`
Localização: `Proposal.cs:145-153`

**Propósito**: Atualiza detalhes do veículo

**Parâmetros**: vehicleModel, vehicleTrim, vehicleColor, vehicleYear, isReadyDelivery

**Comportamento**:
- Atualiza todas as propriedades relacionadas ao veículo
- Atualiza timestamp `UpdatedAt`

**Faltando**: Nenhum evento de domínio publicado para atualizações de informação do veículo (inconsistente com outros métodos de atualização)

#### `UpdatePaymentInfo(...)`
Localização: `Proposal.cs:155-162`

**Propósito**: Atualiza configuração de pagamento

**Parâmetros**: vehiclePrice, paymentMethod, downPayment, installments

**Comportamento**:
- Atualiza propriedades relacionadas ao pagamento
- Atualiza timestamp `UpdatedAt`

**Faltando**: Nenhum evento de domínio publicado para atualizações de informação de pagamento (inconsistente com outros métodos de atualização)

### Métodos de Gestão de Itens

#### `AddItem(ProposalItem item)`
Localização: `Proposal.cs:164-169`

**Propósito**: Adiciona item/serviço adicional à proposta

**Comportamento**:
- Adiciona item à coleção Items
- Publica `ProposalUpdatedEvent` com descrição do item
- Atualiza timestamp `UpdatedAt`

**Padrão de Design**: Proposal controla ciclo de vida do item, itens não podem existir independentemente

#### `RemoveItem(Guid itemId)`
Localização: `Proposal.cs:171-180`

**Propósito**: Remove item da proposta

**Comportamento**:
- Encontra item por Id
- Remove se encontrado
- Publica `ProposalUpdatedEvent` com descrição do item
- Atualiza timestamp `UpdatedAt`

**Segurança**: Nenhuma exceção lançada se item não for encontrado (falha silenciosa)

### Método de Referência de Avaliação

#### `SetUsedVehicleEvaluationId(Guid evaluationId)`
Localização: `Proposal.cs:182-186`

**Propósito**: Define referência para avaliação de veículo usado

**Comportamento**:
- Atualiza `UsedVehicleEvaluationId`
- Atualiza timestamp `UpdatedAt`

**Faltando**: Nenhum evento de domínio publicado (padrão inconsistente)

### Método de Cálculo Privado

#### `CalculateTotalValue()`
Localização: `Proposal.cs:197-204`

**Propósito**: Computa valor final da proposta

**Fórmula**:
```
TotalValue = (VehiclePrice - DiscountAmount + ItemsTotal - TradeInValue)
Onde:
- ItemsTotal = Soma de todos os preços dos itens
- Resultado = max(TotalValue, Money.Zero) // Previne valores negativos
```

**Chamado**: Toda vez que a propriedade `TotalValue` é acessada (cálculo lazy)

**Padrão de Design**: Propriedade computada garante cálculo sempre atual sem dados obsoletos

---

## 4. Eventos de Domínio

### Infraestrutura de Eventos

#### Interface IDomainEvent
Localização: `Events/IDomainEvent.cs`

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

Todos os eventos de domínio implementam esta interface, fornecendo identificação e timestamp consistente para eventos.

#### Gestão de Eventos BaseEntity
Localização: `Entities/BaseEntity.cs`

A BaseEntity fornece infraestrutura de coleção de eventos:
- `DomainEvents` (IReadOnlyCollection<IDomainEvent>): Acesso somente leitura aos eventos
- `AddEvent(IDomainEvent)`: Método protegido para adicionar eventos
- `ClearEvents()`: Método público para remover eventos despachados

### Eventos Específicos de Proposal

#### ProposalCreatedEvent
Localização: `Events/ProposalCreatedEvent.cs`

```csharp
public record ProposalCreatedEvent(Guid ProposalId, Guid LeadId) : IDomainEvent
```

**Publicado Por**: Método factory `Proposal.Create()` (linha 79)

**Propósito**: Sinaliza criação de nova proposta no domínio

**Payload**:
- `ProposalId`: Identificador único da proposta criada
- `LeadId`: Identificador do lead associado

**EventId**: Identificador único gerado automaticamente
**OccurredAt**: Timestamp UTC gerado automaticamente

**Casos de Uso**:
- Disparar notificação para equipe de vendas
- Atualizar status do lead
- Inicializar rastreamento da proposta
- Log de auditoria

#### ProposalUpdatedEvent
Localização: `Events/ProposalUpdatedEvent.cs`

```csharp
public record ProposalUpdatedEvent(Guid ProposalId, string Description) : IDomainEvent
```

**Publicado Por**:
- `SetAwaitingEvaluation()` - "Avaliação de seminovo solicitada"
- `ApplyEvaluationResult()` - "Avaliação de seminovo concluída"
- `SetTradeInValue()` - "Valor de seminovo confirmado"
- `ApplyDiscount()` - "Desconto aplicado"
- `ApproveDiscount()` - "Desconto aprovado"
- `AddItem()` - "Item adicionado: {item.Description}"
- `RemoveItem()` - "Item removido: {item.Description}"

**Propósito**: Sinaliza modificações na proposta com descrição legível por humanos

**Payload**:
- `ProposalId`: Identificador único da proposta atualizada
- `Description`: Descrição legível da mudança em Português

**Não Publicado Para** (Inconsistências):
- `UpdateVehicleInfo()` - Evento faltando
- `UpdatePaymentInfo()` - Evento faltando
- `SetUsedVehicleEvaluationId()` - Evento faltando

**Casos de Uso**:
- Atualizar histórico/cronograma da proposta
- Disparar notificações para mudanças significativas
- Manutenção de trilha de auditoria
- Atualizações de UI em tempo real via websockets

#### SaleClosedEvent
Localização: `Events/SaleClosedEvent.cs`

```csharp
public record SaleClosedEvent(Guid ProposalId, Guid LeadId, Money TotalValue) : IDomainEvent
```

**Publicado Por**: Método `Proposal.Close()` (linha 142)

**Propósito**: Sinaliza conclusão de venda bem-sucedida

**Payload**:
- `ProposalId`: Identificador da proposta fechada
- `LeadId`: Identificador do lead associado
- `TotalValue`: Valor final da venda (propriedade calculada)

**EventId**: Identificador único gerado automaticamente
**OccurredAt**: Timestamp UTC gerado automaticamente

**Casos de Uso**:
- Disparar processo de criação de pedido
- Atualizar status do lead para "Convertido"
- Iniciar fluxo de entrega
- Cálculo de comissão de vendas
- Relatório de receitas
- Atualizações de inventário

### Padrão de Publicação de Eventos

**Padrão Consistente**:
1. Lógica de domínio executa
2. Mudanças de estado ocorrem
3. `AddEvent(new DomainEvent(...))` chamado
4. Timestamp `UpdatedAt` atualizado
5. Evento permanece na entidade até ser despachado pela camada de aplicação

**Ciclo de Vida do Evento**:
1. Entidade adiciona evento à coleção `_domainEvents`
2. Camada de aplicação salva entidade via UnitOfWork
3. UnitOfWork recupera eventos via propriedade `DomainEvents`
4. Despachante de eventos processa eventos
5. Eventos limpos via `ClearEvents()`

---

## 5. Regras de Negócio

### Invariantes

#### Regra 1: Modelo do Veículo Não Pode Ser Vazio
Localização: `Proposal.cs:56-57`

```csharp
if (string.IsNullOrWhiteSpace(vehicleModel))
    throw new ArgumentException("Vehicle model cannot be empty", nameof(vehicleModel));
```

**Aplicação**: Método factory Create

**Justificativa**: Modelo do veículo é obrigatório para identificação da proposta

**Consequência de Violação**: ArgumentException previne criação da entidade

#### Regra 2: Preço do Veículo Deve Ser Positivo
Localização: `Proposal.cs:59-60`

```csharp
if (vehiclePrice.Amount <= 0)
    throw new ArgumentException("Vehicle price must be positive", nameof(vehiclePrice));
```

**Aplicação**: Método factory Create

**Justificativa**: Previne propostas de veículos gratuitas ou com preço negativo

**Consequência de Violação**: ArgumentException previne criação da entidade

#### Regra 3: Aprovação de Desconto Obrigatória Acima de 5%
Localização: `Proposal.cs:106-121`

```csharp
var discountPercentage = amount.Amount / VehiclePrice.Amount * 100;
if (discountPercentage > 5)
{
    Status = ProposalStatus.AwaitingDiscountApproval;
}
```

**Aplicação**: Método ApplyDiscount

**Cálculo**: (DiscountAmount / VehiclePrice) * 100

**Limite**: 5% do preço do veículo aciona requisito de aprovação

**Justificativa**: Supervisão de gerente para descontos significativos

**Estados**:
- <= 5%: Aprovação automática, status permanece `AwaitingCustomer`
- > 5%: Requer aprovação de gerente, status muda para `AwaitingDiscountApproval`

#### Regra 4: Não Pode Fechar com Aprovação de Desconto Pendente
Localização: `Proposal.cs:188-191`

```csharp
if (Status == ProposalStatus.AwaitingDiscountApproval)
    throw new DomainException("Não é possível fechar proposta com desconto pendente de aprovação");
```

**Aplicação**: Método Close via ValidateCanClose()

**Justificativa**: Previne finalizar vendas com descontos não aprovados

**Consequência de Violação**: DomainException previne fechamento da proposta

#### Regra 5: Desconto Máximo de 10% no Fechamento
Localização: `Proposal.cs:193-194`

```csharp
if (DiscountAmount.Amount > VehiclePrice.Amount * 0.1M)
    throw new DomainException("Desconto excede o limite permitido");
```

**Aplicação**: Método Close via ValidateCanClose()

**Cálculo**: Desconto máximo = VehiclePrice * 0.10 (10%)

**Justificativa**: Regra de negócio previne descontos excessivos

**Consequência de Violação**: DomainException previne fechamento da proposta

**Nota de Conflito**: Regra permite aplicação de desconto >5% (requerendo aprovação), mas bloqueia fechamento se >10%, criando uma "zona cinza" entre 5-10% que requer aprovação mas pode ser fechada.

#### Regra 6: Money Não Pode Ser Negativo
Localização: `Money.cs:16-17`

```csharp
if (amount < 0)
    throw new DomainException("Valor não pode ser negativo");
```

**Aplicação**: Construtor do value object Money

**Justificativa**: Integridade financeira previne valores monetários negativos

**Consequência de Violação**: DomainException previne criação do objeto Money

#### Regra 7: Operações Money Requerem Mesma Moeda
Localização: `Money.cs:52-56`

```csharp
private static void ValidateSameCurrency(Money a, Money b)
{
    if (a.Currency != b.Currency)
        throw new DomainException("Não é possível operar valores em moedas diferentes");
}
```

**Aplicação**: Todos os operadores aritméticos de Money (+, -, >, <)

**Justificativa**: Previne mistura inválida de moedas em cálculos

**Consequência de Violação**: DomainException previne operação

**Implementação Atual**: Todas as instâncias de Money padrão para "BRL" (Real Brasileiro)

### Regras da Máquina de Estados

#### Transições de Status Válidas

Baseado no enum ProposalStatus, transições de estado implícitas:

1. **AwaitingCustomer** (estado inicial)
   - → AwaitingUsedVehicleEvaluation (SetAwaitingEvaluation)
   - → AwaitingDiscountApproval (ApplyDiscount > 5%)
   - → Closed (Close)

2. **AwaitingUsedVehicleEvaluation**
   - → AwaitingCustomer (ApplyEvaluationResult)

3. **AwaitingDiscountApproval**
   - → AwaitingCustomer (ApproveDiscount)

4. **Closed**
   - Estado terminal (sem transições de saída)

**Estados Não Implementados**:
- Draft: Nenhum caminho de criação leva a este estado
- InNegotiation: Não usado na implementação atual
- Approved: Não usado na implementação atual
- Lost: Não usado na implementação atual

### Regras de Cálculo

#### Cálculo de Valor Total
Localização: `Proposal.cs:197-204`

```csharp
var itemsTotal = Items.Sum(item => item.Price.Amount);
var netVehiclePrice = VehiclePrice - DiscountAmount;
var total = netVehiclePrice + new Money(itemsTotal) - TradeInValue;
return total.Amount > 0 ? total : Money.Zero;
```

**Fórmula**: Total = (PreçoVeículo - Desconto + TotalItens - ValorTroca)

**Caso de Borda**: Valores totais negativos são limitados a Money.Zero

**Tipo de Cálculo**: Avaliação lazy (computado no acesso à propriedade)

**Precisão**: Valores monetários arredondados para 2 casas decimais

---

## 6. Avaliação de Qualidade do Modelo de Domínio

### Análise de Modelo de Domínio Rico

#### Pontos Fortes

**1. Lógica de Negócio Encapsulada**
A entidade Proposal demonstra forte encapsulamento de regras de negócio:
- Fluxo de aprovação de desconto (limite de 5%)
- Validação de fechamento (desconto máx. 10%)
- Transições de estado controladas via métodos
- Propriedades calculadas (TotalValue)

**2. Padrão Aggregate Root**
Proposal atua adequadamente como raiz de agregado:
- Controla ciclo de vida de ProposalItem
- Gerencia eventos de domínio
- Impõe invariantes
- Sem referências externas diretas para ProposalItem

**3. Eventos de Domínio**
Publicação rica de eventos permite:
- Integração desacoplada com outros contextos delimitados
- Geração de trilha de auditoria
- Disparo de notificações
- Padrões de consistência eventual

**4. Value Objects**
Uso efetivo do value object Money:
- Previne obsessão por primitivos
- Garante consistência de moeda
- Encapsula cálculos financeiros
- Fornece sobrecarga de operadores para sintaxe natural

**5. Método Factory Estático**
`Proposal.Create()` fornece:
- Ponto único de construção
- Imposição de invariantes
- Garantia de publicação de evento
- Expressão de intenção clara

**6. Métodos Ricos em Comportamento**
Comportamentos de domínio expressos como métodos:
- `ApplyDiscount()` - Encapsula lógica de desconto
- `ApproveDiscount()` - Encapsula fluxo de aprovação
- `Close()` - Encapsula validação de fechamento
- `CalculateTotalValue()` - Encapsula lógica de cálculo

#### Pontos Fracos

**1. Sintomas de Anti-Pattern Anêmico**

Embora a entidade tenha comportamentos, ela exibe algumas tendências anêmicas:

**Regras de Negócio Faltando** (Deveriam estar no domínio, atualmente ausentes):
- Nenhuma validação de compatibilidade de método de pagamento com entrada
  - Ex: Pagamento em dinheiro não deveria ter parcelas
  - Ex: Financiamento deveria exigir parcelas
- Nenhuma validação de limites de parcelas baseados no valor
- Nenhuma validação de porcentagens mínimas de entrada
- Nenhuma regra de negócio para número máximo de itens
- Nenhuma validação de razoabilidade de preço de item

**Lógica Externalizada** (Encontrada em handlers, deveria estar na entidade):
- Atualizações de status de Lead acontecem em handlers de aplicação
  - `CreateProposalHandler` atualiza status de Lead para `ProposalSent`
  - `CloseProposalHandler` atualiza status de Lead para `Converted`
  - Isso cria acoplamento temporal e vazamento de conhecimento

**2. Encapsulamento Incompleto**

**Exposição de Setter Público** (de propriedades):
```csharp
public Money VehiclePrice { get; private set; }
public PaymentMethod PaymentMethod { get; private set; }
```

Embora `private set` previna modificação externa, os métodos de atualização (`UpdateVehicleInfo`, `UpdatePaymentInfo`) fornecem atualizações em massa sem controle granular ou validação.

**3. Inconsistências de Parâmetros**

**Parâmetros Não Usados**:
- `ApplyDiscount(Money amount, string reason, Guid salesPersonId)` - salesPersonId aceito mas não armazenado
- `Close(Guid salesPersonId)` - salesPersonId aceito mas não armazenado

Isso sugere implementação incompleta de trilha de auditoria. Ou:
- Armazenar o salesPersonId para propósitos de auditoria
- Remover o parâmetro se não necessário

**4. Inconsistências de Publicação de Eventos**

**Eventos Faltando**:
- `UpdateVehicleInfo()` - Nenhum evento publicado
- `UpdatePaymentInfo()` - Nenhum evento publicado
- `SetUsedVehicleEvaluationId()` - Nenhum evento publicado

**Eventos Publicados**:
- `AddItem()` - Evento publicado
- `RemoveItem()` - Evento publicado
- `ApplyDiscount()` - Evento publicado
- `ApproveDiscount()` - Evento publicado

Essa inconsistência cria uma trilha de auditoria incompleta e quebra o princípio de publicação uniforme de eventos para mudanças de estado.

**5. Inflação de Enum de Status**

O enum `ProposalStatus` tem 8 valores, mas apenas 4 são ativamente usados:
- **Usados**: AwaitingCustomer, AwaitingUsedVehicleEvaluation, AwaitingDiscountApproval, Closed
- **Não Usados**: Draft, InNegotiation, Approved, Lost

Isso sugere ou:
- Implementação incompleta (comum em sistemas em evolução)
- Valores de enum desnecessários poluindo o modelo

**6. Localização de Validação**

**Validações de Domínio** (Apropriadas):
- Modelo do veículo não vazio
- Preço do veículo positivo
- Limite de aprovação de desconto
- Limite de desconto no fechamento

**Validações Faltando** (Deveriam existir):
- Razoabilidade do ano do veículo (datas futuras, muito antigo)
- Cor do veículo obrigatória (atualmente permite null/vazio via métodos update)
- Versão do veículo obrigatória (atualmente permite null/vazio via métodos update)
- Compatibilidade de entrada com método de pagamento
- Validação de parcelas positivas

**7. Colocação do Método de Cálculo**

`CalculateTotalValue()` é privado e chamado toda vez que `TotalValue` é acessado:

```csharp
public Money TotalValue => CalculateTotalValue();
```

**Prós**:
- Cálculo sempre atual
- Sem risco de dados obsoletos

**Contras**:
- Impacto de performance em acesso frequente
- Sem mecanismo de cache

**Alternativa**: Considerar calcular na mudança de estado e armazenar como campo.

### Aderência ao Padrão DDD

#### Alinhamento com Princípios DDD

**✅ Padrão Aggregate Root**
- Proposal controla ciclo de vida de ProposalItem
- Acesso externo a itens vai através de Proposal
- ProposalItem não tem repositório independente

**✅ Linguagem Ubíqua**
- Nomes de métodos refletem terminologia de negócio: ApplyDiscount, ApproveDiscount, Close
- Nomes de propriedades correspondem a conceitos de domínio: VehiclePrice, TradeInValue, DownPayment
- Enum de status captura estados de negócio

**✅ Eventos de Domínio**
- Eventos representam ocorrências de negócio significativas
- Nomes de eventos são declarações de domínio no passado: ProposalCreated, SaleClosed

**✅ Value Objects**
- Money encapsula conceitos monetários
- Money fornece igualdade baseada em valor, não identidade
- Money é imutável

**⚠️ Integração de Bounded Context**
- Lead entity referenciada mas não parte do agregado (apropriado)
- Atualizações de status de Lead acontecem na camada de aplicação (questionável)
- UsedVehicleEvaluation referenciada mas comportamento não encapsulado (apropriado)

**❌ Implementação do Padrão Repository**
O `IProposalRepository` inclui métodos específicos de dashboard:
```csharp
Task<int> CountByStatusAsync(ProposalStatus status, string? salesPersonId, CancellationToken cancellationToken);
Task<IReadOnlyList<Proposal>> GetPendingActionProposalsAsync(string? salesPersonId, int limit, CancellationToken cancellationToken);
```

**Problema**: Eses são interesses específicos de consulta, não responsabilidades de repositório por DDD. Deveriam estar em serviços de consulta ou read models separados.

### Pontuação de Padrões de Modelo de Domínio

| Padrão | Implementação | Pontuação | Notas |
|---|---|---|---|
| Aggregate Root | Excelente | 9/10 | Controle de ciclo de vida adequado, pequenas lacunas de auditoria |
| Value Objects | Excelente | 9/10 | Money bem desenhado, poderia adicionar mais |
| Eventos de Domínio | Bom | 7/10 | Bons eventos, publicação inconsistente |
| Factory Method | Excelente | 9/10 | Factory estático impõe invariantes |
| Encapsulamento | Bom | 7/10 | Setters privados, faltam algumas validações |
| Comportamento Rico | Bom | 7/10 | Comportamentos chave presentes, alguns faltando |
| Imposição de Invariante | Bom | 7/10 | Invariantes chave protegidos, algumas lacunas |

**Qualidade Geral do Modelo de Domínio: 7.7/10**

A entidade Proposal demonstra fundamentos fortes de DDD com uso efetivo de agregados, value objects e eventos de domínio. O modelo é rico em comportamento e encapsula lógica de negócio central. No entanto, inconsistências na publicação de eventos, parâmetros não usados e validações faltando previnem que alcance a excelência.

### Recomendações para Melhoria do Modelo de Domínio

**Alta Prioridade**:
1. Corrigir consistência de publicação de eventos (todas mudanças de estado devem publicar eventos)
2. Adicionar validações de compatibilidade de método de pagamento
3. Armazenar ou remover parâmetros salesPersonId não usados
4. Adicionar validação de informações de veículo em métodos de atualização

**Média Prioridade**:
1. Considerar cache de cálculo TotalValue na mudança de estado
2. Remover valores de enum não usados ou implementar suas transições de estado
3. Extrair queries de dashboard do repositório para serviços de consulta separados
4. Adicionar regras de validação de quantidade/valor de item

**Baixa Prioridade**:
1. Considerar trilha de auditoria para todas as modificações
2. Adicionar versionamento de proposta para histórico de mudanças
3. Considerar serviço de domínio para regras complexas de cálculo de desconto
4. Adicionar eventos de domínio para mudanças de estado perdidas

---

## 7. Cobertura de Testes

### Testes Unitários
Localização: `/services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Domain/Entities/ProposalTests.cs`

**Cobertura de Teste**:
- Validação do factory method Create
- Aplicação de desconto (cenários < 5% e > 5%)
- Fluxo de aprovação de desconto
- Fechamento de proposta com validação
- Verificação de emissão de evento de domínio

**Contagem de Testes**: 7 métodos de teste

**Avaliação de Qualidade**:
- Boa cobertura de caminhos felizes e casos de borda
- Eventos de domínio verificados adequadamente
- Validações de regra de negócio testadas
- Faltando: Testes UpdateVehicleInfo, UpdatePaymentInfo, AddItem, RemoveItem

### Testes de Integração
Localização: `/services/commercial/5-Tests/GestAuto.Commercial.IntegrationTest/`

- Testes de Repositório: `ProposalRepositoryTests.cs`
- Testes de API: `ProposalApiTests.cs`

### Lacunas de Cobertura de Teste

**Métodos Não Testados**:
- `SetAwaitingEvaluation()`
- `ApplyEvaluationResult()`
- `SetTradeInValue()`
- `UpdateVehicleInfo()`
- `UpdatePaymentInfo()`
- `AddItem()`
- `RemoveItem()`
- `SetUsedVehicleEvaluationId()`
- Casos de borda de `CalculateTotalValue()`

**Cenários Não Testados**:
- Fluxo de valor de troca ponta-a-ponta
- Operações de gestão de item
- Cálculo de valor total com várias combinações
- Cenários de validação de método de pagamento

---

## 8. Pontos de Integração

### Interface de Repositório
Localização: `Interfaces/IProposalRepository.cs`

**Operações de Persistência**:
- `GetByIdAsync(Guid id)`
- `GetByLeadIdAsync(Guid leadId)`
- `AddAsync(Proposal proposal)`
- `UpdateAsync(Proposal proposal)`

**Operações de Consulta**:
- `ListAsync(salesPersonId, leadId, status, page, pageSize)`
- `CountAsync(salesPersonId, leadId, status)`

**Operações de Dashboard** (Posicionamento questionável):
- `CountByStatusAsync(status, salesPersonId)`
- `GetPendingActionProposalsAsync(salesPersonId, limit)`

### Integração com Camada de Aplicação

**Command Handlers**:
- `CreateProposalHandler`: Cria propostas, atualiza status do Lead
- `UpdateProposalHandler`: Atualiza informações da proposta
- `CloseProposalHandler`: Fecha propostas, atualiza Lead para Converted
- `AddProposalItemHandler`: Adiciona itens a propostas
- `RemoveProposalItemHandler`: Remove itens de propostas

**Query Handlers**:
- `GetProposalQuery`: Recupera proposta única
- `ListProposalsQuery`: Lista propostas com filtro

### Integração com Eventos de Domínio

**Consumidores de Evento** (Não na camada de domínio, mas acionados por eventos Proposal):
- ProposalCreatedEvent → Mudança de status de Lead, notificações
- ProposalUpdatedEvent → Atualizações de timeline, notificações
- SaleClosedEvent → Criação de pedido, cálculo de comissão, relatórios de receita

---

## 9. Conclusão

A entidade de domínio Proposal representa um agregado DDD bem estruturado com forte encapsulamento de lógica de negócio e uso efetivo de padrões de domínio. O modelo demonstra maturidade na modelagem de domínio de concessionária de veículos com clara separação de preocupações.

**Pontos Fortes Principais**:
- Modelo de domínio rico com encapsulamento de comportamento
- Implementação adequada de padrão aggregate root
- Uso efetivo de value objects (Money)
- Arquitetura orientada a eventos de domínio
- Imposição de invariante clara

**Principais Áreas para Melhoria**:
- Publicação de evento consistente para todas as mudanças de estado
- Regras de validação completas (compatibilidade de método de pagamento)
- Resolução de parâmetros não usados
- Cobertura de teste para todos os métodos públicos
- Separação de preocupações de consulta do repositório

**Avaliação Geral**: A entidade Proposal é um modelo de domínio pronto para produção que implementa com sucesso padrões DDD enquanto mantém lógica de negócio clara. Com melhorias menores em consistência e completeza, ela serviria como uma excelente implementação de referência para outros agregados no sistema.

---

**Relatório Gerado**: 23/01/2026
**Versão do Componente**: Atual (análise baseada no código mais recente)
**Analisador**: Agente Analisador Profundo de Componentes
**Caminho do Relatório**: `/docs/agents/component-deep-analyzer/component-proposal-entity-2026-01-23-10:14:43.md`
