# Review Task 6.0 - Editor de Propostas (Básico)

## Status
- [x] Implementação concluída
- [x] Build validado (`npm run build` no frontend)
- [x] Lint validado (com exceção de warnings pré-existentes)

## Alterações Realizadas

### Frontend
- **Nova Página**: `ProposalEditorPage` (`/commercial/proposals/new`)
  - Layout com grid (Formulário à esquerda, Resumo à direita).
  - Integração com `react-hook-form` e `zod`.
- **Novos Componentes**:
  - `VehicleSelection`: Formulário para dados do veículo (Modelo, Versão, Cor, Ano, Preço, Disponibilidade).
  - `PaymentForm`: Formulário para condições de pagamento (Método, Entrada, Parcelas).
  - `ProposalSummary`: Card fixo lateral com cálculo em tempo real do valor financiado e total.
- **Hooks**:
  - `useProposals`: Adicionado hook `useCreateProposal` para integração com API.
- **Rotas**:
  - Adicionada rota `/commercial/proposals/new`.
  - Adicionado botão "Nova Proposta" na listagem.
- **Utils**:
  - Adicionada função `formatCurrency` em `lib/utils.ts`.

## Pontos de Atenção
- A seleção de veículo atualmente é manual (inputs de texto) conforme definido na tarefa básica. Futuramente poderá ser integrada ao estoque.
- O cálculo de parcelas no resumo é uma estimativa simples (simulação). A lógica real de juros deve vir do backend futuramente.
- O campo `leadId` está hardcoded temporariamente até haver seleção de Lead na criação da proposta (ou vir via parâmetro de URL da tela de detalhes do Lead).

## Próximos Passos
- Implementar a listagem real de propostas (Task 7.0).
- Implementar a visualização de detalhes da proposta.
