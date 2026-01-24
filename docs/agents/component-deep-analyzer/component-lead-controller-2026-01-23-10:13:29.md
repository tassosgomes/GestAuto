# Relatório de Análise Profunda de Componente - LeadController

**Gerado**: 23/01/2026 10:13:29
**Componente**: LeadController
**Localização**: `services/commercial/1-Services/GestAuto.Commercial.API/Controllers/LeadController.cs`
**Linhas de Código**: 335

---

## 1. Resumo Executivo

O LeadController é um controller de API na Camada de Apresentação do serviço Comercial responsável por gerenciar leads e interações com clientes. Ele expõe 8 endpoints REST para operações CRUD em leads, incluindo criação de lead, qualificação, gerenciamento de status e rastreamento de interação. O controller segue princípios de Clean Architecture com clara separação entre camadas de API, Aplicação e Domínio. Ele implementa autorização baseada em papel restringindo acesso a vendedores e gerentes, com lógica de filtragem sofisticada para impor limites de visibilidade de dados.

**Principais Descobertas**:
- Controller bem estruturado seguindo padrão CQRS com handlers de comando/query separados
- Implementa autorização adequada com requisito de política SalesPerson
- Contém lógica sofisticada de filtragem de vendedor para acesso a dados baseado em papel
- Carece de testes unitários dedicados para o próprio controller
- Dependência direta de 8 handlers e 1 serviço através de injeção de construtor
- Logging apropriado implementado para todas as operações
- Design de API RESTful com verbos HTTP e códigos de status apropriados

---

## 2. Análise de Fluxo de Dados

O fluxo completo de requisição-para-resposta através do LeadController:

```
1. Requisição HTTP chega ao endpoint LeadController
   ↓
2. Filtro de Autorização valida política "SalesPerson"
   ↓
3. SalesPersonFilterService extrai contexto de usuário (salesPersonId, role manager)
   ↓
4. DTOs de requisição são desserializados (CreateLeadRequest, UpdateLeadRequest, etc.)
   ↓
5. Controller cria objeto Command ou Query com contexto extraído
   - CreateLeadCommand, QualifyLeadCommand, ChangeLeadStatusCommand, etc.
   - GetLeadQuery, ListLeadsQuery, ListInteractionsQuery
   ↓
6. Command/Query é passado para handler apropriado via interface ICommandHandler/IQueryHandler
   - CreateLeadHandler, QualifyLeadHandler, UpdateLeadHandler, etc.
   - GetLeadHandler, ListLeadsHandler, ListInteractionsHandler
   ↓
7. Handler executa lógica de negócio:
   - Validação de entidade de domínio (Value Objects Email, Phone)
   - Parsing de Enum com matching case-insensitive
   - Invocações de método de entidade de domínio (Lead.Create, Lead.Qualify, etc.)
   - Operações de repositório (AddAsync, UpdateAsync, GetByIdAsync)
   - Commit de transação UnitOfWork
   ↓
8. Eventos de domínio podem ser disparados (LeadCreatedEvent, LeadScoredEvent, LeadStatusChangedEvent)
   ↓
9. Handler retorna DTO de Resposta (LeadResponse, InteractionResponse, PagedResponse)
   ↓
10. Controller loga resultado da operação
   ↓
11. DTO de Resposta é serializado para JSON e retornado com código de status HTTP apropriado:
    - 201 Created para criação de recurso
    - 200 OK para operações bem-sucedidas
    - 404/400 tratados por ExceptionHandlerMiddleware para casos de erro
```

---

## 3. Regras de Negócio e Lógica

### Visão Geral das Regras de Negócio

| Tipo de Regra | Descrição | Localização |
|---------------|-----------|-------------|
| Autorização | Apenas usuários com papel SalesPerson podem acessar endpoints | LeadController.cs:17 |
| Atribuição de Vendedor | Leads devem ser atribuídos a um ID de vendedor válido | LeadController.cs:73-83 |
| Privilégio de Gerente | Gerentes podem contornar filtragem de vendedor e acessar todos os leads | LeadController.cs:131-134 |
| Status Inicial de Lead | Novos leads automaticamente começam com status "New" | Lead.cs:47 |
| Pontuação Inicial de Lead | Novos leads automaticamente começam com pontuação "Bronze" | Lead.cs:48 |
| Qualificação de Lead | Qualificar um lead muda automaticamente status para "InNegotiation" | Lead.cs:64 |
| Cálculo de Pontuação | Pontuação do lead é recalculada quando dados de qualificação são fornecidos | Lead.cs:62 |
| Validação de Email | Email deve ter formato válido (Value Object Email) | Email.cs |
| Validação de Telefone | Telefone deve corresponder ao formato (XX) XXXXX-XXXX (Value Object Phone) | Phone.cs |
| Timestamp de Interação | Interações registram quando ocorreram, não apenas quando criadas | LeadController.cs:297 |
| Paginação | Operações de listagem suportam paginação com tamanho de página configurável | LeadController.cs:124-125 |

---

### Regras de Negócio Detalhadas

---

#### **Regra de Negócio: Autorização e Controle de Acesso**

**Visão Geral**:
O LeadController impõe controle de acesso baseado em papel garantindo que apenas usuários autenticados com papéis apropriados possam gerenciar leads. O mecanismo de autorização opera em dois níveis: autorização em nível de endpoint via políticas ASP.NET Core, e filtragem em nível de dados baseada em papéis de usuário.

**Descrição**:
Todos os endpoints no LeadController são protegidos pela política de autorização "SalesPerson", que requer que usuários sejam autenticados e tenham um papel relacionado a vendas. O controller implementa um modelo de controle de acesso de duas camadas:

1. **Vendedores**: Podem apenas acessar e gerenciar leads atribuídos ao seu próprio ID de vendedor. Sua visão dos dados é filtrada para incluir apenas seus leads.

2. **Gerentes**: Têm privilégios elevados que permitem contornar o filtro de vendedor. Gerentes podem ver todos os leads no sistema e podem opcionalmente filtrar por IDs de vendedor específicos ao consultar a lista de leads.

A lógica de autorização é implementada através do SalesPersonFilterService, que extrai claims do token JWT e determina o papel do usuário. O serviço verifica claims de papel incluindo "MANAGER", "SALES_MANAGER", e "ADMIN" para determinar status de gerente. Para vendedores, ele extrai a claim "sales_person_id" ou recorre à claim "sub" (subject) que contém o ID do usuário.

**Fluxo da Regra**:
1. Requisição HTTP chega com token bearer JWT no header Authorization
2. Middleware de autenticação ASP.NET Core valida token e cria ClaimsPrincipal
3. Filtro de autorização verifica requisito de política "SalesPerson"
4. Na execução do endpoint, SalesPersonFilterService.GetCurrentSalesPersonId() extrai ID de vendedor das claims
5. SalesPersonFilterService.IsManager() verifica se usuário tem claims de papel de gerente
6. Para não-gerentes: operações são implicitamente filtradas por ID de vendedor
7. Para gerentes: filtro de vendedor é contornado ou tornado opcional
8. Comandos/Queries são criados com contexto de vendedor apropriado
9. Handlers impõem acesso a dados em nível de repositório baseado no ID de vendedor fornecido

**Pontos de Validação**:
- Linhas 73-83 (Endpoint Create): Valida que um ID de vendedor válido pode ser extraído, lança UnauthorizedException se ausente
- Linhas 128-134 (Endpoint List): Implementa lógica de privilégio de gerente para contornar filtragem de vendedor
- Linhas 169-171 (Endpoint GetById): Passa ID de vendedor para query handler para filtragem de dados
- Linhas 75-78 (Endpoint Create): Tratamento especial para gerentes criando leads sem atribuição de vendedor

**Condições de Erro**:
- UnauthorizedException lançada quando ID de vendedor não pode ser determinado (linha 82)
- 401 Unauthorized retornado por ExceptionHandlerMiddleware para autenticação ausente ou inválida
- 403 Forbidden retornado por política de autorização se usuário não tem papéis necessários

---

#### **Regra de Negócio: Criação e Atribuição de Lead**

**Visão Geral**:
Ao criar um novo lead, o sistema garante que o lead seja adequadamente atribuído a um vendedor e inicializado com valores padrão de status e pontuação. O processo de criação valida dados de entrada e estabelece o estado inicial da entidade lead.

**Descrição**:
A criação de lead requer informações obrigatórias do cliente incluindo nome, email, telefone e fonte. Dados opcionais de interesse em veículo (modelo, versão, cor) também podem ser fornecidos. Após criação, o lead é automaticamente atribuído com status "New" e pontuação "Bronze" como valores padrão. O lead deve ser associado a um ID de vendedor válido extraído do contexto do usuário atual.

O processo de criação segue um padrão factory através do método estático Lead.Create, que encapsula as regras de negócio para inicializar uma entidade lead. Os Value Objects Email e Phone fornecem validação para seus respectivos formatos de dados. Se informação de interesse em veículo for fornecida, ela é definida via método UpdateInterest após criação.

Para vendedores, o lead criado é automaticamente atribuído ao seu ID de vendedor. Para gerentes, o sistema permite criar leads para si mesmos (usando seu ID de usuário como ID de vendedor) ou para outros vendedores (embora a implementação atual atribua ao ID de usuário do gerente).

**Fluxo da Regra**:
1. POST /api/v1/leads recebe CreateLeadRequest com dados do cliente
2. SalesPersonFilterService extrai ID de vendedor do usuário atual
3. Se ID de vendedor é nulo e usuário é gerente, usa ID do usuário atual
4. Valida que um ID de vendedor válido (não vazio) foi obtido
5. Cria CreateLeadCommand com dados da requisição e ID de vendedor
6. Command handler cria Value Objects Email e Phone (validando formato)
7. Parse de enum LeadSource de string (case-insensitive)
8. Invoca método factory Lead.Create com parâmetros fornecidos
9. Entidade Lead inicializa com padrões Status=New, Score=Bronze
10. Se dados de interesse em veículo fornecidos, chama lead.UpdateInterest()
11. Repositório persiste o novo lead via AddAsync
12. UnitOfWork commita transação
13. Domínio dispara LeadCreatedEvent
14. LeadResponse DTO retornado com header Location apontando para endpoint GetById

**Pontos de Validação**:
- Linha 80-83: Valida ID vendas não nulo/vazio, lança UnauthorizedException se inválido
- Validação Value Object Email (Email.cs): Valida formato e estrutura de email
- Validação Value Object Phone (Phone.cs): Valida formato de telefone (XX) XXXXX-XXXX
- Linha 38-39 (CreateLeadHandler): Tratamento de dados opcionais de interesse em veículo
- Linha 39 (Lead.cs): Método factory valida nome não vazio

**Condições de Erro**:
- UnauthorizedException: ID de vendedor não pode ser determinado ou está vazio
- ArgumentException: nome está vazio (de Lead.Create)
- Exceções de validação de Email/Phone de construtores de Value Object
- Exceção de parse de Enum se string LeadSource é inválida

---

#### **Regra de Negócio: Qualificação e Pontuação de Lead**

**Visão Geral**:
Qualificação de lead captura informações detalhadas sobre intenção de compra do cliente, capacidade financeira e preferências. Esses dados são usados para calcular automaticamente uma pontuação de lead (Bronze, Prata, Ouro, Diamante) e transita o status do lead para "InNegotiation", indicando engajamento ativo de vendas.

**Descrição**:
O processo de qualificação coleta dados estruturados sobre a prontidão de compra do cliente incluindo se possui veículo para troca, método de pagamento preferido (venda direta vs financiamento), renda mensal estimada, data esperada de compra e interesse em test drive. Se existe veículo de troca, informações detalhadas do veículo são coletadas incluindo marca, modelo, ano, quilometragem, placa, cor, condição geral e histórico de serviço.

A regra de negócio impõe que a qualificação só pode ser realizada uma vez por lead, já que a propriedade Qualification é definida diretamente ao invés de ser aditiva. Após qualificação, o LeadScoringService analisa os dados de qualificação para calcular uma pontuação de lead adequada. O algoritmo de pontuação considera fatores como urgência de compra (data esperada), capacidade financeira (renda mensal, método de pagamento) e nível de engajamento (interesse em test drive). A lógica de pontuação é encapsulada no serviço de domínio LeadScoringService.

Após qualificação, o status do lead muda automaticamente para "InNegotiation" para refletir a progressão no funil de vendas. Esta mudança de status é rastreada disparando um LeadStatusChangedEvent. A pontuação calculada também é rastreada via um LeadScoredEvent.

**Fluxo da Regra**:
1. POST /api/v1/leads/{id}/qualify recebe QualifyLeadRequest com dados de qualificação
2. Cria QualifyLeadCommand com ID do lead e parâmetros de qualificação
3. QualifyLeadHandler recupera a entidade Lead do repositório por ID
4. Lança NotFoundException se lead não existe
5. Se HasTradeInVehicle=true e dados de TradeInVehicle fornecidos, cria Value Object TradeInVehicle
6. Parse de enum PaymentMethod de string (case-insensitive)
7. Cria Value Object Qualification com todos dados de qualificação
8. Chama método lead.Qualify(qualification, scoringService)
9. Dentro do método Qualify:
   - Qualification é atribuída à propriedade lead.Qualification
   - LeadScoringService.Calculate() analisa qualificação e retorna nova pontuação
   - Propriedade Lead.Score é atualizada com pontuação calculada
   - Lead.Status é alterado para InNegotiation
   - Timestamp UpdatedAt é definido para hora UTC atual
   - LeadScoredEvent é adicionado à coleção de eventos de domínio
10. Repositório atualiza o lead via UpdateAsync
11. UnitOfWork commita transação
12. LeadResponse retornado com pontuação e status atualizados

**Pontos de Validação**:
- QualifyLeadHandler linha 32-33: Valida existência de lead, lança NotFoundException se não encontrado
- Validação Value Object TradeInVehicle (se fornecido): Valida campos obrigatórios do veículo
- Parsing enum PaymentMethod: Valida string correspondente a método de pagamento válido
- ExpectedPurchaseDate padrão para 30 dias a partir de agora se não fornecido (linha 56)
- Validação de domínio dentro do construtor de Value Object Qualification

**Condições de Erro**:
- NotFoundException: Lead com ID especificado não existe
- ArgumentException: String PaymentMethod inválida que não pode ser parseada para enum
- Exceções de validação de Value Object TradeInVehicle se campos obrigatórios ausentes

**Lógica de Pontuação** (encapsulada em LeadScoringService):
O algoritmo de pontuação analisa múltiplos fatores para atribuir nível Bronze/Prata/Ouro/Diamante:
- Urgência de compra (mais cedo = maior pontuação)
- Capacidade financeira (renda maior, pagamento à vista = maior pontuação)
- Nível de engajamento (interesse em test drive = maior pontuação)
- Presença de veículo de troca (indica comprador sério = maior pontuação)

---

#### **Regra de Negócio: Gerenciamento de Status de Lead**

**Visão Geral**:
O status do lead rastreia a progressão de leads através do funil de vendas, do contato inicial até a conversão ou perda. A operação de mudança de status fornece uma maneira controlada de atualizar status com log de auditoria automático e notificação de evento.

**Descrição**:
O status do lead segue um fluxo de trabalho predefinido com sete valores possíveis: New, InContact, InNegotiation, TestDriveScheduled, ProposalSent, Lost, ou Converted. O status pode ser alterado através de um endpoint PATCH dedicado para garantir que transições de status sejam explicitamente controladas e logadas.

A regra de negócio não impõe uma máquina de estado estrita com transições permitidas, significando que qualquer status pode ser alterado para qualquer outro status. Isso fornece flexibilidade para vendedores lidarem com casos excepcionais ou reverterem decisões. No entanto, a falta de validação de transição significa que sequências inválidas (ex: diretamente de New para Converted sem passos intermediários) são tecnicamente possíveis, dependendo de treinamento de usuário e aderência a processo de negócio para prevenir mau uso.

Cada mudança de status atualiza automaticamente o timestamp UpdatedAt para propósitos de auditoria e dispara um LeadStatusChangedEvent, que pode ser consumido por outros serviços ou componentes para notificações, relatórios ou automação de fluxo de trabalho.

**Fluxo da Regra**:
1. PATCH /api/v1/leads/{id}/status recebe ChangeLeadStatusRequest com nova string de status
2. Cria ChangeLeadStatusCommand com ID do lead e novo status
3. ChangeLeadStatusHandler recupera entidade Lead do repositório
4. Parse de enum LeadStatus de string de status (case-insensitive)
5. Chama método lead.ChangeStatus(newStatus)
6. Dentro do método ChangeStatus:
   - Propriedade Lead.Status é atualizada com novo valor de status
   - Timestamp UpdatedAt é definido para hora UTC atual
   - LeadStatusChangedEvent é adicionado à coleção de eventos de domínio com ID lead e novo status
7. Repositório atualiza o lead via UpdateAsync
8. UnitOfWork commita transação
9. LeadResponse retornado com status atualizado

**Pontos de Validação**:
- Handler valida existência de lead (lança NotFoundException se não encontrado)
- Parsing de enum LeadStatus valida string correspondente a valor válido de status
- Sem validação de adequação de transição de status (design flexível)

**Condições de Erro**:
- NotFoundException: Lead com ID especificado não existe
- ArgumentException: String de status inválida que não pode ser parseada para enum LeadStatus

**Valores de Status**:
- New (1): Estado inicial, lead recém criado
- InContact (2): Contato inicial feito com cliente
- InNegotiation (3): Negociações ativas em progresso
- TestDriveScheduled (4): Agendamento de test drive marcado
- ProposalSent (5): Proposta/cotação de vendas enviada ao cliente
- Lost (6): Lead perdido para competidor ou não mais interessado
- Converted (7): Lead convertido com sucesso para venda

---

#### **Regra de Negócio: Filtragem de Dados de Lead por Papel**

**Visão Geral**:
A operação de listagem de leads implementa filtragem de dados baseada em papel para garantir que vendedores vejam apenas leads atribuídos a eles, enquanto gerentes têm visibilidade sobre todos os leads com filtragem opcional por vendedores específicos.

**Descrição**:
O endpoint ListLeadsQuery suporta capacidades de filtragem sofisticadas além de simples paginação. Ele implementa uma camada de segurança que restringe visibilidade de dados baseada no papel do usuário. Para vendedores regulares, a query filtra automaticamente resultados para incluir apenas leads onde SalesPersonId corresponde ao seu próprio ID de vendedor. Para gerentes, o filtro de vendedor é removido, permitindo-lhes ver todos os leads no sistema.

Gerentes também têm a opção de filtrar explicitamente por um ID de vendedor específico através do parâmetro de query. Isso permite que gerentes visualizem o pipeline para membros individuais do time. A lógica de filtragem usa padrões de coalescência nula para determinar o ID de vendedor efetivo a aplicar: se o usuário não é gerente e tem um ID de vendedor, usa esse; se o usuário é gerente, usa o parâmetro de query se fornecido; caso contrário nenhum filtro é aplicado.

Parâmetros adicionais de query suportam filtragem por status do lead (ex: mostrar apenas leads "New"), pontuação (ex: apenas leads "Hot"), busca textual (busca em nome, email, telefone), intervalo de data de criação e controles de paginação. A implementação de paginação usa um wrapper PagedResponse que inclui metadados sobre contagem total, total de páginas e navegação (tem página próxima/anterior).

**Fluxo da Regra**:
1. GET /api/v1/leads com parâmetros de query (status, score, search, createdFrom, createdTo, salesPersonId, page, pageSize)
2. SalesPersonFilterService.GetCurrentSalesPersonId() extrai ID de vendedor do usuário atual
3. SalesPersonFilterService.IsManager() verifica se usuário tem papel de gerente
4. Determina effectiveSalesPersonId:
   - Se usuário atual tem ID de vendedor, usa esse (para filtragem de vendedores)
   - Se usuário atual é gerente, usa parâmetro salesPersonId (opcional)
   - Caso contrário, null (para gerentes visualizando todos os leads)
5. Cria ListLeadsQuery com effectiveSalesPersonId e todos parâmetros de filtro
6. ListLeadsHandler consulta repositório com filtros aplicados
7. Repositório aplica condições WHERE baseadas nos filtros fornecidos
8. Resultados são paginados usando Skip/Take baseado em page e pageSize
9. Contagem total é calculada para metadados de paginação
10. PagedResponse<LeadListItemResponse> construído com itens e metadados
11. Resposta retornada com status 200 OK

**Pontos de Validação**:
- Nenhuma validação explícita no controller - filtros são opcionais
