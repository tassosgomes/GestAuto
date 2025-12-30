# Relatório de Validação UX vs PRD — Módulo Comercial (Frontend)

Data: 2025-12-30  
Ambiente: https://gestauto.tasso.local  
Objetivo: Validar se o frontend do módulo Comercial foi entregue 100% conforme o PRD (tasks/prd-frontend-comercial/prd.md), incluindo RBAC (menus/guards), fluxos e adequação de UX.

## Metodologia
- Navegação E2E via navegador automatizado (Playwright MCP) com captura de evidências (snapshots/screenhots).
- Validação com usuários:
  - seller / 123456 (deve ver menu Comercial)
  - evaluator / 123456 (deve ver menu Avaliações)
  - admin / admin (deve ver Comercial + Avaliações + Admin)
  - viewer / 123456 (deve ver somente Avaliações)
- Critérios de avaliação:
  - Aderência às rotas e telas do PRD
  - Componentes/UX: feedback (Toaster), loading (Skeleton), responsividade, breadcrumbs
  - Funcionalidades por tela (Dashboard, Leads, Lead Details, Proposals, Test-drives, Approvals)

## Evidências
Pasta criada para evidências: `e2e/commercial/evidence/`.

Nota: nesta execução via Playwright MCP, os arquivos de screenshot/snapshot são gerados no diretório temporário do runner e não foram persistidos automaticamente no workspace. Para não perder rastreabilidade, cada item do relatório inclui **Evidência (snapshot Playwright)** em formato textual (URL + trecho do snapshot/console) suficiente para reprodução e verificação.

## Sumário Executivo
Status geral: **NÃO entregue 100% conforme PRD**.

Principais conclusões:
- **RBAC está quebrado**: perfis `evaluator` e `viewer` conseguem ver e navegar no módulo Comercial (inclusive “Aprovações”).
- **Fluxos centrais do Comercial estão incompletos**:
  - Propostas: listagem não implementada; editor diverge do PRD (sem aprovação/fechamento/validação de desconto/trade-in).
  - Lead Details: falta “Alterar Status”, falta aba “Test-Drives” e aba “Propostas” do lead não implementada.
  - Leads: listagem sem colunas/filtros do PRD; status exibido como enum técnico.
  - Test-Drives: erro HTTP 400 no carregamento e ausência do fluxo de execução (mobile-first).

Severidades (estimativa pelo que foi observado):
- **Blockers**: 3
- **Altas**: 8+
- **Médias/Baixas**: várias (ver itens abaixo)

## Checklist de Aderência ao PRD
- **1. Dashboard Comercial (`/commercial`)**: Parcial
  - KPIs (cards superiores): OK (renderiza cards e valores)
  - “Leads Quentes” / “Aguardando Você”: Parcial (lista existe; status com enum técnico)
  - Atalhos rápidos: Parcial ("Novo Lead" quebra via atalho do dashboard)

- **2. Gestão de Leads (`/commercial/leads`)**: Parcial
  - Tabela/colunas conforme PRD: NÃO (faltam Nome/Contato + WhatsApp, Última Interação, Origem)
  - Filtros avançados: NÃO (apenas status + busca)
  - Cadastro (Modal): Parcial (existe modal; faltam campo Versão/Cor e opções de origem)
  - Ordenação padrão Score > Data: Não evidenciado

- **2.3 Detalhes do Lead (`/commercial/leads/{id}`)**: Parcial
  - Cabeçalho (avatar/contato/badges): Parcial (existe; status com enum técnico)
  - “Alterar Status” dropdown: NÃO
  - Abas: Parcial (faltam “Test-Drives”; “Propostas” do lead não implementada)
  - Timeline (nova interação com tipo): OK
  - Qualificação (campos do PRD): Parcial (não evidenciado “Renda Mensal” e “Prazo de Compra” do PRD)

- **3. Gestão de Propostas (`/commercial/proposals`)**: NÃO
  - Listagem com colunas/filtros: NÃO implementada
  - Editor conforme PRD (`/commercial/proposals/{id}/edit`): NÃO (há `/new` com formulário parcial)

- **4. Gestão de Test-Drives (`/commercial/test-drives`)**: Parcial/Bugado
  - Agenda diária/semanal + slots: NÃO
  - Modal agendamento: Parcial (modal existe; campos simples)
  - Execução (mobile-first): NÃO
  - Carregamento: erro HTTP 400

- **5. Telas Gerenciais**: Parcial
  - Aprovação de Descontos (`/commercial/approvals`): Parcial (tela existe; RBAC incorreto; erros no console)
  - Pipeline (Kanban): NÃO (rota não encontrada)

## Bugs e Ajustes Necessários
> Formato por item:
> - **ID**
> - **Severidade**: Blocker | Alta | Média | Baixa
> - **Área/Tela**
> - **Passos para reproduzir**
> - **Resultado atual**
> - **Resultado esperado (PRD)**
> - **Evidência** (link/arquivo)
> - **Sugestão de correção**

(Preencher durante a execução)

### RBAC-001 — Seller vê menu/rota “Aprovações” (gerencial)
- **Severidade**: Blocker
- **Área/Tela**: Menu lateral → Comercial
- **Passos para reproduzir**:
  1. Acessar `https://gestauto.tasso.local`
  2. Logar com `seller / 123456`
  3. Expandir o menu “Comercial”
- **Resultado atual**: O item “Aprovações” aparece no menu do usuário seller.
- **Resultado esperado (PRD)**: Tela “Aprovação de Descontos” é acesso `MANAGER` (seção “5. Telas Gerenciais”). Logo, seller não deveria ver este item/rota.
- **Evidência (snapshot Playwright)**:
  - URL: `/` (pós-login)
  - Menu “Comercial” expandido contém: `Dashboard`, `Leads`, `Propostas`, `Test-Drives`, `Aprovações`.
  - Trecho (texto): `link "Aprovações"  /url: /commercial/approvals`
- **Sugestão de correção**:
  - Ajustar RBAC do menu e do guard de rota `/commercial/approvals` para exigir role `MANAGER` (ou equivalente do realm) e ocultar o item para roles não autorizadas.

### RBAC-002 — Seller vê menu “Avaliações” e “Configurações” (potencial excesso de permissão)
- **Severidade**: Alta
- **Área/Tela**: Menu lateral
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Observar o menu lateral
- **Resultado atual**: seller visualiza “Avaliações” e “Configurações”.
- **Resultado esperado**:
  - O README de validação manual informa que seller “deve ver menu Comercial”. Se a intenção for restrição por perfil, estes itens não deveriam aparecer.
  - Se “Configurações” for global, precisa estar explicitado e com conteúdo apropriado ao perfil.
- **Evidência (snapshot Playwright)**:
  - Menu lateral contém: `Home`, `Comercial`, `Avaliações`, `Configurações`.
- **Sugestão de correção**:
  - Confirmar regra de negócio (README vs RBAC atual). Se a regra for restritiva, ocultar/ bloquear `Avaliações` e `Configurações` para seller.

### RBAC-003 — evaluator e viewer enxergam Comercial (e conseguem expandir submenu completo)
- **Severidade**: Blocker
- **Área/Tela**: Menu lateral / guards de rota
- **Passos para reproduzir**:
  1. Logar com `evaluator / 123456` (ou `viewer / 123456`)
  2. Observar o menu lateral
  3. Expandir “Comercial”
- **Resultado atual**:
  - evaluator e viewer visualizam o menu “Comercial”.
  - Ao expandir, aparecem `Dashboard`, `Leads`, `Propostas`, `Test-Drives`, `Aprovações`.
- **Resultado esperado**:
  - Conforme README de validação manual:
    - evaluator: deve ver **somente Avaliações**
    - viewer: deve ver **somente Avaliações**
  - Mesmo que o menu esteja errado, **guards de rota** deveriam impedir acesso direto ao módulo Comercial.
- **Evidência (snapshot Playwright)**:
  - evaluator pós-login mostra menu com `Comercial`, `Avaliações`, `Configurações`.
  - viewer pós-login mostra menu com `Comercial` e submenu completo ao expandir.
- **Sugestão de correção**:
  - Corrigir cálculo de menus com base nas roles.
  - Adicionar/ajustar guard de rota para bloquear `/commercial/*` quando o usuário não tem role adequada.

### UX-001 — Página Home exibe “Configuração Runtime” (conteúdo técnico) para usuários finais
- **Severidade**: Média
- **Área/Tela**: `/` (Home)
- **Passos para reproduzir**: Logar com qualquer usuário e acessar a Home.
- **Resultado atual**: Exibe um card com JSON de configuração (Keycloak base URL, realm, etc.).
- **Resultado esperado**: Tela inicial não deveria expor informações técnicas em ambiente de uso normal.
- **Evidência (snapshot Playwright)**: card “Configuração Runtime” com JSON renderizado.
- **Sugestão de correção**: esconder por feature-flag/env (ex.: apenas em `/design` ou somente em dev).

### COM-DASH-001 — Status exibido como enum técnico (não amigável)
- **Severidade**: Média
- **Área/Tela**: `/commercial` → cards “Leads Quentes”
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Ir em “Comercial → Dashboard”
  3. Olhar os itens em “Leads Quentes”
- **Resultado atual**: Status aparece como `InNegotiation` (formato técnico/enum).
- **Resultado esperado (PRD)**: Badge/Status “colorido” com rótulos amigáveis (ex.: `Em Negociação`, `Novo`, etc.).
- **Evidência (snapshot Playwright)**:
  - Em “Leads Quentes” aparece `Lead Seller Success` com status `InNegotiation`.
- **Sugestão de correção**:
  - Mapear enums de status para labels pt-BR consistentes com PRD (e padronizar também em Leads/Detalhes).

### COM-LEAD-NEW-001 — Fluxo “Novo Lead” quebra (rota /commercial/leads/new)
- **Severidade**: Blocker
- **Área/Tela**: `/commercial` (Dashboard) → Atalhos Rápidos → “Novo Lead”
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Acessar `/commercial`
  3. Clicar em “Novo Lead”
- **Resultado atual**:
  - Navega para `/commercial/leads/new`
  - Exibe mensagem: “Erro ao carregar detalhes do lead.”
  - Console do browser reporta falhas 404 (recursos não encontrados).
- **Resultado esperado (PRD)**:
  - “Cadastro de Lead” deve ser um **Modal** de cadastro rápido (seção “2.2 Cadastro de Lead (Modal)”) com campos obrigatórios e máscara de telefone.
  - Mesmo que implementado via rota dedicada, não deve apresentar erro e deve exibir formulário.
- **Evidência (snapshot Playwright)**:
  - URL: `/commercial/leads/new`
  - Conteúdo principal: `Erro ao carregar detalhes do lead.`
  - Console: `Failed to load resource: the server responded with a status of 404`.
- **Sugestão de correção**:
  - Implementar o modal conforme PRD (preferencial) na tela de listagem.
  - Se mantiver rota `/new`, ajustar para não reutilizar “detalhes do lead” (provável tentativa de carregar `{id}` inexistente) e garantir renderização do formulário.

### COM-LEADS-001 — Listagem de Leads não atende colunas do PRD
- **Severidade**: Alta
- **Área/Tela**: `/commercial/leads`
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Ir em “Comercial → Leads”
- **Resultado atual**:
  - Colunas presentes: `Nome`, `Status`, `Score`, `Interesse`, `Data Criação`, `Ações`.
  - Não há exibição do **contato** (telefone/email) no grid nem atalho de WhatsApp.
  - Não há `Última Interação` nem `Origem` na tabela.
- **Resultado esperado (PRD)**:
  - Colunas: `Nome/Contato` (com link WhatsApp), `Status` (badge), `Score` (ícone), `Interesse`, `Última Interação` (data relativa), `Origem` (badge), além das ações.
- **Evidência (snapshot Playwright)**:
  - Linha exemplo renderizada: `SellerTest Clean | InNegotiation | Ouro | Civic | 24/12/2025 18:05 | Detalhes`.
- **Sugestão de correção**:
  - Ajustar DataGrid para incluir as colunas do PRD e padronizar badges/labels.
  - Adicionar ícone/ação de WhatsApp em `Nome/Contato`.

### COM-LEADS-002 — Filtros avançados do PRD ausentes / status não é multi-select
- **Severidade**: Alta
- **Área/Tela**: `/commercial/leads`
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Ir em “Comercial → Leads”
  3. Ver área de filtros
- **Resultado atual**: Apenas busca por nome e um combobox “Filtrar por Status” (não foi evidenciado multi-select; não há filtros por Score, Data de Criação, nem Vendedor).
- **Resultado esperado (PRD)**:
  - Filtros avançados: Status (multi-select), Score, Data de Criação e Vendedor (apenas gerentes).
  - Ordenação padrão: Score desc > Data Criação desc.
- **Evidência (snapshot Playwright)**: existe apenas o combobox “Filtrar por Status”.
- **Sugestão de correção**:
  - Implementar filtros adicionais e multi-select de status; aplicar ordenação padrão definida.

### COM-LEADS-003 — Status exibido como enum técnico (ProposalSent/InNegotiation)
- **Severidade**: Média
- **Área/Tela**: `/commercial/leads` (coluna Status)
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Ir em “Comercial → Leads”
- **Resultado atual**: Status aparece como `ProposalSent`, `InNegotiation`.
- **Resultado esperado (PRD)**: Rótulos amigáveis em pt-BR (`Novo`, `Em Negociação`, etc.) com badge colorido.
- **Evidência (snapshot Playwright)**: coluna Status mostra `ProposalSent` e `InNegotiation`.
- **Sugestão de correção**: mapear enums → labels e aplicar componente `Badge` com variante.

### COM-LEAD-NEW-002 — Modal “Novo Lead” incompleto vs PRD (campos/opções)
- **Severidade**: Média
- **Área/Tela**: `/commercial/leads` → Modal “Novo Lead”
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Ir em “Comercial → Leads”
  3. Clicar em “Novo Lead”
- **Resultado atual**:
  - Campos: `Nome`, `Email`, `Telefone`, `Origem`, `Modelo de Interesse (Opcional)`.
  - Opções de origem: `Site / Google`, `Instagram`, `Indicação`, `Loja Física`, `Telefone`, `Outros`.
  - Validação de obrigatórios funciona (mensagens: “Nome deve ter pelo menos 3 caracteres”, “Email inválido”, “Telefone inválido”, “Origem é obrigatória”).
- **Resultado esperado (PRD)**:
  - Obrigatórios: Nome Completo, Telefone (com máscara), Email, Origem.
  - Opcionais: Modelo de Interesse e `Versão/Cor`.
  - Origem (lista): `instagram`, `indicacao`, `google`, `loja`, `telefone`, `showroom`, `portal_classificados`, `outros`.
- **Evidência (snapshot Playwright)**: listbox de origem não inclui “showroom” nem “portal_classificados”; não há campo Versão/Cor.
- **Sugestão de correção**:
  - Incluir campo `Versão/Cor`.
  - Completar opções de origem e alinhar labels/values ao PRD (mantendo UX amigável, mas sem perder cobertura).

### COM-LEAD-DETAIL-001 — Detalhes do Lead: faltam abas/ações do PRD
- **Severidade**: Alta
- **Área/Tela**: `/commercial/leads/{id}`
- **Passos para reproduzir**:
  1. Logar com `seller / 123456`
  2. Ir em `/commercial/leads` → “Detalhes” em um lead
- **Resultado atual**:
  - Ações no header: “Agendar Test-Drive” e “Criar Proposta”.
  - Abas visíveis: `Visão Geral`, `Qualificação`, `Timeline`, `Propostas`.
  - Não existe “Alterar Status” (dropdown) e não existe aba “Test-Drives”.
- **Resultado esperado (PRD)**:
  - Cabeçalho: badges de Status + Score e ações: “Alterar Status” (dropdown), “Agendar Test-Drive” (modal), “Criar Proposta”.
  - Abas: Visão 360º, Timeline, Propostas, Test-Drives.
- **Evidência (snapshot Playwright)**:
  - Heading mostra `InNegotiation` (texto), e tablist não inclui “Test-Drives”.
- **Sugestão de correção**:
  - Implementar dropdown “Alterar Status”.
  - Implementar aba “Test-Drives” com histórico (Agendado/Realizado/Cancelado).

### COM-LEAD-DETAIL-002 — Detalhes do Lead: status exibido como enum técnico
- **Severidade**: Média
- **Área/Tela**: `/commercial/leads/{id}`
- **Passos para reproduzir**: Abrir um lead e observar o header.
- **Resultado atual**: Aparece `InNegotiation`.
- **Resultado esperado (PRD)**: Status amigável pt-BR com badge.
- **Evidência (snapshot Playwright)**: heading inclui `InNegotiation`.
- **Sugestão de correção**: mapear enums → labels; padronizar com listagem e dashboard.

### COM-LEAD-DETAIL-003 — Qualificação: não contempla “Renda Mensal Estimada” e “Prazo de Compra” do PRD
- **Severidade**: Média
- **Área/Tela**: `/commercial/leads/{id}` → aba “Qualificação”
- **Passos para reproduzir**: Abrir um lead → aba “Qualificação”.
- **Resultado atual**: Campos incluem forma de pagamento, previsão de compra (data), checkboxes e dados de troca (detalhados), mas não foi evidenciado campo de renda mensal.
- **Resultado esperado (PRD)**: Formulário inclui “Renda Mensal Estimada” e “Prazo de Compra” (Imediato/15 dias/30 dias+).
- **Evidência (snapshot Playwright)**: não há campo “Renda”; “Previsão de Compra” aparece como combobox.
- **Sugestão de correção**: alinhar o formulário aos campos do PRD (podendo manter campos extras se não conflitar).

### COM-LEAD-DETAIL-004 — Salvar Qualificação muda o usuário de aba (comportamento inesperado)
- **Severidade**: Baixa
- **Área/Tela**: `/commercial/leads/{id}` → aba “Qualificação”
- **Passos para reproduzir**: Na aba “Qualificação”, clicar “Salvar Qualificação”.
- **Resultado atual**: A UI retorna para a aba “Visão Geral”.
- **Resultado esperado**: Permanecer na aba atual e apenas exibir feedback de sucesso (Toaster), evitando perda de contexto.
- **Evidência (snapshot Playwright)**: após salvar, tab selecionada passa para “Visão Geral”.
- **Sugestão de correção**: manter aba selecionada; exibir Toaster e/ou atualização do score no header.

### COM-LEAD-DETAIL-005 — Aba “Propostas” do Lead não implementada
- **Severidade**: Alta
- **Área/Tela**: `/commercial/leads/{id}` → aba “Propostas”
- **Passos para reproduzir**: Abrir um lead → aba “Propostas”.
- **Resultado atual**: Mensagem “Funcionalidade de Propostas em desenvolvimento (Tarefa 7.0).”
- **Resultado esperado (PRD)**: Lista de cards das propostas do lead (Veículo, Valor, Status).
- **Evidência (snapshot Playwright)**: texto explícito de “em desenvolvimento”.
- **Sugestão de correção**: implementar listagem/consulta das propostas por lead.

### COM-PROP-001 — Listagem de Propostas não implementada
- **Severidade**: Alta
- **Área/Tela**: `/commercial/proposals`
- **Passos para reproduzir**: Acessar “Comercial → Propostas”.
- **Resultado atual**: “Listagem de propostas será implementada na próxima tarefa.”
- **Resultado esperado (PRD)**: Tabela com colunas (Nº, Cliente, Veículo, Valor Total, Status, Data) e filtros por Status.
- **Evidência (snapshot Playwright)**: mensagem de backlog.
- **Sugestão de correção**: implementar a listagem e filtros conforme PRD.

### COM-PROP-002 — Editor de Proposta divergente do PRD (seções/ações/validações)
- **Severidade**: Alta
- **Área/Tela**: `/commercial/proposals/new` (fluxo “Nova Proposta”)
- **Passos para reproduzir**: “Comercial → Propostas → Nova Proposta”.
- **Resultado atual**:
  - Existe formulário com: Veículo de Interesse (modelo/versão/cor/ano/preço/disponibilidade), itens/acessórios simples, switch “veículo na troca”, condições básicas (forma, entrada, parcelas) e um resumo.
  - Não existe fluxo de “Solicitar Aprovação”/“Fechar Venda”/“Gerar PDF”, nem validação de desconto > 5% (alerta + bloqueio) e nem campos de simulação (taxa/valor parcela).
  - Trade-in não implementa o fluxo do PRD (dados + “Solicitar Avaliação” + estados aguardando/avaliado + aceite/recusa com motivo).
- **Resultado esperado (PRD)**: Editor em `/commercial/proposals/{id}/edit` com seções A–D, barra lateral sticky completa e regras de desconto/aprovação.
- **Evidência (snapshot Playwright)**: rota observada é `/commercial/proposals/new` e não há botões “Solicitar Aprovação/Fechar Venda/Gerar PDF”.
- **Sugestão de correção**:
  - Alinhar rotas e UX ao PRD.
  - Implementar validações de desconto (% e valor), fluxo de aprovação e ações finais.
  - Implementar integração de avaliação de seminovo (trade-in) conforme estados.

### COM-TD-001 — Test-Drives: falhas no carregamento (HTTP 400) e agenda simplificada
- **Severidade**: Alta
- **Área/Tela**: `/commercial/test-drives`
- **Passos para reproduzir**: Acessar “Comercial → Test-Drives”.
- **Resultado atual**:
  - Console reporta erro ao carregar test-drives (HTTP 400).
  - Tela mostra uma tabela simples “Agenda” e “Nenhum test-drive agendado.”
- **Resultado esperado (PRD)**:
  - Agenda diária/semanal; slots ocupados por outros vendedores.
  - Modal de agendamento com seleção de veículo (frota), data/hora e vínculo ao lead.
  - Execução mobile-first com checklist pré/retorno e “Iniciar/Finalizar”.
- **Evidência (snapshot Playwright)**: console: `Failed to load test drives` e `status 400`.
- **Sugestão de correção**:
  - Corrigir chamada/contrato da API (400) e tratar erros com Toaster + estado vazio.
  - Implementar visualizações e fluxo de execução conforme PRD.

### COM-PIPE-001 — Tela de Pipeline (Kanban gerencial) inexistente
- **Severidade**: Alta
- **Área/Tela**: PRD “5.2 Visão de Pipeline”
- **Passos para reproduzir**:
  1. Logar com `admin / admin`
  2. Acessar `/commercial/pipeline`
- **Resultado atual**: “Página não encontrada”.
- **Resultado esperado (PRD)**: Kanban com colunas por status e filtro por vendedor.
- **Evidência (snapshot Playwright)**: heading “Página não encontrada”.
- **Sugestão de correção**: implementar rota/tela conforme PRD ou ajustar PRD se mudou.

## Observações de UX (não-bug)
- Recomenda-se padronizar a terminologia (pt-BR) e o uso de badges/ícones nas telas (Dashboard, Leads e Detalhes) para reduzir carga cognitiva.
- A Home exibindo JSON técnico (“Configuração Runtime”) tende a confundir usuários finais e expor detalhes desnecessários.
