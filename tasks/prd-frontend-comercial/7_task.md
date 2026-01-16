---
status: completed
parallelizable: false
blocked_by: ["6.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>9.0</unblocks>
</task_context>

# Tarefa 7.0: Funcionalidade: Editor de Propostas (Avançado e Integrações)

## Visão Geral
Adição de funcionalidades avançadas ao editor: Itens acessórios, Avaliação de Seminovo e Aprovação de Desconto.

## Requisitos
- Adicionar/Remover itens acessórios.
- Solicitar avaliação de seminovo (integração).
- Validação de desconto (regra de 5%).

## Subtarefas
- [x] 7.1 Implementar Seção de Itens/Acessórios.
- [x] 7.2 Implementar Seção de Seminovo (Formulário + Botão Solicitar Avaliação).
- [x] 7.3 Implementar lógica de validação de desconto e status `AGUARDANDO_APROVACAO`.
- [x] 7.4 Finalizar fluxo de fechamento de venda.

## Detalhes de Implementação
- Integração com API de avaliação (mockada ou real se disponível).

## Critérios de Sucesso
- Usuário consegue adicionar acessórios e ver total atualizado.
- Usuário consegue solicitar avaliação de seminovo.
- Sistema bloqueia fechamento se desconto > 5% sem aprovação.
