## markdown

## status: completed

<task_context>
<domain>engine/infra</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>database</dependencies>
</task_context>

# Tarefa 1.0: Configuração Inicial do Projeto e Infraestrutura

## Visão Geral

Configurar o projeto Spring Boot para o microserviço de avaliação de veículos, incluindo estrutura base, dependências Maven, configurações de banco de dados, e preparação do ambiente de desenvolvimento.

**IMPORTANTE**: Este microserviço deve ser criado como uma nova pasta dentro do diretório `services/` existente no GestAuto, seguindo o padrão dos outros microserviços do projeto.

<requirements>
- Estrutura de 5 camadas following GestAuto patterns
- Spring Boot 3.2 com Java 21
- Conexão com PostgreSQL existente (schema vehicle_evaluation)
- Configuração RabbitMQ
- Docker setup para desenvolvimento
- Flyway para migrations
- Perfis de ambiente (dev, prod)
</requirements>

## Subtarefas

- [x] 1.1 Criar estrutura base do projeto Maven dentro da pasta `services/vehicle-evaluation/` ✅
- [x] 1.2 Configurar pom.xml com dependências necessárias ✅
- [x] 1.3 Criar Dockerfile para containerização ✅
- [x] 1.4 Configurar application.yml com perfis ✅
- [x] 1.5 Setup de Flyway migrations ✅
- [x] 1.6 Criar classe principal VehicleEvaluationApplication ✅

## Validações Adicionais Concluídas

- [x] 1.7 Definição da tarefa, PRD e tech spec validados ✅
- [x] 1.8 Análise de regras e conformidade verificadas ✅
- [x] 1.9 Revisão de código completada ✅
- [x] 1.10 Projeto compila sem erros ✅
- [x] 1.11 Pronto para próxima fase (domínio) ✅

## Detalhes de Implementação

### Estrutura de Diretórios
```
**IMPORTANTE**: O projeto deve ser criado dentro da pasta `services/` existente no GestAuto, criando uma nova pasta para o microserviço.

services/vehicle-evaluation/
├── pom.xml
├── Dockerfile
└── src/main/java/com/gestauto/vehicleevaluation/
    ├── 1-service/ (controllers, dtos)
    ├── 2-application/ (commands, queries, handlers)
    ├── 3-domain/ (entities, value objects, repositories)
    ├── 4-infra/ (repositories, external services)
    └── config/ (database, security, rabbitmq)
```

### Dependências Principais
- Spring Boot Starter Web, Data JPA, Redis, AMQP
- PostgreSQL Driver
- Flyway Core
- AWS SDK para Cloudflare R2
- Jackson para JSON
- Lombok, MapStruct
- JWT (jjwt)
- iText para PDF

### Configurações de Ambiente
- Database: PostgreSQL existente com schema vehicle_evaluation
- RabbitMQ: host localhost:5672
- Redis: localhost:6379
- Profiles: default, dev, docker, prod

## Critérios de Sucesso

- [x] Projeto compila sem erros
- [x] Conexão com PostgreSQL estabelecida
- [x] Flyway conectado ao schema vehicle_evaluation
- [x] Docker build funcional
- [x] Spring Boot inicia sem erros nos perfis dev e docker
- [x] Health checks funcionando em /actuator/health
- [x] Logs configurados adequadamente

## Sequenciamento

- Bloqueado por: Nenhuma
- Desbloqueia: Todas as outras tarefas
- Paralelizável: Não (tarefa de fundação obrigatória)

## Tempo Estimado

- Desenvolvimento: 4 horas
- Testes básicos: 2 horas
- Total: 6 horas