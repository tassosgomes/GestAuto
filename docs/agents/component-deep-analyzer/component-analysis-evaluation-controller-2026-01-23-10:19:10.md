# Relatório de Análise Profunda de Componente: EvaluationController

**Relatório Gerado**: 23/01/2026 10:19:10
**Nome do Componente**: EvaluationController
**Camada de Arquitetura**: Camada de Apresentação (API)
**Linhas de Código**: 174
**Linguagem**: C# / ASP.NET Core

---

## 1. Resumo Executivo

### Propósito do Componente
O EvaluationController é um controller de API responsável por gerenciar avaliações de veículos seminovos dentro do serviço comercial GestAuto. Ele expõe endpoints HTTP para que vendedores solicitem avaliações de veículos, consultem avaliações existentes e registrem respostas de clientes aos resultados da avaliação.

### Papel no Sistema
Este controller serve como ponto de entrada para o fluxo de trabalho de avaliação de veículos seminovos, conectando as aplicações front-end com os handlers de comanda/query da camada de aplicação. Ele implementa o padrão CQRS delegando comandos e queries para handlers especializados na camada de aplicação.

### Principais Descobertas
- **Arquitetura**: Segue princípios de Clean Architecture com clara separação da lógica de negócios
- **Implementação de Padrão**: Implementa CQRS adequadamente com separação de comad/query
- **Segurança**: Implementa autorização baseada em papel com política "SalesPerson"
- **Validação**: Confia na validação de domínio em handlers e entidades
- **Testes**: Testes unitários existem para handlers mas sem testes específicos de controller identificados
- **Complexidade**: Controller de baixa complexidade com responsabilidades finas

---

## 2. Análise de Fluxo de Dados

### Fluxo de Requisição: POST /api/v1/used-vehicle-evaluations (Criar Avaliação)

```
1. Requisição HTTP POST chega em EvaluationController.Request()
2. Verificação de Autorização: Usuário deve ter política "SalesPerson"
3. GetCurrentUserId() extrai ID do usuário da claim "sub" do JWT
4. RequestEvaluationCommand instanciado a partir do corpo da requisição + ID do usuário
5. Comando delegado para RequestEvaluationHandler
6. Handler valida se proposta existe via ProposalRepository
7. Entidade UsedVehicle criada com validação
8. Entidade UsedVehicleEvaluation criada via método de fábrica
9. Avaliação persistida via EvaluationRepository
10. Proposta atualizada para aguardar status de avaliação
11. UnitOfWork commita transação
12. EvaluationResponse mapeado da entidade
13. Resposta HTTP 201 Created retornada com cabeçalho Location
```

### Fluxo de Requisição: GET /api/v1/used-vehicle-evaluations (Listar Avaliações)

```
1. Requisição HTTP GET chega em EvaluationController.List()
2. Parâmetros de query extraídos e validados (restrições page, pageSize)
3. ListEvaluationsQuery instanciada
4. Query delegada para ListEvaluationsHandler
5. Handler consulta EvaluationRepository com filtros
6. Resultados paginados recuperados do banco de dados
7. Entidades mapeadas para DTOs EvaluationListItemResponse
8. Wrapper PagedResponse construído
9. Resposta HTTP 200 OK retornada
```

### Fluxo de Requisição: GET /api/v1/used-vehicle-evaluations/{id} (Obter Avaliação)

```
1. Requisição HTTP GET chega em EvaluationController.GetById()
2. Parâmetro ID extraído da rota
3. GetEvaluationQuery instanciada
4. Query delegada para GetEvaluationHandler
5. Handler recupera avaliação via EvaluationRepository
6. Entidade mapeada para DTO EvaluationResponse
7. Resposta HTTP 200 OK retornada (ou 404 se não encontrado)
```

### Fluxo de Requisição: POST /api/v1/used-vehicle-evaluations/{id}/customer-response (Registrar Resposta)

```
1. Requisição HTTP POST chega em EvaluationController.CustomerResponse()
2. ID da avaliação da rota, corpo da requisição com flag Accepted
3. RegisterCustomerResponseCommand instanciado
4. Comando delegado para RegisterCustomerResponseHandler
5. Handler recupera avaliação e valida que status é Completed
6. Baseado na flag Accepted:
   - Se true: evaluation.CustomerAccept(), proposta atualizada com valor de troca
   - Se false: evaluation.CustomerReject(reason)
7. Mudanças persistidas via UnitOfWork
8. Resposta HTTP 200 OK retornada
```

---

## 3. Regras de Negócio e Lógica

### Visão Geral das Regras de Negócio

| Tipo de Regra | Descrição | Localização |
|---------------|-----------|-------------|
| Autorização | Apenas usuários com política "SalesPerson" podem acessar endpoints | EvaluationController.cs:15 |
| Paginação | Page deve ser >= 1, PageSize entre 1-100 | EvaluationController.cs:103-104 |
| Validação de Status | Cliente só pode responder a avaliações com status "Completed" | RegisterCustomerResponseHandler.cs:32 |
| Existência de Proposta | Não pode criar avaliação para proposta inexistente | RequestEvaluationHandler.cs:31-32 |
| Vínculo de Proposta | Avaliação deve ser vinculada a proposta existente | RequestEvaluationHandler.cs:54 |
| Validação de Veículo | Marca e modelo não podem ser vazios, ano deve ser válido | UsedVehicleEvaluation.cs:29-36 |
| Validação de Quilometragem | Quilometragem não pode ser negativa | UsedVehicleEvaluation.cs:38-39 |
| Transição de Status | Avaliações seguem fluxo estrito: Requested → Completed → Accepted/Rejected | UsedVehicleEvaluation.cs:98-127 |

---

### Regras de Negócio Detalhadas

#### Regra de Negócio: Autorização de Vendedor Necessária

**Visão Geral**:
Todos os endpoints expostos pelo EvaluationController requerem autenticação e autorização. Apenas usuários atribuídos ao papel/política "SalesPerson" podem acessar operações de avaliação de veículo usado. Isso garante que dados sensíveis de avaliação veicular e interação com cliente sejam acessíveis apenas para equipe de vendas autorizada.

**Descrição Detalhada**:
A autorização é imposta no nível do controller usando o atributo ASP.NET Core [Authorize] com a política "SalesPerson". Esta política é aplicada consistentemente através de todos os quatro endpoints (Request, List, GetById, CustomerResponse). A verificação da política ocorre antes de qualquer lógica de método do controller executar, fornecendo uma fronteira de segurança na camada de API. Requisições não autorizadas (faltando autenticação ou claims necessárias) são rejeitadas com HTTP 401 Unauthorized antes de alcançar a lógica de negócio.

**Fluxo de Implementação**:
1. Requisição HTTP chega com token JWT bearer no cabeçalho Authorization
2. Middleware de autenticação ASP.NET Core valida assinatura e claims do token
3. Middleware de autorização verifica se usuário tem política "SalesPerson"
4. Se autorizado, requisição prossegue para método do controller
5. Se não autorizado, HTTP 401 Unauthorized retornado imediatamente

**Considerações de Segurança**:
- Identificação do usuário extraída da claim "sub" no método auxiliar GetCurrentUserId()
- Empty GUID retornado se falhar parse de claim (preocupação de segurança potencial)
- Sem verificações de autorização adicionais no nível do handler (confia no controller)

---

#### Regra de Negócio: Restrições de Paginação

**Visão Geral**:
O endpoint List impõe restrições em parâmetros de paginação para prevenir problemas de performance e recuperação excessiva de dados. Números de página devem ser inteiros positivos, e o tamanho da página é limitado a 100 itens por requisição.

**Descrição Detalhada**:
Quando clientes solicitam uma lista de avaliações, eles podem fornecer parâmetros opcionais de query `_page` e `_size`. O controller valida estes parâmetros antes de passá-los para o query handler. Números de página abaixo de 1 são automaticamente corrigidos para 1. Tamanhos de página abaixo de 1 ou acima de 100 são automaticamente corrigidos para o padrão de 20. Isso garante que consultas de banco de dados permaneçam performáticas e tamanhos de resposta gerenciáveis. A correção automática previne erros de cliente mas pode não corresponder às expectativas do cliente, potencialmente causando confusão se clientes solicitarem 200 itens mas receberem apenas 100.

**Lógica de Validação**:
```
if (page < 1) page = 1;
if (pageSize < 1 || pageSize > 100) pageSize = 20;
```

**Casos de Uso**:
- Requisição padrão (sem params de paginação): Retorna página 1 com 20 itens
- Requisição com tamanho de página pequeno: Respeita valores >= 1
- Requisição com tamanho de página grande: Limita em 100 para prevenir carga excessiva
- Número de página inválido: Corrige para página 1

**Casos de Borda**:
- Page = 0: Corrigido para 1
- PageSize = 0: Corrigido para padrão (20)
- PageSize = 1000: Corrigido para máximo (100)
- Valores negativos: Corrigidos para padrões

---

#### Regra de Negócio: Validação de Existência de Proposta

**Visão Geral**:
Avaliações de veículos seminovos não podem ser criadas isoladamente; elas devem ser associadas com uma proposta de venda existente. Esta regra garante integridade de dados e mantém o relacionamento entre avaliações e as oportunidades de vendas que elas suportam.

**Descrição Detalhada**:
Quando um vendedor solicita uma avaliação de veículo, ele deve fornecer um ProposalId válido. O RequestEvaluationHandler valida isso tentando recuperar a proposta do repositório antes de criar a avaliação. Se a proposta não existir, uma NotFoundException é lançada, o que se traduz em uma resposta HTTP 404. Isso previne avaliações órfãs e garante que o contexto de negócio (proposta → avaliação) seja mantido. Adicionalmente, quando uma avaliação é criada, a proposta é automaticamente atualizada para refletir que está aguardando uma avaliação, criando um link bidirecional entre as entidades.

**Fluxo de Validação**:
1. Extrai ProposalId de RequestEvaluationCommand
2. Chama ProposalRepository.GetByIdAsync(proposalId)
3. Se nulo, lança NotFoundException com ID da proposta na mensagem
4. Se encontrado, prossegue com criação da avaliação
5. Atualiza proposta com ID da avaliação e status
6. Persiste ambas entidades na transação

**Tratamento de Erro**:
- Proposta não encontrada: HTTP 404 com mensagem "Proposta {id} não encontrada"
- Erros de repositório: Propagados como HTTP 500 Internal Server Error

---

#### Regra de Negócio: Validação de Dados de Veículo

**Visão Geral**:
Informação de veículo fornecida em uma requisição de avaliação deve satisfazer regras de validação de domínio para garantir qualidade de dados e prevenir entradas inválidas. A validação cobre campos obrigatórios (marca, modelo), restrições lógicas (faixa de ano, quilometragem não negativa), e regras de negócio (formato de placa).

**Descrição Detalhada**:
A entidade UsedVehicle encapsula lógica de validação em seu método de fábrica Create(). Quando o handler cria uma instância UsedVehicle, o construtor valida todos inputs. Marca e modelo não podem ser nulos, vazios ou espaços em branco. O ano deve ser entre 1900 e o ano atual mais um (para permitir veículos de ano modelo futuro). Quilometragem não pode ser negativa. A placa deve ser fornecida como objeto de valor que impõe suas próprias regras de formatação. Estas validações ocorrem na camada de domínio, garantindo que dados de veículo inválidos nunca alcancem o banco de dados. As falhas de validação lançam ArgumentException, que são capturadas pelo middleware de tratamento de erro da API e retornadas como respostas HTTP 400 Bad Request.

**Regras de Validação**:
- Marca: Obrigatório, string não vazia
- Modelo: Obrigatório, string não vazia
- Ano: 1900 <= ano <= DateTime.Now.Year + 1
- Quilometragem: Deve ser >= 0
- Placa: Deve ser objeto de valor LicensePlate válido
- Cor: Obrigatório, string não vazia
- Condição Geral: Obrigatório, string não vazia
- Histórico de Serviço em Concessionária: Flag booleana (opcional)

**Casos de Uso**:
- Avaliação de carro novo: Ano pode ser atual ou próximo ano
- Avaliação de carro usado: Quilometragem valida histórico de transação
- Veículos importados: Validação de ano previne datas irrealistas

---

#### Regra de Negócio: Ciclo de Vida de Status de Avaliação

**Visão Geral**:
Avaliações seguem uma máquina de estado estrita com transições definidas. A progressão de status garante integridade de processo de negócio: avaliações não podem ser aceitas ou rejeitadas antes de serem completadas pelo departamento de seminovos, e avaliações completadas não podem ser modificadas.

**Descrição Detalhada**:
O enum EvaluationStatus define quatro estados: Requested (1), Completed (2), Accepted (3), e Rejected (4). Quando uma avaliação é criada, ela está no estado "Requested". Apenas após o departamento de seminovos responder com uma avaliação ela transita para "Completed". Neste ponto, o cliente pode aceitar ou rejeitar a avaliação, transitando para "Accepted" ou "Rejected" respectivamente. O domínio impõe essas transições através de métodos que verificam o estado atual antes de permitir mudanças. MarkAsCompleted() só funciona do estado Requested. CustomerAccept() e CustomerReject() só funcionam do estado Completed. Transições inválidas lançam InvalidOperationException, prevenindo corrupção de estado.

**Diagrama de Transição de Estado**:
```
Requested (Inicial)
    ↓ MarkAsCompleted()
Completed
    ↓ CustomerAccept()      ↓ CustomerReject()
Accepted                    Rejected
(Terminal)                  (Terminal)
```

**Cenários de Negócio**:
- Requisição inicial: Status = Requested, aguardando avaliação
- Avaliação recebida: Status = Completed, EvaluatedValue populado
- Cliente aceita troca: Status = Accepted, valor adicionado à proposta
- Cliente rejeita oferta: Status = Rejected, razão capturada
- Operação inválida: Tentar aceitar antes de completar lança exceção

**Restrições de Estado**:
- Não pode marcar avaliação já completada como completada novamente
- Não pode aceitar avaliação que ainda não foi valorada
- Não pode rejeitar avaliação que ainda não foi valorada
- Sem mecanismo de rollback (uma vez aceita/rejeitada, estado é final)

---

#### Regra de Negócio: Restrição Temporal de Resposta do Cliente

**Visão Geral**:
Clientes só podem responder a avaliações após o departamento de seminovos ter completado sua avaliação. Isso previne clientes de aceitar ou rejeitar avaliações antes de uma valoração estar disponível, garantindo que respostas sejam informadas e significativas.

**Descrição Detalhada**:
O RegisterCustomerResponseHandler valida que a avaliação está no estado "Completed" antes de processar uma resposta de cliente. Se a avaliação ainda estiver em status "Requested" (aguardando valoração), o handler lança uma DomainException com a mensagem "Avaliação ainda não foi respondida pelo setor de seminovos". Esta validação garante integridade de processo de negócio: clientes não podem aceitar ou rejeitar ofertas que não viram. Quando o cliente responde, se aceitar, o valor de troca é automaticamente aplicado à proposta associada. Se rejeitar, o motivo da rejeição é capturado para referência futura mas a proposta permanece inalterada.

**Lógica de Validação**:
```csharp
if (evaluation.Status != EvaluationStatus.Completed)
    throw new DomainException("Avaliação ainda não foi respondida pelo setor de seminovos");
```

**Fluxo de Resposta do Cliente**:
1. Cliente visualiza avaliação com status Completed e EvaluatedValue
2. Cliente decide aceitar ou rejeitar a oferta
3. POST para /api/v1/used-vehicle-evaluations/{id}/customer-response
4. Handler valida que status é Completed
5. Se aceito: evaluation.CustomerAccept(), proposta atualizada com valor de troca
6. Se rejeitado: evaluation.CustomerReject(reason), proposta inalterada
7. Status transita para Accepted ou Rejected

**Cenários de Erro**:
- Avaliação não encontrada: HTTP 404
- Avaliação ainda Requested: HTTP 400 com mensagem de exceção de domínio
- Avaliação já Accepted/Rejected: Falharia verificação de status em métodos de domínio

---

## 4. Estrutura do Componente

```
GestAuto.Commercial.API/Controllers/
├── EvaluationController.cs (174 linhas)
│   ├── Dependências (Injeção de Construtor)
│   │   ├── ICommandHandler<RequestEvaluationCommand, EvaluationResponse>
│   │   ├── ICommandHandler<RegisterCustomerResponseCommand, EvaluationResponse>
│   │   ├── IQueryHandler<GetEvaluationQuery, EvaluationResponse>
│   │   ├── IQueryHandler<ListEvaluationsQuery, PagedResponse<EvaluationListItemResponse>>
│   │   └── ILogger<EvaluationController>
│   │
│   ├── Endpoints
│   │   ├── POST /api/v1/used-vehicle-evaluations
│   │   │   └── Request() - Criar nova avaliação
│   │   ├── GET /api/v1/used-vehicle-evaluations
│   │   │   └── List() - Listar avaliações com paginação
│   │   ├── GET /api/v1/used-vehicle-evaluations/{id}
│   │   │   └── GetById() - Obter avaliação por ID
│   │   └── POST /api/v1/used-vehicle-evaluations/{id}/customer-response
│   │       └── CustomerResponse() - Registrar aceite/rejeite de cliente
│   │
│   └── Helpers Privados
│       └── GetCurrentUserId() - Extrair ID de usuário das claims JWT
│
└── Componentes Relacionados (Externos)
    ├── Commands/
    │   ├── RequestEvaluationCommand.cs (17 linhas)
    │   └── RegisterCustomerResponseCommand.cs (10 linhas)
    ├── Queries/
    │   ├── GetEvaluationQuery.cs (6 linhas)
    │   └── ListEvaluationsQuery.cs (11 linhas)
    ├── DTOs/
    │   └── EvaluationDTOs.cs (139 linhas)
    │       ├── RequestEvaluationRequest
    │       ├── CustomerResponseRequest
    │       ├── EvaluationResponse
    │       ├── EvaluationListItemResponse
    │       └── UsedVehicleResponse
    ├── Handlers/
    │   ├── RequestEvaluationHandler.cs (61 linhas)
    │   ├── RegisterCustomerResponseHandler.cs (56 linhas)
    │   ├── GetEvaluationHandler.cs (27 linhas)
    │   └── ListEvaluationsHandler.cs (41 linhas)
    └── Domain/
        ├── Entities/UsedVehicleEvaluation.cs (128 linhas)
        ├── Enums/EvaluationStatus.cs (10 linhas)
        └── Interfaces/IUsedVehicleEvaluationRepository.cs
```

---

## 5. Análise de Dependências

### Dependências Internas

```
EvaluationController
├── Camada de Aplicação (Commands & Queries)
│   ├── RequestEvaluationCommand
│   ├── RegisterCustomerResponseCommand
│   ├── GetEvaluationQuery
│   └── ListEvaluationsQuery
│
├── Camada de Aplicação (DTOs)
│   ├── RequestEvaluationRequest
│   ├── CustomerResponseRequest
│   ├── EvaluationResponse
│   ├── EvaluationListItemResponse
│   └── PagedResponse<T>
│
├── Camada de Aplicação (Interfaces)
│   ├── ICommandHandler<TCommand, TResult>
│   └── IQueryHandler<TQuery, TResult>
│
└── Camada de Domínio (Implícito via handlers)
    ├── UsedVehicleEvaluation (Entidade)
    ├── UsedVehicle (Entidade)
    ├── EvaluationStatus (Enum)
    ├── LicensePlate (Value Object)
    └── Money (Value Object)
```

### Dependências de Handler (Camada de Aplicação)

```
RequestEvaluationHandler depende de:
├── IUsedVehicleEvaluationRepository
├── IProposalRepository
└── IUnitOfWork

RegisterCustomerResponseHandler depende de:
├── IUsedVehicleEvaluationRepository
├── IProposalRepository
└── IUnitOfWork

GetEvaluationHandler depende de:
└── IUsedVehicleEvaluationRepository

ListEvaluationsHandler depende de:
└── IUsedVehicleEvaluationRepository
```

### Dependências Externas

```
EvaluationController
├── Microsoft.AspNetCore.Authorization
│   └── Atributo [Authorize]
│
├── Microsoft.AspNetCore.Mvc
│   ├── ApiController
│   ├── ControllerBase
│   ├── Atributo Route
│   ├── Atributo HttpPost
│   ├── Atributo HttpGet
│   ├── Atributo FromBody
│   ├── Atributo FromQuery
│   ├── Atributo Produces
│   └── ActionResult<T>
│
├── Microsoft.Extensions.Logging
│   └── ILogger<EvaluationController>
│
└── System (Implícito)
    ├── Guid
    ├── CancellationToken
    └── DateTime
```

### Configuração de Injeção de Dependência

O controller usa injeção de construtor com quatro handlers de comando/query e um logger. O container de injeção de dependência (configurado no startup da API) é responsável por:
- Resolver implementações ICommandHandler para comandos
- Resolver implementações IQueryHandler para queries
- Fornecer instância ILogger para logging estruturado
- Gerenciar tempo de vida de handler (provavelmente scoped ou transient)

---

## 6. Análise de Acoplamento

### Acoplamento Aferente (Ca)
**Quem depende de EvaluationController?**
- Aplicações Front-end (web/mobile clients)
- API Gateway / Reverse Proxy
- Testes de integração
- Consumidores de API (sistemas externos)

**Estimativa**: Baixo (3-5 consumidores diretos)

### Acoplamento Eferente (Ce)
**De que EvaluationController depende?**
- 4 Interfaces de Handler Command/Query
- 6 classes DTO (requests/responses)
- Framework ASP.NET Core (5+ atributos)
- Infraestrutura de Logging

**Estimativa**: Moderado (15+ dependências)

### Métrica de Instabilidade (I = Ce / (Ce + Ca))
I = 15 / (15 + 4) = 15 / 19 = 0.79

**Análise**: Alta instabilidade (0.79 na escala 0-1)
- Isso é **apropriado** para um controller
- Controllers devem ser dependentes estáveis, não dependências
- Instabilidade alta é esperada e aceitável na fronteira da API
- Mudanças são mais prováveis de vir de requisitos de negócio do que de consumidores

### Avaliação de Criticidade

| Componente | Ca | Ce | Criticidade | Notas |
|------------|----|----|-------------|-------|
| EvaluationController | 4 | 15+ | Média | Fronteira de API, instabilidade alta esperada |
| RequestEvaluationHandler | 1 | 3 | Baixa | Tratamento de comando simples |
| RegisterCustomerResponseHandler | 1 | 3 | Baixa | Tratamento de comando simples |
| GetEvaluationHandler | 1 | 1 | Baixa | Tratamento de query simples |
| ListEvaluationsHandler | 1 | 1 | Baixa | Tratamento de query simples |

**Tipo de Acoplamento**: Acoplamento fraco via interfaces
- Controller depende de abstrações (ICommandHandler, IQueryHandler)
- Handlers dependem de interfaces de repositório
- Sem dependências diretas em implementações concretas
- Facilita testes e manutenibilidade

---

## 7. Endpoints

| Endpoint | Método | Descrição | Corpo da Requisição | Resposta | Códigos de Status |
|----------|--------|-----------|---------------------|----------|-------------------|
| /api/v1/used-vehicle-evaluations | POST | Solicitar nova avaliação de veículo usado | RequestEvaluationRequest | EvaluationResponse | 201, 400, 404, 401 |
| /api/v1/used-vehicle-evaluations | GET | Listar avaliações com filtros opcionais | Parâmetros de Query | PagedResponse<EvaluationListItemResponse> | 200, 400, 401 |
| /api/v1/used-vehicle-evaluations/{id} | GET | Obter avaliação por ID | N/A | EvaluationResponse | 200, 404, 401 |
| /api/v1/used-vehicle-evaluations/{id}/customer-response | POST | Registrar aceite/rejeite de cliente | CustomerResponseRequest | EvaluationResponse | 200, 400, 404, 401 |

### Detalhes de Endpoint

#### POST /api/v1/used-vehicle-evaluations
**Propósito**: Iniciar uma solicitação de avaliação de veículo usado vinculada a uma proposta de venda

**Estrutura do Corpo da Requisição**:
```json
{
  "proposalId": "guid",
  "brand": "string",
  "model": "string",
  "year": 2020,
  "mileage": 50000,
  "licensePlate": "string",
  "color": "string",
  "generalCondition": "string",
  "hasDealershipServiceHistory": true
}
```

**Resposta de Sucesso** (201 Created):
```json
{
  "id": "guid",
  "proposalId": "guid",
  "status": "Requested",
  "vehicle": { ... },
  "evaluatedValue": null,
  "evaluationNotes": null,
  "requestedAt": "2024-01-23T10:00:00Z",
  "respondedAt": null,
  "customerAccepted": null,
  "customerRejectionReason": null
}
```
Cabeçalho Location: `/api/v1/used-vehicle-evaluations/{id}`

**Respostas de Erro**:
- 400 Bad Request: Dados de veículo inválidos (falha na validação)
- 404 Not Found: Proposta não encontrada
- 401 Unauthorized: Autenticação ausente ou inválida

---

#### GET /api/v1/used-vehicle-evaluations
**Propósito**: Recuperar lista paginada de avaliações com filtro opcional

**Parâmetros de Query**:
- `proposalId` (opcional): Filtrar por GUID da proposta
- `status` (opcional): Filtrar por status ("Requested", "Completed", "Accepted", "Rejected")
- `_page` (opcional): Número da página, padrão 1
- `_size` (opcional): Tamanho da página, padrão 20, máx 100

**Resposta de Sucesso** (200 OK):
```json
{
  "items": [
    {
      "id": "guid",
      "proposalId": "guid",
      "status": "Completed",
      "vehicleInfo": "Toyota Corolla 2020",
      "evaluatedValue": 85000.00,
      "requestedAt": "2024-01-23T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42
}
```

**Respostas de Erro**:
- 400 Bad Request: Parâmetros de query inválidos
- 401 Unauthorized: Autenticação ausente ou inválida

---

#### GET /api/v1/used-vehicle-evaluations/{id}
**Propósito**: Recuperar informação detalhada sobre uma avaliação específica

**Parâmetro de URL**: `id` (GUID, obrigatório)

**Resposta de Sucesso** (200 OK):
```json
{
  "id": "guid",
  "proposalId": "guid",
  "status": "Completed",
  "vehicle": {
    "brand": "Toyota",
    "model": "Corolla",
    "year": 2020,
    "mileage": 50000,
    "licensePlate": "ABC1234",
    "color": "Branco",
    "generalCondition": "Boa",
    "hasDealershipServiceHistory": true
  },
  "evaluatedValue": 85000.00,
  "evaluationNotes": "Veículo em bom estado...",
  "requestedAt": "2024-01-23T10:00:00Z",
  "respondedAt": "2024-01-23T14:30:00Z",
  "customerAccepted": null,
  "customerRejectionReason": null
}
```

**Respostas de Erro**:
- 404 Not Found: Avaliação não encontrada
- 401 Unauthorized: Autenticação ausente ou inválida

---

#### POST /api/v1/used-vehicle-evaluations/{id}/customer-response
**Propósito**: Registrar decisão do cliente de aceitar ou rejeitar o valor avaliado

**Parâmetro de URL**: `id` (GUID, obrigatório)

**Estrutura do Corpo da Requisição**:
```json
{
  "accepted": true,
  "rejectionReason": null
}
```
OU
```json
{
  "accepted": false,
  "rejectionReason": "Valor abaixo do mercado"
}
```

**Resposta de Sucesso** (200 OK):
```json
{
  "id": "guid",
  "proposalId": "guid",
  "status": "Accepted", // ou "Rejected"
  "vehicle": { ... },
  "evaluatedValue": 85000.00,
  "evaluationNotes": "...",
  "requestedAt": "2024-01-23T10:00:00Z",
  "respondedAt": "2024-01-23T14:30:00Z",
  "customerAccepted": true, // ou false
  "customerRejectionReason": null // ou texto do motivo
}
```

**Respostas de Erro**:
- 400 Bad Request: Avaliação não completada ainda, ou dados inválidos
- 404 Not Found: Avaliação não encontrada
- 401 Unauthorized: Autenticação ausente ou inválida

---

## 8. Pontos de Integração

### Integração de Serviço Interno

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|------------|------|-----------|-----------|------------------|--------------------|
| RequestEvaluationHandler | In-Process | Tratar lógica de criação de avaliação | Chamada Direta de Método | Objeto Command | Propagação de Exceção |
| RegisterCustomerResponseHandler | In-Process | Tratar lógica de resposta de cliente | Chamada Direta de Método | Objeto Command | Propagação de Exceção |
| GetEvaluationHandler | In-Process | Tratar query de avaliação única | Chamada Direta de Método | Objeto Query | Propagação de Exceção |
| ListEvaluationsHandler | In-Process | Tratar lista de avaliação paginada | Chamada Direta de Método | Objeto Query | Propagação de Exceção |

### Integração de Acesso a Dados

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|------------|------|-----------|-----------|------------------|--------------------|
| IUsedVehicleEvaluationRepository | Padrão Repository | Persistir e recuperar avaliações | Abstração de Interface | Objetos Entity | Lança NotFoundException |
| IProposalRepository | Padrão Repository | Recuperar e atualizar propostas | Abstração de Interface | Objetos Entity | Lança NotFoundException |
| IUnitOfWork | Gerenciamento de Transação | Coordenar atualizações atômicas | Abstração de Interface | Escopo de Transação | Rollback em exceção |

### Integração de Domínio

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|------------|------|-----------|-----------|------------------|--------------------|
| UsedVehicle Domain Entity | Object Value | Encapsular validação de dados de veículo | Chamada Direta de Método | Propriedades de Entidade | ArgumentException |
| UsedVehicleEvaluation Domain Entity | Aggregate Root | Impor regras de negócio e transições de estado | Chamada Direta de Método | Métodos de Entidade | InvalidOperationException |
| EvaluationStatus Enum | Domain Enum | Definir estados válidos de avaliação | Uso Direto | Valores Enum | Parse Enum Inválido → filtro nulo |

### Integração Orientada a Eventos (Async)

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erro |
|------------|------|-----------|-----------|------------------|--------------------|
| UsedVehicleEvaluationRequestedEvent | Evento de Domínio | Notificar quando avaliação é solicitada | Event Bus (MassTransit) | Mensagem de Evento | Publicado Async |
| Used Vehicle Module | Serviço Externo | Responder a solicitações de avaliação | Message Queue (RabbitMQ) | Mensagem AsyncAPI | Tratado pelo Stock Service |

**Nota**: O EvaluationController não publica eventos diretamente. Publicação de evento ocorre na camada de domínio quando UsedVehicleEvaluation.Request() é chamado, o que adiciona um evento de domínio à coleção de eventos da entidade. O evento é publicado por dispatchers de infraestrutura após o UnitOfWork completar.

---

## 9. Padrões de Design e Arquitetura

### Padrões Identificados

| Padrão | Implementação | Localização | Propósito |
|---------|---------------|-------------|-----------|
| **CQRS** | Separar comandos e queries com handlers dedicados | Commands/, Queries/, Handlers/ | Separação de preocupações leitura/escrita para escalabilidade |
| **Injeção de Dependência** | Injeção de construtor de interfaces handler | EvaluationController.cs:25-37 | Baixo acoplamento, testabilidade, gerenciamento de container IoC |
| **Padrão Repository** | IUsedVehicleEvaluationRepository, IProposalRepository | Application/Handlers/ | Abstrair lógica de acesso a dados |
| **Unit of Work** | IUnitOfWork coordena commits de transação | Handlers/ | Garantir operações atômicas através de repositórios |
| **Factory Method** | UsedVehicle.Create(), UsedVehicleEvaluation.Request() | Domain/Entities/ | Encapsular criação de objeto complexo com validação |
| **Value Object** | LicensePlate, Money como tipos imutáveis | Domain/ValueObjects/ | Representar conceitos sem identidade, prevenir obsessão primitiva |
| **Padrão DTO** | Objetos Request/Response separados de entidades | Application/DTOs/ | Formatar dados para camada API, prevenir over-posting |
| **Padrão Specification (Implícito)** | Parâmetros de filtro em ListEvaluationsQuery | ListEvaluationsQuery.cs:6-11 | Encapsular critérios de consulta |
| **Padrão Null Object** | Nullable EvaluatedValue, CustomerAccepted | EvaluationResponse.cs:52,60 | Tratar ausência de valor explicitamente |
| **Route Prefixing** | [Route("api/v1/used-vehicle-evaluations")] | EvaluationController.cs:14 | Centralizar definição de rota, habilitar versionamento |

### Decisões Arquiteturais

#### Implementação de Clean Architecture
O controller segue princípios SOLID dependendo apenas de abstrações (interfaces) da camada de aplicação. Lógica de negócio é completamente delegada para handlers, mantendo o controller como um adaptador HTTP fino. Esta separação permite que a lógica de negócio evolua independentemente do framework de API.

#### Design de API RESTful
Endpoints seguem convenções REST com verbos HTTP apropriados (GET para queries, POST para comandos). URIs de recurso são hierárquicas e baseadas em substantivos. Códigos de status alinham com semântica HTTP (201 para criação, 200 para sucesso, 404 para não encontrado, 400 para erros de cliente). HATEOAS é parcialmente implementado via cabeçalho Location na resposta POST.

#### Separação CQRS
Comandos (operações de escrita) e queries (operações de leitura) são estritamente separados. Comandos retornam resultados mas são primariamente de efeito colateral. Queries são somente leitura e nunca modificam estado. Esta separação permite otimização independente de caminhos de leitura e escrita.

#### Estratégia de Validação
A validação é distribuída através de camadas:
- **Camada de Controller**: Sem validação (delega para handlers)
- **Camada de Handler**: Validação de workflow (proposta existe, transições de status válidas)
- **Camada de Domínio**: Validação de regra de negócio (restrições de dados de veículo)
- **Camada de Entidade**: Imposição de invariante (regras de máquina de estado)

Esta validação multicamada garante integridade de dados em cada fronteira arquitetural.

#### Abordagem de Tratamento de Erro
Exceções lançadas por handlers e domínio são permitidas propagar para o middleware ASP.NET Core, onde são transformadas em respostas HTTP apropriadas. Isso centraliza lógica de tratamento de erro e previne poluição do controller com blocos try-catch.

---

## 10. Dívida Técnica e Riscos

### Problemas Identificados

| Nível de Risco | Área | Problema | Impacto | Mitigação |
|----------------|------|----------|---------|-----------|
| **Baixo** | Tratamento de Erro | Sem blocos try-catch em métodos de controller | Exceções não tratadas podem vazar stack traces sensíveis | Garantir middleware de tratamento de exceção global configurado |
| **Baixo** | Logging | Logging existe mas níveis de detalhe inconsistentes | Difícil debugar problemas de produção | Considerar adicionar correlation IDs, logging estruturado |
| **Médio** | Validação | RequestEvaluationRequest não tem atributos de validação | Dados inválidos alcançam camada de handler antes da validação | Adicionar FluentValidation ou data annotations |
| **Baixo** | Paginação | Auto-correção de params inválidos de página pode confundir clientes | Mudança de comportamento silenciosa, potenciais problemas UX | Retornar avisos ou rejeitar params inválidos explicitamente |
| **Baixo** | Extração de User ID | GetCurrentUserId retorna EmptyGuid em falha de parse | Poderia criar avaliações com rastreamento de usuário inválido | Lançar exceção se claim de user ID for inválida ou ausente |
| **Médio** | Testes | Sem testes específicos de controller encontrados | Problemas de integração podem não ser capturados | Adicionar testes de integração de API para todos endpoints |
| **Baixo** | Documentação | Documentação XML existe mas não abrangente | Consumidores de API podem precisar de clarificação | Considerar usar anotações Swagger/OpenAPI |
| **Baixo** | Performance | Sem caching em endpoints GET | Consultas de banco de dados repetidas para mesmos dados | Considerar adicionar cache de resposta para endpoint GetById |

### Observações de Qualidade de Código

**Aspectos Positivos**:
- Responsabilidade Única: Controller lida apenas com preocupações HTTP
- Injeção de Dependência: Uso adequado de injeção de construtor
- Async/Await: Padrões async corretos por toda parte
- RESTfulness: Segue convenções REST apropriadamente
- Logging: Logging estruturado com informação contextual
- Documentação: Comentários XML em todos os métodos públicos

**Melhorias Potenciais** (Não Prescritivo):
- Considerar adicionar versionamento de API além de versionamento de caminho de URL
- Considerar implementar metadados de paginação (X-Pagination headers)
- Considerar adicionar rate limiting para criação de avaliação
- Considerar adicionar logging de auditoria para respostas de cliente
- Considerar implementar links HATEOAS em respostas

---

## 11. Análise de Cobertura de Teste

### Testes Unitários Encontrados

| Arquivo de Teste | Linhas | Testes Cobertos | Qualidade |
|------------------|--------|-----------------|-----------|
| RequestEvaluationHandlerTests.cs | 173 | 3 casos de teste | Boa cobertura de caminho feliz e casos de erro |
| UsedVehicleEvaluationRespondedConsumerTests.cs | Desconhecido | Testes de Integração/Consumer | Testa tratamento de evento |

### Detalhamento de Cobertura de Teste

**RequestEvaluationHandlerTests.cs**:
- ✅ Caminho feliz: Criar avaliação quando proposta existe
- ✅ Caminho de erro: Proposta não encontrada lança NotFoundException
- ✅ Caminho de validação: Marca inválida lança ArgumentException

**Cobertura de Teste Ausente**:
- ❌ Testes de integração de Controller (não encontrado)
- ❌ Testes de RegisterCustomerResponseHandler (não encontrado)
- ❌ Testes de GetEvaluationHandler (não encontrado)
- ❌ Testes de ListEvaluationsHandler (não encontrado)
- ❌ Casos de borda de paginação (page = 0, pageSize > 100)
- ❌ Testes de fluxo de aceitação de cliente
- ❌ Testes de fluxo de rejeição de cliente
- ❌ Testes de validação de transição de status
- ❌ Testes de Autorização/Autenticação
- ❌ Testes de criação de avaliação concorrente

### Avaliação de Qualidade de Teste

**Pontos Fortes**:
- Testes usam mocking adequado (Moq) para dependências
- FluentAssertions fornecem asserções legíveis
- Testes seguem padrão Arrange-Act-Assert
- Casos de borda considerados (marca nula/vazia)
- Verificação de interações de mock (Times.Once)

**Lacunas**:
- Sem testes de nível de controller (contexto HTTP, autorização, model binding)
- Sem testes de integração (ciclo requisição/resposta completo)
- Faltando testes de handler para 3 de 4 handlers
- Sem testes negativos para razões de rejeição
- Sem testes de limite de paginação
- Sem testes de acesso concorrente
- Sem testes de performance/carga

**Cobertura Estimada**: ~30% (apenas handlers, sem controller ou integração)

---

## 12. Avaliação de Segurança

### Autenticação & Autorização

| Mecanismo | Implementação | Status |
|-----------|---------------|--------|
| Autenticação | JWT Bearer Token (inferido de claim "sub") | ✅ Implementado |
| Autorização | [Authorize(Policy = "SalesPerson")] | ✅ Implementado |
| Identificação de Usuário | GetCurrentUserId() extrai claim "sub" | ⚠️ Retorna EmptyGuid em falha (problema potencial) |

### Validação de Dados

| Tipo de Input | Validação | Localização | Status |
|---------------|-----------|-------------|--------|
| Parâmetros de Rota | Validação formato GUID | Framework | ✅ Framework lida |
| Parâmetros de Query | Checagem de limites Page/size | Controller:103-104 | ✅ Implementado |
| Corpo de Requisição | Validação de domínio em entidades | UsedVehicle.Create() | ✅ Implementado |
| SQL Injection | Queries parametrizadas (EF Core) | Camada de Repositório | ✅ Protegido por ORM |

### Preocupações de Segurança

**Risco Baixo**:
- Auto-correção de params de paginação poderia ser abusada (embora limitada em 100)
- Sem rate limiting visível (pode estar no nível de API Gateway)
- Falha de parse de ID de usuário retorna EmptyGuid ao invés de lançar exceção

**Riscos Mitigados**:
- SQL injection: Protegido por parametrização Entity Framework Core
- XSS: Framework codificação automática em respostas JSON
- CSRF: Não aplicável (auth JWT stateless)
- Mass Assignment: Mapeamento DTO previne overposting (tipos record, propriedades imutáveis)

---

## 13. Considerações de Performance

### Características de Performance Potenciais

| Endpoint | Chamadas de Banco | Caching | Risco N+1 | Notas |
|----------|-------------------|---------|-----------|-------|
| POST (Create) | 3 (Proposal + Evaluation + Update) | Nenhum | Baixo | Transacional, escrita única |
| GET (List) | 1 (Paged query) | Nenhum | Baixo | Paginado, eficiente |
| GET (ById) | 1 (Single entity) | Nenhum | Baixo | Lookup chave primária |
| POST (Response) | 2 (Evaluation + Proposal) | Nenhum | Baixo | Transacional, update condicional |

### Oportunidades de Otimização

**Estado Atual**:
- Limites de paginação previnem conjuntos de resultados massivos
- Camada de repositório provavelmente usa queries SQL eficientes
- Sem problemas de query N+1 aparentes em handlers

**Melhorias Potenciais** (Não Prescritivo):
- Caching de resposta para avaliações acessadas frequentemente
- Otimização de query de banco com indexação adequada em ProposalId e Status
- Considerar réplicas de leitura para operações GET
- Adicionar compressão de resposta para payloads de lista grandes

---

## 14. Conformidade & Padrões

### Conformidade com Padrões de Código

**Convenções C#**:
- ✅ PascalCase para métodos e propriedades públicas
- ✅ Uso de async/await apropriado
- ✅ Comentários de documentação XML presentes
- ✅ Nomes de variáveis significativos
- ✅ Indentação e formatação consistentes

**Melhores Práticas ASP.NET Core**:
- ✅ Controller deriva de ControllerBase (não Controller)
- ✅ Atributo [ApiController] para validação automática
- ✅ Atributo [Produces] para negociação de tipo de conteúdo
- ✅ Atributos ProducesResponseType para documentação OpenAPI
- ✅ Propagação de CancellationToken para operações async
- ✅ Injeção de construtor para dependências

### Qualidade da Documentação

**Documentação de Código**:
- Comentários XML presentes em todos métodos públicos
- Comentários em língua Portuguesa alinhados com domínio de negócio
- Descrições de parâmetros incluídas
- Documentação de valor de retorno incluída

**Documentação de API**:
- Estrutura de endpoint RESTful auto-documentável
- Uso de verbo HTTP alinha com RFC 2616
- Códigos de status seguem padrões HTTP
- Estruturas requisição/resposta implicitamente documentadas via tipos

---

## 15. Conclusão

### Resumo

O EvaluationController é um controller API bem estruturado e fino que implementa princípios de Clean Architecture adequadamente. Ele delega com sucesso toda lógica de negócio para a camada de aplicação enquanto trata preocupações específicas de HTTP como roteamento, autorização, e formatação de resposta. O componente segue padrões CQRS, usa injeção de dependência efetivamente, e mantém acoplamento fraco através de abstrações de interface.

### Avaliação de Saúde do Componente

| Aspecto | Classificação | Notas |
|---------|---------------|-------|
| Arquitetura | **Bom** | Separação limpa de preocupações, camadas adequadas |
| Qualidade de Código | **Bom** | Segue convenções, documentado, legível |
| Cobertura de Teste | **Regular** | Testes de handler presentes, testes de controller/integração ausentes |
| Segurança | **Bom** | Autenticação/autorização implementada, validação em lugar |
| Manutenibilidade | **Bom** | Baixa complexidade, responsabilidades claras, fracamente acoplado |
| Performance | **Bom** | Padrões de acesso a dados eficientes, paginação em lugar |
| Documentação | **Bom** | Comentários de código presentes, estrutura de API auto-documentável |

### Principais Pontos Fortes

1. **Clean Architecture**: Separação adequada entre camadas de API, aplicação e domínio
2. **Implementação CQRS**: Separação clara de comandos e queries com handlers dedicados
3. **Injeção de Dependência**: Injeção de construtor com abstrações de interface habilita testes
4. **Domain-Driven Design**: Modelo de domínio rico com lógica de negócio encapsulada
5. **Design RESTful**: Semântica HTTP seguida apropriadamente
6. **Logging**: Logging estruturado com informação contextual para debugging

### Áreas para Atenção

1. **Cobertura de Teste**: Adicionar testes de integração de controller e testes de handler faltantes
2. **Validação**: Considerar adicionar atributos de validação para DTOs de requisição
3. **Tratamento de Erro**: Revisar configuração de middleware de tratamento de exceção global
4. **User ID Parsing**: Tratar falha de parse de claim mais explicitamente que retornar EmptyGuid
5. **Comportamento de Paginação**: Documentar ou ajustar comportamento de auto-correção para clareza

---

**Análise Completada**: 23/01/2026 10:19:10
**Caminho do Componente**: `/services/commercial/1-Services/GestAuto.Commercial.API/Controllers/EvaluationController.cs`
**Analisado Por**: Agente de Análise Profunda de Componente
**Versão do Relatório**: 1.0
