---
status: pending
parallelizable: false
blocked_by: ["3.0"]
---

<task_context>
<domain>frontend/integration</domain>
<type>refactoring</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>react_router</dependencies>
<unblocks>"6.0"</unblocks>
</task_context>

# Tarefa 5.0: Integração do Layout e Migração de Páginas

## Visão Geral
Aplicar o novo `AppLayout` às rotas autenticadas e refatorar as páginas principais (`HomePage`, `AdminPage`) para usar os novos componentes e estilos, removendo dependências antigas.

## Requisitos
- Envolver rotas protegidas no `AppLayout` em `App.tsx`.
- Refatorar `HomePage` para usar Shadcn/Tailwind.
- Refatorar `AdminPage` para usar Shadcn/Tailwind.
- Substituir ícones Material Symbols por Lucide React nas páginas migradas.

## Subtarefas
- [x] 5.1 Atualizar `App.tsx` para usar `AppLayout` como wrapper das rotas privadas.
- [x] 5.2 Refatorar `HomePage`: substituir HTML/CSS legado por componentes Shadcn (`Card`, `Button`, etc.).
- [x] 5.3 Refatorar `AdminPage`: adequar ao novo layout e componentes.
- [x] 5.4 Substituir todos os ícones legados nas páginas por `lucide-react`.
- [x] 5.5 Verificar navegação e responsividade das páginas migradas.

## Sequenciamento
- Bloqueado por: 3.0
- Desbloqueia: 6.0
- Paralelizável: Não

## Detalhes de Implementação
- Manter lógica de negócio existente, alterar apenas a camada de apresentação.
- Garantir que o `Outlet` do Router esteja funcionando corretamente dentro do Layout.

## Critérios de Sucesso
- Aplicação navega corretamente com o novo Layout.
- `HomePage` e `AdminPage` estão visualmente consistentes com o novo Design System.
- Nenhum ícone quebrado.
