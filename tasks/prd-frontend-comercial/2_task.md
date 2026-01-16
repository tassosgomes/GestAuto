---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>react-router</dependencies>
<unblocks>3.0</unblocks>
</task_context>

# Tarefa 2.0: Estrutura Base do Módulo Comercial

## Visão Geral
Criação da estrutura de diretórios e rotas base para o módulo comercial, garantindo isolamento e organização do código.

## Requisitos
- Criar estrutura de pastas em `src/modules/commercial`.
- Definir rotas internas do módulo em `src/modules/commercial/routes.tsx`.
- Criar `CommercialLayout` com navegação lateral ou breadcrumbs específicos.
- Integrar rotas do módulo no `App.tsx`.

## Subtarefas
- [ ] 2.1 Criar pastas: `components`, `hooks`, `pages`, `services`, `types`.
- [ ] 2.2 Criar `CommercialLayout.tsx` (pode ser um wrapper simples inicialmente).
- [ ] 2.3 Criar páginas placeholder: `DashboardPage`, `LeadListPage`, `ProposalListPage`.
- [ ] 2.4 Configurar rotas em `src/modules/commercial/routes.tsx` e importar no `App.tsx`.

## Detalhes de Implementação
- Estrutura sugerida na Tech Spec.
- Rotas devem ser protegidas (já garantido pelo `RequireAuth` global, mas verificar roles se necessário).

## Critérios de Sucesso
- Navegação para `/commercial` carrega o `CommercialLayout` e a `DashboardPage`.
- Navegação para `/commercial/leads` carrega a `LeadListPage`.
