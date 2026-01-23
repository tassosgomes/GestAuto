---
status: pending
parallelizable: false
blocked_by: ["10.0"]
---

<task_context>
<domain>documentation</domain>
<type>documentation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies></dependencies>
<unblocks>""</unblocks>
</task_context>

# Tarefa 11.0: Atualizar Documentação do Módulo de Estoque

## Visão Geral

Atualizar o README e documentação do módulo de estoque para incluir o novo fluxo de cadastro de veículo com check-in. A documentação deve explicar o fluxo, os componentes criados, e como usar a funcionalidade.

<requirements>
- Atualizar README do módulo de estoque
- Documentar fluxo de cadastro + check-in
- Documentar componentes criados
- Incluir screenshots ou diagramas do fluxo
- Documentar regras de validação por categoria
- Documentar integração com serviço de avaliações
</requirements>

## Subtarefas

- [ ] 11.1 Localizar README do módulo de estoque (ou criar se não existir)
- [ ] 11.2 Adicionar seção sobre fluxo de cadastro + check-in
- [ ] 11.3 Documentar campos obrigatórios por categoria
- [ ] 11.4 Documentar mapeamento origem → categorias
- [ ] 11.5 Adicionar diagrama de fluxo
- [ ] 11.6 Documentar tratamento de erros e fallbacks
- [ ] 11.7 Revisar e publicar documentação

## Sequenciamento

- **Bloqueado por**: 10.0 (Acessibilidade)
- **Desbloqueia**: Nenhum — tarefa final
- **Paralelizável**: Não — documentação final

## Detalhes de Implementação

### Estrutura do README

```markdown
# Módulo de Estoque

## Visão Geral

O módulo de estoque gerencia veículos em estoque, incluindo cadastro, check-in, 
check-out, reservas e movimentações.

## Funcionalidades

### Cadastro de Veículo com Check-in

Fluxo unificado para cadastrar um veículo e registrar sua entrada no estoque 
em uma única operação.

#### Acesso

- **URL**: `/stock/check-in`
- **Roles permitidas**: `STOCK_PERSON`, `STOCK_MANAGER`, `MANAGER`, `ADMIN`

#### Fluxo

1. **Selecionar Origem**: Define de onde o veículo está vindo
   - Montadora → categoria "Novo" auto-selecionada
   - Compra Cliente/Seminovo → categoria "Seminovo" auto-selecionada
   - Transferência → permite todas as categorias
   - Frota Interna → categoria "Demonstração" auto-selecionada

2. **Selecionar Categoria** (se aplicável): Define o tipo do veículo
   - Novo: veículo 0km
   - Seminovo: veículo usado com placa
   - Demonstração: veículo para test-drive/exposição

3. **Preencher Formulário**: Campos variam por categoria

4. **Registrar Entrada**: Cria veículo + registra check-in

#### Campos por Categoria

| Campo | Novo | Seminovo | Demonstração |
|-------|------|----------|--------------|
| VIN | ✅ | ✅ | ✅ |
| Marca | ✅ | ✅ | ✅ |
| Modelo | ✅ | ✅ | ✅ |
| Ano | ✅ | ✅ | ✅ |
| Cor | ✅ | ✅ | ✅ |
| Placa | ○ | ✅ | ○* |
| KM | ○ | ✅ | ○ |
| ID Avaliação | ○ | ✅ | ○ |
| Finalidade | ○ | ○ | ✅ |

✅ = obrigatório | ○ = opcional | * = obrigatório se emplacado

#### Validação de Avaliação (Seminovos)

Para veículos seminovos, o sistema valida se o ID de avaliação informado 
existe no serviço `vehicle-evaluation`. Se o serviço estiver indisponível, 
o cadastro prossegue com um aviso.

## Componentes

### VehicleCheckInPage

Página principal do fluxo de cadastro + check-in.

### OriginSelector

Componente de seleção de origem via cards clicáveis.

### CategorySelector

Componente de seleção de categoria com auto-seleção.

### DynamicVehicleForm

Formulário dinâmico com campos condicionais por categoria.

## Integração com APIs

### Endpoints Utilizados

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/vehicles` | POST | Cadastro de veículo |
| `/api/v1/vehicles/{id}/check-ins` | POST | Registro de check-in |
| `/api/v1/evaluations/{id}` | GET | Validação de avaliação |

## Tratamento de Erros

| Erro | Mensagem | Ação |
|------|----------|------|
| VIN duplicado | "Já existe um veículo com este VIN" | Corrigir VIN |
| Avaliação não encontrada | "Avaliação não encontrada" | Verificar ID |
| Serviço indisponível | Warning + prossegue | Log + fallback |
```

### Diagrama de Fluxo para Documentação

```
┌─────────────────────────────────────────────────────────────────┐
│                   FLUXO DE CADASTRO + CHECK-IN                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐       │
│  │   ORIGEM    │────▶│  CATEGORIA  │────▶│ FORMULÁRIO  │       │
│  │  (cards)    │     │ (auto/tabs) │     │  (dinâmico) │       │
│  └─────────────┘     └─────────────┘     └─────────────┘       │
│        │                   │                   │                │
│        │                   │                   ▼                │
│        │                   │          ┌─────────────┐          │
│        │                   │          │  VALIDAÇÃO  │          │
│        │                   │          │ (avaliação) │          │
│        │                   │          └──────┬──────┘          │
│        │                   │                 │                  │
│        │                   │                 ▼                  │
│        │                   │          ┌─────────────┐          │
│        │                   │          │   SUBMIT    │          │
│        │                   │          │ (create +   │          │
│        │                   │          │  check-in)  │          │
│        │                   │          └──────┬──────┘          │
│        │                   │                 │                  │
│        ▼                   ▼                 ▼                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              VEÍCULO DISPONÍVEL NO ESTOQUE              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Critérios de Sucesso

- [ ] README atualizado com nova funcionalidade
- [ ] Campos obrigatórios documentados por categoria
- [ ] Diagrama de fluxo incluído
- [ ] Endpoints e integrações documentados
- [ ] Tratamento de erros documentado
- [ ] Documentação revisada por outro membro da equipe
