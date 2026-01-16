# PRD - Fluxo Completo de Test-Drive (MVP)

## Referências (Repo)

- Issue guarda-chuva (contexto e status): [e2e/commercial/issues/COM-TD-001-testdrives-erro-400-e-fluxo-incompleto.md](../../e2e/commercial/issues/COM-TD-001-testdrives-erro-400-e-fluxo-incompleto.md)
- Subtarefas relacionadas:
	- [e2e/commercial/issues/COM-TD-002-agenda-slots-diaria-semanal.md](../../e2e/commercial/issues/COM-TD-002-agenda-slots-diaria-semanal.md)
	- [e2e/commercial/issues/COM-TD-003-agendamento-sem-mocks.md](../../e2e/commercial/issues/COM-TD-003-agendamento-sem-mocks.md)
	- [e2e/commercial/issues/COM-TD-004-execucao-mobile-first-iniciar-e-checklist-saida.md](../../e2e/commercial/issues/COM-TD-004-execucao-mobile-first-iniciar-e-checklist-saida.md)
	- [e2e/commercial/issues/COM-TD-005-frota-catalogo-veiculos-testdrive.md](../../e2e/commercial/issues/COM-TD-005-frota-catalogo-veiculos-testdrive.md)
- Dependência de produto (catálogo/frota e estados do veículo): [tasks/prd-modulo-estoque/prd.md](../prd-modulo-estoque/prd.md)

## Visão Geral

O **Fluxo de Test-Drive** do módulo Comercial permite que um vendedor agende, execute e conclua um test-drive de forma controlada e auditável, reduzindo conflitos de agenda, garantindo rastreabilidade (quem ficou responsável pelo carro, quando saiu, quando voltou) e protegendo o negócio contra perdas operacionais.

Este PRD descreve o comportamento de negócio do fluxo completo de test-drive **no MVP**, com as seguintes premissas oficiais:
- Perfis do MVP: `SALES_PERSON` (vendedor) e `MANAGER` (gerência)
- Horário padrão da agenda: **08:00–18:00**
- Duração padrão do test-drive: **1 hora**
- Bloqueio de horários: **geral por dia/horário (slot)**, não por veículo (bloqueio por veículo fica fora do MVP)
- Ocorrência/danos: registro interno (texto livre), upload opcional de fotos, data/hora e responsável; vinculada ao veículo e ao test-drive

Importante: o MVP **não depende** do serviço de Estoque/Inventário estar pronto. A seleção de veículo pode iniciar com identificador manual (ex.: UUID) e evoluir para um catálogo/frota assim que o módulo de Estoque estiver disponível.

## Objetivos

- Garantir que o comercial consiga **agendar e executar test-drives** sem conflito de agenda e com visibilidade de ocupação.
- Garantir rastreabilidade mínima de **check-out (saída)** e **check-in (retorno)** do veículo.
- Padronizar o fluxo de execução em etapas, adequado para uso mobile no pátio.
- Permitir ação de gerência para **resolver exceções operacionais** (cancelar, encerrar pendente, bloquear horários, registrar ocorrência sensível).

Métricas sugeridas (para validação do produto):
- % de test-drives com check-out e check-in registrados (meta: 100%)
- # de conflitos de horário evitados (monitorar via tentativas bloqueadas)
- Tempo médio entre agendamento e conclusão
- % de test-drives com ocorrência registrada quando aplicável

## Histórias de Usuário

### Vendedor (SALES_PERSON)
- Como **vendedor**, quero **ver a agenda por slots** para escolher um horário disponível.
- Como **vendedor**, quero **agendar um test-drive** vinculando cliente/lead e um veículo para garantir que o compromisso esteja reservado.
- Como **vendedor**, quero **registrar a saída (check-out)** com dados mínimos (ex.: km inicial e observações) para formalizar a responsabilidade pelo carro.
- Como **vendedor**, quero **registrar o retorno (check-in)** com dados mínimos (ex.: km final e observações) para concluir o fluxo com rastreabilidade.
- Como **vendedor**, quero **registrar uma ocorrência** (se houver) para que o veículo não volte indevidamente ao estado “normal” e a gerência decida o próximo passo.

### Gerência (MANAGER)
- Como **gerência**, quero **bloquear horários** da agenda por motivo operacional para impedir agendamentos indevidos.
- Como **gerência**, quero **cancelar um test-drive** quando necessário.
- Como **gerência**, quero **encerrar um test-drive pendente** (ex.: ficou aberto) para manter consistência.
- Como **gerência**, quero **registrar ocorrências sensíveis** e direcionar o veículo para um estado de exceção.

## Funcionalidades Principais

### F1. Agenda por Slots (Diária/Semanal)

A agenda deve ser apresentada por **slots de 1 hora** dentro do horário de funcionamento do MVP.

**Requisitos Funcionais:**
- **RF1.1** O sistema deve considerar como horário padrão de funcionamento **08:00–18:00** para exibição e geração de slots.
- **RF1.2** O sistema deve utilizar **duração padrão de 1 hora** por slot de test-drive.
- **RF1.3** O sistema deve exibir uma visão **diária** e uma visão **semanal** baseadas em slots.
- **RF1.4** O sistema deve impedir que um mesmo slot seja agendado para mais de um test-drive que conflite no mesmo horário de loja.
- **RF1.5** O sistema deve expor/consumir ocupação do slot de forma que o vendedor consiga distinguir “livre” vs “ocupado”, sem necessidade de ver detalhes sensíveis de outros vendedores.

Observação (MVP): pode existir buffer operacional (15–30min) para logística, mas **não é necessário modelar** no MVP.

### F2. Bloqueio de Horários (Gerência)

A gerência deve conseguir bloquear horários na agenda por motivos operacionais.

**Requisitos Funcionais:**
- **RF2.1** O sistema deve permitir que `MANAGER` bloqueie um ou mais slots por dia/horário (bloqueio geral).
- **RF2.2** O bloqueio deve impedir novos agendamentos no(s) slot(s) bloqueado(s).
- **RF2.3** O bloqueio deve conter, no mínimo, motivo (texto livre) e período (data/hora inicial e final).
- **RF2.4** O sistema deve exibir slots bloqueados como indisponíveis.

**Fora do MVP:** bloqueio por veículo específico (manutenção, recall, etc.).

### F3. Agendamento de Test-Drive

Agendamento cria um compromisso e vincula contexto comercial.

**Requisitos Funcionais:**
- **RF3.1** O sistema deve permitir que `SALES_PERSON` crie um agendamento com: data/hora do slot, referência do cliente (ex.: lead) e referência do veículo.
- **RF3.2** O sistema deve validar que o slot está dentro do horário de funcionamento do MVP (08:00–18:00) e respeita a duração padrão (1h).
- **RF3.3** O sistema deve impedir agendamento em slot bloqueado.
- **RF3.4** O sistema deve permitir listar agendamentos por intervalo de datas para suportar visualização diária/semanal.

### F4. Seleção do Veículo (MVP sem Estoque)

Enquanto o módulo de Estoque/Frota não estiver pronto, o fluxo não deve ficar bloqueado.

**Requisitos Funcionais:**
- **RF4.1** No MVP, o sistema deve permitir informar o veículo por um identificador manual (ex.: UUID) no agendamento.
- **RF4.2** O sistema deve manter o modelo preparado para evoluir para seleção por catálogo/frota quando o módulo de Estoque estiver disponível.

**Nota:** o catálogo/frota completo é uma dependência de produto e terá seu PRD/épicos próprios (ex.: seleção de veículo de test-drive, estados de disponibilidade e vida útil).

### F5. Execução (Check-out → Iniciar → Check-in → Concluir)

O fluxo de execução deve ser adequado para uso no pátio e minimizar erros operacionais.

**Permissões (MVP):**
- **Check-out (saída para test-drive):** somente `SALES_PERSON`
- **Check-in (retorno):** `SALES_PERSON` e `MANAGER`
- Cliente nunca executa ações no sistema.

**Requisitos Funcionais:**
- **RF5.1** O sistema deve permitir que `SALES_PERSON` registre o **check-out** do test-drive, armazenando data/hora e responsável.
- **RF5.2** O sistema deve permitir que o responsável autorizado registre o **check-in** do test-drive, armazenando data/hora e responsável.
- **RF5.3** O sistema deve suportar a execução em etapas (pré-saída, iniciar, retorno, finalizar) para reduzir omissões de informação.
- **RF5.4** O sistema deve permitir que `MANAGER` cancele um test-drive quando necessário.
- **RF5.5** O sistema deve permitir que `MANAGER` encerre test-drive pendente (ex.: aberto indevidamente) registrando responsável e motivo.

**Evolução futura (não obrigatório no MVP):** papel opcional de pátio (ex.: `YARD_ATTENDANT`) pode existir no futuro para registrar check-in, mas não é obrigatório no MVP.

### F6. Ocorrência / Danos (Registro Interno)

Ocorrências durante o test-drive devem ser registradas e devem afetar o fluxo para proteger o estoque.

**Requisitos Funcionais:**
- **RF6.1** O sistema deve permitir registrar ocorrência vinculada ao test-drive e ao veículo.
- **RF6.2** A ocorrência deve conter no mínimo: texto livre, data/hora e responsável.
- **RF6.3** O sistema deve permitir anexar fotos à ocorrência (upload opcional) no MVP.
- **RF6.4** Quando houver ocorrência, o veículo **não** deve retornar automaticamente ao estado normal de disponibilidade (ex.: “em_estoque”).
- **RF6.5** Quando houver ocorrência, o veículo deve transitar para um estado de exceção operacional (ex.: “em_preparacao” ou equivalente), e a gerência decide o próximo passo.

**Fora do MVP:** integrações com seguradora, oficina, jurídico.

### F7. Auditoria e Rastreabilidade

O fluxo deve ser auditável para evitar divergências e permitir investigação.

**Requisitos Funcionais:**
- **RF7.1** O sistema deve registrar responsável e data/hora em todas as ações críticas: agendar, cancelar, check-out, check-in, finalizar, bloquear horário, registrar ocorrência.
- **RF7.2** O sistema deve permitir consultar o histórico do test-drive e suas transições principais.

## Experiência do Usuário

O MVP deve privilegiar clareza e execução rápida, principalmente em mobile:
- O vendedor visualiza agenda por slots (08:00–18:00) e identifica rapidamente slots livres/ocupados/bloqueados.
- Ao executar, o fluxo é guiado em etapas para reduzir falhas (pré-saída → iniciar → retorno → finalizar).
- Ações de gerência ficam disponíveis para exceções (cancelar, encerrar pendente, bloquear horários, ocorrência sensível).

Requisitos de acessibilidade: seguir padrões do produto (teclado, labels, contraste), a serem detalhados em PRDs de frontend quando necessário.

## Restrições Técnicas de Alto Nível

- O MVP não deve depender do serviço de Estoque estar pronto; deve existir caminho funcional com identificação manual do veículo.
- O sistema deve suportar autenticação e autorização por perfis `SALES_PERSON` e `MANAGER`.
- O sistema deve manter consistência de agenda (sem overbooking por slot e respeitando bloqueios).
- O sistema deve considerar consistência de data/hora (timezone da loja) e evitar ambiguidades de horário.

## Não-Objetivos (Fora de Escopo)

- ❌ Catálogo/frota completo de veículos de test-drive (integração com Estoque) — previsto para fase posterior
- ❌ Bloqueio por veículo específico (manutenção/recall)
- ❌ Integrações de ocorrência com seguradora, oficina ou jurídico
- ❌ Otimização formal de logística (buffer 15–30 min como regra de sistema)
- ❌ Portal/ações do cliente (cliente não usa o sistema)

## Questões em Aberto

- Qual será o modelo final de “veículo de test-drive” quando o módulo de Estoque estiver pronto (campos mínimos e regras de elegibilidade)?
- Qual estado padrão de exceção será adotado quando houver ocorrência (ex.: “em_preparacao” vs estado dedicado)?
- Precisaremos suportar multi-loja (timezone e agendas separadas) em fases futuras?
