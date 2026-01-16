---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: []
---

<task_context>
<domain>frontend/build/performance</domain>
<type>optimization</type>
<scope>tech_debt</scope>
<complexity>medium</complexity>
<dependencies>build_pipeline</dependencies>
<unblocks>""</unblocks>
</task_context>

# Tarefa 13.0: Reduzir tamanho de chunks do build do frontend

## Visão Geral
Resolver o aviso do Vite sobre chunks > 500 kB, aplicando code-splitting e/ou configuração de bundle para reduzir o tamanho do bundle principal sem quebrar navegação.

## Requisitos
- Identificar os maiores chunks no build (Vite/Rollup).
- Aplicar `dynamic import()` em módulos pesados ou páginas do Stock/Commercial que não precisam carregar no bundle inicial.
- Avaliar `build.rollupOptions.output.manualChunks` no Vite para separar vendor/lib(s) maiores quando apropriado.
- Manter o comportamento de rotas e carregamento (incluindo lazy loading com fallback de loading).
- Não aumentar o tempo de carregamento percebido em páginas críticas.

## Subtarefas
- [x] 13.1 Medir bundle atual (ex.: `vite build --report` ou plugin equivalente) e registrar os maiores módulos.
- [x] 13.2 Aplicar lazy loading nas rotas/páginas de maior peso.
- [x] 13.3 Ajustar `manualChunks` se necessário para separar vendor grandes.
- [x] 13.4 Reexecutar build e validar redução dos chunks (>500 kB).

## Sequenciamento
- Bloqueado por: -
- Desbloqueia: -
- Paralelizável: Sim

## Detalhes de Implementação
- Vite: `build.rollupOptions.output.manualChunks`.
- React Router: `React.lazy` + `Suspense` para code splitting de rotas.

## Critérios de Sucesso
- Build sem warnings de chunks > 500 kB (ou justificativa documentada).
- Rotas e páginas principais carregam corretamente com lazy loading.

## Checklist de Conclusão
- [x] 13.0 Reduzir tamanho de chunks do build do frontend ✅ CONCLUÍDA
	- [x] 13.1 Implementação completada
	- [x] 13.2 Definição da tarefa, PRD e tech spec validados
	- [x] 13.3 Análise de regras e conformidade verificadas
	- [x] 13.4 Revisão de código completada
	- [x] 13.5 Pronto para deploy
