# Relatório de Análise Profunda de Componente: TestDriveController

**Data da Análise**: 23/01/2026
**Componente**: TestDriveController
**Localização**: `services/commercial/1-Services/GestAuto.Commercial.API/Controllers/TestDriveController.cs`
**Linhas de Código**: 282
**Linguagem**: C# (.NET 8+)
**Camada Arquitetural**: Camada de Apresentação (API)

---

## 1. Resumo Executivo

O TestDriveController é um controlador API REST responsável por gerenciar operações de test drives no sistema de gestão de veículos comerciais GestAuto. Ele serve como ponto de entrada para todas as requisições HTTP relacionadas a test drives de veículos, implementando operações CRUD e transições de estado (agendar, completar, cancelar) seguindo princípios CQRS com separação de comando/consulta.

**Principais Responsabilidades**:
- Agendamento de test drives para leads
- Conclusão de test drives com checklist de inspeção de veículo
- Cancelamento de test drives
- Listagem paginada com capacidades de filtragem
- Recuperação individual de test drives

**Principais Descobertas**:
- Implementação de arquitetura limpa com separação adequada de preocupações
- Tratamento de erros abrangente com códigos de status HTTP específicos
- Autorização baseada em função com política SalesPerson
- Integração com 5 manipuladores de comando/consulta (handlers)
- Uso adequado de DTOs para mapeamento de requisição/resposta
- Logging em pontos-chave de operação
- Filtragem de vendedor através de serviço dedicado

---

## 2. Análise de Fluxo de Dados

### 2.1 Fluxo de Agendamento de Test Drive

```
1. HTTP POST /api/v1/test-drives
   ↓
2. Método TestDriveController.Schedule() (linhas 62-92)
   ↓
3. GetCurrentUserId() extrai usuário dos claims JWT (linhas 277-281)
   ↓
4. Cria ScheduleTestDriveCommand com dados da requisição + salesPersonId (linhas 71-77)
   ↓
5. _scheduleHandler.HandleAsync(command, cancellationToken) (linha 79)
   ↓
6. ScheduleTestDriveHandler valida existência do lead (TestDriveCommandHandlers.cs:32-33)
   ↓
7. Handler verifica disponibilidade do veículo (TestDriveCommandHandlers.cs:36-42)
   ↓
8. Handler cria entidade TestDrive via TestDrive.Schedule() (TestDriveCommandHandlers.cs:45-51)
   ↓
9. Handler atualiza status do lead para TestDriveScheduled (TestDriveCommandHandlers.cs:56-57)
   ↓
10. Handler salva no banco de dados via UnitOfWork (TestDriveCommandHandlers.cs:59)
   ↓
11. Retorna TestDriveResponse mapeado da entidade (TestDriveCommandHandlers.cs:61)
   ↓
12. Controller retorna 201 Created com header Location (linha 80)
```

### 2.2 Fluxo de Listagem de Test Drives

```
1. HTTP GET /api/v1/test-drives com parâmetros de consulta
   ↓
2. Método TestDriveController.List() (linhas 110-144)
   ↓
3. _salesPersonFilter.GetCurrentSalesPersonId() para filtragem (linha 119)
   ↓
4. Valida parâmetros page e pageSize (linhas 122-123)
   ↓
5. Cria ListTestDrivesQuery com filtros (linhas 127-134)
   ↓
6. _listHandler.HandleAsync(query, cancellationToken) (linha 136)
   ↓
7. ListTestDrivesHandler consulta contagem total (TestDriveQueryHandlers.cs:43-49)
   ↓
8. Handler consulta test drives paginados do repositório (TestDriveQueryHandlers.cs:52-60)
   ↓
9. Handler enriquece cada item com nome do lead via lead repository (TestDriveQueryHandlers.cs:64-75)
   ↓
10. Handler retorna PagedResponse com itens e metadados (TestDriveQueryHandlers.cs:77-81)
   ↓
11. Controller retorna 200 OK com resposta paginada (linha 137)
```

### 2.3 Fluxo de Conclusão de Test Drive

```
1. HTTP POST /api/v1/test-drives/{id}/complete
   ↓
2. Método TestDriveController.Complete() (linhas 197-232)
   ↓
3. GetCurrentUserId() extrai usuário que completou (linhas 277-281)
   ↓
4. Cria CompleteTestDriveCommand com dados do checklist (linhas 207-212)
   ↓
5. _completeHandler.HandleAsync(command, cancellationToken) (linha 214)
   ↓
6. CompleteTestDriveHandler recupera entidade TestDrive (TestDriveCommandHandlers.cs:82-83)
   ↓
7. Handler faz parse do enum fuel level de string (TestDriveCommandHandlers.cs:86)
   ↓
8. Handler cria objeto de valor TestDriveChecklist (TestDriveCommandHandlers.cs:87-92)
   ↓
9. Handler chama método de domínio testDrive.Complete() (TestDriveCommandHandlers.cs:94)
   ↓
10. Domínio valida transição de status e atualiza entidade (TestDrive.cs:56-71)
   ↓
11. Handler salva no banco de dados (TestDriveCommandHandlers.cs:96-97)
   ↓
12. Retorna TestDriveResponse com dados completados (TestDriveCommandHandlers.cs:99)
   ↓
13. Controller retorna 200 OK (linha 215)
```

### 2.4 Fluxo de Cancelamento de Test Drive

```
1. HTTP POST /api/v1/test-drives/{id}/cancel
   ↓
2. Método TestDriveController.Cancel() (linhas 250-275)
   ↓
3. GetCurrentUserId() extrai usuário que cancelou (linhas 277-281)
   ↓
4. Cria CancelTestDriveCommand com motivo (linha 260)
   ↓
5. _cancelHandler.HandleAsync(command, cancellationToken) (linha 261)
   ↓
6. CancelTestDriveHandler recupera entidade TestDrive (TestDriveCommandHandlers.cs:120-121)
   ↓
7. Handler chama método de domínio testDrive.Cancel() (TestDriveCommandHandlers.cs:123)
   ↓
8. Domínio valida que status não pode ser Completed (TestDrive.cs:73-81)
   ↓
9. Handler atualiza banco de dados (TestDriveCommandHandlers.cs:125-126)
   ↓
10. Retorna TestDriveResponse com status cancelado (TestDriveCommandHandlers.cs:128)
   ↓
11. Controller retorna 200 OK (linha 263)
```

---

## 3. Regras de Negócio e Lógica

### 3.1 Visão Geral das Regras de Negócio

| Tipo de Regra | Descrição | Localização |
|---------------|-----------|-------------|
| Validação | ID do Lead é obrigatório | TestDriveValidators.cs:10-11 |
| Validação | ID do Veículo é obrigatório | TestDriveValidators.cs:13-14 |
| Validação | Data do Test-drive deve ser futura | TestDriveValidators.cs:16-18 |
| Validação | Data do Test-drive dentro de 3 meses | TestDriveValidators.cs:18 |
| Validação | ID do SalesPerson é obrigatório | TestDriveValidators.cs:20-21 |
| Validação | Checklist obrigatório para conclusão | TestDriveValidators.cs:32-33 |
| Validação | Quilometragem inicial não negativa | TestDriveValidators.cs:35-37 |
| Validação | Quilometragem final >= quilometragem inicial | TestDriveValidators.cs:39-42 |
| Validação | Nível de combustível deve ser valor enum válido | TestDriveValidators.cs:44-46 |
| Autorização | Apenas role SalesPerson pode acessar | TestDriveController.cs:17 |
| Autorização | Gerentes ignoram filtro de vendedor | SalesPersonFilterService.cs:39-46 |
| Lógica de Negócio | Verificação de disponibilidade do veículo (1 hora padrão) | TestDriveRepository.cs:62-81 |
| Lógica de Negócio | Status do Lead atualiza para TestDriveScheduled | TestDriveCommandHandlers.cs:56 |
| Lógica de Negócio | Apenas test drives agendados podem ser completados | TestDrive.cs:58-59 |
| Lógica de Negócio | Test drives completados não podem ser cancelados | TestDrive.cs:75-76 |
| Lógica de Negócio | Cancelamento requer motivo | TestDriveController.cs:252 |

### 3.2 Regras de Negócio Detalhadas

#### Regra: Validação de Agendamento de Test-Drive

**Visão Geral**:
O agendamento de test-drive impõe múltiplas regras de validação para garantir a integridade do negócio. O sistema valida se todas as entidades necessárias existem (lead, veículo, vendedor), se o horário agendado é válido (data futura dentro de 3 meses), e se o veículo está disponível no horário solicitado.

**Descrição Detalhada**:
Ao agendar um test drive, o sistema realiza um processo de validação em várias camadas. Primeiro, no nível do controlador da API, a requisição é mapeada para um comando com o ID do usuário atual extraído automaticamente dos claims JWT. O comando então flui para o ScheduleTestDriveHandler que realiza validações críticas de negócio. O handler verifica se o lead referenciado existe no banco de dados; se não encontrado, uma NotFoundException é lançada. Em seguida, o sistema verifica a disponibilidade do veículo consultando quaisquer test drives agendados existentes para o mesmo veículo dentro de uma janela de 1 hora do horário solicitado. Isso previne conflitos de reserva dupla. Uma vez que as validações passam, o handler cria uma entidade TestDrive usando um método de fábrica (TestDrive.Schedule) que impõe regras de nível de domínio, incluindo que o horário agendado deve ser no futuro e todos os IDs devem ser GUIDs válidos. Após criar o test drive, o sistema atualiza automaticamente o status do lead associado para TestDriveScheduled, refletindo a progressão no pipeline de vendas. Finalmente, todas as mudanças são persistidas atomicamente através de um padrão UnitOfWork.

**Fluxo**:
`TestDriveController.Schedule()` → `ScheduleTestDriveCommand` → `ScheduleTestDriveHandler.HandleAsync()` → Verificação de existência do Lead → Verificação de disponibilidade do veículo → Fábrica `TestDrive.Schedule()` → Atualização de status do Lead → Commit no banco via UnitOfWork → Retorno `TestDriveResponse`

---

#### Regra: Conclusão de Test-Drive com Checklist

**Visão Geral**:
A conclusão do test-drive requer um checklist de inspeção do veículo obrigatório documentando a condição do veículo antes e após o drive. O checklist captura leituras de quilometragem, nível de combustível e observações visuais, com regras de validação garantindo consistência dos dados.

**Descrição Detalhada**:
Ao completar um test drive, o sistema requer um checklist abrangente documentando o estado do veículo. O checklist é um objeto de valor (TestDriveChecklistDto) contendo quatro informações críticas: quilometragem inicial (deve ser não negativa), quilometragem final (deve ser maior ou igual à quilometragem inicial), nível de combustível (deve corresponder a um valor enum válido: Empty, Low, Half, ThreeQuarter ou Full), e observações visuais opcionais. O CompleteTestDriveValidator impõe essas regras usando FluentValidation. Quando o CompleteTestDriveHandler processa a requisição, ele primeiro recupera a entidade TestDrive e lança NotFoundException se não encontrada. O handler então faz o parse da string de nível de combustível para o enum FuelLevel fortemente tipado e constrói o objeto de valor TestDriveChecklist. O método Complete() do domínio é chamado, o qual valida se o test drive está no status Scheduled (apenas drives agendados podem ser completados). Após o sucesso, o sistema atualiza o status do test drive para Completed, registra o timestamp de conclusão (CompletedAt), armazena o checklist e feedback do cliente, e emite um evento de domínio TestDriveCompletedEvent. Todas as mudanças são persistidas atomicamente.

**Fluxo**:
`TestDriveController.Complete()` → `CompleteTestDriveCommand` com checklist → `CompleteTestDriveHandler.HandleAsync()` → Recuperar entidade TestDrive → Parse do enum nível de combustível → Criar objeto de valor `TestDriveChecklist` → Método de domínio `testDrive.Complete()` → Atualizar repositório → Commit UnitOfWork → Retorno `TestDriveResponse` com checklist

---

#### Regra: Restrições de Cancelamento de Test-Drive

**Visão Geral**:
O cancelamento de test-drive permite que test drives agendados ou no-show sejam cancelados com um motivo documentado. No entanto, o sistema impõe uma regra de negócio crítica: test drives completados não podem ser cancelados pois representam uma transação de negócio finalizada.

**Descrição Detalhada**:
O fluxo de cancelamento implementa uma guarda de transição de estado para prevenir mudanças de estado inválidas. Quando uma requisição de cancelamento é recebida, o CancelTestDriveHandler recupera a entidade TestDrive alvo e lança NotFoundException se ela não existir. O handler então chama o método Cancel() do domínio, que implementa uma cláusula de guarda verificando o status atual. Se o test drive já estiver no status Completed, o método lança uma InvalidOperationException com a mensagem "Completed test drives cannot be cancelled". Isso previne cancelamento irreversível de test drives finalizados que já têm feedback de cliente, registros de quilometragem e dados de checklist completados. Para drives agendados ou no-show, o cancelamento prossegue atualizando o status para Cancelled, registrando o motivo do cancelamento e atualizando o timestamp. O sistema requer um parâmetro de motivo de cancelamento (embora anulável) para capturar contexto para análises de negócio. Todas as mudanças são persistidas através do repositório e padrão UnitOfWork.

**Fluxo**:
`TestDriveController.Cancel()` → `CancelTestDriveCommand` com motivo → `CancelTestDriveHandler.HandleAsync()` → Recuperar entidade TestDrive → Método de domínio `testDrive.Cancel()` (valida se não está Completed) → Atualizar repositório → Commit UnitOfWork → Retorno `TestDriveResponse` com status cancelado

---

#### Regra: Filtragem e Autorização de Vendedores

**Visão Geral**:
O sistema implementa filtragem de dados baseada em função para garantir que vendedores acessem apenas seus próprios test drives enquanto gerentes têm visibilidade global. A política de autorização "SalesPerson" é aplicada no nível do controlador, e um serviço especializado lida com filtragem em tempo de execução baseada em funções do usuário.

**Descrição Detalhada**:
O controle de acesso opera em dois níveis: autenticação/autorização e filtragem de dados. No nível do controlador, o atributo [Authorize(Policy = "SalesPerson")] garante que apenas usuários autenticados com a role SalesPerson podem acessar quaisquer endpoints de test drive. Dentro do endpoint List, o método ISalesPersonFilterService.GetCurrentSalesPersonId() implementa filtragem baseada em função. Este serviço inspeciona os claims do usuário HTTP atual para determinar se ele é um gerente (verificando claims de role MANAGER, SALES_MANAGER ou ADMIN). Gerentes recebem null de GetCurrentSalesPersonId(), o que se traduz em "sem filtro" nas consultas, permitindo que vejam todos os test drives de todos os vendedores. Vendedores regulares têm seu claim sales_person_id extraído e retornado, fazendo com que ListTestDrivesQuery filtre resultados apenas para seus test drives atribuídos. Para operações de não-listagem (get por ID, complete, cancel), o sistema conta com padrões de acesso em nível de repositório e assume que vendedores só podem acessar test drives que criaram ou aos quais estão atribuídos. O método auxiliar GetCurrentUserId() extrai o claim "sub" do JWT para fins de trilha de auditoria, registrando qual usuário realizou operações de agendamento/conclusão/cancelamento.

**Fluxo**:
Requisição HTTP com JWT → Verificação `[Authorize(Policy = "SalesPerson")]` → `ISalesPersonFilterService.GetCurrentSalesPersonId()` → Verificar claims de usuário para role de gerente → Retornar ID do vendedor (ou null para gerentes) → Aplicar filtro a `ListTestDrivesQuery` → Repositório filtra resultados por ID do vendedor

---

#### Regra: Detecção de Conflito de Disponibilidade de Veículo

**Visão Geral**:
O sistema previne reserva dupla de veículos implementando uma verificação de disponibilidade sofisticada que detecta conflitos de agendamento. O algoritmo usa uma janela de duração padrão de 1 hora e verifica sobreposição de test drives agendados.

**Descrição Detalhada**:
A verificação de disponibilidade de veículo ocorre durante o agendamento de test drive e é crítica para prevenir insatisfação do cliente por overbooking. O método CheckVehicleAvailabilityAsync no TestDriveRepository implementa o algoritmo de detecção de conflito. Quando chamado com um ID de veículo e horário agendado, o método padroniza para uma duração de 1 hora se não especificado, então calcula um horário de término adicionando esta duração ao horário de início agendado. A consulta então busca no banco de dados por quaisquer test drives correspondentes ao ID do veículo com status Scheduled que se sobreponham à janela de tempo solicitada. A lógica de detecção de sobreposição verifica duas condições: (1) test drives existentes onde o horário de início agendado é antes do horário de término solicitado E o horário de conclusão (se definido) é após o horário de início solicitado, OU (2) test drives que ainda não foram completados (CompletedAt é null) mas iniciam antes do horário de término solicitado. Isso captura tanto sobreposições exatas quanto parciais. Se quaisquer test drives conflitantes forem encontrados (hasConflict é true), o método retorna false (não disponível). O ScheduleTestDriveHandler captura isso e lança uma DomainException com a mensagem "Vehicle is not available at the requested time". Esta validação ocorre antes da criação do test drive, prevenindo entrada de dados inválidos no sistema.

**Fluxo**:
`ScheduleTestDriveHandler.HandleAsync()` → `CheckVehicleAvailabilityAsync(vehicleId, scheduledAt)` → Calcular endTime = scheduledAt + 1 hora → Consultar test-drives agendados existentes para veículo → Verificar condição de sobreposição (existing.ScheduledAt < requested.endTime AND existing.CompletedAt > requested.scheduledAt) → Retornar !hasConflict

---

---

## 4. Estrutura do Componente

```
TestDriveController (Camada de API)
├── Dependências (Injeção de Construtor)
│   ├── ICommandHandler<ScheduleTestDriveCommand, TestDriveResponse> _scheduleHandler
│   ├── ICommandHandler<CompleteTestDriveCommand, TestDriveResponse> _completeHandler
│   ├── ICommandHandler<CancelTestDriveCommand, TestDriveResponse> _cancelHandler
│   ├── IQueryHandler<GetTestDriveQuery, TestDriveResponse> _getHandler
│   ├── IQueryHandler<ListTestDrivesQuery, PagedResponse<TestDriveListItemResponse>> _listHandler
│   ├── ISalesPersonFilterService _salesPersonFilter
│   └── ILogger<TestDriveController> _logger
│
├── Métodos Públicos (Endpoints)
│   ├── Schedule() - POST /api/v1/test-drives (linhas 62-92)
│   ├── List() - GET /api/v1/test-drives (linhas 110-144)
│   ├── GetById() - GET /api/v1/test-drives/{id} (linhas 159-176)
│   ├── Complete() - POST /api/v1/test-drives/{id}/complete (linhas 197-232)
│   └── Cancel() - POST /api/v1/test-drives/{id}/cancel (linhas 250-275)
│
├── Métodos Auxiliares Privados
│   └── GetCurrentUserId() - Extrair ID de usuário dos claims JWT (linhas 277-281)
│
└── Configuração
    ├── Rota: "api/v1/test-drives"
    ├── Autorização: política "SalesPerson"
    └── Tipo de Resposta: "application/json"
```

**Arquivos Relacionados**:

```
Dependências do TestDriveController:

DTOs de Requisição/Resposta:
└── 2-Application/GestAuto.Commercial.Application/DTOs/TestDriveDTOs.cs
    ├── ScheduleTestDriveRequest (entrada para agendamento)
    ├── CompleteTestDriveRequest (entrada para conclusão)
    ├── CancelTestDriveRequest (entrada para cancelamento)
    ├── TestDriveChecklistDto (dados do checklist)
    ├── TestDriveResponse (saída)
    ├── TestDriveChecklistResponse (saída aninhada)
    └── TestDriveListItemResponse (saída de item de lista)

Comandos:
└── 2-Application/GestAuto.Commercial.Application/Commands/TestDriveCommands.cs
    ├── ScheduleTestDriveCommand
    ├── CompleteTestDriveCommand
    └── CancelTestDriveCommand

Queries:
└── 2-Application/GestAuto.Commercial.Application/Queries/TestDriveQueries.cs
    ├── GetTestDriveQuery
    └── ListTestDrivesQuery

Validadores:
└── 2-Application/GestAuto.Commercial.Application/Validators/TestDriveValidators.cs
    ├── ScheduleTestDriveValidator
    ├── CompleteTestDriveValidator
    └── CancelTestDriveValidator
```
