# COM-LEADS-001 — Listagem de Leads não atende colunas do PRD

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads
- Usuário: `seller / 123456`

## Contexto (PRD)
- PRD exige colunas: Nome/Contato (com WhatsApp), Status (badge), Score (ícone), Interesse, Última Interação (relativa), Origem (badge).

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial/leads`

## Resultado atual
- Colunas presentes: `Nome`, `Status`, `Score`, `Interesse`, `Data Criação`, `Ações`.
- Não há contato/WhatsApp, Última Interação e Origem.

## Resultado esperado
- Colunas conforme PRD, com foco em operação (contato rápido e priorização).

## Evidência
- Linha exemplo: `SellerTest Clean | InNegotiation | Ouro | Civic | 24/12/2025 18:05 | Detalhes`.

## Reteste (2026-01-09)

### Resultado
- Coluna **Nome/Contato** inclui telefone/email e ação de WhatsApp.
- Coluna **Última Interação** mostra valor relativo.
- Coluna **Origem** aparece como badge.

## Critérios de aceite
- [x] Coluna `Nome/Contato` inclui telefone/email e ação de WhatsApp.
- [x] Coluna `Última Interação` exibe data relativa.
- [x] Coluna `Origem` exibe badge.

## Sugestão de correção
- Ajustar DataGrid e o DTO/consulta para trazer `origem` e `ultimaInteracao`.
