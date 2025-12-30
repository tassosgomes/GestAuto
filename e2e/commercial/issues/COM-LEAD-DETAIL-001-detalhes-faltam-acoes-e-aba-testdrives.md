# COM-LEAD-DETAIL-001 — Detalhes do Lead sem “Alterar Status” e sem aba “Test-Drives”

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads/{id}
- Usuário: `seller / 123456`

## Contexto (PRD)
- Cabeçalho deve ter ação “Alterar Status” (dropdown).
- Abas devem incluir “Test-Drives”.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Ir em `/commercial/leads` → clicar “Detalhes” em um lead

## Resultado atual
- Ações no header: “Agendar Test-Drive” e “Criar Proposta”.
- Abas: `Visão Geral`, `Qualificação`, `Timeline`, `Propostas`.
- Não existe dropdown “Alterar Status” e não existe aba “Test-Drives”.

## Reteste (30/12/2025)

### Resultado
- Dropdown “Alterar Status”: presente no header.
- Aba “Test-Drives”: presente.
- Ao clicar em “Test-Drives”: carrega corretamente e exibe estado vazio (“Nenhum test-drive encontrado para este lead.”), sem erro.

## Resultado esperado
- Dropdown “Alterar Status”.
- Aba “Test-Drives” com histórico e status.

## Evidência
- Tablist não inclui “Test-Drives”.

### Evidência do reteste
- Print do reteste (aba “Test-Drives” aberta com estado vazio e botão “Alterar Status” visível).

## Critérios de aceite
- [x] Dropdown de status implementado e funcional.
- [x] Aba “Test-Drives” implementada com histórico (Agendado/Realizado/Cancelado).

## Sugestão de correção
- Implementar atualização de status com feedback (Toaster) e refresh do lead.
