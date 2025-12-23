---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>frontend/layout</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>auth_context</dependencies>
<unblocks>"5.0"</unblocks>
</task_context>

# Tarefa 3.0: Implementação do Layout (Sidebar e Header)

## Visão Geral
Desenvolver os componentes estruturais da aplicação: Sidebar (navegação lateral), Header (topo) e o componente container AppLayout.

## Requisitos
- Criar componente `Sidebar` responsivo (desktop fixo, mobile drawer/sheet).
- Criar componente `Header` com título, busca visual e menu de usuário.
- Criar `AppLayout` que integre Sidebar e Header e renderize o conteúdo filho (`Outlet`).
- Integrar com contexto de autenticação para exibir dados do usuário no Header.
- Sidebar deve destacar a rota ativa.

## Subtarefas
- [x] 3.1 Criar componente `Sidebar` com navegação baseada em configuração (array de rotas).
- [x] 3.2 Implementar responsividade na Sidebar (usar componente `Sheet` do Shadcn para mobile).
- [x] 3.3 Criar componente `Header` com `UserNav` (avatar + dropdown de logout).
- [x] 3.4 Criar componente `AppLayout` combinando Sidebar e Header.
- [x] 3.5 Adicionar testes unitários para renderização do Layout.

## Sequenciamento
- Bloqueado por: 2.0
- Desbloqueia: 5.0
- Paralelizável: Sim (com 4.0)

## Detalhes de Implementação
- `src/components/layout/Sidebar.tsx`
- `src/components/layout/Header.tsx`
- `src/components/layout/AppLayout.tsx`
- Usar `lucide-react` para ícones do menu.
- Usar `useLocation` do react-router para active state.

## Critérios de Sucesso
- Layout renderiza corretamente em Desktop e Mobile.
- Menu mobile abre/fecha corretamente.
- Header exibe avatar do usuário (mock ou real se auth estiver pronta).
