# PRD - Frontend Módulo de Estoque

## Visão Geral

Este documento define os requisitos para a implementação das interfaces de usuário (telas) do **Módulo de Estoque** no frontend do GestAuto.

O objetivo é operacionalizar a gestão do ciclo de vida de veículos (status único, movimentações, reservas, test-drive, preparação e baixas) por meio de uma UI clara e rápida, consumindo a **GestAuto Stock API**.

Referências de UI (mockups HTML):
- [Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design/code.html](../../Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design/code.html)
- [Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design2/code.html](../../Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design2/code.html)
- [Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design3/code.html](../../Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design3/code.html)
- [Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design4/code.html](../../Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design4/code.html)
- [Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design5/code.html](../../Modelos/stock/stitch_gestauto_vis_o_geral_do_sistema_de_design5/code.html)

## Objetivos

- **Visibilidade Operacional**: permitir que usuários consultem rapidamente disponibilidade/status do veículo e entendam o “porquê” (histórico).
- **Execução de Rotinas**: suportar operações de check-in/check-out, mudanças de status, reserva e controle de test-drive conforme permissões.
- **Consistência com o backend**: refletir a fonte única de verdade do status (Stock API), incluindo auditoria/histórico.
- **RBAC estrito e ocultação**: menu e ações devem ser **ocultados** quando não autorizados (não apenas desabilitados).
- **UX amigável**: rótulos humanos (em PT-BR) para status/categorias, e erros amigáveis.

## Usuários e Perfis (Personas)

- **Vendedor (Comercial)**: consulta disponibilidade, cria/acompanha reserva, acompanha status de test-drive.
- **Gerente Comercial**: cancela reserva de terceiros, prorroga reserva, visão gerencial.
- **Gestor/Operação (MANAGER/ADMIN)**: movimentações (entrada/saída), alterações de status e exceções.
- **Oficina/Preparação**: acompanha e atualiza status de preparação (quando aplicável).
- **Financeiro**: acompanha/libera status ligados a venda/faturamento (quando aplicável).

Observação: os nomes de roles atuais do sistema devem ser respeitados (Keycloak).

## Escopo (MVP)

Base route: **`/stock`**.

Telas do MVP (definidas pelo usuário):
1. Visão geral / Dashboard
2. Detalhe do veículo + auditoria/histórico
3. Gestão de reservas
4. Movimentações (check-in / check-out)
5. Controle de test-drive (monitoramento)
6. Preparação / oficina
7. Liberação financeira / vendas
8. Baixas / exceções

## Histórias de Usuário (Foco em UI)

### Vendedor
- Como vendedor, quero uma **listagem** com filtros (VIN/Modelo/Placa, categoria, status) para encontrar rapidamente um veículo.
- Como vendedor, quero ver o **detalhe do veículo** com ficha técnica e histórico para entender disponibilidade.
- Como vendedor, quero **criar uma reserva** (quando permitido) e informar tipo/prazo para bloquear o veículo.
- Como vendedor, quero visualizar o **status do test-drive** (em andamento/finalizado e resultado) para orientar o próximo passo com o cliente.

### Gerente Comercial
- Como gerente, quero **cancelar reservas** e **prorrogar reservas** para destravar fluxo quando necessário.

### Gestor/Operação
- Como gestor/operador, quero registrar **entrada** e **saída** (com motivo, data, responsável e observações) para rastreabilidade.
- Como gestor, quero alterar **status manual** quando permitido e registrar motivo.

## Funcionalidades Principais (Detalhamento de Telas)

### 1) Visão Geral do Estoque (`/stock`)

Inspirado no mockup “dashboard + tabela”.

- **KPIs (cards superiores)** (mínimo):
  - Total disponíveis (ex.: status `em_estoque`)
  - Reservados
  - Em preparação
  - Em test-drive
- **Busca/Filtros**:
  - Busca textual por VIN/Modelo/Placa (`q`)
  - Filtro por categoria (`category`)
  - Filtro por status (`status`)
- **Tabela de veículos**:
  - Identificação: imagem (se houver), marca/modelo/versão, ano, cor
  - Identificadores: VIN, placa
  - Categoria (badge)
  - Status (badge com rótulo humano)
  - “Dias no estoque” (derivado; se não houver campo no backend, pode ser aproximado por `createdAt`)
  - Ações: abrir detalhe; ações rápidas conforme permissão (reservar, iniciar test-drive etc.)

Integração (mínimo):
- `GET /api/v1/vehicles` com paginação e filtros.

### 2) Veículos (Listagem) (`/stock/vehicles`)

Listagem dedicada e mais completa (pode reutilizar a UI da Visão Geral com filtros avançados/paginação).

Ações esperadas (conforme RBAC):
- Abrir detalhe
- Criar reserva
- Iniciar test-drive
- Alterar status (somente roles autorizadas)

### 3) Detalhe do Veículo + Auditoria (`/stock/vehicles/{id}`)

Inspirado no mockup “Ficha Técnica + timeline”.

- **Cabeçalho**: identificação do veículo + badge de status + ações principais.
- **Ficha técnica (sidebar)**:
  - Preço (se existir no backend; se não existir, ocultar)
  - Placa, VIN, KM, localização (se existir), dias no estoque
- **Aba Auditoria & Histórico**:
  - Timeline cronológica (exibir `occurredAtUtc`, tipo e resumo)
  - Tipos: check-in, check-out, reserva (criada/cancelada/prorrogada/expirada), test-drive (início/fim), mudança de status

Integração:
- `GET /api/v1/vehicles/{id}`
- `GET /api/v1/vehicles/{id}/history`

### 4) Gestão de Reservas (`/stock/reservations`)

- Listagem de reservas (ativas e recentes) com:
  - Veículo (link)
  - Tipo (padrão/entrada paga/aguardando banco)
  - Status (ativa/cancelada/concluída/expirada)
  - Vendedor responsável
  - Datas: criada, expira, prazo do banco (se existir), cancelamento
- Ações (RBAC):
  - Cancelar a própria reserva (vendedor)
  - Cancelar reserva de outro vendedor (gerente)
  - Prorrogar reserva (gerente)

Observação de UX: “prazo do banco” deve ser entrada **apenas de data** na UI (mesmo que a API use `date-time`).

Integração:
- Criação no contexto do veículo: `POST /api/v1/vehicles/{vehicleId}/reservations`
- Cancelamento: `POST /api/v1/reservations/{reservationId}/cancel`
- Prorrogação: `POST /api/v1/reservations/{reservationId}/extend`

### 5) Movimentações (Entrada/Saída) (`/stock/movements`)

Inspirado no mockup de abas.

- Aba “Registrar Entrada”:
  - Seleção de origem (cards): montadora, compra cliente/seminovo, transferência entre lojas, frota interna
  - Formulário com dados do veículo conforme categoria (vin/placa/km/avaliação etc. conforme backend)
  - Observações
  - Confirmação com feedback
- Aba “Registrar Saída”:
  - Motivo: venda, test-drive, transferência, baixa sinistro/perda total
  - Observações
  - Confirmação com feedback

Integração:
- `POST /api/v1/vehicles/{id}/check-ins`
- `POST /api/v1/vehicles/{id}/check-outs`

### 6) Controle de Test-drive (Monitoramento) (`/stock/test-drives`)

Objetivo: monitorar test-drives em andamento (sem agendamento; o agendamento é do Comercial).

- KPIs: em andamento, atrasados, finalizados hoje (se possível)
- Lista de test-drives em andamento:
  - Veículo
  - Início e tempo decorrido
  - Vendedor/cliente (se existir `customerRef`)
  - Destaque visual quando ultrapassar um SLA (ex.: 2h) — configurável depois
- Ação (RBAC): finalizar test-drive (se permitido), com registro do resultado e **observações (texto opcional)**.

Integração (conforme Swagger):
- Início: `POST /api/v1/vehicles/{id}/test-drives/start`
- Finalização: `POST /api/v1/test-drives/{testDriveId}/complete`

### 7) Preparação/Oficina (`/stock/preparation`)

Telas voltadas a status de preparação.

- Listagem de veículos “em preparação”
- Ações: alterar status para “em estoque/pronto para venda” quando aplicável

Integração:
- `PATCH /api/v1/vehicles/{id}/status`

### 8) Financeiro/Vendas (`/stock/finance`)

Tela voltada a acompanhamento/controle de veículos vendidos/aguardando etapas.

- Listagem de veículos em status relevantes (ex.: vendido)
- Ações limitadas por RBAC

Integração:
- `GET /api/v1/vehicles` + filtro por status

### 9) Baixas/Exceções (`/stock/write-offs`)

- Listagem de veículos baixados
- Ação: registrar baixa via check-out com motivo “baixa sinistro/perda total” (quando permitido)

Integração:
- `POST /api/v1/vehicles/{id}/check-outs`

## RBAC e Regras de Acesso (UI)

Requisito: **ocultar menu e ações** sem permissão.

Regras de negócio (referência do PRD de estoque):
- Reservar veículo: Comercial (vendedor) e acima
- Cancelar a própria reserva: mesmo vendedor e acima
- Cancelar reserva de outro vendedor: gerente comercial e acima
- Alterar status manualmente: gestor (estoque/geral)
- Baixar veículo: gerente geral/diretor (a mapear para roles existentes)

Mapeamento de roles (definido):
- Criar roles específicas para Estoque:
  - `STOCK_PERSON`: usuário focado em operações do estoque
  - `STOCK_MANAGER`: usuário gestor do estoque
- Menu Estoque visível para: `STOCK_PERSON`, `STOCK_MANAGER`, `SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`
- Ações operacionais “sensíveis” (status manual, baixas, telas Financeiro/Preparação): `STOCK_MANAGER`, `MANAGER`, `ADMIN`

Dependência: o bootstrap do Keycloak deve provisionar as novas roles no realm (ver `scripts/keycloak`).

## Experiência do Usuário (UX)

- **Padrões de loading**: Skeletons nas tabelas/cards.
- **Erros amigáveis**:
  - Interpretar `ProblemDetails` (`title`, `detail`, `status`) e exibir toaster amigável.
  - Mensagem padrão: “Não foi possível concluir a ação. Tente novamente.” + detalhe quando existir.
- **Rótulos em PT-BR**:
  - Enums do backend são numéricos; a UI deve mapear para labels humanas (status/categoria/tipos/motivos).
- **Responsividade**:
  - Listagens e detalhe devem funcionar bem em mobile (uso no pátio).

### Convenção: Prazo do banco (date-only)

Mesmo que a API use `bankDeadlineAtUtc` (`date-time`), a UI deve solicitar somente a **data**.

Regra: ao usuário escolher a data, o frontend deve enviar o horário como **fim do dia comercial (18:00)**.

Observação: o valor deve ser enviado em UTC (`...AtUtc`), convertendo 18:00 do horário local de operação para UTC.

## Integração com API

Fonte de contrato: Swagger em `http://localhost:8089/swagger/v1/swagger.json`.

Principais endpoints:
- Veículos:
  - `GET /api/v1/vehicles` (filtros `q`, `status`, `category`, paginação `_page`/`_size`)
  - `GET /api/v1/vehicles/{id}`
  - `GET /api/v1/vehicles/{id}/history`
  - `PATCH /api/v1/vehicles/{id}/status`
- Movimentações:
  - `POST /api/v1/vehicles/{id}/check-ins`
  - `POST /api/v1/vehicles/{id}/check-outs`
- Reservas:
  - `POST /api/v1/vehicles/{vehicleId}/reservations`
  - `POST /api/v1/reservations/{reservationId}/cancel`
  - `POST /api/v1/reservations/{reservationId}/extend`
- Test-drive:
  - `POST /api/v1/vehicles/{id}/test-drives/start`
  - `POST /api/v1/test-drives/{testDriveId}/complete`

## Não-Objetivos (Fora de Escopo)

- Agendamento de test-drive (é responsabilidade do módulo Comercial).
- Gestão completa do contexto comercial (lead/proposta) dentro do estoque; o estoque apenas exibe vínculo/identificadores quando existirem.
- Relatórios avançados, BI e dashboards analíticos (além dos KPIs operacionais simples).
- Upload/gestão de fotos do veículo (a menos que já exista endpoint/campo).

## Questões em Aberto

1. Campo de texto na finalização do test-drive (requisito de backend — a desenvolver):
  - A UI deve exibir um campo opcional de texto (“observações/feedback”).
  - Requisito: a Stock API deve aceitar e persistir esse texto ao concluir um test-drive.
  - Proposta de contrato (mínimo): adicionar campo opcional `notes` (string) em `CompleteTestDriveRequest`.
  - Persistência:
    - Armazenar `notes` no registro do test-drive (ex.: entidade/registro de `TestDriveSession`) e refletir no histórico/auditoria do veículo.
  - Exposição:
    - Incluir `notes` no retorno do endpoint de finalização (`CompleteTestDriveResponse`) e no `GET /api/v1/vehicles/{id}/history` para eventos de test-drive.
  - Validações (mínimo):
    - Campo opcional; quando enviado, aplicar `trim` e limitar tamanho (ex.: 500–1000 caracteres).
    - Tratar como texto puro (sem HTML).
  - OpenAPI/Swagger:
    - Garantir que o Swagger publicado inclua o endpoint e o novo campo, para destravar o desenvolvimento do frontend.
2. Endpoint de finalização não encontrado no Swagger do ambiente:
  - No código do serviço Stock existe `POST /api/v1/test-drives/{testDriveId}/complete` (controller `TestDrivesController`).
  - Se o Swagger do ambiente local não exibe o endpoint, validar se a versão em execução está desatualizada ou se há problema na publicação do OpenAPI.
3. Campos de “preço” e “localização”:
  - No Swagger atual, `VehicleResponse` não expõe esses campos. A UI deve ocultar esses itens até que existam no contrato.
  - “Dias no estoque” pode ser derivado de `createdAt`.

## Plano de Desenvolvimento (alto nível)

1. Montar rotas `/stock` e navegação com RBAC.
2. Criar services/types do Stock alinhados ao Swagger.
3. Implementar listagem/dashboard + detalhe/histórico.
4. Implementar reservas (drawer/modal) + cancelamento/prorrogação.
5. Implementar movimentações (check-in/out) + validações e UX de erro.
6. Implementar monitor de test-drive + finalização.
7. Refinar labels, empty-states, acessibilidade e responsividade.
