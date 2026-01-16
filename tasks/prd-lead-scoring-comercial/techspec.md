# Especificação Técnica: Visualização e Qualificação de Leads (Lead Scoring)

## Visão Geral

Esta especificação técnica detalha a implementação do frontend para a funcionalidade de Lead Scoring no módulo Comercial. O objetivo é fornecer uma interface para qualificação de leads e visualização do score calculado pelo backend, permitindo que vendedores priorizem atendimentos baseados na rentabilidade potencial.

A solução será construída em React, integrando-se à API existente `GestAuto.Commercial.API` e utilizando os componentes de UI padrão do projeto (`shadcn/ui`).

## Arquitetura

A implementação é puramente no **Frontend**, consumindo endpoints já existentes no Backend.

### Diagrama de Componentes

```mermaid
graph TD
    Page[LeadDetailsPage] --> Header[LeadHeader]
    Page --> Tabs[Tabs]
    Header --> Badge[LeadScoreBadge]
    Tabs --> TabQual[Tab: Qualificação]
    TabQual --> Form[LeadQualificationForm]
    TabQual --> Feedback[LeadActionFeedback]
    Form --> Service[leadService.qualify]
    Service --> API[POST /leads/{id}/qualify]
```

### Fluxo de Dados

1.  **Leitura:** Ao carregar `LeadDetailsPage`, o objeto `Lead` é recuperado via `useLead` (React Query). O objeto já contém `score` e `qualification`.
2.  **Visualização:** `LeadHeader` renderiza `LeadScoreBadge` baseado no `lead.score`.
3.  **Edição:** O usuário acessa a aba "Qualificação" e preenche `LeadQualificationForm`.
4.  **Escrita:** Ao submeter, `leadService.qualify` é chamado.
5.  **Atualização:** Após sucesso da API, o cache do React Query é invalidado, forçando o refresh da página e atualização imediata do Badge e Feedback.

## Design de Componentes

### 1. `LeadScoreBadge` (Novo Componente)
Componente visual reutilizável para exibir o score.

*   **Props:** `{ score: string | undefined, showSla?: boolean }`
*   **Lógica:** Mapeia o enum de score (Diamond, Gold, Silver, Bronze) para cores, ícones (Lucide React) e textos de SLA.
*   **Uso:** `LeadHeader`, `LeadListPage` (Card).

### 2. `LeadQualificationForm` (Novo Componente)
Formulário para entrada de dados de qualificação.

*   **Tecnologia:** `react-hook-form` + `zod` resolver.
*   **Schema de Validação:**
    *   `paymentMethod`: Obrigatório.
    *   `hasTradeInVehicle`: Booleano.
    *   `tradeInVehicle`: Obrigatório se `hasTradeInVehicle` for true.
        *   `year`, `mileage`: Numéricos, obrigatórios.
        *   `generalCondition`: Enum (Excellent, Good, Fair).
*   **Comportamento:**
    *   Campos de veículo de troca aparecem condicionalmente.
    *   Botão de salvar exibe estado de loading.
    *   Toast de sucesso/erro após submissão.

### 3. `LeadActionFeedback` (Novo Componente)
Exibe recomendações baseadas no score atual.

*   **Props:** `{ score: string }`
*   **Lógica:** Switch case simples para retornar o texto de recomendação (ex: "Focar em Financiamento Parcial" para Prata).
*   **UI:** Componente `Alert` ou `Card` com cor de fundo suave correspondente ao score.

## Interfaces e Modelos

As interfaces já existem em `frontend/src/modules/commercial/types/index.ts`. Nenhuma alteração necessária nos tipos base, apenas garantir que o Frontend esteja alinhado com o Backend.

```typescript
// Referência (já existente)
export interface QualifyLeadRequest {
  hasTradeInVehicle: boolean;
  tradeInVehicle?: TradeInVehicle;
  paymentMethod?: string;
  expectedPurchaseDate?: string;
  interestedInTestDrive: boolean;
}
```

## Pontos de Integração

### Backend API
*   **Endpoint:** `POST /api/v1/leads/{id}/qualify`
*   **Serviço Frontend:** `leadService.qualify(id, data)` (Já implementado).

### Frontend Pages
*   **`LeadDetailsPage.tsx`:**
    *   Adicionar nova `TabsContent` com value="qualification".
    *   Renderizar `LeadQualificationForm` dentro desta aba.
*   **`LeadHeader.tsx`:**
    *   Substituir a lógica atual de badge pelo componente `LeadScoreBadge`.
*   **`LeadListPage.tsx`:**
    *   Atualizar o card de listagem para incluir `LeadScoreBadge` (versão compacta).

## Análise de Impacto

*   **Performance:** Baixo impacto. A validação é client-side e a chamada de API é leve.
*   **Segurança:** O endpoint requer autenticação (já tratada pelo `api.ts` interceptor).
*   **UX:** Melhoria significativa na visibilidade de prioridades.

## Estratégia de Testes

### Testes Unitários (Vitest + Testing Library)
*   **`LeadScoreBadge`:** Verificar se renderiza a cor e texto corretos para cada score.
*   **`LeadQualificationForm`:**
    *   Verificar validação de campos obrigatórios.
    *   Verificar se campos de troca aparecem/somem baseados no checkbox.
    *   Verificar chamada do `onSubmit` com dados formatados.

### Testes de Integração
*   Não aplicável para este escopo (foco em componentes isolados).

## Observabilidade
*   Logs de erro no `console.error` em caso de falha na API (capturados por ferramentas de monitoramento se existirem).
*   Feedback visual via `Toast` para o usuário.

## Plano de Rollout

1.  Criar componentes UI (`LeadScoreBadge`, `LeadActionFeedback`).
2.  Criar formulário (`LeadQualificationForm`) com validação Zod.
3.  Integrar componentes na `LeadDetailsPage` e `LeadHeader`.
4.  Atualizar `LeadListPage`.
5.  Testes manuais e unitários.
