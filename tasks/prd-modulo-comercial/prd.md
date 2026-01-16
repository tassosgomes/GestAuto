# PRD - Módulo Comercial GestAuto

## Visão Geral

O **GestAuto** é um sistema de gestão para concessionárias de veículos. Este PRD define o **Módulo Comercial**, responsável por gerenciar todo o fluxo de vendas desde a captação do lead até o fechamento da proposta.

O módulo resolve os seguintes problemas:
- **Falta de rastreabilidade** de leads e oportunidades de venda
- **Processos manuais** e despadronizados na construção de propostas
- **Comunicação ineficiente** entre comercial e setor de seminovos
- **Ausência de visibilidade** do funil de vendas para gestores

O foco desta fase é o **backend**, com uma arquitetura preparada para integrações via eventos e APIs REST.

## Objetivos

| Objetivo | Métrica de Sucesso |
|----------|-------------------|
| Digitalizar 100% do fluxo comercial | Todas as 7 etapas do funil implementadas |
| Rastrear origem e conversão de leads | Taxa de conversão por origem mensurável |
| Reduzir tempo de resposta ao cliente | Tempo médio de primeira resposta registrado |
| Padronizar propostas comerciais | 100% das propostas seguindo template único |
| Integrar com avaliação de seminovos | Comunicação via eventos funcionando |
| Preparar arquitetura para integrações | Sistema publicando/consumindo eventos |

## Histórias de Usuário

### Vendedor

- Como **vendedor**, quero **registrar um novo lead** para que eu possa **acompanhar todas as oportunidades de venda**.
- Como **vendedor**, quero **registrar tentativas de contato e conversas** para que eu tenha **histórico completo do relacionamento**.
- Como **vendedor**, quero **qualificar o cliente** para que eu possa **priorizar leads e montar propostas adequadas**.
- Como **vendedor**, quero **agendar e registrar test-drives** para que eu tenha **controle das demonstrações realizadas**.
- Como **vendedor**, quero **construir propostas comerciais completas** para que eu possa **apresentar ofertas claras ao cliente**.
- Como **vendedor**, quero **enviar seminovos para avaliação** para que eu possa **incluir o valor na negociação**.
- Como **vendedor**, quero **acompanhar o status do pedido** para que eu possa **manter o cliente informado**.

### Gerente Comercial

- Como **gerente**, quero **visualizar o funil de vendas** para que eu possa **acompanhar a performance da equipe**.
- Como **gerente**, quero **aprovar descontos em propostas** para que eu tenha **controle sobre a margem de vendas**.
- Como **gerente**, quero **reatribuir leads entre vendedores** para que eu possa **balancear a carga de trabalho**.

### Administrativo

- Como **administrativo**, quero **consultar propostas fechadas** para que eu possa **encaminhar ao financeiro**.
- Como **administrativo**, quero **gerar relatórios de leads por origem** para que eu possa **avaliar canais de marketing**.

## Funcionalidades Principais

### F1. Gestão de Leads (Captação)

Permite o registro e acompanhamento de potenciais clientes.

**Requisitos Funcionais:**

- **RF1.1** O sistema deve permitir cadastrar lead com campos obrigatórios: nome, telefone, e-mail, origem (`instagram`, `indicacao`, `google`, `loja`, `telefone`, `showroom`, `portal_classificados`, `outros`)
- **RF1.2** O sistema deve permitir registrar campos opcionais de interesse: modelo, versão, cor, forma de pagamento pretendida
- **RF1.3** O sistema deve atribuir um vendedor responsável ao lead
- **RF1.4** O sistema deve permitir registrar tentativas de contato (data, hora, canal, resultado)
- **RF1.5** O sistema deve permitir registrar conversas/anotações no histórico do lead
- **RF1.6** O sistema deve gerenciar status do lead: `novo`, `em_contato`, `em_negociacao`, `test_drive_agendado`, `proposta_enviada`, `perdido`, `convertido`
- **RF1.7** O sistema deve emitir evento `LeadCriado` ao cadastrar novo lead
- **RF1.8** O sistema deve emitir evento `LeadStatusAlterado` em mudanças de status

### F2. Qualificação do Cliente (Lead Scoring)

Permite registrar informações estratégicas para priorização e construção de propostas, com foco em **maximizar rentabilidade** e **giro de estoque**.

#### Lógica de Negócio - Cenários de Lucratividade

| Cenário | Descrição | Valor para Concessionária |
|---------|-----------|---------------------------|
| À Vista | Fluxo de caixa imediato | Margem limitada (cliente pede desconto) |
| À Vista + Usado | Bom caixa + Lucro futuro | Revenda do usado gera margem adicional |
| Financiado | Lucro da venda + Retorno bancário | Comissão do banco sobre contrato |
| Financiado + Usado | **"Santo Graal"** | Lucro + Comissão + Revenda do usado |

#### Classificação de Leads (SLA de Atendimento)

| Classificação | Critério | SLA | Ação |
|---------------|----------|-----|------|
| 💎 **Diamante** | Financiado + Usado + Compra < 15 dias | Atender em até **10 min** | Gerente acompanha negociação |
| 🥇 **Ouro** | (À Vista + Usado) OU (Financiado) + Compra < 15 dias | Atender em até **30 min** | Vendedor sênior, foco em fechar rápido |
| 🥈 **Prata** | À Vista puro | Atender em até **2 horas** | Defender preço, tentar converter para financiamento |
| 🥉 **Bronze** | Compra > 30 dias OU restrição de crédito | Nutrição automática | Fluxo de automação (e-mail/WhatsApp) |

#### Critérios Extras de Pontuação

| Critério | Peso | Valorização |
|----------|------|-------------|
| **Tempo de Compra** | Alto | "Imediato" ou "Até 7 dias" = mais pontos |
| **Modelo de Interesse** | Médio | Estoque parado > Lançamentos |
| **Origem do Lead** | Alto | Showroom/Telefone > Indicação > Site > Portal classificados |
| **Estado do Usado** | Médio | Baixa km + Revisões na marca = sobe de nível |

**Requisitos Funcionais:**

- **RF2.1** O sistema deve registrar se o cliente possui seminovo para troca (marca, modelo, ano, km, estado geral)
- **RF2.2** O sistema deve registrar a forma de pagamento preferida: `a_vista`, `financiamento`, `consorcio`
- **RF2.3** O sistema deve registrar a data/prazo ideal de compra do cliente
- **RF2.4** O sistema deve registrar interesse em test-drive
- **RF2.5** O sistema deve calcular automaticamente a classificação do lead (Diamante, Ouro, Prata, Bronze) baseado na regra:
  - `Diamante`: Financiado + Usado + Compra < 15 dias
  - `Ouro`: (À Vista + Usado) OU (Financiado) + Compra < 15 dias
  - `Prata`: À Vista puro
  - `Bronze`: Compra > 30 dias OU sem informações suficientes
- **RF2.6** O sistema deve aplicar bonificação no score quando:
  - Origem = Showroom ou Telefone (+1 nível)
  - Usado com baixa km e revisões na marca (+1 nível)
  - Modelo de interesse está em estoque parado (+1 nível)
- **RF2.7** O sistema deve permitir filtrar leads por classificação (Diamante, Ouro, Prata, Bronze)
- **RF2.8** O sistema deve ordenar leads por classificação e tempo de cadastro
- **RF2.9** O sistema deve emitir evento `LeadClassificado` com a classificação calculada
- **RF2.10** O sistema deve recalcular classificação quando dados de qualificação forem atualizados

### F3. Gestão de Test-Drive

Permite agendar, controlar e registrar test-drives realizados.

**Requisitos Funcionais:**

- **RF3.1** O sistema deve permitir agendar test-drive com data, horário e veículo
- **RF3.2** O sistema deve verificar disponibilidade do veículo de test-drive
- **RF3.3** O sistema deve registrar o vendedor responsável pelo test-drive
- **RF3.4** O sistema deve permitir registrar checklist simples pré e pós test-drive (combustível, quilometragem, observações visuais)
- **RF3.5** O sistema deve registrar a realização do test-drive (data/hora efetiva, cliente, vendedor)
- **RF3.6** O sistema deve atualizar automaticamente o status do lead para `test_drive_agendado`
- **RF3.7** O sistema deve emitir evento `TestDriveAgendado` e `TestDriveRealizado`

### F4. Construção de Proposta Comercial

Permite criar propostas estruturadas e acompanhar seu ciclo de vida.

**Requisitos Funcionais:**

- **RF4.1** O sistema deve vincular proposta a um lead existente
- **RF4.2** O sistema deve registrar veículo da proposta: modelo, versão, cor, ano
- **RF4.3** O sistema deve indicar se é pronta entrega ou pedido de fábrica
- **RF4.4** O sistema deve registrar preço do veículo (tabela)
- **RF4.5** O sistema deve permitir adicionar itens extras: acessórios, película, tapete, rastreador, etc.
- **RF4.6** O sistema deve registrar descontos (valor e motivo)
- **RF4.7** O sistema deve exigir aprovação gerencial para descontos acima de **5%** do valor do veículo
- **RF4.8** O sistema deve registrar forma de pagamento: à vista, financiamento (entrada + parcelas), consórcio
- **RF4.9** O sistema deve calcular valor total da proposta
- **RF4.10** O sistema deve gerenciar status da proposta: `rascunho`, `em_negociacao`, `aguardando_avaliacao_seminovo`, `aguardando_aprovacao_desconto`, `aguardando_cliente`, `aprovada`, `fechada`, `perdida`
- **RF4.11** O sistema deve permitir vincular avaliação de seminovo à proposta
- **RF4.12** O sistema deve emitir evento `PropostaCriada`, `PropostaAtualizada`, `PropostaFechada`

### F5. Integração com Avaliação de Seminovos

Permite enviar veículos usados para avaliação e receber o retorno.

**Requisitos Funcionais:**

- **RF5.1** O sistema deve registrar dados do seminovo: marca, modelo, ano, quilometragem, placa, cor
- **RF5.2** O sistema deve permitir enviar solicitação de avaliação ao setor de seminovos
- **RF5.3** O sistema deve emitir evento `AvaliacaoSeminovoSolicitada`
- **RF5.4** O sistema deve consumir evento `AvaliacaoSeminvoRespondida` com valor aprovado
- **RF5.5** O sistema deve atualizar a proposta com o valor do seminovo automaticamente
- **RF5.6** O sistema deve permitir registrar aceite ou recusa do cliente sobre o valor
- **RF5.7** O sistema deve manter histórico de avaliações solicitadas por proposta

### F6. Fechamento da Venda

Permite formalizar a aprovação do cliente e encaminhar para o financeiro.

**Requisitos Funcionais:**

- **RF6.1** O sistema deve registrar a aprovação formal do cliente
- **RF6.2** O sistema deve validar que a proposta está completa (veículo, valor, forma de pagamento definidos)
- **RF6.3** O sistema deve alterar status da proposta para `fechada`
- **RF6.4** O sistema deve emitir evento `VendaFechada` para o módulo financeiro
- **RF6.5** O sistema deve registrar data/hora do fechamento e vendedor responsável
- **RF6.6** O sistema deve impedir alterações na proposta após fechamento

### F7. Acompanhamento do Pedido

Permite ao comercial acompanhar o andamento pós-fechamento.

**Requisitos Funcionais:**

- **RF7.1** O sistema deve consumir eventos de atualização do módulo financeiro
- **RF7.2** O sistema deve exibir status atual do pedido: `aguardando_documentacao`, `em_analise_credito`, `credito_aprovado`, `credito_reprovado`, `aguardando_veiculo`, `pronto_entrega`, `entregue`
- **RF7.3** O sistema deve registrar previsão de chegada do veículo (quando pedido)
- **RF7.4** O sistema deve permitir registrar anotações de acompanhamento
- **RF7.5** O sistema deve notificar o vendedor sobre mudanças de status relevantes

## Experiência do Usuário

Como o foco atual é **backend-first**, a experiência do usuário será através de:

- **API REST** bem documentada seguindo padrões RESTful
- **Respostas padronizadas** com códigos HTTP apropriados
- **Mensagens de erro claras** e acionáveis
- **Paginação e filtros** em listagens
- **Validações de entrada** com feedback específico

### Personas Técnicas

| Persona | Necessidade |
|---------|-------------|
| Frontend Developer | APIs claras, documentadas, consistentes |
| Integrador | Eventos bem definidos, contratos estáveis |
| DevOps | Logs estruturados, health checks, métricas |

## Restrições Técnicas de Alto Nível

### Arquitetura

- **Domain-Driven Design (DDD)**: Modelagem rica do domínio comercial
- **Arquitetura orientada a eventos**: Comunicação assíncrona entre módulos via **RabbitMQ**
- **API REST**: Interface síncrona para operações CRUD e consultas

### Integrações

- Sistema deve **publicar eventos** para:
  - Módulo de Seminovos (solicitação de avaliação)
  - Módulo Financeiro (venda fechada)
  - Futuros módulos/sistemas externos

- Sistema deve **consumir eventos** de:
  - Módulo de Seminovos (resposta de avaliação)
  - Módulo Financeiro (atualizações de status do pedido)

### Requisitos Não-Funcionais

- **Auditoria**: Todas as operações críticas devem ser auditadas (quem, quando, o quê)
- **Idempotência**: Eventos devem ser processados de forma idempotente
- **Consistência eventual**: Aceita-se consistência eventual entre módulos via eventos

### Autorização e Permissões

O sistema deve implementar controle de acesso baseado em roles (RBAC):

| Role | Permissões |
|------|------------|
| **Vendedor** | Visualiza e gerencia apenas seus próprios leads e propostas |
| **Gerente** | Visão geral de todos os leads e propostas da equipe; aprova descontos > 5% |

- **RF-AUTH.1** O sistema deve autenticar usuários antes de permitir acesso
- **RF-AUTH.2** O sistema deve filtrar leads/propostas automaticamente baseado na role do usuário
- **RF-AUTH.3** O sistema deve permitir que gerentes visualizem todos os leads e propostas
- **RF-AUTH.4** O sistema deve restringir vendedores a visualizar apenas seus próprios registros
- **RF-AUTH.5** O sistema deve registrar em auditoria quem acessou/alterou cada registro

## Não-Objetivos (Fora de Escopo)

### Excluído desta versão

- ❌ Frontend/Interface de usuário
- ❌ Módulo Financeiro completo (apenas integração via eventos)
- ❌ Gestão de estoque de veículos novos
- ❌ Avaliação técnica de seminovos (responsabilidade do módulo Seminovos)
- ❌ Integração direta com bancos/financeiras
- ❌ Emissão de documentos fiscais
- ❌ Gestão de comissões de vendedores
- ❌ CRM avançado (campanhas, automações de marketing)
- ❌ App mobile

### Considerações Futuras

- Integração com sistemas de financiamento bancário
- Dashboard de BI para análise de vendas
- Notificações push/SMS para clientes
- Workflow de aprovações configurável

## Questões em Aberto

| # | Questão | Status | Resposta/Decisão |
|---|---------|--------|------------------|
| 1 | Limite de desconto para aprovação gerencial | ✅ Resolvido | **Acima de 5%** |
| 2 | Regras do score de qualificação | ✅ Resolvido | Matriz Diamante/Ouro/Prata/Bronze (ver F2) |
| 3 | Tecnologia de mensageria | ✅ Resolvido | **RabbitMQ** |
| 4 | Integração com CRM existente | ✅ Resolvido | Não existe, será construído do zero |
| 5 | Campos obrigatórios do lead | ✅ Resolvido | Nome, telefone, e-mail, origem. Opcionais: modelo, versão, cor |
| 6 | Autenticação e roles | ✅ Resolvido | Vendedor (visão própria) e Gerente (visão geral) |

### Questões Pendentes

*Nenhuma questão pendente no momento.*

---

**Documento criado em:** 08/12/2024  
**Versão:** 1.1  
**Status:** Aprovado
