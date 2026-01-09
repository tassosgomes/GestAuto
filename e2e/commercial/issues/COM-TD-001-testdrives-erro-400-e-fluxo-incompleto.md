# COM-TD-001 — Test-Drives: erro HTTP 400 no carregamento e fluxo incompleto

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/test-drives
- Usuário: `seller / 123456`

## Contexto (PRD)
- Agenda diária/semanal e visualização de slots ocupados.
- Modal de agendamento com veículo da frota, data/hora e vínculo ao lead.
- Execução mobile-first com checklists e iniciar/finalizar.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial/test-drives`

## Resultado atual
- Console reporta falha ao carregar test-drives (HTTP 400).
- UI mostra tabela simples “Agenda” e estado vazio “Nenhum test-drive agendado.”

## Resultado esperado
- Carregamento sem erros.
- Implementação de agenda/execução conforme PRD.

## Evidência
- Console: `Failed to load test drives` + `status 400`.

## Critérios de aceite
## Reteste (2026-01-09)

### Resultado
- Carregamento/erros tratados com Toaster e estado vazio adequado.
- Agenda com visualização diária e semanal.
- Execução disponível via fluxo “Iniciar/Finalizar” (modal) para agendados.

## Critérios de aceite
- [x] Endpoint/contrato corrigido (sem 400).
- [x] Erros tratados com Toaster + estado vazio adequado.
- [x] Agenda (diária/semanal) e execução (mobile-first) implementadas.

## Sugestão de correção
- Revisar request/params enviados ao backend e alinhar com swagger.
- Implementar componentes de agenda e fluxo de execução do PRD.
