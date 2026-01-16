# Especificação Técnica - Frontend Módulo Comercial

## Resumo Executivo

Esta especificação detalha a implementação do frontend para o Módulo Comercial do GestAuto. A solução será construída sobre a arquitetura React existente, introduzindo uma estrutura baseada em módulos (`src/modules/commercial`) para encapsular a complexidade do domínio. Utilizaremos **React Query** para gerenciamento de estado do servidor (caching/loading), **React Hook Form** com **Zod** para formulários complexos e **Axios** para comunicação com a API REST, garantindo interceptação segura de tokens Keycloak.

## Arquitetura do Sistema

### Estrutura de Diretórios (Feature-based)

Para manter a escalabilidade e isolamento do domínio, adotaremos uma estrutura de módulo:

```
src/
├── modules/
│   └── commercial/
│       ├── components/       # Componentes de UI específicos (Cards, Widgets)
│       ├── hooks/            # Hooks de negócio (useLeads, useProposals)
│       ├── pages/            # Páginas (Dashboard, LeadList, ProposalEditor)
│       ├── services/         # Camada de API (Axios calls)
│       ├── types/            # Definições TypeScript (DTOs)
│       └── routes.tsx        # Definição de rotas internas do módulo
```

### Visão Geral dos Componentes

- **CommercialLayout**: Wrapper para as rotas comerciais, gerenciando breadcrumbs e contexto comum.
- **DashboardPage**: Agregador de widgets (KPIs, Listas Rápidas).
- **LeadManagement**: Conjunto de componentes para listagem (DataGrid) e edição (Modal/Tabs) de leads.
- **ProposalEditor**: Componente complexo de formulário multi-etapa (Wizard ou Accordion) para construção de propostas.
- **TestDriveScheduler**: Interface de calendário e modal de execução de test-drive.

### Fluxo de Dados

1.  **Leitura**: Componentes consomem hooks (`useLeads`, `useProposal`) que utilizam `React Query` para buscar dados da API.
2.  **Escrita**: Mutações (`useCreateLead`, `useUpdateProposal`) enviam dados via `Axios`.
3.  **Estado Local**: Formulários gerenciam estado local com `react-hook-form` antes do envio.
4.  **Estado Global**: `AuthContext` fornece o token JWT para as requisições.

## Design de Implementação

### Interfaces Principais (Types)

Baseado no `swagger.json`:

```typescript
// src/modules/commercial/types/lead.ts
export interface Lead {
  id: string;
  name: string;
  phone: string;
  email: string;
  status: 'NOVO' | 'EM_ATENDIMENTO' | 'AGUARDANDO_VISITA' | 'PROPOSTA_ENVIADA' | 'VENDIDO' | 'PERDIDO';
  score: 'DIAMANTE' | 'OURO' | 'PRATA' | 'BRONZE';
  origin: string;
  createdAt: string;
  salesPersonId: string;
}

// src/modules/commercial/types/proposal.ts
export interface Proposal {
  id: string;
  leadId: string;
  vehicle: VehicleInfo;
  paymentMethod: 'A_VISTA' | 'FINANCIAMENTO' | 'CONSORCIO';
  totalValue: number;
  status: 'RASCUNHO' | 'AGUARDANDO_APROVACAO' | 'APROVADA' | 'FECHADA';
  tradeInVehicle?: TradeInVehicle;
}
```

### Serviços de API

Camada de abstração para chamadas HTTP. Recomenda-se criar uma instância configurada do Axios em `src/lib/api.ts` para injetar o token automaticamente.

```typescript
// src/modules/commercial/services/leadService.ts
import { api } from '@/lib/api';
import { Lead, CreateLeadDTO } from '../types';

export const leadService = {
  getAll: async (params: LeadFilterParams) => {
    const { data } = await api.get<PagedResponse<Lead>>('/leads', { params });
    return data;
  },
  create: async (dto: CreateLeadDTO) => {
    const { data } = await api.post<Lead>('/leads', dto);
    return data;
  },
  qualify: async (id: string, data: QualificationDTO) => {
    return api.post(`/leads/${id}/qualify`, data);
  }
};
```

### Rotas e Navegação

Atualização em `src/App.tsx` e `src/config/navigation.tsx`.

**Rotas (`src/modules/commercial/routes.tsx`):**
- `/commercial` (Dashboard)
- `/commercial/leads` (Listagem)
- `/commercial/leads/:id` (Detalhes)
- `/commercial/proposals` (Listagem)
- `/commercial/proposals/new` (Criação)
- `/commercial/proposals/:id/edit` (Edição)
- `/commercial/test-drives` (Agenda)

### Dependências Novas Recomendadas

Para suportar os requisitos, recomenda-se adicionar:

- `axios`: Cliente HTTP robusto.
- `@tanstack/react-query`: Gerenciamento de estado assíncrono (cache, loading, error).
- `react-hook-form`: Gerenciamento de formulários performático.
- `zod`: Validação de esquemas (integração com hook-form).
- `@hookform/resolvers`: Adaptador Zod para hook-form.
- `date-fns`: Manipulação de datas.

## Pontos de Integração

### Backend API (`commercial`)
- **Autenticação**: Header `Authorization: Bearer <token>` em todas as requisições.
- **Tratamento de Erros**: Interceptor do Axios para capturar 401 (refresh token/logout) e 403 (permissão negada).

### Keycloak (Auth)
- Utilizar `useAuth()` existente para obter o token e verificar roles (`SALES_PERSON`, `SALES_MANAGER`) para renderização condicional de botões (ex: Aprovar Desconto).

## Estratégia de Testes

### Testes Unitários (Vitest)
- **Services**: Mockar `axios` para testar tratamento de respostas e erros.
- **Utils/Helpers**: Testar formatadores de moeda, data e lógica de score local (se houver).

### Testes de Componentes (React Testing Library)
- **Formulários**: Testar validação (Zod), submissão e feedback de erro.
- **Listagens**: Testar renderização correta de dados e estados vazios.
- **Hooks**: Testar `useLeads` com `renderHook` e mock do QueryClient.

## Observabilidade e Monitoramento

- **Logs de Erro**: Capturar erros de API no console (e futuramente em ferramenta de APM) com contexto (URL, Status).
- **Feedback de UI**: Utilizar componentes de `Toast` para feedback visual imediato ao usuário em caso de falhas de rede.

## Plano de Rollout

1.  **Infraestrutura Frontend**: Instalar deps, configurar Axios e QueryClient.
2.  **Módulo Base**: Criar estrutura de pastas e rotas.
3.  **Funcionalidade Leads**: Implementar listagem e cadastro.
4.  **Funcionalidade Detalhes**: Implementar abas e qualificação.
5.  **Funcionalidade Propostas**: Implementar editor e integração.
