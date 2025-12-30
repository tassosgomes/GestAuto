# COM-LEAD-NEW-001 — Atalho “Novo Lead” no Dashboard quebra (rota /commercial/leads/new)

## Severidade
Blocker

## Ambiente
- URL: https://gestauto.tasso.local
- Usuário: `seller / 123456`

## Contexto (PRD)
- PRD define **Cadastro de Lead como Modal** na gestão de leads (seção 2.2).

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial`
3. Clicar em “Novo Lead” (Atalhos Rápidos)

## Resultado atual
- Navega para `/commercial/leads/new`.
- Tela exibe: “Erro ao carregar detalhes do lead.”
- Console reporta 404 (resource not found).

## Resultado esperado
- Abrir o modal de “Novo Lead” (ou, no mínimo, exibir um formulário funcional), sem erros.

## Evidência
- URL: `/commercial/leads/new`
- Conteúdo: `Erro ao carregar detalhes do lead.`
- Console: `Failed to load resource: the server responded with a status of 404`.

## Critérios de aceite
- [ ] Clicar em “Novo Lead” no dashboard abre o modal correto (ou rota funcional).
- [ ] Não há erro de carregamento/404.
- [ ] Fluxo cria lead com sucesso e exibe Toaster.

## Sugestão de correção
- Preferir: disparar o mesmo modal usado em `/commercial/leads`.
- Se mantiver rota `/new`: não tentar carregar detalhes por `{id}` inexistente.
