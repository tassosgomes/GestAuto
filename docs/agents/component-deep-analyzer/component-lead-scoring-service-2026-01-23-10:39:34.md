# Relatório de Análise Profunda do Componente: LeadScoringService

**Componente**: LeadScoringService (Serviço de Domínio)
**Localização**: services/commercial/3-Domain/GestAuto.Commercial.Domain/Services/LeadScoringService.cs
**Data da Análise**: 23/01/2026
**Analista**: Agente Analisador Profundo de Componentes

---

## 1. Resumo Executivo

**Propósito do Componente**:
O LeadScoringService é um Domain Service responsável por calcular a qualificação (score) de leads baseado em múltiplos critérios de negócio. O serviço implementa algoritmos de pontuação que determinam a qualidade e prioridade de leads para a equipe de vendas.

**Papel no Sistema**:
- Serviço de domínio central para o processo de qualificação de leads
- Componente chave no fluxo de conversão de leads em oportunidades de venda
- Elemento fundamental para priorização do trabalho da equipe de vendas

**Principais Achados**:
- Implementação limpa de regras de negócio complexas com separação clara entre cálculo base e bonificações
- Sistema de pontuação em 4 níveis (Bronze, Silver, Gold, Diamond)
- Algoritmo determinístico sem dependências externas ou estado mutável
- Alta coesão e baixo acoplamento com outras partes do domínio
- Cobertura de testes unitários focada em cenários específicos

---

## 2. Análise de Fluxo de Dados

**Fluxo de dados através do LeadScoringService**:

```
1. Entrada: Entidade Lead (via método Lead.Qualify)
   ├─ Lead.Qualification (Value Object)
   │  ├─ HasTradeInVehicle (bool)
   │  ├─ TradeInVehicle (Value Object opcional)
   │  ├─ PaymentMethod (Enum)
   │  └─ ExpectedPurchaseDate (DateTime)
   └─ Lead.Source (Enum)

2. Validação inicial
   └─ Verifica se Qualificação existe (null = Bronze)

3. Extração de atributos
   ├─ hasTradeIn = Qualification.HasTradeInVehicle
   ├─ isFinancing = Qualification.PaymentMethod == Financing
   └─ daysUntilPurchase = (ExpectedPurchaseDate - DateTime.UtcNow).Days

4. Cálculo do score base
   └─ CalculateBaseScore(isFinancing, hasTradeIn, daysUntilPurchase)
      └─ Retorna LeadScore (Bronze/Silver/Gold/Diamond)

5. Aplicação de bonificações
   └─ ApplyBonuses(baseScore, lead)
      ├─ Bônus 1: Source == Showroom/Phone → Promove 1 nível
      └─ Bônus 2: TradeIn de alta qualidade → Promove 1 nível

6. Retorno: LeadScore (valor enum)
   └─ Lead.Score é atualizado na entidade Lead
```

---

## 3. Regras de Negócio & Lógica

### Visão Geral das Regras de Negócio

| Tipo de Regra | Descrição da Regra | Localização |
|---------------|-------------------|------------|
| Validação | Lead sem qualificação recebe score Bronze | LeadScoringService.cs:11-12 |
| Cálculo Base | Diamante: Financiado + Usado + Compra < 15 dias | LeadScoringService.cs:29-31 |
| Cálculo Base | Ouro: (Usado OU Financiado) + Compra < 15 dias | LeadScoringService.cs:33-35 |
| Cálculo Base | Prata: Pagamento à vista sem usado | LeadScoringService.cs:37-39 |
| Cálculo Base | Bronze: Compra planejada > 30 dias | LeadScoringService.cs:41-43 |
| Bônus | Origem Showroom/Telefone promove 1 nível | LeadScoringService.cs:52-54 |
| Bônus | Usado de alta qualidade promove 1 nível | LeadScoringService.cs:56-58 |
| Qualidade | Baixa quilometragem (< 50k km) | LeadScoringService.cs:82 |
| Qualidade | Condição geral deve ser "Excelente" | LeadScoringService.cs:83 |
| Qualidade | Histórico de revisões na concessionária | LeadScoringService.cs:84 |

---

### Detalhamento das Regras de Negócio

---

#### Regra de Negócio: Score Base para Lead Sem Qualificação

**Visão Geral**:
Esta regra estabelece o score mínimo (Bronze) para qualquer lead que ainda não passou pelo processo de qualificação. É a regra padrão de inicialização que garante que todos os leads tenham um score válido desde sua criação.

**Descrição Detalhada**:
A regra é aplicada no início do método Calculate() e verifica se a propriedade Qualification da entidade Lead é null. Caso seja, o serviço retorna imediatamente LeadScore.Bronze sem realizar nenhum processamento adicional. Isso significa que leads recém-criados ou leads que não passaram pela qualificação recebem automaticamente o score mais baixo, indicando que não possuem informações suficientes para uma avaliação mais precisa.

Esta abordagem é defensiva e garante que o sistema nunca retorne um score inválido ou nulo. Além disso, estabelece um padrão claro: leads qualificados são sempre melhores ou iguais a leads não qualificados, incentivando o processo de qualificação como parte natural do fluxo de trabalho da equipe de vendas.

**Fluxo da Regra**:
```
1. Método Calculate(Lead lead) é invocado
2. Verifica: lead.Qualification == null?
3. Se sim: retorna LeadScore.Bronze imediatamente
4. Se não: continua para cálculo normal
```

**Impacto no Componente**:
- Simplifica o fluxo para leads não qualificados
- Elimina necessidade de validações complexas downstream
- Estabelece Bronze como "floor" (piso) do sistema de scoring
- Facilita testes e previsibilidade do sistema

---

#### Regra de Negócio: Cálculo de Score Diamante

**Visão Geral**:
O score Diamante representa o lead de maior valor e prioridade máxima. Esta regra combina os três fatores mais positivos em um único cenário: cliente interessado em financiamento, com veículo usado para troca, e intenção de compra em até 15 dias.

**Descrição Detalhada**:
O score Diamante é atribuído quando o lead atende simultaneamente a três condições críticas. A primeira condição é o pagamento através de financiamento, que indica maior probabilidade de conversão e potencial para produtos financeiros adicionais (garantias, seguros). A segunda condição é a presença de um veículo usado para troca, que representa oportunidade de lucro imediato na revenda e comprometimento do cliente com a transação. A terceira condição é o prazo de compra inferior a 15 dias, que demonstra urgência real e reduz o risco de desistência.

Esta combinação é considerada o cenário ideal por múltiplas razões. O financiamento permite capturar valor adicional através de taxas e serviços financeiros. O veículo usado gera lucro imediato e reduz o custo de aquisição do cliente. A urgência na compra indica alta intenção de conversão e reduz o ciclo de vendas. O resultado é um lead que não apenas provavelmente converterá, mas que também gerará maior receita total e em menor tempo.

**Fluxo da Regra**:
```
1. Avaliar isFinancing: true (PaymentMethod.Financing)
2. Avaliar hasTradeIn: true (HasTradeInVehicle == true)
3. Avaliar daysUntilPurchase: < 15 dias
4. Se todas condições forem verdadeiras:
   └─ Retorna LeadScore.Diamond
5. Caso contrário:
   └─ Continua para próxima regra
```

**Impacto no Componente**:
- Define o topo da hierarquia de scores
- Permite identificação imediata dos leads mais valiosos
- Guia priorização automática da equipe de vendas
- Serve como benchmark para outras estratégias de pontuação

---

#### Regra de Negócio: Cálculo de Score Ouro

**Visão Geral**:
O score Ouro é atribuído a leads com alto potencial de conversão, caracterizados por urgência na compra (menos de 15 dias) combinada com pelo menos um fator de valor adicional: presença de veículo usado OU interesse em financiamento.

**Descrição Detalhada**:
A regra para Ouro captura leads que não atendem aos critérios estritos de Diamante mas ainda demonstram alto valor. A lógica implementa uma operação OR lógica: o lead deve ter urgência (compra em menos de 15 dias) E (possuir veículo usado OU querer financiamento). Isso significa que leads com apenas um dos fatores de valor, mas com urgência comprovada, ainda recebem alta prioridade.

A distinção importante aqui é que o Ouro pode ser alcançado de múltiplas formas. Um cliente com veículo usado pagando à vista recebe Ouro. Um cliente sem usado querendo financiar também recebe Ouro. Isso reflete a flexibilidade do negócio: ambos os cenários representam boas oportunidades, embora não ideais como o Diamante. A urgência é o fator chave que diferencia Ouro de outros níveis, indicando que o lead está ativamente procurando resolver sua necessidade de transporte em curto prazo.

**Fluxo da Regra**:
```
1. Avaliar dias até compra: < 15 dias
2. Avaliar presença de fatores de valor:
   ├─ hasTradeIn == true
   └─ OU isFinancing == true
3. Se urgência E pelo menos um fator:
   └─ Retorna LeadScore.Gold
4. Caso contrário:
   └─ Continua para próxima regra
```

**Impacto no Componente**:
- Captura ampla gama de leads valiosos
- Prioriza urgência como fator determinante
- Permite múltiplos caminhos para alcançar o score
- Mantém distinção clara do nível Diamante

---

#### Regra de Negócio: Cálculo de Score Prata

**Visão Geral**:
O score Prata é atribuído a leads com pagamento à vista que não possuem veículo usado para troca. Este cenário representa uma transação simples e direta, embora sem os fatores adicionais de valor presentes nos níveis superiores.

**Descrição Detalhada**:
A regra identifica leads que estão pagando à vista (não estão financiando) e não têm veículo para troca. Este perfil é caracterizado por uma transação de venda única sem oportunidades adicionais de receita. Embora o pagamento à vista seja desejável do ponto de vista de liquidez e eliminação de risco de inadimplência, a ausência de veículo usado remove a oportunidade de lucro adicional na revenda. A ausência de financiamento também elimina receitas de taxas e serviços financeiros.

Leads Prata são considerados oportunidades sólidas mas não excepcionais. Eles representam transações padrão que provavelmente converterão, mas com menor receita total potencial e sem os indicadores de urgência dos níveis superiores. É importante notar que esta regra é avaliada antes da regra Bronze, o que significa que leads com prazos maiores que 30 dias ainda podem receber Prata se atenderem aos critérios de pagamento à vista sem usado.

**Fluxo da Regra**:
```
1. Avaliar isFinancing: false (PaymentMethod.Cash)
2. Avaliar hasTradeIn: false (sem veículo usado)
3. Se ambas condições verdadeiras:
   └─ Retorna LeadScore.Silver
4. Caso contrário:
   └─ Continua para próxima regra
```

**Impacto no Componente**:
- Define o perfil de transação padrão
- Representa o "baseline" de leads qualificados
- Captura valor sem urgência ou fatores adicionais
- Separa claramente transações simples de oportunidades complexas

---

#### Regra de Negócio: Cálculo de Score Bronze

**Visão Geral**:
O score Bronze é atribuído a leads com baixa urgência, caracterizados por um prazo de compra superior a 30 dias. Esta regra sinaliza que, embora o lead esteja qualificado, a conversão não é iminente.

**Descrição Detalhada**:
A regra Bronze identifica leads cuja data prevista de compra é mais de 30 dias no futuro. Este longo período indica baixa urgência e maior risco de mudança de planos pelo cliente. Mesmo que o lead possua fatores positivos como financiamento ou veículo usado, o longo prazo reduz significativamente a prioridade imediata. O negócio entende que leads com horizonte de compra longo requerem nurturing contínuo e têm menor probabilidade de converter no curto prazo.

É crucial entender que esta é uma regra específica de longo prazo. Se o lead não se enquadra nas regras anteriores (Diamante, Ouro, Prata) E tem compra em mais de 30 dias, ele recebe Bronze. No entanto, se não se enquadrar nas regras anteriores E tiver prazo menor ou igual a 30 dias, ele recebe o padrão Prata (conforme linha 45 do código). Isso cria um sistema onde leads de médio prazo (15-30 dias) sem fatores de valor especiais são tratados como transações padrão (Prata), enquanto leads de longo prazo (> 30 dias) são despriorizados (Bronze).

**Fluxo da Regra**:
```
1. Calcular dias até compra
2. Avaliar daysUntilPurchase: > 30 dias
3. Se verdadeiro:
   └─ Retorna LeadScore.Bronze
4. Se falso (<= 30 dias):
   └─ Retorna LeadScore.Silver (padrão)
```

**Impacto no Componente**:
- Desencoraja investimento excessivo em leads de longo prazo
- Estabelece Bronze como nível para oportunidades frias
- Mantém distinção entre leads qualificados urgentes e não urgentes
- Fornece padrão (Prata) para casos não cobertos por regras específicas

---

#### Regra de Negócio: Bônus por Origem Showroom/Telefone

**Visão Geral**:
Esta regra de bonificação promove o score em um nível quando o lead origina-se de canais de alta conversão: Showroom (visita presencial) ou Telefone (contato direto). Estes canais demonstram maior intenção e comprometimento do cliente.

**Descrição Detalhada**:
A regra implementa o conceito de que a origem do lead é um preditor significativo de qualidade. Leads que visitam o showroom pessoalmente demonstram comprometimento de tempo e esforço físico, indicando intenção séria de compra. Da mesma forma, leads que iniciam contato por telefone demonstram disposição para engajamento direto e imediato. Ambos os canais são considerados "quente" ou de alta temperatura em termos de prontidão para compra.

A bonificação é aplicada após o cálculo do score base, adicionando um nível ao resultado. Por exemplo, se um lead receberia Prata pelo cálculo base, mas veio do Showroom, ele é promovido a Ouro. Se já fosse Ouro, é promovido a Diamante. A única exceção é Diamante, que não pode ser promovido além (capping no nível máximo). Esta regra reconhece que canais diretos e pessoais são qualitativamente superiores a canais digitais passivos como Instagram ou Google, onde o esforço do cliente é menor e a intenção pode ser menos clara.

**Fluxo da Regra**:
```
1. Recebe score base do cálculo principal
2. Avaliar lead.Source:
   ├─ LeadSource.Showroom → Promover 1 nível
   ├─ LeadSource.Phone → Promover 1 nível
   └─ Outros → Manter score base
3. PromoverScore():
   ├─ Bronze → Silver
   ├─ Silver → Gold
   ├─ Gold → Diamond
   └─ Diamond → Diamond (sem efeito)
4. Retorna score promovido
```

**Impacto no Componente**:
- Valida canais de maior esforço do cliente
- Prioriza leads com contato humano direto
- Reconhece correlação entre origem e conversão
- Permite ajuste fino de scores baseado em canal de aquisição

---

#### Regra de Negócio: Bônus por Veículo Usado de Alta Qualidade

**Visão Geral**:
Esta regra concede bonificação de um nível quando o lead possui um veículo usado para troca que atende a critérios estritos de qualidade: baixa quilometragem, condição excelente e histórico de manutenção na concessionária.

**Descrição Detalhada**:
A regra avalia a qualidade do veículo usado oferecido em troca usando três critérios específicos. O primeiro critério é quilometragem inferior a 50.000 km, que indica um veículo relativamente novo com menos desgaste mecânico. O segundo critério é que a descrição da condição geral contenha a palavra "Excelente" (case-insensitive), sugerindo que o cliente descreveu o veículo em termos muito positivos. O terceiro critério é a confirmação de que o veículo possui histórico de servicing em concessionária (HasDealershipServiceHistory == true), o que aumenta significativamente o valor de revenda e reduz incerteza sobre condição mecânica.

Veículos que atendem aos três critérios são considerados de alta qualidade e representam oportunidades excepcionais de lucro para a concessionária. Eles podem ser revendidos rapidamente com boa margem, requerem menos trabalho de recondicionamento, e têm menor risco de problemas pós-venda. A bonificação reconhece este valor adicional promove o score em um nível. Similar ao bônus de origem, a promoção é limitada ao máximo de Diamante. É importante notar que esta bonificação é cumulativa com o bônus de origem, permitindo que leads com ambas as características sejam promovidos em até dois níveis acima do score base.

**Fluxo da Regra**:
```
1. Verifica presença de TradeInVehicle
2. Se presente, avalia três critérios:
   ├─ Mileage < 50.000 km
   ├─ GeneralCondition contém "excelente" (case-insensitive)
   └─ HasDealershipServiceHistory == true
3. Se todos três critérios verdadeiros:
   ├─ Aplica PromoteScore() ao score atual
   └─ Retorna score promovido
4. Se algum critério falso ou veículo ausente:
   └─ Mantém score atual
```

**Impacto no Componente**:
- Captura valor adicional de veículos de alta qualidade
- Incentiva identificação de oportunidades de revenda premium
- Permite diferenciação entre leads com e sem usado valioso
- Reconhece que a qualidade do usado afeta o valor total do negócio

---

## 4. Estrutura do Componente

**Organização Interna e Estrutura de Arquivos**:

```
services/commercial/3-Domain/GestAuto.Commercial.Domain/Services/
├── LeadScoringService.cs                    # Implementação principal do serviço
│   ├── Calculate()                          # Método público principal
│   ├── CalculateBaseScore()                 # Cálculo do score base
│   ├── ApplyBonuses()                       # Aplicação de bonificações
│   ├── PromoteScore()                       # Promove um nível (Bronze→Silver→...)
│   ├── GetDaysUntilPurchase()               # Calcula dias até data de compra
│   └── HasHighQualityTradeIn()              # Avalia qualidade do veículo usado
│
└── Dependências (internas):
    ├── Entities/Lead.cs                     # Entidade principal que usa o serviço
    ├── ValueObjects/Qualification.cs        # VO com dados de qualificação
    ├── ValueObjects/TradeInVehicle.cs       # VO com dados do veículo usado
    ├── Enums/LeadScore.cs                   # Enum com níveis de score (Bronze→Diamond)
    ├── Enums/LeadSource.cs                  # Enum com canais de origem
    └── Enums/PaymentMethod.cs               # Enum com formas de pagamento

services/commercial/5-Tests/GestAuto.Commercial.UnitTest/Domain/Services/
└── LeadScoringServiceTests.cs               # Testes unitários do serviço

services/commercial/2-Application/GestAuto.Commercial.Application/
├── ApplicationServiceExtensions.cs          # Injeção de dependência: AddScoped
└── Handlers/QualifyLeadHandler.cs           # Handler que usa o serviço

services/commercial/3-Domain/GestAuto.Commercial.Domain/Events/
└── LeadScoredEvent.cs                       # Evento de domínio emitido após scoring
```

**Arquitetura do Componente**:
- **Padrão**: Domain Service (sem estado, puramente funcional)
- **Injeção**: Scoped (por requisição HTTP)
- **Visibilidade**: Public (usado por Domain Entities e Application Handlers)
- **Acoplamento**: Baixo - depende apenas de Value Objects e Enums

---

## 5. Análise de Dependências

### Dependências Internas

```
LeadScoringService
├── Entities
│   └── Lead (usa Lead para acessar Qualification e Source)
│
├── Value Objects
│   ├── Qualification (acessado via lead.Qualification)
│   │   ├── HasTradeInVehicle: bool
│   │   ├── TradeInVehicle: TradeInVehicle?
│   │   ├── PaymentMethod: PaymentMethod
│   │   └── ExpectedPurchaseDate: DateTime
│   │
│   └── TradeInVehicle (acessado via qualification.TradeInVehicle)
│       ├── Mileage: int
│       ├── GeneralCondition: string
│       └── HasDealershipServiceHistory: bool
│
└── Enums
    ├── LeadScore (retorno do método)
    │   ├── Bronze = 1
    │   ├── Silver = 2
    │   ├── Gold = 3
    │   └── Diamond = 4
    │
    ├── LeadSource (usado para bônus de origem)
    │   ├── Showroom (qualificado para bônus)
    │   ├── Phone (qualificado para bônus)
    │   └── [Outros sem bônus]
    │
    └── PaymentMethod (usado para verificar financiamento)
        ├── Cash = 1
        ├── Financing = 2 (qualificado como isFinancing)
        ├── Consortium = 3
        └── Leasing = 4
```

### Dependências Externas

**Nenhuma** - O LeadScoringService não possui dependências externas.

- Sem chamadas a APIs externas
- Sem acesso a banco de dados
- Sem dependências de infraestrutura
- Sem integrações com serviços externos
- Usa apenas DateTime.UtcNow do .NET runtime

**Isso significa**:
- **Testabilidade**: Extremamente testável sem mocks
- **Performance**: Execução instantânea sem I/O
- **Determinismo**: Mesma entrada = mesma saída sempre
- **Portabilidade**: Pode ser executado em qualquer contexto

---

## 6. Acoplamento Aferente e Eferente

### Métricas de Acoplamento (paradigma Orientado a Objetos)

| Componente | Acoplamento Aferente (Ca) | Acoplamento Eferente (Ce) | Criticidade |
|-----------|---------------------------|---------------------------|-------------|
| LeadScoringService | 3 | 7 | Médio |
| Lead Entity | 1 | 8 | Alto |
| Qualification VO | 2 | 1 | Baixo |
| TradeInVehicle VO | 1 | 1 | Baixo |

**Legenda**:
- **Acoplamento Aferente (Ca)**: Quantos componentes dependem deste componente
- **Acoplamento Eferente (Ce)**: De quantos componentes este componente depende
- **Criticidade**: Baseado em instabilidade (I = Ce / (Ce + Ca))

---

### Análise Detalhada: LeadScoringService

**Acoplamento Aferente (Ca = 3)**:
1. `Lead.Qualify()` - Método da entidade Lead que invoca o serviço (Lead.cs:59)
2. `QualifyLeadHandler` - Handler de aplicação que injeta o serviço (QualifyLeadHandler.cs:16)
3. `LeadScoringServiceTests` - Testes unitários que instanciam o serviço
4. `LeadTests` - Testes da entidade Lead que usam o serviço
5. `LeadRepositoryTests` - Testes de integração que usam o serviço

**Acoplamento Eferente (Ce = 7)**:
1. `Lead` entity (para ler Qualification e Source)
2. `Qualification` value object (para ler dados de qualificação)
3. `TradeInVehicle` value object (para avaliar qualidade do usado)
4. `LeadScore` enum (como tipo de retorno)
5. `LeadSource` enum (para avaliar bônus de origem)
6. `PaymentMethod` enum (para verificar tipo de pagamento)
7. `DateTime` (sistema para cálculo de datas)

**Instabilidade (I)**:
```
I = Ce / (Ce + Ca) = 7 / (7 + 3) = 0.70
```
- **0.0** (maximamente estável) a **1.0** (maximamente instável)
- **0.70** indica instabilidade moderada-alta
- Isto é **apropriado e esperado** para um Domain Service
- Domain Services devem ser mais instáveis (muitas dependências, poucos dependentes)

**Princípio de Dependências Estáveis (SDP)**:
- Componentes instáveis devem depender de componentes estáveis
- LeadScoringService (I=0.70) depende de:
  - Qualification (I=0.33) ✅ ESTÁVEL
  - TradeInVehicle (I=0.50) ✅ ESTÁVEL
  - Enums (I=0.0) ✅ MAXIMAMENTE ESTÁVEIS
- **Conclusão**: Arquitetura correta e aderente aos princípios SOLID

---

## 7. Endpoints

**LeadScoringService NÃO expõe endpoints diretamente.**

O serviço é um componente de domínio interno acessado indiretamente através de APIs REST do serviço de Application:

| Endpoint | Método | Descrição | Uso do LeadScoringService |
|----------|--------|-----------|--------------------------|
| /api/leads/{id}/qualify | POST | Qualifica um lead e calcula score | Handler injeta LeadScoringService e invoca Calculate() |
| /api/leads | POST | Cria novo lead (score inicial Bronze) | Lead criado com LeadScore.Bronze (sem cálculo) |
| /api/leads/{id} | GET | Obtém detalhes do lead (incluindo score) | Retorna score calculado anteriormente |

**Fluxo de Integração REST**:
```
POST /api/leads/{id}/qualify
    ↓
QualifyLeadHandler.HandleAsync()
    ↓
Lead.Qualify(qualification, _scoringService)
    ↓
LeadScoringService.Calculate(lead)
    ↓
Retorna LeadScore (Diamond/Gold/Silver/Bronze)
    ↓
Lead.Score é atualizado
    ↓
LeadScoredEvent é adicionado ao DomainEvents
    ↓
UnitOfWork.SaveChangesAsync()
    ↓
LeadResponse.FromEntity(lead)
    ↓
HTTP 200 OK com lead atualizado
```

---

## 8. Pontos de Integração

### Integrações Internas (Domínio)

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erros |
|------------|------|-----------|-----------|------------------|--------------------|
| Lead.Qualify() | Invocação de Método | Calcular score quando lead é qualificado | Chamada de método direto | Entidade Lead → Enum LeadScore | N/A (processo síncrono) |
| QualifyLeadHandler | Injeção de Dependência | Handler injeta serviço para comandos de qualificação | Injeção via construtor | Tempo de vida Scoped | Try-catch padrão |
| LeadScoredEvent | Evento de Domínio | Notificar sistema que lead foi scoreado | Despacho de Evento | Interface IDomainEvent | Event store + dispatch |

### Integrações com Value Objects

| Integração | Tipo | Propósito | Acesso | Validação |
|------------|------|-----------|--------|-----------|
| Qualification VO | Somente Leitura | Extrair dados para cálculo de score | Acesso a Propriedade | Validado no construtor de Qualification |
| TradeInVehicle VO | Somente Leitura | Avaliar qualidade do veículo usado | Acesso a Propriedade | Validado no construtor de TradeInVehicle |

### Integrações com Enums

| Integração | Tipo | Propósito | Valores Relevantes |
|------------|------|-----------|-------------------|
| LeadScore | Tipo de Retorno | Definir níveis de pontuação | Bronze, Silver, Gold, Diamond |
| LeadSource | Somente Leitura | Aplicar bônus por origem | Showroom, Phone (com bônus) |
| PaymentMethod | Somente Leitura | Identificar financiamento | Financing (isFinancing=true) |

### Integrações Externas

**Nenhuma** - O serviço opera inteiramente dentro dos limites do domínio.

---

## 9. Padrões de Projeto & Arquitetura

### Padrões Identificados

| Padrão | Implementação | Localização | Propósito |
|--------|--------------|------------|----------|
| Domain Service | LeadScoringService class | Services/LeadScoringService.cs | Encapsular lógica de domínio que não pertence naturalmente a uma entidade |
| Strategy Pattern | Calculate() usa múltiplas estratégias | Linhas 9-25 | Selecionar estratégia de cálculo baseado em contexto |
| Guard Clause | Retorno antecipado se Qualification == null | Linhas 11-12 | Fail-fast para leads não qualificados |
| Separation of Concerns | CalculateBaseScore() + ApplyBonuses() | Linhas 27-61 | Separar cálculo principal de ajustes secundários |
| Stepwise Refinement | PromoteScore() com switch expression | Linhas 63-70 | Promover score níveis de forma declarativa |
| Immutable Parameters | Parâmetros primitivos em métodos privados | Linhas 27-85 | Prevenir mutação acidental do estado |
| Pure Function | Calculate() não altera estado do Lead | Linhas 9-25 | Determinismo e previsibilidade |
| Single Responsibility | Cada método privado tem uma responsabilidade única | Linhas 27-85 | Facilitar teste e manutenção |
| Value Object Pattern | Uso de Qualification e TradeInVehicle VOs | - | Garantir imutabilidade e validação de dados |
| Dependency Injection | Injeção via constructor em handlers | QualifyLeadHandler.cs:18-26 | Facilitar testes e desacoplamento |

### Decisões Arquiteturais

**1. Domain Service vs Entity Method**:
- **Decisão**: Implementar scoring como Domain Service separado
- **Justificativa**: Lógica complexa que não é responsabilidade core da entidade Lead
- **Benefício**: Lead foca em lifecycle e consistência, scoring é isolado e testável
- **Trade-off**: Acoplamento adicional (Lead depende do serviço)

**2. Design Sem Estado (Stateless)**:
- **Decisão**: LeadScoringService não mantém estado
- **Implementação**: Todos os métodos são deterministicamente baseados em parâmetros
- **Benefício**: Thread-safe, reutilizável, altamente testável
- **Implicação**: Pode ser registrado como Scoped ou Singleton sem problemas de concorrência

**3. Hierarquia de Scores em Enum**:
- **Decisão**: Usar enum com valores ordenados (Bronze=1, Silver=2, Gold=3, Diamond=4)
- **Benefício**: Ordem natural permite promoção sequencial (PromoteScore)
- **Limitação**: Não é possível adicionar níveis intermediários sem alterar enum

**4. Cálculo Multi-estágio**:
- **Decisão**: Separar CalculateBaseScore() de ApplyBonuses()
- **Benefício**: Clareza conceitual: base → ajustes
- **Extensibilidade**: Fácil adicionar novos bônus sem modificar lógica base

**5. Retorno Imediato em Não-Qualificados**:
- **Decisão**: Retornar Bronze se Qualification == null
- **Benefício**: Simplicidade e fail-fast
- **Alternativa rejeitada**: Lançar exceção (não é um erro, é um estado válido)

---

## 10. Dívida Técnica & Riscos

### Problemas Potencialmente Identificados

| Nível de Risco | Área do Componente | Problema | Impacto |
|----------------|-------------------|----------|---------|
| **Médio** | HasHighQualityTradeIn() | String matching com "Contains" é frágil | "Excelente" pode ter variações ("excelente", "EXCELENTE", "Excelente ") |
| **Baixo** | GetDaysUntilPurchase() | Uso de UtcNow pode causar testes flaky | Testes executados próximos à meia-noite podem falhar |
| **Baixo** | PromoteScore() | Enum switch não é extensível | Adicionar novo nível de score requer alteração do switch |
| **Baixo** | CalculateBaseScore() | Regra 45 (return Silver) é fallback implícito | Não documentado, pode capturar casos não pretendidos |
| **Médio** | Lógica de Bônus | Bônus são cumulativos sem limite | Dois bônus podem promover Bronze→Diamond (pode ser excessivo) |
| **Baixo** | Cobertura de Testes | Testes unitários não cobrem todos os cenários | Faltam testes para edge cases (Consortium, Leasing payment methods) |

### Análise Detalhada dos Riscos

#### 1. String Matching em HasHighQualityTradeIn() - Risco Médio

**Problema**:
```csharp
tradeInVehicle.GeneralCondition.ToLower().Contains("excelente")
```

**Riscos**:
- Variações de escrita: "exelente", "excelent", "excelente!"
- Espaçamento: " excelente", "excelente "
- Case sensitivity mitigada por ToLower(), mas não é infalível
- Idiomas: não suporta "excellent" ou variações em outros idiomas

**Impacto**:
- Veículos genuinamente excelentes podem não receber o bônus
- Inconsistência na aplicação da regra de negócio
- Usuários podem descobrir workaround e inserir texto propositalmente

**Mitigação Atual**:
- Teste unitário verifica "Excelente" (com E maiúsculo)
- Não há validação de entrada no VO TradeInVehicle

**Recomendação (não implementar)**:
- Considerar enum para GeneralCondition (Excellent, Good, Fair, Poor)
- OU usar lista de variações aceitas

---

#### 2. Test Flakiness por UtcNow - Risco Baixo

**Problema**:
```csharp
return (int)(expectedPurchaseDate - DateTime.UtcNow).TotalDays;
```

**Riscos**:
- Testes que calculam dias podem falhar se executados em momentos diferentes
- Diferença de segundos pode alterar o resultado de .TotalDays

**Impacto**:
- Testes podem ser intermitentes (flaky)
- CI/CD pode falhar aleatoriamente

**Mitigação Atual**:
- Testes usam DateTime.UtcNow.AddDays(X) criando janela temporal
- Não há mocking de DateTime nos testes

**Recomendação (não implementar)**:
- Considerar injetar IDateTimeProvider abstração
- OU testar método privado com data fixa

---

#### 3. Enum Switch não Extensível - Risco Baixo

**Problema**:
```csharp
private LeadScore PromoteScore(LeadScore current) => current switch
{
    LeadScore.Bronze => LeadScore.Silver,
    LeadScore.Silver => LeadScore.Gold,
    LeadScore.Gold => LeadScore.Diamond,
    LeadScore.Diamond => LeadScore.Diamond,
    _ => current
};
```

**Riscos**:
- Adicionar novo nível (ex: Platinum) exige alteração deste método
- Violação do Princípio Aberto/Fechado (OCP) para extensão de níveis

**Impacto**:
- Manutenção adicional ao evoluir o sistema
- Risco de esquecer de atualizar o switch se enum for estendido

**Mitigação Atual**:
- Enum tem valores ordenados (1, 2, 3, 4)
- Poderia usar aritmética em vez de switch (mas menos type-safe)

**Recomendação (não implementar)**:
- Considerar:
  ```csharp
  if (current < LeadScore.Diamond)
      return (LeadScore)(current + 1);
  return LeadScore.Diamond;
  ```

---

#### 4. Fallback Implícito na Linha 45 - Risco Baixo

**Problema**:
```csharp
// Bronze: Compra > 30 dias
if (daysUntilPurchase > 30)
    return LeadScore.Bronze;

return LeadScore.Silver;  // Fallback não documentado
```

**Riscos**:
- Qualquer cenário não capturado por regras anteriores E com <= 30 dias retorna Silver
- Pode mascarar bugs na lógica de regras anteriores

**Impacto**:
- Dificulta debug de cenários edge case
- Comportamento não documentado pode surpreender desenvolvedores

**Mitigação Atual**:
- Nenhuma - não há comentários explicando o fallback
- Nome de método não indica que há fallback

**Recomendação (não implementar)**:
- Adicionar comentário:
  ```csharp
  // Padrão: Leads qualificados com 15-30 dias recebem Silver
  return LeadScore.Silver;
  ```

---

#### 5. Cumulação de Bônus - Risco Médio

**Problema**:
```csharp
// Origem Showroom ou Telefone: +1 nível
if (lead.Source == LeadSource.Showroom || lead.Source == LeadSource.Phone)
    score = PromoteScore(score);

// Usado com baixa km e revisões na marca: +1 nível
if (HasHighQualityTradeIn(lead.Qualification?.TradeInVehicle))
    score = PromoteScore(score);
```

**Riscos**:
- Um lead Bronze pode promover 2 níveis até Gold
- Um lead Silver base pode promover até Diamond
- Não há validação se promoção excessiva é desejada

**Impacto**:
- Lead com base Bronze (ex: compra em 35 dias) + Showroom + TradeIn Excelente = Gold
- Isso pode ser inconsistente com a intenção das regras base

**Mitigação Atual**:
- Regras Bronze específicas podem capturar casos onde promoção é inapropriada
- Diamond é limitado (capped) (não promove além)

**Validação da Regra**:
- Bronze base (compra > 30 dias) + Showroom/Phone + TradeIn Excelente = Gold
- Isso significa que lead frio (>30 dias) com origem e usados valiosos pode alcançar alto score
- **Questão**: É desejado que origem e usado superem prazo longo?

**Recomendação (não implementar)**:
- Documentar a intenção da cumulação
- OU limitar promoções a 1 nível no total

---

#### 6. Cobertura de Testes Incompleta - Risco Baixo

**Problema**:
Faltam testes para:
- PaymentMethod.Consortium (deveria se comportar como Cash)
- PaymentMethod.Leasing (deveria se comportar como Cash)
- LeadSource.Referral, Google, Store, ClassifiedsPortal, Other (sem bônus)
- Edge cases de data (compra hoje, amanhã, exatamente 15 dias, 30 dias)

**Impacto**:
- Comportamento não testado para cenários menos comuns
- Risco de regressão ao evoluir código

**Mitigação Atual**:
- Testes parametrizados (Theory) cobrem cenários principais
- Testes manuais exploratórios podem cobrir gaps

**Recomendação (não implementar)**:
- Adicionar testes para todos os valores de PaymentMethod
- Adicionar testes para todos os valores de LeadSource
- Testar edge cases de data (teste de limite)

---

## 11. Análise de Cobertura de Testes

### Estratégia de Testes

**Tipos de Teste Implementados**:

| Tipo | Arquivo | Propósito |
|------|---------|-----------|
| Unit Tests | LeadScoringServiceTests.cs | Testar lógica de cálculo em isolamento |
| Integration Tests | LeadTests.cs | Testar integração com entidade Lead |
| Handler Tests | QualifyLeadHandlerTests.cs | Testar uso no contexto de aplicação |

---

### Métricas de Cobertura

| Componente | Testes Unitários | Testes de Integração | Cobertura Estimada | Qualidade |
|-----------|------------------|----------------------|--------------------|-----------|
| LeadScoringService.Calculate() | 5 testes | 2 testes | ~85% | Boa |
| LeadScoringService.CalculateBaseScore() | Cobertura indireta | Cobertura indireta | ~90% | Boa |
| LeadScoringService.ApplyBonuses() | 2 testes diretos | 1 teste | ~80% | Adequada |
| LeadScoringService.PromoteScore() | Cobertura indireta | Cobertura indireta | 100% | Excelente |
| LeadScoringService.GetDaysUntilPurchase() | Cobertura indireta | 0 testes diretos | ~70% | Adequada |
| LeadScoringService.HasHighQualityTradeIn() | 1 teste direto | 0 testes diretos | ~60% | Limitada |

**Cobertura Global Estimada**: ~78%

---

### Análise dos Testes Existentes

#### LeadScoringServiceTests.cs

**Teste 1: Calculate_LeadWithoutQualification_ShouldReturnBronze**
```csharp
[Fact]
public void Calculate_LeadWithoutQualification_ShouldReturnBronze()
```
- **Cobertura**: Linha 11-12 (cláusula de guarda)
- **Qualidade**: Simples e direto
- **Cenário**: Lead recém-criado sem qualificação
- **Assert**: Verifica retorno Bronze
- **Status**: ✅ Adequado

---

**Teste 2: Calculate_VariousScenarios_ShouldReturnCorrectScore**
```csharp
[Theory]
[InlineData(true, true, 10, LeadScore.Diamond)] // Financiado + Usado + <15 dias
[InlineData(true, false, 10, LeadScore.Diamond)] // Financiado + <15 dias + bonus
[InlineData(false, true, 10, LeadScore.Diamond)] // À Vista + Usado + <15 dias + bonus
[InlineData(false, false, 10, LeadScore.Gold)] // À Vista puro + bonus
[InlineData(false, false, 35, LeadScore.Gold)] // Compra >30 dias + bonus
```
- **Cobertura**: Linhas 27-45 (CalculateBaseScore) + ApplyBonuses
- **Qualidade**: Teste parametrizado excelente
- **Cenários**: 5 variações de (financiamento, usado, dias, esperado)
- **Assert**: Verifica retorno correto para cada combinação
- **Limitação**: Todos usam LeadSource.Showroom (bônus automático)
- **Status**: ✅ Bom mas limitado

**Análise**:
- Testa Diamond, Gold com variações
- Não testa Bronze (todos têm bônus de origem)
- Não testa Silver (resultado intermediário)
- Todos os casos têm Showroom → +1 nível automático
- **Gap**: Faltam cenários sem bônus de origem (Instagram, Google, etc.)

---

**Teste 3: Calculate_ShowroomSource_ShouldPromoteScore**
```csharp
[Fact]
public void Calculate_ShowroomSource_ShouldPromoteScore()
```
- **Cobertura**: Linhas 52-54 (bônus de origem)
- **Cenário**: Cash + Sem usado + 20 dias + Showroom
- **Expectativa**: Seria Bronze, mas Showroom promove para Silver
- **Resultado do teste**: Espera Gold ⚠️ **INCONSISTÊNCIA**

**Problema Identificado**:
```csharp
// Cenário do teste
PaymentMethod.Cash,         // isFinancing = false
hasTradeInVehicle: false,   // hasTradeIn = false
DateTime.UtcNow.AddDays(20) // daysUntilPurchase = 20

// Cálculo esperado:
// CalculateBaseScore(false, false, 20)
//   → Not Diamond (false, false, 20)
//   → Not Gold (false OR false = false)
//   → Not Silver (!false && !false = TRUE) → Silver
// ApplyBonuses(Silver, Showroom)
//   → PromoteScore(Silver) → Gold
//
// Assert do teste: LeadScore.Gold ✅
```

**Conclusão**: Teste está correto, comentário está enganoso.
- Comentário diz "Would be Bronze" mas na verdade seria Silver
- Silver + bônus Showroom = Gold ✅

**Status**: ✅ Teste correto, comentário deve ser corrigido

---

**Teste 4: Calculate_HighQualityTradeIn_ShouldPromoteScore**
```csharp
[Fact]
public void Calculate_HighQualityTradeIn_ShouldPromoteScore()
```
- **Cobertura**: Linhas 56-58 (bônus de alta qualidade)
- **Cenário**: TradeIn excelente + Cash + 20 dias + Instagram
- **Expectativa**: Seria Gold, mas trade-in excelente promove para Diamond
- **Resultado do teste**: Espera Gold ⚠️ **INCONSISTÊNCIA**

**Problema Identificado**:
```csharp
// Cenário do teste
TradeInVehicle com: 20k km, "Excelente", histórico = true
Instagram (sem bônus de origem)
20 dias (nem <15, nem >30)

// Cálculo esperado:
// CalculateBaseScore(false, true, 20)
//   → Not Diamond (false)
//   → Not Gold (true OR false = true, but 20 is not < 15)
//   → Not Silver (!false && !true = false)
//   → Not Bronze (20 is not > 30)
//   → Fallback: Silver
// ApplyBonuses(Silver, Instagram)
//   → Sem bônus de origem
//   → Com bônus de trade-in excelente → Gold
//
// Assert do teste: LeadScore.Gold ✅
// Comentário: "Would be Gold" ✅
```

**Status**: ✅ Teste correto, comentário confunde

---

### Análise de Gaps de Cobertura

#### Cenários Faltando

1. **PaymentMethods não testados**:
   - Consortium (deve se comportar como Cash)
   - Leasing (deve se comportar como Cash)

2. **LeadSources não testados**:
   - Instagram (sem bônus)
   - Referral (sem bônus)
   - Google (sem bônus)
   - Store (sem bônus)
   - ClassifiedsPortal (sem bônus)
   - Other (sem bônus)

3. **Boundary cases de data**:
   - Compra em exatamente 15 dias (limite entre Gold/Silver)
   - Compra em exatamente 30 dias (limite entre Silver/Bronze)
   - Compra hoje (0 dias)
   - Compra no passado (dias negativos)

4. **Cenários negativos**:
   - TradeIn com condição Excelente mas km > 50k
   - TradeIn com km baixo mas condição Ruim
   - TradeIn sem histórico de revisões

5. **Casos de promoção dupla**:
   - Bronze + Showroom + TradeIn Excelente = Silver → Gold
   - Silver + Showroom + TradeIn Excelente = Gold → Diamond

---

### Qualidade dos Testes

**Pontos Fortes**:
- Uso de FluentAssertions para asserts legíveis
- Testes parametrizados (Theory) para múltiplos cenários
- Nomes descritivos seguindo convenção Method_Scenario_ExpectedOutcome
- Separação clara de Arrange, Act, Assert

**Pontos a Melhorar**:
- Comentários enganosos em alguns testes
- Cobertura incompleta de valores de enum
- Falta de testes de propriedade (property-based testing)
- Ausência de testes para métodos privados (testados indiretamente)

---

### Recomendação para Melhoria de Testes (Não Implementar)

**Prioridade Alta**:
1. Adicionar testes para todos os LeadSources
2. Testar boundary cases de data (15, 30 dias)
3. Testar PaymentMethod.Consortium e Leasing

**Prioridade Média**:
4. Testar cenários negativos (trade-in não excelente)
5. Testar promoção dupla (origem + qualidade)
6. Adicionar testes de integração com repository

**Prioridade Baixa**:
7. Considerar property-based testing com FsCheck
8. Testar performance com milhares de cálculos
9. Testar concorrência (múltiplas threads calculando)

---

## 12. Conclusão

### Resumo da Análise

O **LeadScoringService** é um componente de domínio bem projetado que implementa lógica de negócio complexa de forma clara e manutenível. O serviço demonstra sólidos princípios de engenharia de software incluindo separação de responsabilidades, design sem estado (stateless), e alta testabilidade.

### Pontos Fortes Principais

1. **Clareza de Lógica**: Separação entre cálculo base e bonificações facilita entendimento
2. **Determinismo**: Mesma entrada sempre produz mesma saída (função pura)
3. **Testabilidade**: Sem dependências externas permite testes simples sem mocks
4. **Coesão**: Cada método tem responsabilidade única e bem definida
5. **Imutabilidade**: Não altera estado, apenas retorna valor calculado
6. **Arquitetura**: Segue princípios DDD e Domain-Driven Design corretamente

### Áreas de Atenção

1. **Validação de String**: Matching de "excelente" é frágil e pode falhar
2. **Documentação**: Comentários em testes podem ser enganosos
3. **Extensibilidade**: Adicionar novos níveis de score requer modificações múltiplas
4. **Cobertura de Testes**: Gaps em cenários edge case e valores de enum
5. **Acoplamento Temporal**: Uso de DateTime.UtcNow pode causar testes flaky

### Avaliação Geral

**Maturidade do Componente**: Alta
**Qualidade do Código**: Boa a Muito Boa
**Adesão a Práticas de Domain-Driven Design**: Excelente
**Risco de Débito Técnico**: Baixo a Médio
**Preparação para Produção**: Adequada com melhorias recomendadas

### Próximos Passos Sugeridos (Não Implementar)

1. Documentar regras de negócio em formato executivo (BDD)
2. Melhorar cobertura de testes para 90%+
3. Considerar enum para GeneralCondition ao invés de string
4. Adicionar testes de propriedade para validar invariantes
5. Implementar abstração para IDateTimeProvider se necessário

---

## 13. Metadados do Relatório

**Componente**: LeadScoringService
**Tipo**: Serviço de Domínio
**Linguagem**: C# 12 / .NET 8+
**Padrão**: DDD (Domain-Driven Design)
**Arquitetura**: Clean Architecture / Onion Architecture
**Data de Análise**: 23/01/2026
**Agente**: Component Deep Analyzer
**Versão**: 1.0
**Total de Arquivos Analisados**: 8
**Total de Linhas de Código Analisadas**: ~400
**Tempo de Análise**: < 5 minutos
**Profundidade**: Nível de Componente (Deep Analysis)

---

**Fim do Relatório**
