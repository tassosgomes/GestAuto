# PRD - Módulo de Estoque (Veículos)

## Visão Geral

O **Módulo de Estoque** é o eixo central operacional de uma concessionária: ele organiza a disponibilidade real de veículos para venda, test-drive, preparação e baixa, garantindo que **cada veículo tenha um status único**, **uma finalidade clara**, **uma origem registrada** e **um histórico completo**.

Este PRD descreve o comportamento de negócio do estoque, com foco **backend-first** (APIs e eventos), para suportar integrações com Comercial, Seminovos, Financeiro, Delivery e Oficina.

Problemas que o módulo resolve:
- Falta de uma fonte única de verdade sobre “qual veículo está disponível agora”
- Conflitos de alocação (ex.: dois vendedores negociando o mesmo veículo)
- Baixa rastreabilidade de entrada/saída (quem, quando, por qual motivo)
- Fluxos de test-drive e preparação sem controle e sem trilha de auditoria

## Objetivos

- Garantir que **todo veículo** no estoque possua **um único status vigente** e uma **finalidade** (novo, seminovo, demonstração).
- Reduzir conflitos de negociação por meio de **reserva explícita** e indisponibilidade para outros vendedores.
- Padronizar e auditar **entrada** (check-in) e **saída** (check-out) com motivo, data e responsável.
- Viabilizar integrações de ponta-a-ponta:
  - Comercial (reserva e venda)
  - Seminovos (entrada de usados)
  - Financeiro (liberação para faturamento)
  - Delivery (entrega)
  - Oficina (preparação e manutenção)

Métricas sugeridas (para validação do produto):
- % de veículos com status válido e único (meta: 100%)
- # de conflitos de negociação por veículo (meta: próximo de 0)
- Tempo médio entre “entrada” e “pronto para venda” (monitorar por categoria)
- % de saídas com motivo/data/responsável preenchidos (meta: 100%)

## Histórias de Usuário

### Vendedor (Comercial)
- Como **vendedor**, quero **consultar rapidamente a disponibilidade e status de um veículo** para que eu saiba se posso oferecer ao cliente.
- Como **vendedor**, quero **reservar um veículo** quando estiver em negociação para que ele fique indisponível para outros vendedores.
- Como **vendedor**, quero **confirmar a venda** e registrar a saída por “venda” para que o estoque reflita a realidade.

### Responsável de Seminovos
- Como **responsável de seminovos**, quero **registrar a entrada de um usado (compra de cliente/troca)** para que o veículo passe a existir no estoque com histórico e responsabilidade.

### Oficina / Preparação
- Como **responsável de oficina/preparação**, quero **marcar o veículo como ‘em preparação’ e depois ‘pronto para venda’** para refletir o progresso operacional.

### Financeiro
- Como **financeiro**, quero **receber sinalização de que um veículo foi vendido e está aguardando liberação** para que eu possa controlar a etapa de faturamento.

### Gestor
- Como **gestor**, quero **auditar o histórico de um veículo** (entradas, saídas, reservas, test-drives) para investigar divergências e controlar processos.

## Funcionalidades Principais

### F1. Cadastro e Identidade do Veículo no Estoque

O sistema deve manter uma representação única do veículo no estoque, com atributos mínimos para rastreabilidade e integração.

**Requisitos Funcionais:**
- **RF1.1** O sistema deve permitir registrar veículo no estoque com um identificador único.
- **RF1.2** O sistema deve registrar dados mínimos do veículo: marca, modelo, versão (se aplicável), ano/modelo, cor, e um identificador de chassi/placa quando disponível.
- **RF1.3** O sistema deve permitir classificar o veículo por **tipo/finalidade**: `novo`, `seminovo`, `demonstracao`.
- **RF1.4** O sistema deve impedir a existência de dois veículos ativos com o mesmo identificador físico (ex.: mesmo chassi), quando essa informação estiver presente.

Campos obrigatórios mínimos por categoria:
- **Veículo Novo**: chassi (VIN) obrigatório; placa não obrigatória; origem = `montadora`; finalidade = venda ou demonstração.
- **Seminovo**: placa obrigatória; chassi (VIN) obrigatório; quilometragem obrigatória; origem = `compra_cliente_seminovo`; avaliação vinculada obrigatória.
- **Demonstração**: chassi (VIN) obrigatório; placa obrigatória se já emplacado; finalidade = `test-drive` ou `frota interna`.

**Requisitos Funcionais (Obrigatoriedade por Categoria):**
- **RF1.5** Para veículos `novo`, o sistema deve exigir chassi (VIN) e origem `montadora` no momento do check-in.
- **RF1.6** Para veículos `seminovo`, o sistema deve exigir: placa, chassi (VIN), quilometragem e uma referência de avaliação vinculada.
- **RF1.7** Para veículos `demonstracao`, o sistema deve exigir chassi (VIN) e finalidade (`test-drive` ou `frota interna`); deve exigir placa quando o veículo estiver em estado “já emplacado”.
- **RF1.8** O sistema deve impedir que um veículo “exista” no estoque (isto é, fique em status ativo) sem cumprir os campos obrigatórios mínimos da sua categoria.

### F2. Tipos/Finalidades e Situações de Negócio

O estoque deve suportar as situações descritas na visão de negócio:
- **Carros Novos**: pronta entrega, pedido em trânsito, reservado para cliente.
- **Seminovos**: avaliados, em preparação, prontos para venda.
- **Demonstração**: test-drive, frota interna.

**Requisitos Funcionais:**
- **RF2.1** O sistema deve permitir registrar a **origem da entrada**: `montadora`, `compra_cliente_seminovo`, `transferencia_entre_lojas`, `frota_interna`.
- **RF2.2** Para veículos `novo`, o sistema deve suportar marcações de negócio como “pronta entrega” e “pedido em trânsito” como atributos operacionais vinculados ao status/histórico.
- **RF2.3** Para veículos `seminovo`, o sistema deve permitir marcar estágios operacionais coerentes com avaliação e preparação.
- **RF2.4** Para veículos `demonstracao`, o sistema deve permitir distingui-los como “test-drive” ou “frota interna”.

Observação: detalhes de UI/relatórios são tratados em outros PRDs.

### F3. Status Único do Veículo (Estado Vigente)

Todo veículo deve possuir um **status único vigente**, que define sua disponibilidade e o que pode ser feito com ele.

Status base (mínimo):
- `em_transito`
- `em_estoque`
- `reservado`
- `em_test_drive`
- `em_preparacao`
- `vendido`
- `baixado`

**Requisitos Funcionais:**
- **RF3.1** O sistema deve garantir que cada veículo possua exatamente **um** status vigente.
- **RF3.2** O sistema deve manter histórico de mudanças de status, com data/hora e responsável.
- **RF3.3** O sistema deve impedir ações incompatíveis com o status (ex.: reservar um veículo já `vendido` ou `baixado`).
- **RF3.4** O sistema deve expor “disponibilidade comercial” derivada do status, de forma consistente (ex.: `em_estoque` disponível; `reservado` indisponível).

### F4. Entrada de Veículos (Check-in)

Toda entrada no estoque deve gerar check-in, responsabilidade e histórico.

**Requisitos Funcionais:**
- **RF4.1** O sistema deve permitir registrar uma entrada (check-in) de veículo com: origem, data/hora, responsável e observações.
- **RF4.2** Ao concluir um check-in, o sistema deve atualizar o status do veículo para um estado coerente com a origem (ex.: `em_transito` ou `em_estoque`).
- **RF4.3** O sistema deve registrar “responsável atual” (owner operacional) do veículo após a entrada.
- **RF4.4** O sistema deve manter o histórico de entradas associado ao veículo.

### F5. Saída de Veículos (Check-out)

Toda saída deve possuir motivo, data e responsável.

Motivos mínimos:
- `venda`
- `test_drive`
- `transferencia`
- `baixa_sinistro_perda_total`

**Requisitos Funcionais:**
- **RF5.1** O sistema deve permitir registrar uma saída (check-out) com: motivo, data/hora, responsável e observações.
- **RF5.2** Ao registrar saída por `venda`, o sistema deve atualizar o status do veículo para `vendido`.
- **RF5.3** Ao registrar saída por `baixa_sinistro_perda_total`, o sistema deve atualizar o status do veículo para `baixado`.
- **RF5.4** O sistema deve manter o histórico de saídas associado ao veículo.

### F6. Reserva de Veículos (Exclusividade Comercial)

Quando um veículo estiver:
- em negociação,
- aguardando pagamento,
- aguardando liberação financeira,

ele deve ficar **reservado** e **indisponível** para outros vendedores.

Regras de permissão (impacto direto em faturamento, comissão e risco jurídico):

| Ação | Quem pode |
|------|----------|
| Reservar veículo | Comercial (vendedor) |
| Cancelar a própria reserva | Mesmo vendedor |
| Cancelar reserva de outro vendedor | Gerente Comercial |
| Alterar status manualmente | Gestor de Estoque / Gerente Geral |
| Baixar veículo (sinistro/perda total) | Gerente Geral ou Diretor |

Regras de expiração de reserva:
- Reserva padrão: **48 horas** (expira automaticamente)
- Reserva com entrada paga: **sem expiração automática**
- Reserva aguardando banco: expiração vinculada ao prazo do banco
- Gerente Comercial pode prorrogar uma reserva (para evitar “carro travado”)

**Requisitos Funcionais:**
- **RF6.1** O sistema deve permitir criar uma reserva vinculada a um contexto comercial (ex.: lead/proposta/oportunidade), registrando vendedor responsável e data/hora.
- **RF6.2** Enquanto reservado, o veículo deve ser considerado indisponível para reserva por outros vendedores.
- **RF6.3** O sistema deve permitir cancelar uma reserva, registrando motivo e responsável.
- **RF6.4** O sistema deve permitir converter uma reserva em saída por venda, mantendo rastreabilidade.
- **RF6.5** O sistema deve manter histórico de reservas e suas transições (criada, ativa, cancelada, concluída).
- **RF6.6** O sistema deve aplicar controle de acesso por ação conforme a tabela de permissões deste PRD.
- **RF6.7** O sistema deve suportar, no mínimo, os tipos de reserva: `padrao`, `entrada_paga`, `aguardando_banco`.
- **RF6.8** Para `padrao`, o sistema deve definir expiração automática de 48 horas a partir da criação (salvo prorrogação autorizada).
- **RF6.9** Para `entrada_paga`, o sistema não deve expirar automaticamente a reserva.
- **RF6.10** Para `aguardando_banco`, o sistema deve exigir e armazenar o prazo limite informado manualmente pelo vendedor no momento da criação (ou atualização) da reserva.
- **RF6.11** O sistema deve permitir que Gerente Comercial prorrogue uma reserva, registrando data/hora, novo prazo e responsável pela prorrogação.
- **RF6.12** Quando uma reserva expirar automaticamente, o sistema deve registrar o evento no histórico e tornar o veículo novamente disponível (status coerente com o fluxo).

### F7. Test-drive como Fluxo Controlado

Test-drive é um fluxo controlado: saída e retorno precisam ser rastreados para evitar “sumir” veículo do estoque.

**Requisitos Funcionais:**
- **RF7.1** O sistema deve permitir iniciar um fluxo de test-drive para um veículo elegível, registrando cliente (ou referência), vendedor responsável e data/hora.
- **RF7.2** Ao iniciar test-drive, o status do veículo deve ser atualizado para `em_test_drive`.
- **RF7.3** O sistema deve permitir encerrar o test-drive, registrando data/hora e resultado (ex.: devolvido ao estoque, convertido em negociação/reserva).
- **RF7.4** Ao encerrar test-drive sem venda, o status do veículo deve retornar para um estado coerente (ex.: `em_estoque` ou `reservado`).

### F8. Integração com Outros Setores (Visão de Negócio)

O módulo de estoque deve suportar integração de processos entre áreas, com foco em consistência de status e rastreabilidade.

**Requisitos Funcionais:**
- **RF8.1** O sistema deve permitir que o Comercial consulte disponibilidade e reserve veículos.
- **RF8.2** O sistema deve suportar a entrada de seminovos proveniente do processo de compra do cliente (setor de Seminovos), gerando check-in e histórico.
- **RF8.3** O sistema deve suportar estados “aguardando liberação” (financeiro) por meio de reserva/status, mantendo o veículo indisponível para outros vendedores.
- **RF8.4** O sistema deve permitir que Oficina registre `em_preparacao` e posteriormente `em_estoque`/“pronto para venda”.
- **RF8.5** O sistema deve suportar sinalização para Delivery/Entrega quando o veículo estiver em fluxo de entrega (detalhes de entrega ficam fora deste escopo).

### F9. Auditoria e Responsabilidade

Cada evento relevante do ciclo do veículo deve ser rastreável.

**Requisitos Funcionais:**
- **RF9.1** O sistema deve registrar “quem fez” (usuário/role), “quando” e “o que mudou” em entradas, saídas, reservas e mudanças de status.
- **RF9.2** O sistema deve permitir consulta do histórico completo do veículo em ordem cronológica.
- **RF9.3** O sistema deve impedir operações críticas sem responsável autenticado.

Eventos de negócio (alto nível) que precisam existir:
- Veículo entrou no estoque
- Veículo mudou de status
- Veículo foi reservado
- Reserva cancelada
- Veículo saiu por venda
- Veículo saiu por test-drive
- Veículo foi baixado

Principais consumidores de negócio:
- Comercial → disponibilidade
- Financeiro → faturamento
- Seminovos → entrada de usados
- Delivery → entrega
- Oficina → preparação

**Requisitos Funcionais (Eventos):**
- **RF9.4** O sistema deve publicar evento de negócio quando um veículo entrar no estoque.
- **RF9.5** O sistema deve publicar evento de negócio quando o status vigente do veículo mudar.
- **RF9.6** O sistema deve publicar evento de negócio quando uma reserva for criada, cancelada, prorrogada ou expirar automaticamente.
- **RF9.7** O sistema deve publicar evento de negócio quando ocorrer saída por venda, saída por test-drive ou baixa do veículo.

## Experiência do Usuário

Como esta fase é **backend-first**, a experiência do usuário será suportada por:
- APIs claras para consulta de estoque, criação de check-in/check-out, reserva e consulta de histórico
- Regras de validação consistentes com mensagens de erro acionáveis
- Controle de acesso por perfis (RBAC) para limitar ações (ex.: apenas Comercial pode reservar; apenas perfis autorizados podem baixar veículo)

Requisitos de acessibilidade de UI não se aplicam diretamente aqui; serão considerados em PRDs de frontend.

## Restrições Técnicas de Alto Nível

- O sistema deve ser capaz de integrar-se com os demais módulos por **API** e, quando aplicável, **eventos** (contratos detalhados pertencem à Tech Spec).
- O sistema deve garantir consistência de status por validações de domínio (status único e transições auditáveis).
- O sistema deve atender requisitos de segurança e privacidade aplicáveis (autenticação, autorização e rastreabilidade de operações).

## Não-Objetivos (Fora de Escopo)

- ❌ **Transferência entre lojas (multi-loja)** nesta fase (apesar de existir como origem/motivo previsto, o fluxo completo não é implementado no MVP)
- ❌ Precificação, margens, comissões e regras comerciais avançadas
- ❌ Emissão de documentos fiscais e faturamento completo
- ❌ Gestão de frota/ativos além do necessário para status de estoque
- ❌ Interface de usuário completa (telas, dashboards) — será tratada em PRDs específicos de frontend
- ❌ Regras detalhadas de agendamento/agenda (ex.: disponibilidade por calendário) para test-drive

## Questões em Aberto

- Quais nomes e contratos (payloads) de eventos serão adotados e quais eventos são obrigatoriamente idempotentes? (detalhar na Tech Spec)
- Como o Estoque irá expor um **catálogo/frota de veículos elegíveis para test-drive** (campos mínimos e regras de elegibilidade), para consumo do módulo Comercial?
- Qual será o comportamento oficial de status quando houver **ocorrência/dano em test-drive**: retorno para `em_preparacao` vs um status dedicado de exceção?
