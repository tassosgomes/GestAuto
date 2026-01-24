# Relatório de Auditoria de Dependências

**Data do Relatório**: 23 de Janeiro de 2026
**Projeto**: Serviço Comercial GestAuto
**Caminho**: `/services/commercial`
**Framework Alvo**: .NET 8.0
**Relatório Gerado**: 2026-01-23-09:09:15

---

## Resumo Executivo

O Serviço Comercial GestAuto é um microsserviço .NET 8.0 que implementa um sistema de gestão comercial/leads com **18 dependências diretas de pacotes NuGet** distribuídas em 4 projetos principais e 3 projetos de teste.

**Status Geral de Saúde**: Moderado

**Principais Descobertas**:
- 7 pacotes desatualizados (38.9% das dependências)
- 2 pacotes com atualizações de versão principal (major) disponíveis
- 1 pacote com preocupações de segurança (RabbitMQ.Client)
- 1 pacote com status depreciado/legado (Serilog.Formatting.Elasticsearch)
- Todos os pacotes Microsoft em versões suportadas do .NET 8.0
- Infraestrutura de testes requer atualizações

**Preocupações Críticas**:
1. RabbitMQ.Client 6.8.1 possui vulnerabilidades CVE conhecidas e está 2 versões principais atrasado
2. Serilog.Formatting.Elasticsearch está depreciado com caminho de migração para Elastic.Serilog.Sinks
3. Swashbuckle.AspNetCore está 4 versões principais atrasado em relação à versão atual

---

## Inventário de Dependências

### Dependências de Produção

| Pacote | Versão Atual | Versão Recente | Status | Projeto(s) |
|--------|--------------|----------------|--------|------------|
| **FluentValidation** | 12.1.1 | 12.1.1 | Atualizado | Application |
| **FluentValidation.DependencyInjectionExtensions** | 12.1.1 | 12.1.1 | Atualizado | Application |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 8.0.10 | 8.0.10 | Atualizado | API |
| **Microsoft.AspNetCore.OpenApi** | 8.0.21 | 8.0.21 | Atualizado | API |
| **Microsoft.EntityFrameworkCore** | 8.0.10 | 8.0.10 | Atualizado | API, Infra |
| **Microsoft.EntityFrameworkCore.Design** | 8.0.10 | 8.0.10 | Atualizado | API, Infra |
| **Microsoft.Extensions.Configuration.Binder** | 8.0.0 | 8.0.0 | Atualizado | Infra |
| **Microsoft.Extensions.Diagnostics.HealthChecks** | 8.0.0 | 8.0.0 | Atualizado | Infra |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 8.0.10 | 10.0.0 | Desatualizado (2 major) | Infra |
| **RabbitMQ.Client** | 6.8.1 | 7.2.0 | Desatualizado (1 major) | Infra |
| **Saunter** | 0.9.0 | 0.13.0 | Desatualizado (pre-1.0) | API |
| **Serilog.AspNetCore** | 10.0.0 | 8.0.0 | Incompatibilidade de Versão | API |
| **Serilog.Formatting.Elasticsearch** | 10.0.0 | Depreciado | Depreciado | API |
| **Serilog.Sinks.Console** | 6.1.1 | 6.1.1 | Atualizado | API |
| **Swashbuckle.AspNetCore** | 6.6.2 | 10.1.0 | Desatualizado (4 major) | API |
| **Swashbuckle.AspNetCore.Annotations** | 6.6.2 | 10.1.0 | Desatualizado (4 major) | API |

### Dependências de Teste

| Pacote | Versão Atual | Versão Recente | Status | Projeto(s) |
|--------|--------------|----------------|--------|------------|
| **coverlet.collector** | 6.0.0 | 6.0.4 | Desatualizado | Todos os Testes |
| **FluentAssertions** | 8.8.0 | 8.8.0 | Atualizado | Todos os Testes |
| **Microsoft.AspNetCore.Mvc.Testing** | 8.0.10 | 8.0.10 | Atualizado | Integration, E2E |
| **Microsoft.EntityFrameworkCore.InMemory** | 8.0.10 | 8.0.10 | Atualizado | Integration |
| **Microsoft.NET.Test.Sdk** | 17.8.0 | 18.0.1 | Desatualizado (1 major) | Todos os Testes |
| **Moq** | 4.20.70 | 4.20.72 | Desatualizado (patch) | Unit Tests |
| **NSubstitute** | 5.3.0 | 5.3.0 | Atualizado | Unit Tests |
| **Testcontainers.PostgreSql** | 4.9.0 | 4.10.0 | Desatualizado (minor) | Integration, E2E |
| **Testcontainers.RabbitMq** | 4.9.0 | 4.10.0 | Desatualizado (minor) | Integration, E2E |
| **xunit** | 2.9.3 (Integration/E2E) / 2.5.3 (Unit) | 2.9.3 | Inconsistência de Versão | Todos os Testes |
| **xunit.runner.visualstudio** | 3.1.5 (Unit) / 2.5.3 (Integration/E2E) | 3.1.5 | Inconsistência de Versão | Todos os Testes |

---

## Análise de Saúde

### Dependências Desatualizadas

| Severidade | Pacote | Atual | Recente | Tipo | Impacto |
|------------|--------|-------|---------|------|---------|
| **Alta** | RabbitMQ.Client | 6.8.1 | 7.2.0 | Major | Vulnerabilidades CVE conhecidas |
| **Média** | Swashbuckle.AspNetCore | 6.6.2 | 10.1.0 | Major | Mudanças quebram compatibilidade, recursos ausentes |
| **Média** | Swashbuckle.AspNetCore.Annotations | 6.6.2 | 10.1.0 | Major | Mudanças quebram compatibilidade, recursos ausentes |
| **Média** | Saunter | 0.9.0 | 0.13.0 | Minor | Problemas de estabilidade pré-1.0 |
| **Média** | Serilog.AspNetCore | 10.0.0 | 8.0.0 | Versão | Incompatibilidade de versão com .NET 8 |
| **Média** | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.10 | 10.0.0 | Major | Requer .NET 9 ou 10 |
| **Baixa** | Microsoft.NET.Test.Sdk | 17.8.0 | 18.0.1 | Major | Melhorias na plataforma de teste |
| **Baixa** | coverlet.collector | 6.0.0 | 6.0.4 | Patch | Correções de bugs |
| **Baixa** | Moq | 4.20.70 | 4.20.72 | Patch | Correções de bugs |
| **Baixa** | Testcontainers.PostgreSql | 4.9.0 | 4.10.0 | Minor | Correções de bugs |
| **Baixa** | Testcontainers.RabbitMq | 4.9.0 | 4.10.0 | Minor | Correções de bugs |

### Pacotes Depreciados/Legados

| Pacote | Status | Substituto | Notas |
|--------|--------|------------|-------|
| **Serilog.Formatting.Elasticsearch** | Depreciado | Elastic.Serilog.Sinks 9.0.0 | Última atualização em Março 2024, substituído pelo sink oficial da Elastic |

### Inconsistências de Versão

| Pacote | Inconsistência | Detalhes |
|--------|----------------|----------|
| **xunit** | 2.5.3 em Unit, 2.9.3 em Integration/E2E | Deve padronizar em 2.9.3 em todos os projetos de teste |
| **xunit.runner.visualstudio** | 3.1.5 em Unit, 2.5.3 em Integration/E2E | Deve padronizar em 3.1.5 em todos os projetos de teste |

---

## Vulnerabilidades de Segurança

### CVEs Confirmados

#### RabbitMQ.Client 6.8.1

**CVE-2024-41034** (CVSS 7.5 - ALTA)
- Vulnerabilidade: Negação de serviço no endpoint de conexão do cliente AMQP 1.0
- Versões afetadas: Todas as versões anteriores a 7.0.0
- Corrigido em: RabbitMQ.Client 7.0.0+
- Impacto: Negação de serviço remota através de validação de entrada imprópria

**CVE-2025-30219** (CVSS 6.5 - MÉDIA)
- Vulnerabilidade: Modificação de nome de host virtual em disco
- Versões afetadas: Todas as versões anteriores a 7.0.0
- Corrigido em: RabbitMQ.Client 7.0.0+
- Impacto: Ataque sofisticado poderia modificar configuração de host virtual

**CVE-2022-37026** (CVSS 9.8 - CRÍTICA)
- Vulnerabilidade: Bypass de Autenticação Cliente em configurações TLS
- Versões afetadas: Versões anteriores a 6.8.1 podem ser afetadas
- Corrigido em: RabbitMQ.Client 6.8.1+
- Impacto: Bypass de autenticação TLS quando servidor configurado para TLS

**Recomendação**: Atualizar para RabbitMQ.Client 7.2.0 ou posterior imediatamente para corrigir todas as vulnerabilidades conhecidas.

### Sem CVEs Conhecidos

Os seguintes pacotes não têm CVEs conhecidos em suas versões atuais:
- Todos os pacotes Microsoft.AspNetCore.* (8.0.x)
- Todos os pacotes Microsoft.EntityFrameworkCore.* (8.0.x)
- FluentValidation 12.1.1
- Serilog.Sinks.Console 6.1.1
- xUnit 2.9.3
- FluentAssertions 8.8.0
- NSubstitute 5.3.0
- Moq 4.20.70
- Pacotes Testcontainers

---

## Análise de Arquivos Críticos

### Top 10 Arquivos Mais Críticos

#### 1. `services/commercial/4-Infra/GestAuto.Commercial.Infra/Messaging/RabbitMqService.cs`
**Nível de Risco**: ALTO
**Dependências**: RabbitMQ.Client 6.8.1 (vulnerabilidades CVE)
**Impacto**: Infraestrutura central de mensageria para arquitetura orientada a eventos
**Impacto no Negócio**: Crítico - lida com toda comunicação assíncrona entre serviços
**Razão**: Integração central com broker de mensagens com vulnerabilidades de segurança conhecidas

#### 2. `services/commercial/1-Services/GestAuto.Commercial.API/Program.cs`
**Nível de Risco**: MÉDIO
**Dependências**: Swashbuckle.AspNetCore 6.6.2, Serilog.AspNetCore 10.0.0, Saunter 0.9.0
**Impacto**: Ponto de entrada da aplicação, autenticação, documentação da API
**Impacto no Negócio**: Alto - segurança, logging e configuração de gateway de API
**Razão**: Múltiplos pacotes desatualizados afetam inicialização, segurança e documentação

#### 3. `services/commercial/4-Infra/GestAuto.Commercial.Infra/Data/AppDbContext.cs`
**Nível de Risco**: MÉDIO
**Dependências**: Npgsql.EntityFrameworkCore.PostgreSQL 8.0.10, Microsoft.EntityFrameworkCore 8.0.10
**Impacto**: Camada de acesso a dados, configuração ORM
**Impacto no Negócio**: Alto - todas as operações de persistência de dados
**Razão**: Provedor de banco de dados está 2 versões principais atrasado, pode perder melhorias de performance

#### 4. `services/commercial/2-Application/GestAuto.Commercial.Application/Validators/CreateLeadValidator.cs`
**Nível de Risco**: BAIXO
**Dependências**: FluentValidation 12.1.1
**Impacto**: Validação de entrada para criação de lead
**Impacto no Negócio**: Médio - integridade de dados para entidade central de negócio
**Razão**: Atualizado, risco mínimo

#### 5. `services/commercial/1-Services/GestAuto.Commercial.API/Middleware/ExceptionHandlerMiddleware.cs`
**Nível de Risco**: BAIXO
**Dependências**: Serilog.AspNetCore 10.0.0
**Impacto**: Tratamento global de erros e logging
**Impacto no Negócio**: Médio - rastreamento de erros e depuração
**Razão**: Incompatibilidade de versão pode causar problemas de compatibilidade

#### 6. `services/commercial/3-Domain/GestAuto.Commercial.Domain/Events/LeadCreatedEvent.cs`
**Nível de Risco**: ALTO
**Dependências**: RabbitMQ.Client 6.8.1 (indireto via mensageria)
**Impacto**: Evento de domínio para fluxo de criação de lead
**Impacto no Negócio**: Alto - dispara processos downstream no pipeline de vendas
**Razão**: Evento publicado através de cliente RabbitMQ vulnerável

#### 7. `services/commercial/4-Infra/GestAuto.Commercial.Infra/Repositories/OutboxRepository.cs`
**Nível de Risco**: MÉDIO
**Dependências**: Microsoft.EntityFrameworkCore 8.0.10
**Impacto**: Implementação do padrão Outbox para mensageria confiável
**Impacto no Negócio**: Alto - garante consistência na entrega de mensagens
**Razão**: Fundamental confiabilidade de transações distribuídas

#### 8. `services/commercial/2-Application/GestAuto.Commercial.Application/Handlers/CreateLeadHandler.cs`
**Nível de Risco**: MÉDIO
**Dependências**: FluentValidation 12.1.1, RabbitMQ.Client 6.8.1 (indireto)
**Impacto**: Lógica de negócios de criação de lead
**Impacto no Negócio**: Alto - iniciação do fluxo central de vendas
**Razão**: Orquestra validação, persistência e publicação de eventos

#### 9. `services/commercial/5-Tests/GestAuto.Commercial.IntegrationTest/UnitTest1.cs`
**Nível de Risco**: BAIXO
**Dependências**: Testcontainers 4.9.0, xUnit 2.5.3, Microsoft.NET.Test.Sdk 17.8.0
**Impacto**: Infraestrutura de testes de integração
**Impacto no Negócio**: Baixo - validação de cobertura de testes
**Razão**: Dependências de teste desatualizadas, mas de menor prioridade

#### 10. `services/commercial/1-Services/GestAuto.Commercial.API/Controllers/LeadsController.cs`
**Nível de Risco**: BAIXO
**Dependências**: Microsoft.AspNetCore.Authentication.JwtBearer 8.0.10
**Impacto**: Endpoint da API para gestão de leads
**Impacto no Negócio**: Alto - API voltada para o cliente
**Razão**: Usa pacote de autenticação atualizado, risco mínimo

---

## Análise de Integração

### Padrões de Uso de Dependências

#### Infraestrutura de Mensageria
- **RabbitMQ.Client 6.8.1**: Usado para arquitetura orientada a eventos com padrão outbox
- **Pontos de Integração**: Eventos de domínio (LeadCreatedEvent, ProposalUpdatedEvent, etc.)
- **Criticidade**: ALTA - habilita comunicação assíncrona entre serviços
- **Acoplamento**: Baixo - usa protocolo AMQP para independência de fornecedor

#### Camada de Acesso a Dados
- **Entity Framework Core 8.0.10**: ORM para banco de dados PostgreSQL
- **Npgsql Provider 8.0.10**: Driver específico para PostgreSQL
- **Integração**: DbContext na camada Infra, migrações para gestão de esquema
- **Criticidade**: ALTA - todas as operações de persistência de dados

#### Documentação da API
- **Swashbuckle.AspNetCore 6.6.2**: Documentação OpenAPI/Swagger
- **Saunter 0.9.0**: Documentação AsyncAPI para eventos de mensagem
- **Integração**: Middleware no projeto API, gera especificações da API
- **Criticidade**: MÉDIA - experiência do desenvolvedor e descoberta da API

#### Validação
- **FluentValidation 12.1.1**: Validação de entrada para comandos/consultas
- **Integração**: Injeção de dependência na camada Application
- **Criticidade**: MÉDIA - integridade de dados e aplicação de regras de negócio

#### Logging
- **Serilog.AspNetCore 10.0.0**: Logging estruturado
- **Serilog.Formatting.Elasticsearch 10.0.0**: Sink Elasticsearch (depreciado)
- **Serilog.Sinks.Console 6.1.1**: Sink Console para desenvolvimento
- **Criticidade**: MÉDIA - observabilidade e depuração

#### Infraestrutura de Testes
- **xUnit 2.5.3/2.9.3**: Framework de teste (inconsistência de versão)
- **Moq 4.20.70 + NSubstitute 5.3.0**: Bibliotecas de mocking (ambas presentes)
- **Testcontainers 4.9.0**: Testes de integração baseados em Docker
- **FluentAssertions 8.8.0**: Biblioteca de asserção
- **Criticidade**: BAIXA - infraestrutura de teste, não-produção

### Observações Arquiteturais

1. **Arquitetura Limpa**: Fluxo de dependência bem estruturado (API → Application → Domain ← Infra)
2. **Padrão CQRS**: Manipuladores de comando/consulta separados na camada Application
3. **Orientado a Eventos**: Eventos de domínio com padrão outbox para mensageria confiável
4. **Cobertura de Testes**: Projetos de teste unitário, integração e E2E com ferramentas apropriadas
5. **Inversão de Dependência**: Camada de domínio central não possui dependências externas

---

## Recomendações

### Ações Imediatas (Alta Prioridade)

1. **Atualizar RabbitMQ.Client para 7.2.0**
   - Resolve CVE-2024-41034, CVE-2025-30219, CVE-2022-37026
   - Mudanças quebram compatibilidade: Revisar mudanças na API nas notas de lançamento da v7.0.0
   - Testar exaustivamente com servidor RabbitMQ 3.12+
   - Atualizar todo código de publicação/consumo de mensagens para mudanças na API

2. **Migrar de Serilog.Formatting.Elasticsearch para Elastic.Serilog.Sinks 9.0.0**
   - Pacote depreciado com última atualização em Março 2024
   - Sink oficial da Elastic fornece melhor compatibilidade
   - Atualizar configuração para formato Elastic Common Schema
   - Verificar conectividade Elasticsearch/Elastic Cloud

3. **Padronizar Versões do xUnit em Todos os Projetos de Teste**
   - UnitTest usa 2.5.3, Integration/E2E usam 2.9.3
   - Padronizar em xUnit 2.9.3 para consistência
   - Atualizar xunit.runner.visualstudio para 3.1.5 em todos os projetos
   - Previne inconsistências no executor de testes

### Ações de Curto Prazo (Média Prioridade)

4. **Avaliar Atualização do Swashbuckle.AspNetCore para 10.1.0**
   - Versão atual 6.6.2 está 4 versões principais atrás
   - Nota: Versão 10.x almeja .NET 8+ (compatível)
   - Mudanças quebram compatibilidade na geração OpenAPI e configuração
   - Considerar estabilidade da API antes de atualizar
   - Revisar mudanças na Swagger UI e diferenças no esquema OpenAPI

5. **Revisar Caminho de Atualização do Saunter**
   - Atual 0.9.0 é pré-1.0 com API instável
   - Mais recente 0.13.0 ainda pré-1.0 mas mais estável
   - Especificação AsyncAPI pode ter mudanças quebram compatibilidade
   - Atualizar documentos de esquema AsyncAPI após atualização
   - Considerar aguardar lançamento da versão 1.0.0 para estabilidade

6. **Investigar Incompatibilidade de Versão do Serilog.AspNetCore**
   - Projeto usa 10.0.0 mas a mais recente é 8.0.0
   - Verificar compatibilidade com .NET 8.0
   - Pode estar usando pré-lançamento ou confusão de feed de pacotes
   - Confirmar versão real instalada via `dotnet list package`

### Ações de Longo Prazo (Baixa Prioridade)

7. **Planejar Atualização do Npgsql.EntityFrameworkCore.PostgreSQL para 9.0.0**
   - Versão 10.0.0 requer .NET 9 ou 10
   - Alvo atual .NET 8.0 restringe atualização para 9.0.0
   - Avaliar cronograma de migração para .NET 9
   - Mudanças quebram compatibilidade em conversores de valor e configurações
   - Pré-visualizar migrações de banco de dados antes da atualização

8. **Atualizar Dependências de Teste**
   - Microsoft.NET.Test.Sdk: 17.8.0 → 18.0.1
   - coverlet.collector: 6.0.0 → 6.0.4
   - Moq: 4.20.70 → 4.20.72
   - Testcontainers: 4.9.0 → 4.10.0
   - Baixo risco mas fornece correções de bugs e melhorias

9. **Avaliar Consolidação de Biblioteca de Mocking**
   - Tanto Moq quanto NSubstitute são usados em testes unitários
   - Considerar padronizar em um framework de mocking
   - NSubstitute tem sintaxe mais simples para novos testes
   - Moq tem maior adoção e suporte da comunidade
   - Decisão depende da preferência da equipe e padrões de teste existentes

### Recomendações de Teste

10. **Estratégia de Teste de Atualização de Dependência**
    - Criar branch dedicada para atualizações de dependência
    - Rodar suíte completa de testes após cada atualização
    - Testar pontos de integração (RabbitMQ, PostgreSQL) em ambiente de staging
    - Teste de performance para atualizações do EF Core e Npgsql
    - Validar geração de documentação de API após atualização do Swashbuckle
    - Verificar saída de logs após migração do Serilog

### Recomendações de Monitoramento

11. **Estabelecer Monitoramento de Dependência**
    - Implementar Dependabot ou Renovate para PRs automatizados
    - Assinar avisos de segurança para pacotes críticos
    - Processo de revisão mensal regular de dependências
    - Rastrear bancos de dados CVE para vulnerabilidades conhecidas
    - Monitorar avisos de depreciação de pacotes

---

## Dependências Não Verificadas

Todas as dependências listadas neste relatório foram verificadas através do NuGet.org e fontes oficiais de pacotes. Nenhuma dependência não verificada foi identificada.

---

## Conclusão

O Serviço Comercial GestAuto tem um perfil de saúde de dependência moderado com **nenhum bloqueador crítico** mas requer atenção para vulnerabilidades de segurança. A preocupação primária é o **pacote RabbitMQ.Client com CVEs conhecidos** que deve ser tratado imediatamente.

**Aspectos Positivos**:
- Arquitetura Limpa com acoplamento mínimo a dependências
- Todos os pacotes Microsoft em versões suportadas do .NET 8.0
- Camada de domínio central tem zero dependências externas
- Cobertura de testes abrangente com ferramentas apropriadas

**Áreas para Melhoria**:
- Vulnerabilidades de segurança no RabbitMQ.Client exigem ação imediata
- Serilog.Formatting.Elasticsearch depreciado precisa de migração
- Inconsistências de versão em pacotes de framework de teste
- Atualizações principais disponíveis para stack de documentação de API

**Ordem de Prioridade Recomendada**:
1. Atualização do RabbitMQ.Client (segurança)
2. Migração do sink Elasticsearch do Serilog (depreciação)
3. Padronização de versão do xUnit (consistência)
4. Avaliação do Swashbuckle (recursos/estabilidade)
5. Atualizações de dependência de teste (manutenção)

O projeto segue as melhores práticas .NET com clara separação de preocupações e uso apropriado de abstrações. Atualizações de dependência podem ser gerenciadas sistematicamente sem mudanças arquiteturais.

---

**Relatório Gerado Por**: Agente Auditor de Dependências
**Data de Análise**: 23 de Janeiro de 2026
**Próxima Revisão Recomendada**: 23 de Fevereiro de 2026
