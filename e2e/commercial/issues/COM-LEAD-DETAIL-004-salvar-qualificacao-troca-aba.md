# COM-LEAD-DETAIL-004 — “Salvar Qualificação” muda de aba (perda de contexto)

## Severidade
Baixa

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads/{id}
- Usuário: `seller / 123456`

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Abrir um lead
3. Ir na aba “Qualificação”
4. Clicar “Salvar Qualificação”

## Resultado atual
- UI retorna automaticamente para a aba “Visão Geral”.

## Resultado esperado
- Permanecer na aba “Qualificação” e exibir feedback (Toaster) sem perder contexto.

## Evidência
- Após salvar, tab selecionada passa para “Visão Geral”.

## Critérios de aceite
- [ ] Permanecer na mesma aba após salvar.
- [ ] Exibir Toaster de sucesso/erro.

## Sugestão de correção
- Não alterar `activeTab` após submit; atualizar estado do lead em background.
