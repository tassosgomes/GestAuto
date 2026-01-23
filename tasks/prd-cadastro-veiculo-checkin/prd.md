# PRD — Cadastro de Veículo + Check-in (MVP)

> Ajustado — Separação clara entre Estoque e Precificação

## Visão Geral

Este PRD define o fluxo de **cadastro de veículo** integrado ao **check-in** no módulo de estoque. O objetivo é permitir que um veículo **passe a existir no estoque** e que a **entrada física** seja registrada no mesmo fluxo, garantindo rastreabilidade **independente de precificação**.

### Princípio de Negócio

> Um veículo pode existir no estoque e estar disponível para negociação **sem possuir preço de venda definido**.  
> O preço nasce na **proposta comercial** e é validado posteriormente pelo Financeiro/Gerência.

**Problema**: O fluxo atual de check-in exige selecionar um veículo já cadastrado e cria dependência indevida com precificação, o que bloqueia a operação e reduz velocidade de venda.

**Proposta de valor**: Unificar cadastro e entrada do veículo, permitindo que o ativo físico exista no estoque de forma imediata, enquanto precificação ocorre em módulo separado, sem travar o Comercial.

## Objetivos

| Objetivo | Métrica de Sucesso |
|----------|-------------------|
| Criar veículo + registrar check-in em um único fluxo | 100% dos check-ins concluídos sem pré-cadastro |
| Garantir validações mínimas por categoria | 0 veículos ativos inválidos |
| Permitir disponibilidade comercial sem preço | 0 bloqueios por "precificação pendente" |
| Manter rastreabilidade desde a entrada | Histórico completo por veículo |
| Reduzir tempo de entrada no estoque | ≤ 2 minutos |

## Princípios de Negócio (Explícitos)

| Princípio | Descrição |
|-----------|-----------|
| Estoque não define preço de venda | Preço não é atributo do módulo de estoque |
| Check-in não depende de precificação | Entrada física é independente de valor comercial |
| Seminovo exige avaliação (custo), não preço | Avaliação define custo de aquisição |
| Preço final só existe dentro de proposta | Comercial define preço na negociação |
| Financeiro valida margem e risco, não cria preço | Financeiro atua na validação, não na definição |

## Histórias de Usuário

### Persona Primária: Responsável pelo Estoque (`STOCK_PERSON`)

- **HU-01**: Como responsável pelo estoque, quero cadastrar um veículo diretamente no check-in para que ele passe a existir no sistema assim que chega fisicamente.
- **HU-02**: Como responsável pelo estoque, quero registrar a origem do veículo para que o status inicial seja aplicado corretamente.
- **HU-03**: Como responsável pelo estoque, quero concluir o check-in mesmo sem preço definido para não travar o fluxo comercial.

### Persona Secundária: Gestor de Estoque (`STOCK_MANAGER`)

- **HU-04**: Como gestor, quero auditar todas as entradas de um veículo para garantir controle operacional.
- **HU-05**: Como gestor, quero garantir que veículos seminovos só entrem no estoque se houver avaliação vinculada.

## Funcionalidades Principais

### F1 — Cadastro de Veículo no Fluxo de Check-in

**O que faz**: Cria o registro do ativo físico (veículo) no sistema no mesmo fluxo do check-in.

**Importante**: Este cadastro **não inclui preço de venda**.

**Requisitos Funcionais**:
- **RF1.1** — Criar veículo diretamente no fluxo de check-in.
- **RF1.2** — Validar campos obrigatórios por categoria.
- **RF1.3** — Impedir duplicidade por VIN/chassi quando informado.
- **RF1.4** — Permitir que o veículo seja criado sem qualquer informação de preço.

### F2 — Registro de Check-in

**O que faz**: Registra a entrada física do veículo no estoque.

**Requisitos Funcionais**:
- **RF2.1** — Registrar origem, data/hora automática, responsável e observações.
- **RF2.2** — Atualizar status inicial coerente com a origem.
- **RF2.3** — Registrar histórico de entrada visível no detalhe do veículo.
- **RF2.4** — Não exigir precificação para conclusão do check-in.

### F3 — Disponibilidade Comercial

**O que faz**: Define quando um veículo pode ser negociado pelo Comercial.

**Regras de Negócio**: Um veículo pode estar disponível para negociação quando:
- Existe no estoque
- Possui status `em_estoque`
- No caso de seminovo: avaliação concluída

**Requisitos Funcionais**:
- **RF3.1** — Veículos sem preço devem aparecer como disponíveis para o Comercial.
- **RF3.2** — O sistema não deve bloquear reserva por ausência de preço.
- **RF3.3** — Status, e não preço, define disponibilidade.

### F4 — Validações por Categoria

| Categoria | Campos Obrigatórios | Observações |
|-----------|---------------------|-------------|
| **Novo** | VIN | Origem = `montadora` |
| **Seminovo** | Placa, VIN, KM, Avaliação vinculada | Avaliação define custo |
| **Demonstração** | VIN, Finalidade | Placa se já emplacado |

> **Preço não é campo obrigatório em nenhuma categoria.**

### F5 — Auditoria

**Requisitos Funcionais**:
- **RF5.1** — Registrar quem, quando e como o veículo foi criado.
- **RF5.2** — Histórico completo acessível por veículo.
- **RF5.3** — Operações críticas exigem usuário autenticado.

## Experiência do Usuário

### Fluxo Principal

```
┌─────────────────────────────────────────────────────────────────┐
│  1. Acessa Registrar Entrada (Check-in)                         │
│                         ↓                                       │
│  2. Seleciona Origem (cards): montadora | compra | transferência│
│                         ↓                                       │
│  3. Escolhe Categoria: novo | seminovo | demonstração           │
│                         ↓                                       │
│  4. Modo veículo: [Selecionar existente] ou [Cadastrar novo]    │
│                         ↓                                       │
│  5. Preenche dados obrigatórios (sem preço)                     │
│                         ↓                                       │
│  6. Clica em "Registrar Entrada"                                │
│                         ↓                                       │
│  7. Sistema cria veículo (se novo) + registra check-in          │
│                         ↓                                       │
│  8. Veículo disponível para Comercial (sem preço)               │
└─────────────────────────────────────────────────────────────────┘
```

### Componentes de Interface

| Componente | Descrição |
|------------|-----------|
| Seletor de Origem | Cards clicáveis (montadora, compra cliente/seminovo, transferência, frota interna) |
| Seletor de Categoria | Botões/tabs (novo, seminovo, demonstração) |
| Toggle de Modo | Alternar entre "Selecionar existente" e "Cadastrar novo" |
| Formulário Dinâmico | Campos variam conforme categoria (sem campos de preço) |
| Campo Observações | Textarea para notas de chegada |
| Botão Primário | "Registrar Entrada" (desabilitado se inválido) |

### Acessibilidade

- Labels associados a todos os campos de formulário
- Mensagens de erro anunciadas via `aria-live`
- Navegação por teclado funcional em todos os componentes
- Contraste mínimo WCAG 2.1 AA

## Restrições Técnicas de Alto Nível

### Integrações

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/vehicles` | POST | Cadastro de veículo (já disponível) |
| `/api/v1/vehicles/{id}/check-ins` | POST | Registro de check-in |
| `/api/v1/vehicles/with-checkin` | POST | Criação + check-in atômico |

**Fluxo de integração recomendado**:
- Utilizar endpoint atômico `POST /api/v1/vehicles/with-checkin` para criar veículo e registrar entrada em uma única operação.

### Controle de Acesso (RBAC)

Perfis com acesso ao fluxo completo:
- `STOCK_PERSON`
- `STOCK_MANAGER`
- `MANAGER`
- `ADMIN`

## Não-Objetivos (Explícitos)

| Item Excluído | Justificativa |
|---------------|---------------|
| ❌ Definir preço de venda | Preço nasce na proposta comercial |
| ❌ Calcular margem | Responsabilidade do Financeiro |
| ❌ Validar rentabilidade | Fora do escopo do estoque |
| ❌ Bloquear estoque por preço pendente | Preço não é requisito para disponibilidade |
| ❌ Upload de fotos | PRD específico |
| ❌ Documentação fiscal | Integração fiscal fora do MVP |
| ❌ Transferência completa entre lojas | Fase posterior |

## Critérios de Aceite

- [ ] Usuário consegue cadastrar veículo e concluir check-in sem sair da tela
- [ ] Validações por categoria funcionam e bloqueiam envio quando formulário inválido
- [ ] Check-in pode ser concluído **sem informar preço**
- [ ] Veículo fica disponível para Comercial imediatamente após check-in
- [ ] Histórico de entrada aparece no detalhe do veículo
- [ ] Duplicidade por VIN é detectada e erro exibido ao usuário

## Questões em Aberto (Atualizadas)

| # | Questão | Status |
|---|---------|--------|
| 1 | Veículo pode existir sem preço? | ✅ Sim |
| 2 | Preço é requisito para venda? | ❌ Não — só para faturamento |
| 3 | Avaliação substitui preço no seminovo? | ✅ Sim (define custo) |
| 4 | Estoque controla margem? | ❌ Não |

> Todas as questões de negócio foram esclarecidas. PRD pronto para implementação.
