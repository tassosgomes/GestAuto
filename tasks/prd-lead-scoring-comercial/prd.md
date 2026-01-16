# Documento de Requisitos de Produto (PRD): Visualização e Qualificação de Leads (Lead Scoring)

## Visão Geral

Este PRD define a implementação da interface de usuário (Frontend) para a funcionalidade de **Lead Scoring** no Módulo Comercial. O backend já possui a lógica de negócio implementada (`LeadScoringService`), capaz de classificar leads em Diamante, Ouro, Prata e Bronze com base em critérios de rentabilidade (forma de pagamento, veículo na troca, prazo de compra).

O foco deste produto é entregar uma interface intuitiva que incentive o vendedor a preencher os dados de qualificação e, em troca, forneça feedback visual imediato sobre a "temperatura" e prioridade do lead, guiando o esforço de vendas para as oportunidades mais rentáveis.

## Objetivos

1.  **Aumentar a captura de dados de qualificação:** Tornar o preenchimento de dados como "Forma de Pagamento" e "Veículo de Troca" parte natural do fluxo de atendimento.
2.  **Direcionar esforço de vendas:** Exibir claramente a classificação do lead (Score) para que o vendedor saiba quais clientes priorizar (SLA de atendimento).
3.  **Feedback Imediato:** Apresentar o Score calculado assim que os dados forem salvos, reforçando o valor do preenchimento das informações.

## Histórias de Usuário

### Vendedor
- **Como** vendedor, **quero** registrar facilmente se o cliente tem um carro na troca e como pretende pagar, **para que** o sistema possa avaliar o potencial do negócio.
- **Como** vendedor, **quero** ver um indicador visual (ex: selo Diamante, Ouro) no card do lead, **para que** eu saiba rapidamente quais clientes exigem atenção imediata (SLA).
- **Como** vendedor, **quero** saber o "SLA de Atendimento" (ex: "Atender em 10 min") associado ao lead, **para que** eu não perca o timing de clientes quentes.

### Gerente Comercial
- **Como** gerente, **quero** identificar visualmente na listagem de leads quais são as oportunidades "Diamante", **para que** eu possa acompanhar de perto essas negociações de alta rentabilidade.

## Funcionalidades Principais

### 1. Formulário de Qualificação de Lead
Interface para entrada dos dados críticos para o cálculo do score.
- **O que faz:** Permite ao vendedor inserir/editar dados de qualificação de um lead existente.
- **Campos:**
    - **Forma de Pagamento:** Seleção (À Vista, Financiamento).
    - **Veículo na Troca?** (Sim/Não). Se Sim:
        - Modelo/Ano.
        - Quilometragem.
        - Condição Geral (Excelente, Bom, Regular).
        - Histórico de Revisões (Sim/Não).
    - **Previsão de Compra:** Data ou Faixa (Imediato, 7 dias, 15 dias, 30+ dias).
    - **Score de Crédito (Declarado):** Checkbox "Cliente afirma ter crédito pré-aprovado".
- **Requisitos Funcionais:**
    1.  Deve consumir o endpoint de qualificação existente na API.
    2.  Deve validar campos obrigatórios condicionalmente (ex: dados do veículo só se "Troca = Sim").

### 2. Visualização de Score e SLA (Badge de Prioridade)
Componente visual que traduz o `LeadScore` retornado pela API em elementos de UI.
- **O que faz:** Exibe a classificação do lead com cores e ícones distintos.
- **Mapeamento Visual:**
    - 💎 **Diamante:** Cor Azul/Roxo + Ícone Diamante + Texto "Prioridade Máxima (10 min)".
    - 🥇 **Ouro:** Cor Dourada + Ícone Medalha + Texto "Alta Prioridade (30 min)".
    - 🥈 **Prata:** Cor Prata/Cinza + Ícone Medalha + Texto "Média Prioridade (2h)".
    - 🥉 **Bronze:** Cor Bronze/Marrom + Ícone Medalha + Texto "Baixa Prioridade".
- **Localização:**
    - Cabeçalho do Detalhe do Lead.
    - Card do Lead na Listagem (Kanban/Lista).

### 3. Feedback de Ações Recomendadas
- **O que faz:** Baseado no Score, exibe uma "Dica de Ação" para o vendedor (texto curto).
- **Exemplos:**
    - Diamante: "Acompanhamento Gerencial Recomendado".
    - Prata: "Focar em Financiamento Parcial".
    - Bronze: "Nutrição Automática".

## Experiência do Usuário (UX)

- **Fluxo:** O vendedor acessa um lead -> Clica na aba/botão "Qualificação" -> Preenche o formulário -> Salva -> O sistema atualiza o cabeçalho do lead instantaneamente com o novo Badge de Score.
- **Visual:** Uso de cores semânticas para indicar urgência. O formulário deve ser limpo, ocultando campos do veículo de troca se não houver troca.

## Restrições Técnicas de Alto Nível

- **Backend Existente:** A solução DEVE utilizar a API existente (`GestAuto.Commercial.API`) e seus endpoints de qualificação (`QualifyLeadCommand`) e cálculo de score (`LeadScoringService`). Não recriar regras de negócio no frontend.
- **Stack Frontend:** Desenvolver em React, seguindo os padrões de componentes do projeto `frontend/`.
- **Responsividade:** A interface deve ser utilizável em dispositivos móveis (tablets/celulares) para vendedores no pátio.

## Não-Objetivos (Fora de Escopo)

- Alteração nas regras de cálculo do Score no Backend (já implementadas).
- Integração real com bureaus de crédito (Serasa/SPC) neste momento (apenas campo declarativo).
- Implementação da automação de marketing (envio de e-mails) para leads Bronze.
- Dashboards gerenciais agregados (foco é na operação do vendedor lead a lead).

## Questões em Aberto

- Existe algum ícone específico na biblioteca de ícones do projeto para "Diamante" e "Medalhas"?
- O endpoint de listagem de leads já retorna o Score calculado ou precisaremos ajustar a projeção (DTO) de listagem? (Verificar `LeadListItemResponse`).
