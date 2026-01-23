# PRD — Cadastro de Veículo + Check-in (MVP)

## Visão Geral
Este PRD define o fluxo de **cadastro de veículo** integrado ao **check‑in** no módulo de estoque. O objetivo é permitir que um veículo **passe a existir no estoque** e que a **entrada** seja registrada no mesmo fluxo, eliminando o bloqueio atual do MVP (dropdown exigindo veículo pré‑existente).

## Problema
O fluxo atual de check‑in exige selecionar um veículo já cadastrado, mas o MVP não oferece uma forma clara de criar esse veículo. Isso interrompe o ciclo completo de entrada no estoque.

## Objetivo
- Permitir **criar o veículo** e **registrar check‑in** no mesmo fluxo.
- Garantir validações de obrigatoriedade por categoria (novo/seminovo/demonstração).
- Manter rastreabilidade: origem, responsável, data/hora e observações.

## Escopo (MVP)
- Tela de Movimentações (`/stock/movements`) com **aba Registrar Entrada (Check‑in)**.
- Inclusão de **modo “Cadastrar veículo”** dentro da própria aba de check‑in.
- Não inclui importação em massa nem multi‑loja.

## Não‑Objetivos
- Precificação, fotos, documentação fiscal.
- Gestão de frota além do necessário para status.
- Transferência entre lojas (fluxo completo) nesta fase.

## Fluxo Principal
1. Usuário acessa **Registrar Entrada (Check‑in)**.
2. Seleciona **Origem do veículo** (cards): montadora, compra cliente/seminovo, transferência, frota interna.
3. Escolhe **Tipo/Categoria**: `novo`, `seminovo`, `demonstracao`.
4. **Modo de seleção do veículo**:
   - **Selecionar veículo existente** (dropdown), OU
   - **Cadastrar novo veículo** (formulário inline).
5. Preenche **dados obrigatórios** conforme categoria.
6. Clica em **Registrar Entrada**.
7. Sistema cria o veículo (se novo) e registra check‑in.

## Requisitos Funcionais

### RF1 — Cadastro de veículo no fluxo
- **RF1.1** Deve permitir criar veículo diretamente no check‑in.
- **RF1.2** Deve validar campos mínimos por categoria (ver Regras de Validação).
- **RF1.3** Deve impedir duplicidade por VIN/chassi quando informado.

### RF2 — Check‑in
- **RF2.1** Deve registrar entrada com origem, data/hora, responsável e observações.
- **RF2.2** Deve atualizar status do veículo para estado coerente com a origem.
- **RF2.3** Deve registrar histórico de entrada.

### RF3 — UX e Mensagens
- **RF3.1** Exibir erros amigáveis (ProblemDetails quando disponíveis).
- **RF3.2** Manter botão “Registrar Entrada” desabilitado quando inválido.

## Regras de Validação (por categoria)
- **Novo**: VIN obrigatório; origem = montadora; placa opcional.
- **Seminovo**: placa obrigatória; VIN obrigatório; km obrigatório; referência de avaliação obrigatória (quando API exigir).
- **Demonstração**: VIN obrigatório; finalidade (`test‑drive` ou `frota interna`); placa obrigatória se já emplacado.

## UX/Interface (referência ao desenho anexado)
- Cards de **Origem do Veículo**.
- Bloco **Dados do Veículo** com campos principais e status de preenchimento.
- Botão primário **Registrar Entrada**.
- Lista lateral de **últimas movimentações** (opcional no MVP).

### Componentes (mínimo)
- Seletor de Origem (cards).
- Seletor de Categoria (botões/tabs).
- Toggle: “Selecionar existente” vs “Cadastrar novo”.
- Formulário de dados do veículo.
- Observações de chegada.

## Integração com API
### Cadastro do veículo
- **POST** `/api/v1/vehicles`

### Check‑in
- **POST** `/api/v1/vehicles/{id}/check-ins`

### Fluxo recomendado (criar + check‑in)
1. `POST /api/v1/vehicles` → retorna `vehicleId`.
2. `POST /api/v1/vehicles/{vehicleId}/check-ins`.

> Se a API suportar criação+check‑in atômico no futuro, substituir por endpoint único.

## RBAC
- Acesso ao check‑in: `STOCK_PERSON`, `STOCK_MANAGER`, `MANAGER`, `ADMIN`.
- Campos de cadastro visíveis para os mesmos perfis.

## Critérios de Aceite
- [ ] Usuário consegue cadastrar veículo e concluir check‑in sem sair da tela.
- [ ] Validações por categoria funcionam e bloqueiam envio quando inválido.
- [ ] Erros da API são exibidos de forma amigável.
- [ ] Histórico de entrada aparece no detalhe do veículo.

## Dependências
- Endpoints de criação de veículo e check‑in disponíveis na Stock API.
- Enum/labels de categoria/origem mapeados no frontend.

## Riscos
- API não suportar criação de veículo no MVP → fluxo continua bloqueado.
- Inconsistências de validação entre frontend e backend.

## Questões em Aberto
1. O endpoint `POST /api/v1/vehicles` já existe no Swagger local?
2. Qual o payload mínimo de criação por categoria?
3. A avaliação de seminovo é obrigatória no MVP ou pode ser opcional com validação posterior?
