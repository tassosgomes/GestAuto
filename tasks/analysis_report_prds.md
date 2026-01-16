# Relatório de Análise de Consistência dos PRDs

**Data:** 13/01/2026
**Escopo:** Análise cruzada dos PRDs dos módulos Comercial, Estoque e Avaliação de Seminovos.

## 1. Resumo Executivo
A análise identificou pontos de desconexão significativos entre os módulos, principalmente no que tange à **identificação unívoca de veículos** durante o processo comercial e ao ciclo de vida da entrada de seminovos (trade-in). Embora os módulos estejam bem definidos isoladamente, as interfaces de integração (eventos e processos de negócio) apresentam lacunas que podem impedir o funcionamento do fluxo "ponta a ponta".

## 2. Inconsistências Críticas (Design & Processo)

### 2.1. Seleção e Reserva de Veículo na Proposta (Comercial vs. Estoque)
*   **Comercial (F4.2 / RF4.3):** O PRD descreve que a proposta registra "modelo, versão, cor, ano" e indica se é "pronta entrega". Não menciona o vínculo com um ID único (Chassi/StockID) do veículo específico.
*   **Estoque (RF6.1 / F3):** O módulo de Estoque exige que a reserva seja feita em um veículo específico (entidade única) para gerenciar o status `reservado` e garantir indisponibilidade.
*   **Problema:** Se o Vendedor selecionar apenas "caraterísticas" na proposta de pronta entrega, o sistema não sabe qual chassi específico bloquear no Estoque. Isso pode levar ao "overselling" (vender o mesmo carro duas vezes) ou impossibilidade de efetivar a reserva descrita no módulo de Estoque.
*   **Recomendação:** O PRD Comercial deve exigir a seleção de um `StockID` específico quando a modalidade for "Pronta Entrega".

### 2.2. Entrada de Seminovos no Estoque (Avaliação vs. Comercial vs. Estoque)
*   **Avaliação (Questões em Aberto):** Pergunta se o veículo aprovado entra automaticamente no estoque.
*   **Comercial (F5/F6):** Usa o valor da avaliação como crédito na proposta e fecha a venda. Não especifica o momento em que a propriedade do seminovo é transferida.
*   **Estoque (RF8.2):** Suporta entrada via "compra de cliente", mas exige "Check-in".
*   **Problema:** Não há gatilho definido para transformar uma "Avaliação Aprovada" vinculada a uma "Venda Fechada" em um "Item de Estoque". O seminovo não pode entrar no estoque apenas pela avaliação (pois a venda do carro novo pode não ocorrer), nem apenas pela venda fechada (pois depende da entrega física).
*   **Recomendação:** Definir que o evento `VendaFechada` (com trade-in) gera uma "Pré-entrada" ou "Ordem de Entrada" no módulo de Estoque, aguardando o Check-in físico para mudar status para `em_preparacao`.

### 2.3. Sincronização de Test-Drive
*   **Comercial (F3):** Permite "Agendar" test-drive (Data/Hora futura).
*   **Estoque (F3 / F7):** Gerencia status imediato `em_test_drive` (Ativo agora).
*   **Problema:** O módulo de Estoque não parece ter previsão para "Bloqueio futuro de agenda". Se o Comercial agendar um test-drive para amanhã, e o Estoque controlar apenas o status atual, não há garantia de que o carro estará disponível amanhã (pode ser vendido hoje).
*   **Recomendação:** Clarificar se o Estoque gerencia apenas *status de disponibilidade imediata* ou também *agenda de recursos*. Caso seja apenas status, o Comercial corre risco de agendar test-drive de carro que será vendido antes.

## 3. Inconsistências Menores e Terminologia

### 3.1. Nomes de Eventos
*   **Comercial (RF5.4):** Menciona consumir evento `AvaliacaoSeminvoRespondida` (Erro de digitação: "Seminvo").
*   **Avaliação:** Não define explicitamente o nome técnico do evento de saída no corpo do texto, apenas nos diagramas mentais de integração.
*   **Recomendação:** Padronizar para `AvaliacaoSeminovoConcluida` ou manter `AvaliacaoSeminovoRespondida` corrigindo o typo.

### 3.2. Status de Reserva vs. Status de Proposta
*   **Comercial:** Possui status detalhados (`aguardando_analise_credito`, etc).
*   **Estoque:** Possui tipos de reserva (`aguardando_banco`).
*   **Alinhamento:** É necessário mapear explicitamente quais status da Proposta disparam a criação/atualização da Reserva no Estoque. Ex: Mudar proposta para `aguardando_aprovacao_banco` deve atualizar o tipo de reserva no Estoque para `aguardando_banco` para ajustar o TTL (Time To Live) da reserva.

## 4. Tabela de Ações Recomendadas

| Módulo | Ação Necessária | Prioridade |
| :--- | :--- | :--- |
| **Comercial** | Incluir campo `StockItemID` (ou Chassi) obrigatório para propostas de "Pronta Entrega". | ALTA |
| **Comercial** | Corrigir typo no evento `AvaliacaoSeminvoRespondida`. | BAIXA |
| **Integração** | Definir fluxo de "Conversão de Avaliação em Estoque" (Gatilho: Venda Fechada + Check-in Físico). | ALTA |
| **Estoque** | Definir se suportará "Reservas de Agenda" (para Test-Drive futuro) ou apenas bloqueio imediato. | MÉDIA |

## 5. Conclusão
Os PRDs estão maduros individualmente, mas a integração transacional (o momento exato onde um objeto de um domínio passa a existir no outro) precisa de refinamento, especialmente no vínculo físico entre os itens (Carro Novo saindo, Carro Velho entrando).
