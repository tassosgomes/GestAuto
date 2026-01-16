# PRD - Frontend Módulo Comercial

## Visão Geral

Este documento define os requisitos para a implementação das interfaces de usuário (telas) do **Módulo Comercial** no frontend do GestAuto.
O objetivo é fornecer uma interface visual e interativa para que Vendedores e Gerentes possam realizar a gestão de Leads, Propostas e Test-Drives, consumindo a API REST já existente (`commercial`).

## Objetivos

- **Operacionalizar o Processo de Vendas**: Permitir que vendedores realizem todo o fluxo (do Lead à Venda) sem sair da aplicação.
- **Visualização de Dados**: Oferecer painéis e listagens que facilitem a priorização de tarefas (ex: Leads "Diamante").
- **Usabilidade**: Interface responsiva e intuitiva, utilizando o Design System (Shadcn UI) já estabelecido.
- **Feedback em Tempo Real**: Notificações de sucesso/erro e validações de formulário.

## Histórias de Usuário (Foco em UI)

### Vendedor
- Como vendedor, quero um **Dashboard** para ver meus leads quentes e tarefas do dia (test-drives).
- Como vendedor, quero visualizar meus leads em formato de **Lista ou Kanban** para facilitar a gestão do funil.
- Como vendedor, quero uma tela de **Detalhes do Lead** onde eu possa ver histórico, registrar interações e qualificar o cliente em um só lugar.
- Como vendedor, quero um **Formulário de Proposta** que calcule automaticamente os totais e parcelas enquanto eu edito.
- Como vendedor, quero solicitar a **Avaliação de Seminovo** diretamente da tela da proposta.

### Gerente
- Como gerente, quero visualizar o **Pipeline da Equipe** (todos os leads).
- Como gerente, quero uma interface para **Aprovar/Rejeitar Descontos** solicitados nas propostas.

## Funcionalidades Principais (Detalhamento de Telas)

### 1. Dashboard Comercial (`/commercial`)
Visão geral para o vendedor/gerente iniciar o dia.

- **KPIs (Cards Superiores)**:
  - **Leads Novos**: Contagem de leads com status `novo` atribuídos ao usuário.
  - **Propostas em Aberto**: Contagem de propostas em negociação.
  - **Test-Drives Hoje**: Agendamentos para a data atual.
  - **Taxa de Conversão** (Mensal): % de leads convertidos em vendas.
- **Listas de Ação Imediata**:
  - **"Leads Quentes"**: Lista dos top 5 leads com classificação `Diamante` ou `Ouro` que não tiveram interação nas últimas 24h.
  - **"Aguardando Você"**: Propostas que precisam de ajuste ou leads novos sem contato.
- **Atalhos Rápidos (FAB ou Botões)**:
  - Novo Lead
  - Nova Proposta

### 2. Gestão de Leads (`/commercial/leads`)

#### 2.1 Listagem de Leads
- **Layout**: Tabela (Data Grid) responsiva.
- **Colunas**:
  - **Nome/Contato**: Nome do cliente, link para Whatsapp (ícone).
  - **Status**: Badge colorido (`Novo`, `Em Negociação`, etc.).
  - **Score**: Ícone representativo (💎 Diamante, 🥇 Ouro, 🥈 Prata, 🥉 Bronze).
  - **Interesse**: Modelo/Versão do veículo.
  - **Última Interação**: Data relativa (ex: "há 2 horas").
  - **Origem**: Badge simples (Instagram, Loja, etc.).
- **Filtros Avançados**:
  - Por Status (Multi-select).
  - Por Classificação (Score).
  - Por Data de Criação.
  - Por Vendedor (Visível apenas para Gerentes).
- **Ordenação Padrão**: Score (Decrescente) > Data de Criação (Decrescente).

#### 2.2 Cadastro de Lead (Modal)
- **Objetivo**: Cadastro rápido para não perder o cliente.
- **Campos Obrigatórios**:
  - Nome Completo.
  - Telefone (com máscara).
  - Email.
  - Origem (Select: `instagram`, `indicacao`, `google`, `loja`, `telefone`, `showroom`, `portal_classificados`, `outros`).
- **Campos Opcionais**:
  - Modelo de Interesse.
  - Versão/Cor.

#### 2.3 Detalhes do Lead (`/commercial/leads/{id}`)
Tela central de trabalho. Layout dividido em Cabeçalho + Abas.

- **Cabeçalho Fixo**:
  - Avatar/Iniciais do Cliente.
  - Nome, Telefone, Email.
  - **Badges**: Status Atual e Score.
  - **Ações Principais**:
    - "Alterar Status" (Dropdown).
    - "Agendar Test-Drive" (Modal).
    - "Criar Proposta" (Navega para editor).
- **Aba 1: Visão 360º (Resumo & Qualificação)**
  - **Card de Qualificação (Lead Scoring)**:
    - Formulário editável para enriquecer dados:
      - Renda Mensal Estimada.
      - Prazo de Compra (`Imediato`, `15 dias`, `30 dias+`).
      - Possui Veículo na Troca? (Sim/Não).
      - Forma de Pagamento (`À Vista`, `Financiado`, `Consórcio`).
    - **Feedback Visual**: Ao salvar, o Score (Diamante/Ouro) atualiza em tempo real.
  - **Card de Interesse**: Veículo desejado (Modelo, Versão, Cor).
- **Aba 2: Timeline (CRM)**
  - Lista vertical de eventos: Criação, Mudanças de Status, Interações, Test-Drives.
  - **Nova Interação**: Área de texto para registrar notas, telefonemas ou visitas.
    - Tipo: `Ligação`, `WhatsApp`, `Email`, `Visita`, `Outros`.
- **Aba 3: Propostas**:
  - Lista de cards resumidos das propostas deste lead (Veículo, Valor, Status).
- **Aba 4: Test-Drives**:
  - Histórico de agendamentos e status (Agendado, Realizado, Cancelado).

### 3. Gestão de Propostas (`/commercial/proposals`)

#### 3.1 Listagem de Propostas
- **Colunas**: Nº Proposta, Cliente, Veículo, Valor Total, Status, Data.
- **Filtros**: Status (`Rascunho`, `Aguardando Aprovação`, `Fechada`, etc.).

#### 3.2 Editor de Proposta (`/commercial/proposals/{id}/edit`)
Interface complexa dividida em passos ou seções colapsáveis (Accordion).

- **Seção A: Veículo Principal**
  - Seleção de Veículo (Busca no catálogo/estoque).
  - Exibição automática do Preço de Tabela.
  - Configuração: Cor, Ano Modelo.
  - Checkbox: "Pronta Entrega" ou "Pedido de Fábrica".
- **Seção B: Avaliação de Seminovo (Trade-in)**
  - **Estado Inicial**: Botão "Adicionar Veículo de Troca".
  - **Formulário**: Placa, Marca, Modelo, Ano, KM.
  - **Ação**: "Solicitar Avaliação".
  - **Estado "Aguardando"**: Mostra status pendente (integração com módulo de avaliação).
  - **Estado "Avaliado"**: Mostra valor aprovado pelo avaliador.
  - **Ação do Cliente**: Botões "Cliente Aceitou" / "Cliente Recusou" (com motivo).
- **Seção C: Itens e Acessórios**
  - Lista dinâmica de itens (Tapetes, Película, Documentação).
  - Campos: Descrição, Valor Unitário.
  - Totalizador de Acessórios.
- **Seção D: Condições de Pagamento**
  - **Entrada**: Valor monetário ou %.
  - **Financiamento**:
    - Nº Parcelas.
    - Taxa de Juros (informativo).
    - Valor da Parcela (cálculo simples ou simulado).
  - **Desconto**:
    - Campo para valor monetário ou %.
    - **Validação**: Se desconto > 5%, exibe alerta "Necessária aprovação do gerente" e bloqueia fechamento direto.
- **Barra Lateral de Resumo (Sticky)**
  - Preço Veículo (+)
  - Acessórios (+)
  - Seminovo (-)
  - Entrada (-)
  - Desconto (-)
  - **Total a Financiar / Pagar (=)**
  - **Botões de Ação**:
    - "Salvar Rascunho".
    - "Solicitar Aprovação" (se houver pendência de desconto).
    - "Gerar PDF" (Mockup).
    - "Fechar Venda" (Disponível apenas se tudo aprovado).

### 4. Gestão de Test-Drives (`/commercial/test-drives`)

#### 4.1 Calendário/Agenda
- Visualização de agenda diária/semanal.
- Slots ocupados por outros vendedores.

#### 4.2 Modal de Agendamento
- Seleção de Veículo (Frota de Test-drive).
- Data e Hora.
- Vínculo com Lead.

#### 4.3 Execução (Mobile First)
- Tela focada no momento do uso.
- **Checklist Pré-saída**: Nível de combustível, Avarias visuais (Sim/Não).
- Botão "Iniciar Test-Drive".
- **Checklist Retorno**: Nível de combustível, Novas avarias, Km final.
- Campo "Feedback do Cliente".
- Botão "Finalizar".

### 5. Telas Gerenciais (Acesso: `MANAGER`)

#### 5.1 Aprovação de Descontos (`/commercial/approvals`)
- Lista de propostas com status `aguardando_aprovacao_desconto`.
- Detalhe da solicitação: Valor do veículo, % de desconto solicitado, justificativa do vendedor.
- Ações: "Aprovar" ou "Rejeitar" (com motivo).

#### 5.2 Visão de Pipeline
- Kanban board com colunas representando os status dos leads.
- Drag-and-drop para mover leads de status (opcional nesta fase).
- Filtro por Vendedor para monitorar performance individual.

## Experiência do Usuário (UX)

- **Feedback**: Utilizar `Toaster` (Shadcn) para confirmar salvamentos e erros de API.
- **Loading**: Utilizar `Skeleton` (Shadcn) durante o carregamento de dados.
- **Responsividade**: Telas de Listagem e Detalhes devem ser utilizáveis em mobile (para vendedores no pátio).
- **Navegação**: Utilizar Breadcrumbs para facilitar o retorno às listagens.

## Integração com API

- Utilizar `axios` ou `fetch` configurado com o token do Keycloak.
- Mapear endpoints do `swagger.json`:
  - `GET /leads` -> Listagem
  - `POST /leads` -> Cadastro
  - `GET /leads/{id}` -> Detalhes
  - `POST /leads/{id}/qualify` -> Aba Qualificação
  - `POST /proposals` -> Editor de Proposta

## Plano de Desenvolvimento

1.  **Setup**: Criar serviços de API (`commercialService.ts`) e tipos TypeScript baseados no Swagger.
2.  **Leads**: Implementar Listagem e Cadastro.
3.  **Detalhes do Lead**: Implementar abas de Interação e Qualificação.
4.  **Propostas**: Implementar Editor de Proposta básico.
5.  **Refinamento**: Adicionar validações complexas e integração com Avaliação.
