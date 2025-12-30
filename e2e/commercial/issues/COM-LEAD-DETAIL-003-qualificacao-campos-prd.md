# COM-LEAD-DETAIL-003 — Qualificação sem “Renda Mensal Estimada” e “Prazo de Compra” do PRD

## Severidade
Média

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads/{id}
- Usuário: `seller / 123456`

## Contexto (PRD)
- Campos esperados: Renda Mensal Estimada; Prazo de Compra (Imediato/15 dias/30 dias+); etc.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Abrir um lead
3. Ir na aba “Qualificação”

## Resultado atual
- Não foi evidenciado campo “Renda Mensal Estimada”.
- “Prazo de Compra” aparece como “Previsão de Compra” (comportamento aparenta ser diferente do PRD).

## Resultado esperado
- Exibir “Renda Mensal Estimada” e “Prazo de Compra” conforme PRD.

## Evidência
- Na aba “Qualificação” não há campo “Renda”; “Previsão de Compra” aparece como combobox.

## Critérios de aceite
- [ ] Campo “Renda Mensal Estimada” presente e persistido.
- [ ] Campo “Prazo de Compra” com opções do PRD.

## Sugestão de correção
- Alinhar modelo de dados e UI aos campos do PRD (mantendo campos extras apenas se não conflitar).
