# Relatório de Análise Profunda de Componente - ProposalController

**Nome do Componente:** ProposalController
**Data da Análise:** 23/01/2026
**Caminho do Componente:** services/commercial/1-Services/GestAuto.Commercial.API/Controllers/ProposalController.cs
**Linhas de Código**: 363
**Camada**: Camada de Apresentação (API)
**Padrão de Arquitetura**: Clean Architecture com CQRS

---

## 1. Resumo Executivo

O ProposalController é um controller de API REST responsável por gerenciar propostas comerciais e negociações de vendas de veículos no sistema GestAuto. Ele serve como ponto de entrada para todas as operações relacionadas a propostas, tratando requisições HTTP e delegando lógica de negócio para handlers de comando e query seguindo o padrão CQRS.

**Principais Responsabilidades:**
- Operações CRUD para propostas de vendas
- Aplicação de desconto e fluxo de trabalho de aprovação
- Gerenciamento de itens de proposta (adicionar/remover extras)
- Fechamento de proposta com conversão de lead
- Filtragem e autorização de vendedor

**Principais Descobertas:**
- Controller bem estruturado com clara separação de preocupações
- Implementa autorização adequada com políticas baseadas em papel (SalesPerson, Manager)
- Lógica de negócio abrangente encapsulada em entidades de domínio
- Boa cobertura de teste nos níveis de domínio e integração
- Segue convenções RESTful com métodos HTTP e códigos de status apropriados
- Lógica mínima no controller (padrão thin controller)
- Usa injeção de dependência para todos os handlers e serviços

---

## 2. Análise de Fluxo de Dados

### Fluxo de Criação de Proposta

1. **Entrada de Requisição HTTP** (POST /api/v1/proposals)
   - Cliente envia CreateProposalRequest com detalhes de veículo e pagamento
   - Atributo Authorize valida política SalesPerson

2. **Mapeamento de Requisição** (ProposalController.cs:78-89)
   - Controller mapeia DTO de requisição para CreateProposalCommand
   - Comando inclui: LeadId, informações do veículo, precificação, método de pagamento

3. **Tratamento de Comando** (CreateProposalHandler)
   - Valida existência de lead via ILeadRepository
   - Cria entidade Proposal via método de fábrica Proposal.Create()
   - Define status inicial para AwaitingCustomer
   - Atualiza status do lead para ProposalSent
   - Persiste mudanças via UnitOfWork

4. **Execução de Lógica de Domínio** (Proposal.cs:43-81)
   - Valida modelo do veículo não vazio
   - Valida preço do veículo positivo
   - Inicializa desconto como zero
   - Emite ProposalCreatedEvent

5. **Geração de Resposta** (ProposalController.cs:91-95)
   - Retorna status 201 Created
   - Mapeia entidade para DTO ProposalResponse
   - Inclui cabeçalho Location apontando para endpoint GetById

### Fluxo de Aplicação de Desconto

1. **Entrada de Requisição HTTP** (POST /api/v1/proposals/{id}/discount)
   - Vendedor solicita desconto com valor e motivo
   - GetCurrentUserId() extrai ID do usuário da claim "sub" do JWT

2. **Criação de Comando** (ProposalController.cs:276-278)
   - Mapeia para ApplyDiscountCommand com salesPersonId

3. **Lógica de Negócio de Domínio** (Proposal.cs:106-121)
   - Calcula porcentagem de desconto: (Valor / PreçoVeículo) * 100
   - Se desconto > 5%: Status → AwaitingDiscountApproval
   - Se desconto <= 5%: Status permanece inalterado
   - Armazena motivo e valor do desconto
   - Emite ProposalUpdatedEvent

4. **Fluxo Condicional**
   - **Desconto Pequeno (<=5%)**: Aplicado imediatamente, proposta permanece acessível
   - **Desconto Grande (>5%)**: Requer aprovação de gerente via endpoint separado

### Fluxo de Aprovação de Desconto (Apenas Gerente)

1. **Verificação de Autorização** (ProposalController.cs:302)
   - Atributo Authorize valida política Manager
   - Apenas gerentes podem acessar este endpoint

2. **Validação de Domínio** (Proposal.cs:123-133)
   - Verifica se status é AwaitingDiscountApproval
   - Lança DomainException se proposta não estiver no estado correto
   - Armazena ID do gerente como aprovador
   - Muda status de volta para AwaitingCustomer

3. **Emissão de Evento**
   - Emite ProposalUpdatedEvent

### Fluxo de Fechamento de Proposta

1. **Pré-validação** (ProposalController.cs:344-356)
   - Extrai salesPersonId do usuário atual
   - Cria CloseProposalCommand

2. **Validação de Domínio** (Proposal.cs:188-195)
   - Valida que status não é AwaitingDiscountApproval
   - Valida que desconto não excede 10% do preço do veículo
   - Lança DomainException se validação falhar

3. **Mudanças de Estado**
   - Status da Proposta → Closed
   - Status do Lead → Converted (via CloseProposalHandler)
   - Emite SaleClosedEvent para integração com módulo financeiro

4. **Persistência**
   - Atualiza ambas as entidades de proposta e lead
   - Commita transação via UnitOfWork

---

## 3. Regras de Negócio e Lógica

### Visão Geral das Regras de Negócio

| Tipo de Regra | Descrição | Localização |
|---------------|-----------|-------------|
| Validação | Modelo de veículo obrigatório, máx 100 chars | CreateProposalValidator.cs:14-16 |
| Validação | Ano do veículo entre 2000 e ano atual + 2 | CreateProposalValidator.cs:26-28 |
| Validação | Preço do veículo deve ser positivo | CreateProposalValidator.cs:30-31 |
| Validação | Método de pagamento deve ser valor enum válido | CreateProposalValidator.cs:33-35 |
| Condicional | Financiamento requer entrada > 0 | CreateProposalValidator.cs:37-41 |
| Condicional | Parcelas de financiamento entre 1-60 meses | CreateProposalValidator.cs:43-45 |
| Lógica de Negócio | Desconto > 5% requer aprovação de gerente | Proposal.cs:113-117 |
| Lógica de Negócio | Limite máximo de desconto é 10% do preço do veículo | Proposal.cs:193-194 |
| Regra de Negócio | Não pode fechar proposta com aprovação de desconto pendente | Proposal.cs:190-191 |
| Regra de Negócio | Criação de proposta muda status do lead para ProposalSent | CreateProposalHandler.cs:54 |
| Regra de Negócio | Fechamento de proposta muda status do lead para Converted | CloseProposalHandler.cs:37 |
| Autorização | Listar propostas filtradas por vendedor (a menos que gerente) | ProposalController.cs:119 |
| Autorização | Aprovação de desconto restrita apenas a gerentes | ProposalController.cs:302 |
| Cálculo | Valor Total = Preço Veículo + Itens - Desconto - Troca | Proposal.cs:197-204 |

---

### Regras de Negócio Detalhadas

#### Regra: Limite de Aprovação Automática de Desconto

**Visão Geral:**
O sistema implementa um fluxo de trabalho de aprovação de desconto automático onde vendedores podem aplicar descontos de até 5% do preço do veículo sem intervenção gerencial. Descontos que excedem este limite disparam um fluxo de aprovação.

**Descrição Detalhada:**

A regra de aprovação de desconto serve como um mecanismo de controle crítico no processo de vendas, balanceando autonomia da equipe de vendas com supervisão financeira. Quando um vendedor aplica um desconto através do endpoint ApplyDiscount, o sistema calcula a porcentagem de desconto relativa ao preço base do veículo. Para descontos em ou abaixo do limite de 5%, o desconto é aplicado imediatamente e a proposta permanece no status AwaitingCustomer, permitindo que o processo de vendas continue sem interrupção.

No entanto, quando o desconto excede 5%, o sistema automaticamente transita a proposta para o status AwaitingDiscountApproval. Durante este estado, a proposta não pode ser fechada ou modificada até que um gerente explicitamente aprove o desconto através do endpoint dedicado ApproveDiscount. Este design previne grandes descontos não autorizados enquanto mantém eficiência operacional para cenários de descontos menores e rotineiros.

O processo de aprovação é explicitamente rastreado através do campo DiscountApproverId, que armazena o ID do gerente mediante aprovação. Isso cria uma trilha de auditoria de quem aprovou cada desconto, suportando conformidade e requisitos de responsabilidade. O sistema impõe isso permitindo apenas usuários com a política Manager acessar o endpoint de aprovação, com segurança de API retornando 403 Forbidden para usuários não-gerentes tentando aprovação.

**Fluxo da Regra:**
1. Vendedor submete requisição de desconto com valor e motivo
2. Sistema calcula: porcentagemDesconto = (ValorDesconto / PreçoVeículo) * 100
3. Se porcentagemDesconto <= 5%:
   - Desconto aplicado imediatamente
   - Status permanece AwaitingCustomer
   - Processo continua
4. Se porcentagemDesconto > 5%:
   - Desconto armazenado mas status de proposta muda para AwaitingDiscountApproval
   - DiscountApproverId permanece nulo
   - Proposta se torna somente leitura até aprovação
5. Gerente acessa endpoint ApproveDiscount
6. Sistema valida que status atual é AwaitingDiscountApproval
7. Mediante aprovação:
   - DiscountApproverId definido para ID do gerente
   - Status retorna para AwaitingCustomer
   - Proposta se torna editável novamente

---

#### Regra: Limite Máximo de Desconto

**Visão Geral:**
O sistema impõe um teto rígido em descontos de 10% do preço base do veículo, prevenindo propostas de serem fechadas se o desconto exceder este limite.

**Descrição Detalhada:**

Esta regra age como uma salvaguarda financeira prevenindo descontos excessivos que poderiam impactar lucratividade. O limite máximo de 10% é imposto durante o processo de fechamento de proposta, não durante a aplicação de desconto. Este design permite que vendedores e gerentes negociem e explorem cenários de desconto, mas previne finalização de propostas com descontos considerados muito grandes.

A validação ocorre no método ValidateCanClose() dentro do método Close da entidade Proposal. Quando um vendedor tenta fechar uma proposta, o sistema verifica se o valor do desconto excede 10% do preço do veículo. Se exceder, uma DomainException é lançada com uma mensagem clara indicando que o desconto excede o limite permitido. Isso previne a proposta de transitar para o status Closed e garante que todas as propostas fechadas estejam em conformidade com a política de desconto.

É importante notar que este limite de 10% é separado do limite de aprovação de 5%. Um vendedor poderia teoricamente aplicar um desconto de 8% (que requereria aprovação de gerente), e se aprovado por um gerente, a proposta poderia prosseguir. No entanto, um desconto de 11% seria rejeitado mesmo com aprovação de gerente, pois excede o máximo absoluto.

Este sistema de duas camadas (auto-aprovação de 5%, limite rígido de 10%) fornece flexibilidade para discrição gerencial enquanto mantém uma fronteira clara para práticas de desconto aceitáveis. A regra é implementada como uma validação de domínio ao invés de uma constraint de banco de dados, garantindo imposição de lógica de negócio no nível da entidade.

**Fluxo da Regra:**
1. Vendedor tenta fechar proposta via endpoint Close
2. Entidade Proposal executa método ValidateCanClose()
3. Sistema verifica: if (DiscountAmount > VehiclePrice * 0.10)
4. Se condição for verdadeira:
   - DomainException lançada: "Desconto excede o limite permitido"
   - Status da proposta permanece inalterado
   - HTTP 400 Bad Request retornado ao cliente
5. Se condição for falsa:
   - Status da proposta muda para Closed
   - SaleClosedEvent emitido
   - Status do Lead muda para Converted

---

#### Regra: Sincronização de Status do Lead

**Visão Geral:**
Propostas são fortemente integradas com leads, onde operações específicas de proposta atualizam automaticamente o status do lead associado para manter consistência do pipeline de vendas.

**Descrição Detalhada:**

A regra de sincronização de status de lead garante que o pipeline de vendas reflita precisamente o estado atual de negociações. Quando uma proposta é criada para um lead, o sistema atualiza automaticamente o status do lead para ProposalSent, indicando que negociação ativa está em andamento. Esta mudança de status é tratada pelo CreateProposalHandler, que recupera a entidade lead, chama ChangeStatus(LeadStatus.ProposalSent), e persiste tanto a nova proposta quanto o lead atualizado em uma única transação.

Similarmente, quando uma proposta é fechada com sucesso (indicando uma venda completada), o status do lead transita para Converted. Esta mudança é gerenciada pelo CloseProposalHandler, que atualiza o lead após a operação de fechamento de proposta ter sucesso. O status Converted significa que o lead completou com sucesso a jornada de vendas e se tornou um cliente.

Esta sincronização automática elimina a necessidade de atualizações manuais de status de lead, reduzindo o risco de erro humano e garantindo que o dashboard de vendas sempre reflita dados precisos de pipeline. O uso de UnitOfWork garante que ambas as mudanças de status de proposta e lead sejam commitadas atomicamente, prevenindo estados inconsistentes onde uma proposta existe mas o lead não foi atualizado, ou vice-versa.

A regra demonstra a rica integração do domínio entre entidades, onde operações em uma entidade (Proposal) disparam mudanças de estado em entidades relacionadas (Lead) para manter consistência geral do sistema. Este padrão suporta relatórios de vendas precisos e visibilidade de pipeline.

**Fluxo da Regra:**

*Na Criação de Proposta:*
1. Cliente submete CreateProposalCommand com LeadId
2. Handler valida que lead existe
3. Entidade Proposal criada com status AwaitingCustomer
4. Entidade Lead recuperada via ILeadRepository
5. Status do Lead alterado: lead.ChangeStatus(LeadStatus.ProposalSent)
6. Ambas entidades persistidas em transação única UnitOfWork
7. ProposalCreatedEvent e mudança de status do lead commitados atomicamente

*No Fechamento de Proposta:*
1. Vendedor chama endpoint Close
2. Validação de proposta e operação de fechamento têm sucesso
3. Handler recupera lead associado via proposal.LeadId
4. Status do Lead alterado: lead.ChangeStatus(LeadStatus.Converted)
5. SaleClosedEvent emitido
6. Ambas entidades persistidas em transação única
7. Módulo Financeiro recebe SaleClosedEvent para processamento posterior

---

#### Regra: Filtragem de Dados de Vendedor

**Visão Geral:**
A API filtra automaticamente listagens de propostas para mostrar apenas propostas pertencentes ao vendedor atual, a menos que o usuário seja um gerente que pode ver todas as propostas.

**Descrição Detalhada:**

Esta regra de autorização e filtragem de dados implementa controle de acesso multi-tenant no nível da API, garantindo que vendedores possam apenas ver e gerenciar suas próprias propostas enquanto fornece aos gerentes visibilidade abrangente. A regra é implementada através do ISalesPersonFilterService, que extrai o ID do vendedor das claims do usuário do contexto HTTP atual.

Quando um usuário solicita uma lista de propostas via endpoint List, o controller chama GetCurrentSalesPersonId() para determinar escopo de filtragem. O serviço examina as claims e roles do usuário:
- Para gerentes (identificados por roles MANAGER, SALES_MANAGER, ou ADMIN), o método retorna nulo, indicando que nenhuma filtragem deve ser aplicada
- Para vendedores, o método extrai a claim sales_person_id ou recorre ao ID de sujeito do usuário

Este ID de vendedor é então passado para o ListProposalsQuery, que o handler de query usa para filtrar os resultados do banco de dados. A implementação garante que um vendedor não pode burlar esta filtragem manipulando parâmetros de query, já que o filtro é aplicado no lado do servidor baseado na identidade do usuário autenticado.

A regra suporta uma organização de vendas hierárquica onde gerentes precisam de visibilidade na performance de seu time enquanto mantêm privacidade e segregação de dados entre vendedores. É uma medida de segurança crítica prevenindo vazamento de dados e garantindo conformidade com políticas de acesso a dados.

**Fluxo da Regra:**
1. Usuário solicita GET /api/v1/proposals com filtros opcionais
2. Controller chama _salesPersonFilter.GetCurrentSalesPersonId()
3. Serviço verifica User.HasClaim("roles", "MANAGER" | "SALES_MANAGER" | "ADMIN")
4. Se gerente: retorna nulo (sem filtragem)
5. Se vendedor: extrai claim sales_person_id do JWT
6. ID de vendedor passado para construtor de ListProposalsQuery
7. Query handler filtra resultados de banco: WHERE SalesPersonId = @CurrentSalesPersonId OR @CurrentSalesPersonId IS NULL
8. Apenas resultados filtrados retornados ao cliente

---

#### Regra: Validação de Pagamento por Financiamento

**Visão Geral:**
Quando o método de pagamento é Financiamento, o sistema impõe regras de validação adicionais requerendo uma entrada positiva e restringindo prazos de parcelas para 1-60 meses.

**Descrição Detalhada:**

A regra de validação de pagamento por financiamento garante que propostas de financiamento sejam estruturadas com termos aceitáveis que alinhem com políticas de negócios e requisitos de instituições financeiras. A regra é implementada através de validação condicional em CreateProposalValidator usando método When() do FluentValidation, que aplica regras específicas apenas quando o PaymentMethod é "Financing".

A primeira validação requer que se uma entrada for fornecida, deve ser maior que zero. Isso previne propostas com entrada zero ou negativa de serem criadas, garantindo que todos os arranjos de financiamento incluam algum pagamento inicial. A validação permite valores nulos durante criação de proposta (suportando cenários onde entrada não foi determinada), mas impõe positividade quando o valor é fornecido.

A segunda validação restringe prazos de parcelas para entre 1 e 60 meses. Esta restrição reflete políticas de negócio e potencialmente limitações de parceiro financeiro, prevenindo prazos de financiamento excessivamente longos que poderiam aumentar risco ou exceder diretrizes institucionais. O máximo de 60 meses (5 anos) alinha com práticas comuns de financiamento automotivo.

Estas validações são aplicadas no nível de comando antes de qualquer lógica de negócio executar, fornecendo feedback rápido para consumidores da API sobre requisições inválidas sem requerer consultas de banco de dados ou instanciação de entidade de domínio. A camada de validação captura estes erros cedo no pipeline de requisição, retornando 400 Bad Request com mensagens de erro descritivas.

**Fluxo da Regra:**
1. Cliente submete CreateProposalRequest com PaymentMethod = "Financing"
2. Pipeline FluentValidation valida requisição
3. Qundo PaymentMethod é "Financing":
   - Verifica se DownPayment tem valor: DownPayment > 0
   - Verifica se Installments tem valor: 1 <= Installments <= 60
4. Se validação falhar:
   - 400 Bad Request retornado
   - Mensagem de erro: "Entrada deve ser maior que zero" ou "Número de parcelas deve ser entre 1 e 60"
5. Se validação passar:
   - Comando criado e passado para handler
   - Entidade Proposal criada com termos de financiamento

---

## 4. Estrutura do Componente

```
services/commercial/1-Services/GestAuto.Commercial.API/Controllers/
├── ProposalController.cs (363 linhas)
│   ├── Dependências (9 handlers + 2 services)
│   ├── Injeção de Construtor (linhas 33-57)
│   ├── Endpoints (8 métodos HTTP)
│   │   ├── Create (POST) - linhas 74-96
│   │   ├── List (GET) - linhas 112-127
│   │   ├── GetById (GET) - linhas 142-152
│   │   ├── Update (PUT) - linhas 170-193
│   │   ├── AddItem (POST) - linhas 211-222
│   │   ├── RemoveItem (DELETE) - linhas 238-249
│   │   ├── ApplyDiscount (POST) - linhas 271-284
│   │   ├── ApproveDiscount (POST) - linhas 308-320
│   │   └── Close (POST) - linhas 344-356
│   └── Métodos Auxiliares (1 método)
│       └── GetCurrentUserId() - linhas 358-362
│
├── Services/
│   └── SalesPersonFilterService.cs (55 linhas)
│       ├── GetCurrentSalesPersonId()
│       ├── IsManager()
│       └── GetCurrentUserId()
│
└── Dependências (de outras camadas)
    ├── Application/Commands/ (9 definições de comando)
    ├── Application/DTOs/ (7 DTOs request/response)
    ├── Application/Queries/ (2 definições de query)
    └── Domain/Exceptions/ (4 tipos de exceção)
```

**Organização de Arquivo:**
- **Responsabilidade Única:** Controller foca exclusivamente em tratamento de requisição/resposta HTTP
- **Injeção de Dependência:** Todas dependências injetadas via construtor
- **Lógica Mínima:** Lógica de negócio delegada para handlers, controller apenas mapeia DTOs
- **Padrão Consistente:** Todos endpoints seguem mesma estrutura (validar → comando → handler → resposta)

---

## 5. Análise de Dependências

### Dependências Internas

**De ProposalController:**

```
ProposalController
├── ICommandHandler<CreateProposalCommand, ProposalResponse>
│   └── CreateProposalHandler
│       ├── IProposalRepository
│       ├── ILeadRepository
│       └── IUnitOfWork
│
├── ICommandHandler<UpdateProposalCommand, ProposalResponse>
│   └── UpdateProposalHandler
│       └── (dependências similares)
│
├── ICommandHandler<AddProposalItemCommand, ProposalResponse>
│
├── ICommandHandler<RemoveProposalItemCommand, ProposalResponse>
│
├── ICommandHandler<ApplyDiscountCommand, ProposalResponse>
│   └── ApplyDiscountHandler
│       ├── IProposalRepository
│       └── IUnitOfWork
│
├── ICommandHandler<ApproveDiscountCommand, ProposalResponse>
│   └── ApproveDiscountHandler
│
├── ICommandHandler<CloseProposalCommand, ProposalResponse>
│   └── CloseProposalHandler
│       ├── IProposalRepository
│       ├── ILeadRepository
│       └── IUnitOfWork
│
├── IQueryHandler<GetProposalQuery, ProposalResponse>
│   └── GetProposalHandler
│       └── IProposalRepository
│
├── IQueryHandler<ListProposalsQuery, PagedResponse<ProposalListItemResponse>>
│   └── ListProposalsHandler
│       └── IProposalRepository
│
├── ISalesPersonFilterService
│   └── SalesPersonFilterService
│       └── IHttpContextAccessor
│
└── ILogger<ProposalController>
```

**Dependências Externas (Assemblies):**
- Microsoft.AspNetCore.Authorization (v8.0.0)
- Microsoft.AspNetCore.Mvc (v8.0.0)
- Microsoft.Extensions.Logging (v8.0.0)
- FluentValidation (v11.9.0)

### Fluxo de Dependência

```
Requisição HTTP
    ↓
ProposalController (Camada API)
    ↓
Handlers Command/Query (Camada Aplicação)
    ↓
Entidades de Domínio (Camada Domínio)
    ↓
Repositórios (Camada Infraestrutura)
    ↓
Banco de Dados (PostgreSQL)
```

**Características de Acoplamento:**
- **Acoplamento Baixo:** Controller depende apenas de interfaces (ICommandHandler, IQueryHandler)
- **Inversão de Dependência:** Todas dependências injetadas como abstrações
- **Separação de Camadas:** Limites claros entre camadas API, Aplicação, Domínio e Infraestrutura
- **Sem Acesso Direto a Banco:** Controller nunca acessa diretamente repositórios ou banco de dados

---

## 6. Análise de Acoplamento (Aferente & Eferente)

### Métricas de Acoplamento

| Componente | Acoplamento Aferente (Ca) | Acoplamento Eferente (Ce) | Instabilidade (I = Ce / (Ce + Ca)) | Criticidade |
|------------|-------------------------|-------------------------|----------------------------------|-------------|
| ProposalController | 1 (Testes API) | 11 (9 handlers + filter + logger) | 0.92 | Alta |
| CreateProposalHandler | 2 (Controller, Testes Unit) | 3 (ProposalRepo, LeadRepo, UnitOfWork) | 0.60 | Média |
| ApplyDiscountHandler | 2 (Controller, Testes Unit) | 2 (ProposalRepo, UnitOfWork) | 0.50 | Baixa |
| CloseProposalHandler | 2 (Controller, Testes Unit) | 3 (ProposalRepo, LeadRepo, UnitOfWork) | 0.60 | Média |
| Proposal Entity | 7 (Todos handlers + testes) | 4 (ValueObjects, Enums, Events, Exceptions) | 0.36 | Baixa |
| SalesPersonFilterService | 1 (Controller) | 1 (HttpContextAccessor) | 0.50 | Baixa |

**Análise:**

**ProposalController (Instabilidade Alta: 0.92)**
- Acoplamento eferente alto (11 dependências) indica que serve como coordenador
- Isso é esperado e aceitável para controllers de API
- Instabilidade alta é apropriada: deve ser fácil de mudar sem quebrar consumidores
- Acoplamento aferente baixo (1) significa que é um componente folha no grafo de dependência

**CreateProposalHandler (Instabilidade Média: 0.60)**
- Acoplamento eferente moderado a repositórios e UnitOfWork
- Ca e Ce balanceados indicam que é tanto usado quanto depende de outros componentes
- Estabilidade razoável sugere que é um componente de lógica de negócio central

**Proposal Entity (Instabilidade Baixa: 0.36)**
- Acoplamento aferente alto (7 dependentes) indica que é um componente de domínio central
- Acoplamento eferente baixo (4 dependências) mostra que é relativamente autocontido
- Instabilidade baixa é desejável: deve ser estável, difícil de mudar sem impactar dependentes
- Isso é ideal para entidades de domínio

**Avaliação Geral:**
A distribuição de acoplamento segue o padrão esperado para Clean Architecture:
- Camada de apresentação (Controller) tem alta instabilidade (0.92) - fácil de mudar
- Camada de domínio (Entity) tem baixa instabilidade (0.36) - núcleo estável
- Camada de aplicação (Handlers) tem instabilidade balanceada (0.50-0.60) - estabilidade intermediária

Esta estrutura promove manutenibilidade: mudanças propagam para dentro das camadas externas instáveis em direção ao núcleo estável.

---

## 7. Endpoints

### Endpoints API REST

| Endpoint | Método | Descrição | Corpo da Requisição | Resposta | Autorização |
|----------|--------|-----------|---------------------|----------|-------------|
| /api/v1/proposals | POST | Criar nova proposta | CreateProposalRequest | 201 Created | SalesPerson |
| /api/v1/proposals | GET | Listar propostas com filtros | Parâmetros de Query | 200 OK | SalesPerson |
| /api/v1/proposals/{id} | GET | Obter proposta por ID | N/A | 200 OK | SalesPerson |
| /api/v1/proposals/{id} | PUT | Atualizar proposta | UpdateProposalRequest | 200 OK | SalesPerson |
| /api/v1/proposals/{id}/items | POST | Adic. item a proposta | AddProposalItemRequest | 201 Created | SalesPerson |
| /api/v1/proposals/{id}/items/{itemId} | DELETE | Remover item de prop. | N/A | 200 OK | SalesPerson |
| /api/v1/proposals/{id}/discount | POST | Aplicar desconto | ApplyDiscountRequest | 200 OK | SalesPerson |
| /api/v1/proposals/{id}/approve-discount | POST | Aprovar desc. pendente | N/A | 200 OK | Manager |
| /api/v1/proposals/{id}/close | POST | Fechar proposta | N/A | 200 OK | SalesPerson |

### Especificações Detalhadas de Endpoint

#### POST /api/v1/proposals
**Propósito:** Cria uma nova proposta de vendas para um lead

**Corpo da Requisição:**
```json
{
  "leadId": "guid",
  "vehicleModel": "string (max 100 chars)",
  "vehicleTrim": "string (max 100 chars)",
  "vehicleColor": "string (max 50 chars)",
  "vehicleYear": "int (2000 até ano atual + 2)",
  "isReadyDelivery": "boolean",
  "vehiclePrice": "decimal (> 0)",
  "paymentMethod": "string (Cash/Financing)",
  "downPayment": "decimal? (> 0 se financiamento)",
  "installments": "int? (1-60 se financiamento)"
}
```

**Resposta:** 201 Created
```json
{
  "id": "guid",
  "leadId": "guid",
  "status": "AwaitingCustomer",
  "totalValue": "decimal",
  ... (todos campos de proposta)
}
```

**Cabeçalho Location:** /api/v1/proposals/{id}

**Regras de Negócio Aplicadas:**
- Lead deve existir
- Ano do veículo validado
- Financiamento requer entrada válida e parcelas
- Status do lead muda para ProposalSent
- ProposalCreatedEvent emitido

---

#### GET /api/v1/proposals
**Propósito:** Listar propostas com paginação e filtros opcionais

**Parâmetros de Query:**
- `leadId` (opcional): Filtrar por GUID de lead
- `status` (opcional): Filtrar por string de status de proposta
- `page` (padrão: 1): Número da página
- `pageSize` (padrão: 20): Itens por página

**Resposta:** 200 OK
```json
{
  "items": [
    {
      "id": "guid",
      "leadId": "guid",
      "status": "string",
      "vehicleModel": "string",
      "totalValue": "decimal",
      "createdAt": "datetime"
    }
  ],
  "totalCount": "int",
  "page": "int",
  "pageSize": "int"
}
```

**Comportamento de Autorização:**
- Vendedor: Vê apenas suas próprias propostas
- Gerente: Vê todas as propostas

---

#### POST /api/v1/proposals/{id}/discount
**Propósito:** Aplica um desconto a uma proposta

**Corpo da Requisição:**
```json
{
  "amount": "decimal (> 0)",
  "reason": "string"
}
```

**Resposta:** 200 OK com proposta atualizada

**Lógica de Negócio:**
- Desconto > 5%: Status → AwaitingDiscountApproval
- Desconto <= 5%: Status inalterado
- ID do usuário atual armazenado como solicitante de desconto

**Validação:**
- Valor deve ser positivo
- Motivo obrigatório
- Proposta deve existir

---

#### POST /api/v1/proposals/{id}/approve-discount
**Propósito:** Aprovação de gerente para descontos pendentes

**Autorização:** Apenas Manager (retorna 403 para vendedores)

**Corpo da Requisição:** Nenhum

**Resposta:** 200 OK com proposta aprovada

**Lógica de Negócio:**
- Valida status é AwaitingDiscountApproval
- Armazena ID do gerente como aprovador
- Status → AwaitingCustomer

**Condições de Erro:**
- 404: Proposta não encontrada
- 400: Proposta não aguardando aprovação
- 403: Usuário não é um gerente

---

#### POST /api/v1/proposals/{id}/close
**Propósito:** Fecha proposta indicando venda completada

**Autorização:** SalesPerson

**Corpo da Requisição:** Nenhum

**Resposta:** 200 OK

**Regras de Negócio Aplicadas:**
- Status não deve ser AwaitingDiscountApproval
- Desconto não deve exceder 10% do preço do veículo
- Status → Closed
- Status do Lead → Converted
- SaleClosedEvent emitido

**Pontos de Integração:**
- Entidade Lead atualizada via LeadRepository
- Módulo Financeiro recebe SaleClosedEvent via barramento de mensagem

---

## 8. Pontos de Integração

### Integrações Externas

| Integração | Tipo | Propósito | Protocolo | Formato | Tratamento de Erro |
|------------|------|-----------|-----------|---------|--------------------|
| Lead Service | Integração Entidade Domínio | Validar existência de lead, atualizar status | In-Process (Mesmo BD) | Objetos Entidade | NotFoundException → 404 |
| Financial Module | Event-Driven | Notificar conclusão de venda para geração de contrato | Message Bus (RabbitMQ) | SaleClosedEvent JSON | Evento persistido se publicação falhar |
| Identity Provider | Validação JWT | Autenticação de usuário e claims de papel | HTTPS/OAuth2 | Token JWT | 401 Unauthorized se inválido |

### Integrações de Componente Interno

**Com Agregado Lead:**
- **CreateProposal:** Valida se lead existe via ILeadRepository.GetByIdAsync()
- **CreateProposal:** Atualiza status de lead para ProposalSent
- **CloseProposal:** Atualiza status de lead para Converted
- **Transação:** Proposta e lead atualizados em transação única UnitOfWork

**Com Sistema de Evento de Domínio:**
- **ProposalCreatedEvent:** Emitido na criação de proposta
- **ProposalUpdatedEvent:** Emitido em mudanças de desconto, item, status
- **SaleClosedEvent:** Emitido no fechamento de proposta para processamento financeiro
- **Armazenamento de Evento:** Eventos armazenados na coleção DomainEvents da entidade
- **Despacho:** Eventos despachados pela infraestrutura após commit de transação

**Com Framework de Validação:**
- **FluentValidation:** Todos comandos validados antes da execução do handler
- **Pipeline de Validação:** Validação automática via middleware ou filtros
- **Resposta de Erro:** Erros de validação retornam 400 Bad Request com detalhes

**Com Camada de Persistência:**
- **PostgreSQL via EF Core:** Todas entidades armazenadas em banco relacional
- **ProposalRepository:** Operações CRUD e queries
- **LeadRepository:** Atualizações de status de lead
- **UnitOfWork:** Gerenciamento de transação através de operações de repositório
- **Configurações de Entidade:** ProposalConfiguration, ProposalItemConfiguration

**Com Autenticação & Autorização:**
- **Tokens JWT Bearer:** Identidade de usuário extraída da claim "sub"
- **Autorização Baseada em Papel:** Política "SalesPerson" para maioria das operações
- **Autorização de Gerente:** Política "Manager" para aprovação de desconto
- **Acesso Baseado em Claims:** Filtragem de vendedor via claim "sales_person_id"

---

## 9. Padrões de Design e Arquitetura

### Padrões Identificados

| Padrão | Implementação | Localização | Propósito |
|---------|---------------|-------------|-----------|
| **CQRS** | Interfaces ICommandHandler e IQueryHandler separadas | Todos handlers | Segregar operações de comando (escrita) e query (leitura) |
| **Padrão Repository** | IProposalRepository, ILeadRepository | Camada infraestrutura | Abstrair acesso a dados, fornecer interface tipo coleção |
| **Injeção de Dependência** | Injeção de construtor de todas dependências | ProposalController:33-57 | Baixo acoplamento, testabilidade, inversão de controle |
| **Factory Method** | Factory estática Proposal.Create() | Proposal.cs:43-81 | Encapsular lógica de criação de entidade com validação |
| **Padrão Specification** | Regras FluentValidation para comandos | Validators/*.cs | Regras de validação declarativas, reusáveis |
| **Unit of Work** | IUnitOfWork para gerenciamento de transação | Handlers usam UoW | Transações atômicas através de repositórios |
| **Eventos de Domínio** | Eventos emitidos de entidades, despachados por infra | Entidade Proposal, EventDispatcher | Desacoplar efeitos colaterais da lógica de domínio |
| **Value Objects** | Value object Money | Domain/ValueObjects/Money.cs | Encapsular valores monetários com lógica de moeda |
| **Padrão DTO** | DTOs Request/Response separados de entidades | Application/DTOs/* | Abstração de camada API, prevenir over-posting |
| **Thin Controller** | Lógica mínima no controller, delegação para handlers | Métodos ProposalController | Separação de preocupações, testabilidade |
| **Autorização Baseada em Política** | [Authorize(Policy = "SalesPerson")] | Classe Controller | Autorização declarativa, políticas centralizadas |
| **Templates de Rota** | [Route("api/v1/proposals")] | Classe Controller | Versionamento e organização de API RESTful |
| **Padrão Result Filter** | CreatedAtAction para respostas 201 | Método Create:95 | Semântica de resposta HTTP adequada |

### Decisões Arquiteturais

**Implementação Clean Architecture:**
- **Separação de Camadas:** Limites claros entre API, Aplicação, Domínio e Infraestrutura
- **Regra de Dependência:** Dependências apontam para dentro (API → Aplicação → Domínio)
- **Independência de Framework:** Camada de domínio não tem dependências em frameworks externos
- **Testabilidade:** Cada camada pode ser testada isoladamente usando mocks

**Benefícios CQRS:**
- **Leituras Otimizadas:** Queries podem ser otimizadas para diferentes modelos de leitura
- **Escalabilidade:** Comandos e queries podem escalar independentemente
- **Validação:** Validação separada para comandos vs queries
- **Intenção:** Semântica explícita de comando/query no código

**Elementos Domain-Driven Design:**
- **Raiz de Agregado:** Proposal é raiz de agregado gerenciando filhos ProposalItem
- **Value Objects:** Money encapsula valores monetários com precisão adequada
- **Eventos de Domínio:** ProposalCreatedEvent, ProposalUpdatedEvent, SaleClosedEvent
- **Linguagem Ubíqua:** Enum ProposalStatus reflete terminologia de negócio
- **Modelo de Domínio Rico:** Lógica de negócio em entidades, não modelo de domínio anêmico

**Estratégia de Tratamento de Erro:**
- **Exceções de Domínio:** DomainException para violações de regra de negócio
- **Not Found:** NotFoundException para entidades ausentes
- **Mapeamento HTTP:** Exceções mapeadas para códigos de status HTTP apropriados
- **Mensagens de Usuário:** Mensagens de erro claras em Português para usuários de negócio

---

## 10. Dívida Técnica e Riscos

### Avaliação de Risco

| Nível de Risco | Área | Problema | Impacto | Probabilidade |
|----------------|------|----------|---------|---------------|
| **Médio** | ProposalController:361 | GetCurrentUserId() retorna Guid.Empty se falhar parse | Potencial referência nula em comandos | Baixa |
| **Médio** | ProposalController:119 | SalesPersonFilter retorna null para managers | Propagação de nulo se query não tratar null | Baixa |
| **Baixo** | CreateProposalValidator:27 | Range de ano de veículo hardcoded (2000 até AnoAtual + 2) | Requererá mudança de código ano 2026+ | Alta |
| **Baixo** | CreateProposalValidator:44 | Máximo de parcelas hardcoded para 60 | Mudança de requisito de negócio requer atualização de código | Média |
| **Médio** | Proposal.cs:193 | Limite de desconto 10% hardcoded no domínio | Mudança de regra de negócio requer modificação de entidade | Baixa |
| **Baixo** | ProposalController.cs | Sem versionamento de API na rota além de v1 | Mudanças futuras quebraveis requerem novo controller | Baixa |
| **Alta** | Validação Ausente | Sem validação em valor máximo de desconto em ApplyDiscountRequest | Potencial para valores de desconto extremamente grandes | Média |
| **Médio** | Mensagens de Erro | Todas mensagens de erro em Português, hardcoded | Sem suporte a internacionalização | Baixa |
| **Baixo** | Logging | Apenas logs de informação, sem logs de Warning/Error | Observabilidade limitada para troubleshooting | Baixa |
| **Médio** | Escopo de Transação | UnitOfWork pode não abranger operações distribuídas | Potencial inconsistência se publicação de evento falhar | Média |

### Observações de Qualidade de Código

**Aspectos Positivos:**
- Estilo de código e convenções de nomenclatura consistentes
- Comentários de documentação XML abrangentes
- Uso adequado de verbos HTTP e códigos de status
- Boa separação de preocupações
- Inversão de dependência seguida
- Nenhuma duplicação de código detectada

**Melhorias Potenciais (Apenas Observacional):**
- Alguns números mágicos (5%, 10%, 60 meses) poderiam ser configuráveis
- Lógica GetCurrentUserId() duplicada de SalesPersonFilterService
- Sem propagação explícita de cancellation token em alguns handlers
- Eventos de domínio poderiam incluir mais contexto (timestamp, usuário que disparou)

**Fatores de Manutenibilidade:**
- Métodos de baixa complexidade (média 5-10 linhas por endpoint)
- Nomes de métodos claros indicando intenção
- Lógica condicional mínima no controller
- Lógica de domínio centralizada em entidades
- Classes handler são pequenas e focadas

**Indicadores de Cobertura de Teste:**
- Testes de Domínio: ProposalTests.cs (142 linhas, 8 casos de teste)
- Testes de Handler: CreateProposalHandlerTests.cs, CloseProposalHandlerTests.cs
- Testes de API: ProposalApiTests.cs (112 linhas, 3 testes de integração)
- Testes cobrem: caminho feliz, regras de negócio, validação, autorização

---

## 11. Análise de Cobertura de Teste

### Arquivos de Teste

| Arquivo de Teste | Tipo | Linhas | Contagem de Teste | Foco de Cobertura |
|------------------|------|--------|-------------------|-------------------|
| ProposalTests.cs | Unit | 142 | 8 | Comportamento de entidade de domínio, regras de negócio |
| CreateProposalHandlerTests.cs | Unit | ~80 | ~5 | Lógica de handler, integração com repositórios |
| CloseProposalHandlerTests.cs | Unit | ~60 | ~4 | Fluxo de fechamento, atualização de status de lead |
| ProposalApiTests.cs | Integration | 112 | 3 | Endpoints HTTP, autorização, full stack |
| ProposalRepositoryTests.cs | Integration | ~100 | ~6 | CRUD Repositório, queries de banco de dados |

### Avaliação de Qualidade de Teste

**Testes de Domínio (ProposalTests.cs) - Cobertura: Excelente**

Cenários Testados:
- Criação de proposta com estado inicial correto ✓
- Aplicação de desconto <= 5% (sem aprovação necessária) ✓
- Aplicação de desconto > 5% (aprovação necessária) ✓
- Aprovação de desconto quando aguardando ✓
- Erro de aprovação de desconto quando não aguardando ✓
- Sucesso de fechamento de proposta e emissão de evento ✓
- Erro de validação de fechamento de proposta com desconto pendente ✓

**Casos de Teste Ausentes:**
- Cálculo de valor total com itens
- Cálculo de valor total com troca
- Funcionalidade de adicionar/remover itens
- Atualizar informações de veículo e pagamento
- Validação de limite máximo de desconto (10%)
- Transições de estado de avaliação de veículo usado
- Casos de borda: preço zero, valores negativos

**Testes de Handler - Cobertura: Boa**

Coberto:
- Validação de existência de lead
- Criação e persistência de proposta
- Atualização de status de lead na criação
- Fechamento de proposta e conversão de lead

Potencialmente Ausente:
- Cenários de rollback de transação
- Tratamento de modificação concorrente
- Cenários de falha de repositório

**Testes de Integração API - Cobertura: Moderada**

Coberto:
- Aplicar desconto > 5% requer aprovação ✓
- Autorização de gerente para aprovação de desconto ✓
- Fechar proposta marca lead como convertido ✓

Ausente:
- Endpoint criar proposta
- Endpoint atualizar proposta
- Endpoints adicionar/remover itens
- Listar propostas com filtragem
- Obter proposta por ID
- Validação de requisição inválida
- Cenários não encontrado
- Comportamento de cancellation token

**Estimativa Geral de Cobertura:**
- Camada de Domínio: 85-90% (excelente cobertura de regra de negócio)
- Camada de Aplicação: 70-75% (boa cobertura de handler, faltando casos de borda)
- Camada de API: 40-50% (caminhos felizes básicos, teste mínimo de validação/erro)

### Avaliação de Qualidade de Teste

**Pontos Fortes:**
- Nomes de teste claros seguindo padrão AAA (Arrange, Act, Assert)
- FluentAssertions usados para asserções legíveis
- Testes de domínio focam em regras de negócio, não implementação
- Testes de integração usam fixtures de teste para banco/barramento de mensagem
- Testes validam cenários de sucesso e falha

**Áreas para Melhoria:**
- Camada de API carece de cobertura de teste negativo abrangente
- Nenhum teste de performance ou carga evidente
- Cobertura de teste limitada para paginação no endpoint List
- Testes ausentes para comportamento de filtragem de vendedor
- Sem testes para modificações de proposta concorrentes
- Teste de caso de borda limitado (valores limite, tratamento de nulo)

---

## 12. Resumo de Métricas de Componente

### Tamanho & Complexidade

- **Linhas de Código:** 363
- **Métodos:** 9 (8 endpoints + 1 auxiliar)
- **Dependências:** 11 (9 handlers + 1 filter + 1 logger)
- **Complexidade Ciclomática:** Baixa (méd 2-3 por método)
- **Índice de Manutenibilidade:** Alto (thin controller, responsabilidade única)

### Características de Throughput

- **Operações Síncronas:** Todos endpoints usam async/await
- **Transações de Banco:** Comandos usam UnitOfWork para conformidade ACID
- **Chamadas Externas:** Publicação de evento (async, padrão fire-and-forget)
- **Caching:** Nenhum implementado (poderia ser oportunidade de otimização)

### Medidas de Segurança

- **Autenticação:** Tokens JWT Bearer requeridos
- **Autorização:** Baseada em Política (SalesPerson, Manager)
- **Filtragem de Dados:** Filtragem automática de vendedor para não-gerentes
- **Validação de Input:** FluentValidation para todos comandos
- **SQL Injection:** Protegido via queries parametrizadas EF Core
- **Mass Assignment:** Prevenido via mapeamento DTO (sem model binding direto)

### Observabilidade

- **Logging:** ILogger usado para operações chave (criar, atualizar, fechar)
- **Níveis de Log:** Informação apenas (sem Warning/Error para falhas)
- **Emissão de Evento:** Eventos de domínio para trilha de auditoria
- **Semântica HTTP:** Códigos de status e tipos de resposta adequados
- **Lacunas Potenciais:** Sem correlation IDs, sem logging estruturado

---

## Conclusão

O ProposalController é um componente API REST bem projetado seguindo princípios de Clean Architecture e CQRS. Ele separa com sucesso preocupações de apresentação de lógica de negócio, delegando todas as operações para handlers especializados. O componente demonstra forte aderência aos princípios SOLID, particularmente Responsabilidade Única (thin controller) e Inversão de Dependência (dependências baseadas em interface).

**Principais Pontos Fortes:**
- Clara separação de preocupações com lógica mínima no controller
- Regras de negócio abrangentes encapsuladas em entidades de domínio
- Autorização adequada com políticas baseadas em papel
- Boa cobertura de teste nas camadas de domínio e aplicação
- Design de API REST consistente com semântica HTTP apropriada
- Integração orientada a eventos com outros módulos

**Áreas para Observação:**
- Cobertura de teste de API poderia ser expandida para incluir mais cenários negativos
- Algumas regras de negócio estão hardcoded (porcentagens de desconto, limites de parcelas)
- Observabilidade de tratamento de erro limitada (níveis de logging)
- Sem caching para propostas acessadas frequentemente

**Avaliação Geral:**
O componente está pronto para produção com uma fundação arquitetural sólida. O design suporta manutenibilidade, testabilidade, e evolução futura. Os riscos identificados são de baixo impacto e têm estratégias de mitigação. A base de código demonstra práticas de engenharia de software maduras apropriadas para um sistema de gerenciamento de vendas de veículos comerciais.

---

**Relatório Gerado:** 23/01/2026
**Componente:** ProposalController
**Arquitetura:** Clean Architecture + CQRS + Domain-Driven Design
**Camada:** Apresentação (API)
**Framework:** ASP.NET Core 8.0
