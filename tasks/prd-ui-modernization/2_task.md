---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/ui</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>"3.0", "4.0"</unblocks>
</task_context>

# Tarefa 2.0: Instalação de Componentes Base e Ícones

## Visão Geral
Instalar a biblioteca de ícones Lucide React e os componentes essenciais do Shadcn UI que serão utilizados no layout e nas páginas.

## Requisitos
- Instalar `lucide-react`.
- Instalar componentes Shadcn listados no PRD/Tech Spec.
- Customizar estilos base se necessário (arredondamento, etc - geralmente feito no init, mas verificar componentes).

## Subtarefas
- [x] 2.1 Instalar `lucide-react`.
- [x] 2.2 Instalar componentes Shadcn: `button`, `card`, `input`, `label`, `separator`, `sheet`, `avatar`, `dropdown-menu`.
- [x] 2.3 Verificar se os componentes foram criados em `src/components/ui`.

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 4.0
- Paralelizável: Não

## Detalhes de Implementação
- Usar CLI do shadcn: `npx shadcn@latest add [component]`.
- Componentes devem residir em `src/components/ui`.

## Critérios de Sucesso
- Todos os componentes listados existem em `src/components/ui`.
- `lucide-react` está listado no `package.json`.
