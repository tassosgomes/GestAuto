# COM-LEAD-DETAIL-005 — Aba “Propostas” do Lead não implementada

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads/{id}
- Usuário: `seller / 123456`

## Contexto (PRD)
- Aba “Propostas”: lista de cards resumidos (Veículo, Valor, Status) das propostas do lead.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Abrir um lead
3. Acessar a aba “Propostas”

## Resultado atual
- Exibe: “Funcionalidade de Propostas em desenvolvimento (Tarefa 7.0).”

## Resultado esperado
- Listagem das propostas do lead, com cards resumidos.

## Evidência
- Texto explícito de “em desenvolvimento”.

## Critérios de aceite
- [ ] Aba lista propostas do lead com status e valores.
- [ ] Estados vazio/erro tratados com mensagens e Toaster.

## Sugestão de correção
- Implementar endpoint/consulta por leadId e renderizar cards.
