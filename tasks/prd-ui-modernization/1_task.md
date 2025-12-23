---
status: completed
parallelizable: false
blocked_by: []
---

<task_context>
<domain>frontend/infra</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>medium</complexity>
<dependencies>none</dependencies>
<unblocks>"2.0"</unblocks>
</task_context>

# Tarefa 1.0: Configuração Inicial e Infraestrutura

## Visão Geral
Configurar a base do projeto para suportar Tailwind CSS e Shadcn UI, incluindo ajustes de build (Vite), TypeScript aliases e definição de variáveis de tema.

## Requisitos
- Instalar Tailwind CSS, PostCSS e Autoprefixer.
- Configurar `vite.config.ts` para suportar path aliases (`@/*`).
- Configurar `tsconfig.json` (e `tsconfig.app.json`) para suportar path aliases.
- Inicializar Shadcn UI no projeto.
- Configurar variáveis CSS globais (`index.css`) com a paleta de cores do `code.html`.
- Configurar fonte "Inter" como padrão.

## Subtarefas
- [x] 1.1 Instalar dependências do Tailwind e inicializar configuração (`tailwind.config.js`, `postcss.config.js`).
- [x] 1.2 Configurar aliases `@` em `vite.config.ts` e `tsconfig.json`.
- [x] 1.3 Executar `npx shadcn@latest init` e configurar `components.json`.
- [x] 1.4 Atualizar `index.css` com as variáveis de cor (CSS variables) baseadas no tema Azul #135bec.
- [x] 1.5 Validar build do projeto (`npm run build`) para garantir que as configurações não quebraram o projeto.

## Sequenciamento
- Bloqueado por: N/A
- Desbloqueia: 2.0
- Paralelizável: Não

## Detalhes de Implementação
- Seguir guia oficial de instalação do Shadcn para Vite.
- As cores devem ser extraídas do `Modelos/code.html` ou definidas conforme Tech Spec (`--primary`, `--background`, etc.).
- Garantir que `src/lib/utils.ts` seja criado (utilitário `cn`).

## Critérios de Sucesso
- Comando `npm run dev` roda sem erros.
- Arquivo `tailwind.config.js` existe e está configurado.
- Alias `@/` funciona para importações.
- Variáveis CSS de tema estão presentes em `index.css`.

## Conclusão
- [x] 1.0 Configuração Inicial e Infraestrutura ✅ CONCLUÍDA
  - [x] 1.1 Implementação completada
  - [x] 1.2 Definição da tarefa, PRD e tech spec validados
  - [x] 1.3 Análise de regras e conformidade verificadas
  - [x] 1.4 Revisão de código completada
  - [x] 1.5 Pronto para deploy
