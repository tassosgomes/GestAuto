---
status: completed
parallelizable: true
blocked_by: ["6.0", "7.0"]
---

<task_context>
<domain>engine/test-drive</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server|database</dependencies>
<unblocks>"9.0, 10.0"</unblocks>
</task_context>

# Tarefa 8.0: Fluxo de test-drive controlado

## Visão Geral
Implementar o fluxo controlado de test-drive (início e encerramento), garantindo rastreabilidade e status coerente. Deve suportar retorno ao estoque ou conversão em reserva.

## Requisitos
- `POST /api/v1/vehicles/{vehicleId}/test-drives/start` (RF7.1–RF7.2).
- `POST /api/v1/test-drives/{testDriveId}/complete` (RF7.3–RF7.4).
- Ao iniciar: status -> `em_test_drive`.
- Ao encerrar: status -> `em_estoque` ou `reservado` (se converter em negociação/reserva).
- Publicar eventos de negócio para start/completion (RF9.7).
- Manter histórico com responsável e timestamps.

## Subtarefas
- [x] 8.1 Criar DTOs e commands: start/complete
- [x] 8.2 Implementar `StartTestDriveCommand` + handler
- [x] 8.3 Implementar `CompleteTestDriveCommand` + handler (com opção de conversão em reserva)
- [x] 8.4 Ajustar validações de status (não permitir iniciar em `vendido`/`baixado`/`em_test_drive`)
- [x] 8.5 (Testes) Unit tests: start/complete; retorno ao estoque; conversão em reserva

## Sequenciamento
- Bloqueado por: 6.0, 7.0
- Desbloqueia: 9.0, 10.0
- Paralelizável: Sim

## Detalhes de Implementação
- A conversão em reserva deve reutilizar o mesmo mecanismo de reserva (7.0) para manter invariantes.

## Critérios de Sucesso
- Test-drive é rastreável e não deixa o veículo “sumir” do estoque.
- Encerramento sempre deixa o veículo em status coerente.

## Checklist de conclusão

- [x] 8.0 Fluxo de test-drive controlado ✅ CONCLUÍDA
	- [x] 8.1 Implementação completada
	- [x] 8.2 Definição da tarefa, PRD e tech spec validados
	- [x] 8.3 Análise de regras e conformidade verificadas
	- [x] 8.4 Revisão de código completada
	- [x] 8.5 Pronto para deploy
