---
status: pending
parallelizable: false
blocked_by: ["7.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>configuration</type>
<scope>middleware</scope>
<complexity>low</complexity>
<dependencies>react-router</dependencies>
<unblocks>"9.0"</unblocks>
</task_context>

# Tarefa 8.0: Configurar Rotas e Navegação

## Visão Geral

Adicionar a rota `/stock/check-in` no sistema de rotas do módulo de estoque e configurar a navegação para a nova página de cadastro de veículo com check-in. Incluir também o link na sidebar ou menu do módulo.

<requirements>
- Adicionar rota `/stock/check-in` em `routes.tsx` do módulo de estoque
- Configurar lazy loading para a página
- Adicionar link na sidebar do módulo de estoque
- Proteger rota com RBAC (roles: STOCK_PERSON, STOCK_MANAGER, MANAGER, ADMIN)
- Configurar título da página para SEO/navegação
</requirements>

## Subtarefas

- [ ] 8.1 Localizar arquivo de rotas do módulo de estoque
- [ ] 8.2 Adicionar rota `/stock/check-in` com lazy loading
- [ ] 8.3 Configurar proteção RBAC para a rota
- [ ] 8.4 Adicionar link na sidebar do módulo de estoque
- [ ] 8.5 Adicionar ícone apropriado para o menu (ex: PackagePlus)
- [ ] 8.6 Testar navegação manualmente

## Sequenciamento

- **Bloqueado por**: 7.0 (Serviço de Orquestração)
- **Desbloqueia**: 9.0 (Testes Vitest)
- **Paralelizável**: Não — depende da página estar funcional

## Detalhes de Implementação

### Configuração de Rota

```typescript
// frontend/src/modules/stock/routes.tsx
import { lazy } from 'react';
import { RouteObject } from 'react-router-dom';
import { RequireRoles } from '@/rbac/RequireRoles';

const VehicleCheckInPage = lazy(() => 
  import('./pages/VehicleCheckInPage').then(m => ({ default: m.VehicleCheckInPage }))
);

export const stockRoutes: RouteObject[] = [
  // ... rotas existentes
  {
    path: 'check-in',
    element: (
      <RequireRoles roles={['STOCK_PERSON', 'STOCK_MANAGER', 'MANAGER', 'ADMIN']}>
        <VehicleCheckInPage />
      </RequireRoles>
    ),
  },
];
```

### Link na Sidebar

```typescript
// frontend/src/modules/stock/navigation.ts (ou onde estiver configurado)
import { PackagePlus, Package, Truck, BarChart3 } from 'lucide-react';

export const stockNavItems = [
  {
    title: 'Registrar Entrada',
    href: '/stock/check-in',
    icon: PackagePlus,
    roles: ['STOCK_PERSON', 'STOCK_MANAGER', 'MANAGER', 'ADMIN'],
  },
  // ... outros itens existentes
  {
    title: 'Dashboard',
    href: '/stock/dashboard',
    icon: BarChart3,
  },
  {
    title: 'Veículos',
    href: '/stock/vehicles',
    icon: Package,
  },
  {
    title: 'Movimentações',
    href: '/stock/movements',
    icon: Truck,
  },
];
```

### Proteção RBAC

As roles permitidas para acesso ao fluxo de check-in são:
- `STOCK_PERSON` - Responsável pelo estoque (persona primária)
- `STOCK_MANAGER` - Gestor de estoque
- `MANAGER` - Gerente geral
- `ADMIN` - Administrador do sistema

### Estrutura de URLs

| URL | Página | Descrição |
|-----|--------|-----------|
| `/stock` | Dashboard | Visão geral do estoque |
| `/stock/check-in` | VehicleCheckInPage | **Nova** - Cadastro + Check-in |
| `/stock/vehicles` | VehiclesList | Lista de veículos |
| `/stock/vehicles/:id` | VehicleDetails | Detalhes do veículo |
| `/stock/movements` | StockMovements | Histórico de movimentações |

### Lazy Loading

O lazy loading é importante para performance, carregando a página apenas quando acessada:

```typescript
const VehicleCheckInPage = lazy(() => 
  import('./pages/VehicleCheckInPage')
);

// Usar Suspense no layout
<Suspense fallback={<PageSkeleton />}>
  <Outlet />
</Suspense>
```

## Critérios de Sucesso

- [ ] Rota `/stock/check-in` acessível e funcional
- [ ] Lazy loading configurado corretamente
- [ ] Link visível na sidebar do módulo de estoque
- [ ] Proteção RBAC funciona (usuários sem role não acessam)
- [ ] Navegação via URL direta funciona
- [ ] Navegação via click no menu funciona
- [ ] Título da página correto na aba do navegador
