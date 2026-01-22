# Relatório de navegação — Módulo de Estoque

Data: 2026-01-22
Ambiente: http://localhost:5173
Usuário: stock

## Resumo
Foram encontrados erros de navegação e páginas com conteúdo vazio/placeholder. Também há gaps funcionais relevantes em relação ao PRD (cadastro, movimentações, reservas, test-drive e auditoria).

## Reteste (2026-01-22) — versão atualizada
Os testes foram refeitos e os resultados mudaram:
- ✅ Menu “Estoque” agora navega para /stock e expande subitens.
- ✅ /stock/movements agora renderiza (aba “Registrar Entrada” visível) sem erro de formulário.
- ⚠️ /stock/test-drives deixou de exibir “A rota solicitada não existe”, porém ainda há erro 404 de API.
- ❌ /stock/vehicles/1 segue sem conteúdo visível.

## Páginas visitadas
- / (Home)
- /stock (Dashboard)
- /stock/vehicles (Veículos)
- /stock/vehicles/1 (Detalhe do veículo)
- /stock/reservations (Reservas)
- /stock/movements (Movimentações)
- /stock/test-drives (Test-drives)

## Bugs encontrados

### 1) Menu “Estoque” não navega a partir da Home
**Passos**:
1. Login com usuário `stock`.
2. Na Home, clicar em “Estoque” na sidebar.

**Resultado atual**: não ocorre navegação/expansão; permanece na Home.
**Resultado esperado**: navegar para /stock e/ou expandir subitens do menu.

**Status no reteste**: corrigido (navega para /stock e expande subitens).

### 2) Tela de Movimentações quebra com erro de formulário
**Passos**:
1. Acessar /stock/movements.

**Resultado atual**: tela fica em branco e o console registra erro:
- `useFormField should be used within <FormField>`

**Resultado esperado**: tela renderiza corretamente (mesmo que vazia).

**Status no reteste**: corrigido (tela renderiza com formulário de entrada).

### 3) Tela de Test-drives exibe erro de rota e falha de API
**Passos**:
1. Acessar /stock/test-drives.

**Resultado atual**:
- Não há mais erro de rota; a página renderiza e mostra estado vazio.
- Console com erro 404 ao carregar recurso da API.

**Resultado esperado**: listagem/gestão de test-drives sem erro de rota e sem 404.

**Status no reteste**: parcialmente corrigido (erro de rota removido, mas 404 de API persiste).

### 4) Detalhe do veículo sem conteúdo
**Passos**:
1. Acessar /stock/vehicles/1.

**Resultado atual**: página sem conteúdo visível.
**Resultado esperado**: detalhes do veículo ou mensagem de “não encontrado”.

## Gaps em relação ao PRD (módulo de estoque)

### Cadastro e identidade do veículo (F1)
- Não há ação/tela de cadastro de veículo.
- Não há validação visível de campos obrigatórios por categoria (novo/seminovo/demonstração).

### Check-in / Check-out (F4/F5)
- Não há fluxo de entrada (check-in) nem saída (check-out) com motivo, responsável e data.
- Ausência de histórico de entradas/saídas.

### Reserva de veículos (F6)
- Tela de reservas é somente listagem vazia; não há criação/cancelamento/prorrogação.
- Não há evidência de regras de expiração, tipos de reserva ou validações.

### Test-drive (F7)
- Não há fluxo de iniciar/encerrar test-drive.
- Página apresenta erro de rota/404.

### Auditoria e histórico (F9)
- Não há visualização de histórico de eventos por veículo.
- Não há rastreabilidade de “quem/quando/o que mudou” nas telas.

## Observações
- O Dashboard e Veículos exibem apenas placeholders com contadores zerados e listas vazias, sem ações primárias (ex.: cadastrar veículo, registrar entrada/saída, reservar).
