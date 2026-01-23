# Relatório de validação de fluxos — Módulo de Estoque

Data: 2026-01-22
Perfil utilizado: `stock` (login via Keycloak)
Ambiente: SPA em http://localhost:5173

## Objetivo
Validar, a partir do PRD de estoque, os principais fluxos do módulo no sistema atual e registrar o que funcionou e o que falhou.

## Navegação realizada
- Login (Keycloak) com credenciais fornecidas.
- Rotas visitadas:
  - /stock (Dashboard)
  - /stock/vehicles
  - /stock/reservations
  - /stock/movements (entrada/saída)
  - /stock/test-drives
  - /stock/preparation
  - /stock/finance
  - /stock/write-offs

## Resultado por fluxo do PRD

### F1. Cadastro e Identidade do Veículo
**O que deu certo**
- Tela de veículos disponível com filtros (busca por VIN/modelo/placa, status, categoria).

**O que deu errado**
- Lista de veículos não carregou. Mensagem: “Não foi possível carregar a lista de veículos.”
- Não foi possível validar criação/cadastro ou detalhes de veículo por falta de dados e erro de API.

### F2. Tipos/Finalidades e Situações de Negócio
**O que deu certo**
- Filtros de status e categoria visíveis na página de veículos.
- Formulário de entrada (check-in) apresenta opções de origem (Montadora, Compra cliente/seminovo, Transferência entre lojas, Frota interna).

**O que deu errado**
- Não foi possível validar mudanças reais de status/categoria (API 404 nas chamadas).

### F3. Status Único do Veículo (Estado Vigente)
**O que deu certo**
- Indicadores no dashboard (Total, Em Estoque, Reservados, Em Test-Drive) visíveis.

**O que deu errado**
- Indicadores e listas não carregaram dados reais (erros de API).
- Não foi possível validar unicidade de status ou histórico.

### F4. Entrada de Veículos (Check-in)
**O que deu certo**
- Formulário “Registrar Entrada” disponível em Movimentações.
- Validações de obrigatoriedade funcionaram:
  - “Veículo é obrigatório”
  - “Origem é obrigatória”

**O que deu errado**
- Combo de veículos não carregou (erro “Erro ao carregar veículos”).
- Não foi possível concluir o check-in por falta de veículo disponível/API.

### F5. Saída de Veículos (Check-out)
**O que deu certo**
- Formulário “Registrar Saída” disponível com motivos (Venda, Test-drive, Transferência).
- Validações de obrigatoriedade funcionaram:
  - “Veículo é obrigatório”
  - “Motivo é obrigatório”

**O que deu errado**
- Combo de veículos não carregou (sem dados para selecionar).
- Não foi possível concluir a saída por falta de veículo/API.

### F6. Reserva de Veículos
**O que deu certo**
- Tela de Reservas disponível com filtros por status/tipo.
- Tabela com colunas de contexto (veículo, tipo, status, vendedor, validade, prazo banco, ações).

**O que deu errado**
- Lista de reservas não carregou (“Erro ao carregar reservas. Tente novamente.”).
- Não foi possível criar/cancelar/converter reserva por ausência de dados/API.

### F7. Test-drive como Fluxo Controlado
**O que deu certo**
- Tela de Test-drives disponível com indicadores (agendados, em andamento, atenção, atrasados).

**O que deu errado**
- Erro de rede ao carregar a lista (“Network Error”).
- Não foi possível iniciar/encerrar test-drive por falha na API.

### F8. Integração com Outros Setores
**O que deu certo**
- Rotas específicas existem (Movimentações, Reservas, Test-drives), sugerindo suporte aos fluxos do PRD.

**O que deu errado**
- Falha geral de integração por ausência/erro nas APIs.

### F9. Auditoria e Responsabilidade
**O que deu certo**
- Validações de formulário exigem dados mínimos, indicando alguma camada de regras.

**O que deu errado**
- Não foi possível consultar histórico/auditoria por falhas de dados/API.

## Controle de acesso (RBAC)
**O que deu certo**
- Bloqueio de acesso às rotas restritas:
  - /stock/preparation
  - /stock/finance
  - /stock/write-offs
  Mensagem exibida: “Acesso negado”.

**O que deu errado**
- Como usuário `stock`, não foi possível validar fluxos de preparação, financeiro e baixa por RBAC.

## Evidências técnicas observadas
- Erros de rede recorrentes para chamadas do estoque (ex.: 404/ERR_FAILED para `https://apis-gestauto.tasso.dev.br/stock/...`).
- Isso impede carregar listas e concluir operações principais.

## Conclusão
A navegação básica e a estrutura das telas do módulo de estoque estão presentes. Validações de obrigatoriedade nos formulários de entrada/saída funcionam. Porém, a maioria dos fluxos críticos (cadastro, reservas, test-drive, histórico e movimentações efetivas) não pôde ser validada por falhas de API ou ausência de dados. O RBAC está ativo e bloqueia corretamente rotas restritas para o perfil utilizado.

## Próximos passos recomendados
1. Corrigir a base URL/roteamento da API de estoque (ou garantir serviço disponível) para eliminar os erros 404.
2. Popular dados de teste (veículos, reservas, test-drives) para validação completa.
3. Validar com um perfil `STOCK_MANAGER` para testar preparação, financeiro e baixa.
