# Revisão da Tarefa 9.0: Implementar Outbox Pattern e RabbitMQ Publisher

**Data da Revisão:** 9 de dezembro de 2025  
**Status:** ⚠️ IMPLEMENTAÇÃO INCOMPLETA  
**Severidade:** ALTA - Componentes críticos não implementados

---

## 1. Resultados da Validação da Definição da Tarefa

### ✅ Alinhamento com Requisitos

A tarefa está bem definida e alinhada com:
- **PRD**: Requisitos RF5.1-RF5.7 (Integração com Seminovos) dependem de eventos publicados corretamente
- **TechSpec**: Especificação detalhada de componentes, interfaces e padrões arquiteturais
- **Objetivos de Negócio**: Suporta garantia transacional de eventos e entrega at-least-once

### ✅ Dependências Satisfeitas

- **Bloqueado por 4.0 (Repositórios)**: ✅ Completado - Repositórios base implementados
- **Desbloqueia 11.0 (Consumers)**: ✅ Estrutura pronta para implementação
- **Paralelizável**: ✅ Pode executar junto com tasks 5.0 e 7.0

---

## 2. Descobertas da Análise de Regras

### Regras Aplicáveis Identificadas

#### 📄 `dotnet-architecture.md`
- **Clean Architecture**: Implementação deve seguir separação de camadas
- **Repository Pattern**: IOutboxRepository já existe, implementação parcial correta
- **CQRS Nativo**: Sem MediatR - padrão já adotado no projeto
- **Tratamento de Erros**: Logs estruturados obrigatórios

#### 📄 `dotnet-coding-standards.md`
- **Nomenclatura**: Usar camelCase para variáveis, PascalCase para classes
- **Métodos**: Máximo 50 linhas, verbo no início, máximo 3 parâmetros
- **Classes**: Máximo 300 linhas
- **Comentários**: Apenas agregam valor, não óbvios

#### 📄 `dotnet-observability.md`
- **Health Checks**: IHealthCheck necessário para RabbitMQ
- **CancellationToken**: Usar em todas operações async
- **Logging Estruturado**: ILogger<T> em todos serviços

#### 📄 `dotnet-performance.md`
- **Retry Pattern**: Implementar backoff exponencial
- **Connection Pooling**: RabbitMQ com reconexão automática

---

## 3. Resumo da Revisão de Código

### ✅ Implementado Corretamente

#### OutboxRepository (Infra/Repositories/OutboxRepository.cs)
```csharp
✅ Interface IOutboxRepository bem definida
✅ Métodos GetPendingMessagesAsync, MarkAsProcessed, MarkAsFailed
✅ Serialização com JsonSerializer.Serialize (System.Text.Json)
✅ BatchSize padrão = 50 (bom padrão)
✅ Ordenação por CreatedAt para FIFO
```

**Encontrado:** Nenhum problema crítico. Implementação segue boas práticas.

#### OutboxMessage Entity (Infra/Entities/OutboxMessage.cs)
```csharp
✅ Herda de BaseEntity (padrão estabelecido)
✅ Propriedades: EventType, Payload, ProcessedAt, Error
✅ Métodos MarkAsProcessed() e MarkAsFailed()
✅ Construtor privado para EF Core
✅ Índice criado em migration para ProcessedAt IS NULL
```

**Encontrado:** Nenhum problema. Estrutura correta.

#### UnitOfWork (Infra/UnitOfWork/UnitOfWork.cs)
```csharp
✅ Implementa IUnitOfWork com gerenciamento de transações
✅ CollectDomainEventsFromEntities() integra eventos das entidades
✅ Salva eventos no outbox antes de SaveChanges
✅ Limpa eventos após salvar com sucesso
```

**Encontrado:** Implementação transacional correta.

#### EntityConfiguration (Migrations)
```csharp
✅ Tabela outbox_messages criada com schema correto
✅ Campos: id, event_type, payload, created_at, processed_at, error
✅ Tipo JSONB para payload
✅ Índice em processed_at para queries eficientes
```

**Encontrado:** Nenhum problema. Migration válida.

---

### ❌ Não Implementado (CRÍTICO)

#### 1. IEventPublisher Interface
**Status:** ❌ Não existe  
**Localização esperada:** `Domain/Interfaces/IEventPublisher.cs`

**Impacto:** 
- Handlers de command/query não conseguem publicar eventos
- RabbitMqPublisher não tem interface padrão
- Difícil fazer mock em testes

**Será necessário:**
```csharp
namespace GestAuto.Commercial.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken) 
        where T : IDomainEvent;
}
```

#### 2. RabbitMqPublisher Implementation
**Status:** ❌ Não existe  
**Localização esperada:** `Infra/Messaging/RabbitMqPublisher.cs`

**Impacto:**
- Não há implementação de publicação para RabbitMQ
- Eventos salvos no outbox não são publicados
- Integração com seminovos/financeiro não funciona

**Requisitos por cumprir:**
- [ ] Usar RabbitMQ.Client (IConnection, IModel)
- [ ] Declarar exchange "commercial" do tipo Topic
- [ ] Implementar método PublishAsync<T>()
- [ ] Routing keys (lead.created, proposal.created, etc.)
- [ ] Properties: MessageId (EventId), Persistent=true
- [ ] Logging estruturado com ILogger<RabbitMqPublisher>
- [ ] Método privado GetRoutingKey(IDomainEvent)

#### 3. RabbitMqConfiguration
**Status:** ❌ Não existe  
**Localização esperada:** `Infra/Messaging/RabbitMqConfiguration.cs`

**Impacto:**
- Sem configuração centralizada
- HostName, Port, UserName hardcoded em múltiplos lugares
- Difícil manutenção e testes

**Requisitos:**
- [ ] Classe com properties: HostName, Port, UserName, Password, VirtualHost
- [ ] Const CommercialExchange = "commercial"
- [ ] Static class RoutingKeys com constantes (lead.created, proposal.created, etc.)
- [ ] Extension method RabbitMqExtensions.AddRabbitMq()

#### 4. OutboxProcessorService (BackgroundService)
**Status:** ❌ Não existe  
**Localização esperada:** `API/Services/OutboxProcessorService.cs`

**Impacto:**
- Eventos salvos no outbox nunca são publicados
- Sistema funciona mas sem comunicação com outros módulos
- Promessa de at-least-once delivery não é cumprida

**Requisitos principais:**
- [ ] Herda de BackgroundService
- [ ] Método ExecuteAsync(CancellationToken) que polling contínuo
- [ ] Injeção: IServiceScopeFactory, IEventPublisher, ILogger<T>
- [ ] Configuração: BatchSize=100, PollingInterval=5s, MaxRetries=3
- [ ] Lógica de deserialização de eventos
- [ ] Retry com backoff exponencial (não implementado no spec)
- [ ] Métricas Prometheus (OutboxMetrics)
- [ ] Logging estruturado

#### 5. RabbitMQ Health Check
**Status:** ❌ Não existe  
**Localização esperada:** `Infra/HealthChecks/RabbitMqHealthCheck.cs`

**Impacto:**
- Kubernetes/orchestradores não sabem status de RabbitMQ
- Load balancers podem enviar tráfego para instância com RabbitMQ down

**Requisitos:**
- [ ] Implementa IHealthCheck
- [ ] Verifica se IConnection.IsOpen
- [ ] Retorna HealthCheckResult (Healthy/Unhealthy)

#### 6. appsettings.json Configuration
**Status:** ⚠️ Incompleto  
**Localização:** `API/appsettings.json`

**Faltam:**
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "gestauto",
    "Password": "gestauto123",
    "VirtualHost": "/"
  },
  "OutboxProcessor": {
    "BatchSize": 100,
    "PollingIntervalSeconds": 5,
    "MaxRetries": 3
  }
}
```

#### 7. Program.cs Configuração
**Status:** ⚠️ Incompleto  
**Localização:** `API/Program.cs`

**Faltam:**
- [ ] `services.AddRabbitMq(configuration)`
- [ ] `services.AddScoped<OutboxProcessorService>()`
- [ ] `services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>()`
- [ ] `app.Services.GetRequiredService<OutboxProcessorService>()` para registrar no container

#### 8. NuGet Dependencies
**Status:** ⚠️ Possível falta  
**Pacotes necessários:**
- [ ] `RabbitMQ.Client` (versão 6.x)
- [ ] Verificar se .csproj contém referência

---

## 4. Problemas Identificados e Análise de Severidade

### 🔴 CRÍTICA (Bloqueia entrega)

| # | Problema | Arquivo | Descrição | Ação |
|----|----------|---------|-----------|------|
| 1 | IEventPublisher não existe | Domain/Interfaces | Interface crítica para publicação | Implementar interface |
| 2 | RabbitMqPublisher não implementado | Infra/Messaging | Publicação RabbitMQ quebrada | Implementar classe |
| 3 | OutboxProcessorService não existe | API/Services | Processamento outbox não acontece | Implementar BackgroundService |
| 4 | RabbitMQ não registrado no DI | API/Program.cs | Componentes RabbitMQ não injetáveis | Registrar em Program.cs |

### 🟠 ALTA (Deve ser feita antes de deploy)

| # | Problema | Arquivo | Descrição | Ação |
|----|----------|---------|-----------|------|
| 5 | RabbitMqConfiguration ausente | Infra/Messaging | Configuração centralizada | Criar classe + Extension |
| 6 | RabbitMq HealthCheck ausente | Infra/HealthChecks | Sem verificação de health | Implementar IHealthCheck |
| 7 | appsettings.json incompleto | API/appsettings.json | Sem config RabbitMQ | Adicionar seção |
| 8 | NuGet: RabbitMQ.Client | .csproj | Possível falta de pacote | Verificar/Instalar |

### 🟡 MÉDIA (Recomendado antes de release)

| # | Problema | Arquivo | Descrição | Ação |
|----|----------|---------|-----------|------|
| 9 | Retry com backoff | OutboxProcessorService | Backoff exponencial não definido | Implementar strategy |
| 10 | Métricas Prometheus | OutboxProcessorService | OutboxMetrics não integrado | Adicionar métricas |
| 11 | Script inicialização RabbitMQ | n/a | Sem bootstrap de exchanges | Criar script ou migration |

---

## 5. Conformidade com Padrões do Projeto

### ✅ Seguindo Corretamente

| Padrão | Evidência | Status |
|--------|-----------|--------|
| Clean Architecture | Separação: Domain/Application/Infra | ✅ |
| CQRS Nativo | Sem MediatR, handlers diretos | ✅ |
| Repository Pattern | IOutboxRepository implementado | ✅ |
| DDD | Entities com domain events | ✅ |
| Entity Framework Core | DbContext, DbSet<T>, migrations | ✅ |
| Logging Estruturado | ILogger<T> em UnitOfWork | ✅ |

### ❌ Não Conformes

| Padrão | Requisito | Status |
|--------|-----------|--------|
| Health Checks | Implementar IHealthCheck para RabbitMQ | ❌ |
| Dependency Injection | Registrar RabbitMqPublisher no DI | ❌ |
| Async/Await | CancellationToken em OutboxProcessorService | ❌ |
| Observability | Métricas Prometheus não integradas | ❌ |

---

## 6. Conformidade com Requisitos do PRD

### Requisitos de Implementação Relacionados

| Requisito | Descrição | Status |
|-----------|-----------|--------|
| RF5.2 | Enviar solicitação de avaliação (evento) | ❌ Bloquear por IEventPublisher |
| RF5.3 | Emitir evento AvaliacaoSeminovoSolicitada | ❌ Publicação RabbitMQ |
| RF4.12 | Emitir eventos PropostaCriada, PropostaAtualizada | ❌ Sem publicação |
| RF1.7 | Emitir evento LeadCriado | ❌ Sem publicação |
| NF-AUD | Auditoria de operações críticas | ⚠️ Parcial (UnitOfWork ok) |

---

## 7. Checklist de Critérios de Sucesso

Da tarefa 9.0:

- [ ] Eventos são salvos no outbox na mesma transação do banco ✅ **PARCIAL** - UnitOfWork implementado
- [ ] OutboxProcessor publica eventos pendentes no RabbitMQ ❌ **NÃO** - Service não existe
- [ ] Eventos publicados são marcados como processados ⚠️ **NÃO** - Sem publicação
- [ ] Erros de publicação são registrados no campo error ⚠️ **NÃO** - Sem publicação
- [ ] Health check de RabbitMQ funciona corretamente ❌ **NÃO** - Check não implementado
- [ ] Métricas de processamento são coletadas ❌ **NÃO** - Sem integração Prometheus
- [ ] Reconexão automática em caso de falha do RabbitMQ ❌ **NÃO** - Sem tratamento
- [ ] Testes de integração com Testcontainers passam ❌ **NÃO** - Testes não existem
- [ ] Logs estruturados registram todas as operações ⚠️ **PARCIAL** - UnitOfWork ok, RabbitMQ não
- [ ] Não há perda de eventos mesmo em caso de falha ⚠️ **PARCIAL** - Outbox ok, publicação não

---

## 8. Recomendações e Próximos Passos

### 🚀 Sequência de Implementação Recomendada

1. **[CRÍTICA]** Criar `Domain/Interfaces/IEventPublisher.cs`
   - Tempo estimado: 15 min
   - Bloqueador para tudo mais

2. **[CRÍTICA]** Criar `Infra/Messaging/RabbitMqConfiguration.cs`
   - Incluir extension method AddRabbitMq()
   - Tempo estimado: 30 min

3. **[CRÍTICA]** Criar `Infra/Messaging/RabbitMqPublisher.cs`
   - Implementar IEventPublisher
   - GetRoutingKey() para mapeamento de eventos
   - Tempo estimado: 1h

4. **[CRÍTICA]** Criar `API/Services/OutboxProcessorService.cs`
   - BackgroundService com polling
   - Deserialização dinâmica de eventos
   - Tempo estimado: 1.5h

5. **[ALTA]** Criar `Infra/HealthChecks/RabbitMqHealthCheck.cs`
   - Implementar IHealthCheck
   - Tempo estimado: 20 min

6. **[ALTA]** Atualizar `API/Program.cs`
   - Registrar serviços RabbitMQ no DI
   - Registrar health check
   - Registrar BackgroundService
   - Tempo estimado: 15 min

7. **[ALTA]** Atualizar `API/appsettings.json` e `appsettings.Development.json`
   - Adicionar seções RabbitMQ e OutboxProcessor
   - Tempo estimado: 10 min

8. **[MÉDIA]** Implementar retry com backoff exponencial
   - Adicionar Polly ou implementação manual
   - Tempo estimado: 45 min

9. **[MÉDIA]** Integrar Prometheus Metrics
   - OutboxMetrics classe
   - Registrar em Program.cs
   - Tempo estimado: 45 min

10. **[MÉDIA]** Criar testes de integração com Testcontainers
    - Testar fluxo completo outbox
    - Testar RabbitMQ publisher
    - Tempo estimado: 2h

11. **[BAIXA]** Criar script de inicialização de exchanges/queues
    - DLX (Dead Letter Exchange) para retry
    - Tempo estimado: 30 min

---

## 9. Requisitos Adicionais por Cumprir

### NuGet Packages

Verificar/instalar:
- `RabbitMQ.Client` (6.x ou 7.x)
- `Polly` (se usar para retry policy)
- `AspNetCore.HealthChecks.RabbitMQ` (opcional, simplifica health check)

### Migrations

✅ Já existe: Tabela `outbox_messages` criada corretamente

### Configuração Docker

Verificar `docker-compose.yml`:
- [ ] Serviço RabbitMQ configurado
- [ ] Variáveis de ambiente para credenciais
- [ ] Porta 5672 (AMQP) exposta
- [ ] Health check de RabbitMQ

---

## 10. Conclusão da Revisão

### Status Geral: ❌ **NÃO PRONTO PARA DEPLOY**

**Progresso de Implementação:**
- ✅ 40% - OutboxRepository e UnitOfWork (transactional message storage)
- ❌ 0% - RabbitMQ Publisher (event publication)
- ❌ 0% - OutboxProcessor Service (background processing)
- ❌ 0% - Health Checks (observability)
- ❌ 0% - Configuration (DI setup)

**Bloqueadores Críticos:**
1. IEventPublisher interface não existe
2. RabbitMqPublisher não implementado
3. OutboxProcessorService não implementado
4. Nenhum evento será publicado até que isso seja feito

**Estimativa de Esforço para Conclusão:**
- **Desenvolvimento:** 6-7 horas
- **Testes:** 2-3 horas
- **Total:** ~10 horas de trabalho

---

## 📝 Decisões de Design

### O que foi bem feito:

1. **OutboxMessage entity**: Design robusto com ProcessedAt e Error fields
2. **UnitOfWork transacional**: Garante atomicidade (evento + BD na mesma transação)
3. **Schema PostgreSQL**: JSONB para payload, índice em ProcessedAt para performance
4. **Serialização**: System.Text.Json (built-in, sem dependências extras)

### O que precisa ser feito:

1. **RabbitMQ connection pooling**: Usar IConnection singleton compartilhado
2. **Retry strategy**: Polly ou implementação manual com jitter
3. **Dead Letter Exchange**: Para eventos que falharam após retries
4. **Observability**: Correlação via EventId, logging estruturado com contexto

---

**Relatório preparado em:** 9 de dezembro de 2025 às 14:00  
**Revisor:** GitHub Copilot  
**Próxima revisão após:** Implementação dos componentes críticos
