---
status: pending
parallelizable: false
blocked_by: ["5.0"]
---

<task_context>
<domain>frontend/cleanup</domain>
<type>chore</type>
<scope>maintenance</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>none</unblocks>
</task_context>

# Tarefa 6.0: Limpeza de Código Legado

## Visão Geral
Remover arquivos de estilo antigos, referências a bibliotecas de ícones descontinuadas e garantir que o projeto esteja limpo e performático.

## Requisitos
- Remover CSS global legado não utilizado.
- Remover links para Material Symbols (CDN ou package.json).
- Verificar e corrigir quaisquer regressões visuais finais.

## Subtarefas
- [x] 6.1 Identificar e remover estilos CSS legados em `index.css` ou outros arquivos CSS.
- [x] 6.2 Remover referências a Material Symbols em `index.html` ou `package.json`.
- [x] 6.3 Rodar linter e testes para garantir integridade.
- [x] 6.4 Validação final visual da aplicação.

## Sequenciamento
- Bloqueado por: 5.0
- Desbloqueia: Conclusão do projeto
- Paralelizável: Não

## Detalhes de Implementação
- Cuidado para não remover estilos necessários para componentes que ainda não foram migrados (se houver). Assumindo migração completa das páginas principais listadas.

## Critérios de Sucesso
- Build limpo sem warnings de CSS não utilizado (se configurado).
- Nenhuma requisição de rede para fontes de ícones antigas.
- Aplicação visualmente estável.
