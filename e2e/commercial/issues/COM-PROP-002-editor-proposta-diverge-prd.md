# COM-PROP-002 — Editor de Proposta diverge do PRD (seções/ações/validações)

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/proposals/new
- Usuário: `seller / 123456`

## Contexto (PRD)
- Editor esperado: `/commercial/proposals/{id}/edit` com seções A–D (Accordion) e barra lateral sticky.
- Regras: desconto > 5% exige aprovação; botões de ação (Salvar Rascunho, Solicitar Aprovação, Gerar PDF, Fechar Venda).
- Trade-in: fluxo de solicitação de avaliação e estados aguardando/avaliado + aceite/recusa.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `Comercial → Propostas → Nova Proposta`

## Resultado atual
- Existe formulário parcial em `/commercial/proposals/new`.
- Não existem botões “Solicitar Aprovação”, “Gerar PDF”, “Fechar Venda”.
- Não há validação de desconto > 5% nem bloqueio de fechamento.
- Trade-in não tem o fluxo do PRD (solicitar avaliação, estados, aceite/recusa com motivo).

## Resultado esperado
- Editor completo conforme PRD, incluindo regras de aprovação e fluxo de trade-in.

## Evidência
- Rota observada `/commercial/proposals/new` e ausência explícita dos botões/validações.

## Critérios de aceite
- [ ] UX/rotas alinhadas ao PRD (`/{id}/edit` ou equivalente documentado).
- [ ] Validação de desconto > 5% com alerta e bloqueio de “Fechar Venda”.
- [ ] Fluxo de aprovação (Solicitar Aprovação) implementado.
- [ ] Trade-in com solicitação de avaliação e estados.

## Sugestão de correção
- Implementar seções A–D, barra sticky e ações do PRD.
- Integrar com endpoints necessários (propostas, approvals, avaliação de seminovo).
