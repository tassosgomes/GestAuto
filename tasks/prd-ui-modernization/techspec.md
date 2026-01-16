# Especificação Técnica - Modernização de UI com Shadcn e Tailwind

## Resumo Executivo

Esta especificação detalha a modernização da interface do frontend do GestAuto através da adoção do **Tailwind CSS** como motor de estilização e **Shadcn UI** como biblioteca de componentes. A abordagem visa substituir o CSS global atual por um sistema utilitário e componentes encapsulados, garantindo consistência visual com o protótipo de referência (`code.html`) e responsividade. A implementação introduzirá um `AppLayout` padronizado para todas as rotas autenticadas e uma página de documentação viva (`/design-system`).

## Arquitetura do Sistema

### Visão Geral dos Componentes

A arquitetura de UI será reestruturada em torno de um layout mestre e componentes atômicos:

- **AppLayout**: Componente container principal que envolve todas as rotas protegidas. Gerencia a disposição da `Sidebar` e do `Header`, além de controlar a responsividade (ex: menu mobile).
- **Sidebar**: Componente de navegação lateral. Consome a configuração de rotas/permissões para renderizar o menu.
- **Header**: Barra superior contendo título da página (contextual), busca global (visual) e menu de perfil do usuário.
- **Biblioteca UI (`src/components/ui`)**: Componentes atômicos (Button, Input, Card, etc.) gerados pelo Shadcn, servindo como blocos de construção para toda a aplicação.
- **DesignSystemPage**: Página isolada para visualização e validação dos componentes implementados.

### Fluxo de Dados (UI)

1.  **Inicialização**: `App.tsx` carrega o `AppLayout` para rotas autenticadas.
2.  **Navegação**: `Sidebar` recebe o estado de autenticação/permissões (via `useAuth` ou `rbac`) para filtrar itens de menu.
3.  **Renderização de Conteúdo**: O conteúdo da página específica é renderizado dentro de um `<Outlet />` (React Router) no corpo do `AppLayout`.

## Design de Implementação

### Configuração e Infraestrutura

Será necessário ajustar a configuração de build para suportar o Shadcn:

- **Path Aliases**: Configurar `@/*` em `tsconfig.json` e `vite.config.ts` apontando para `./src/*`.
- **Tailwind**: Inicializar `tailwind.config.js` e `postcss.config.js`.
- **Variáveis CSS**: Definir tokens de cor (ex: `--primary: 222.2 47.4% 11.2%`) em `index.css` baseados no `code.html`.

### Interfaces Principais

```typescript
// Definição para itens de navegação da Sidebar
interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>; // Lucide Icon
  permission?: string; // Opcional: chave de permissão RBAC
}

// Props do Layout
interface AppLayoutProps {
  children?: React.ReactNode; // Caso não use Outlet diretamente em alguns contextos
}
```

### Estrutura de Diretórios Proposta

```text
src/
  components/
    ui/           # Componentes Shadcn (Button, Card, etc.)
    layout/       # Componentes estruturais
      AppLayout.tsx
      Sidebar.tsx
      Header.tsx
      UserNav.tsx
  lib/
    utils.ts      # Utilitários do Shadcn (cn, clsx)
```

### Modelos de Dados

Não há novos modelos de domínio de backend. O estado é puramente de UI (ex: `isSidebarOpen`, `currentTheme`).

## Pontos de Integração

- **React Router**: O `AppLayout` deve integrar-se com o `Outlet` do `react-router-dom` v7.
- **Auth/RBAC**: A `Sidebar` e o `Header` (perfil) devem consumir o contexto de autenticação (`useAuth`) para exibir informações do usuário e filtrar menus.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
| :--- | :--- | :--- | :--- |
| `frontend/src/App.tsx` | Refatoração | Mudança na estrutura de rotas para envolver páginas no `AppLayout`. Risco Médio. | Testar navegação pós-migração. |
| `frontend/src/index.css` | Substituição | Remoção de estilos globais legados. Substituição por diretivas Tailwind. Risco Baixo. | Verificar regressão visual. |
| `frontend/vite.config.ts` | Configuração | Adição de alias para `@`. Risco Baixo. | Validar build. |
| Páginas Existentes | Refatoração Visual | Páginas atuais (`HomePage`, etc.) perderão estilos globais antigos e devem ser migradas para Tailwind/Shadcn. Risco Médio. | Refatorar layout interno de cada página. |
| Ícones (Material Symbols) | Remoção | Todos os ícones legados devem ser substituídos por `lucide-react`. Risco Baixo. | Substituir em todo o projeto. |

## Abordagem de Testes

### Testes Unitários

- **Componentes de Layout**: Testar renderização da `Sidebar` e `Header`.
- **Responsividade**: Verificar se o menu mobile é acionado em viewports pequenos (via testes ou verificação visual).
- **Shadcn**: Componentes do Shadcn são pré-testados, mas customizações críticas devem ser validadas.

### Validação Visual

- Utilizar a rota `/design-system` para validar manualmente a fidelidade visual (cores, tipografia, estados de hover) contra o `code.html`.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1.  **Infraestrutura Base**:
    - Instalar Tailwind CSS, PostCSS, Autoprefixer.
    - Configurar `vite.config.ts` e `tsconfig.json` (aliases).
    - Inicializar Shadcn UI (`npx shadcn@latest init`).
    - Configurar tema (cores e fontes) em `index.css` e `tailwind.config.js`.

2.  **Biblioteca de Componentes**:
    - Instalar componentes core: `button`, `card`, `input`, `label`, `separator`, `sheet`, `avatar`, `dropdown-menu`.
    - Instalar `lucide-react`.

3.  **Layout Estrutural**:
    - Implementar `Sidebar` (desktop e mobile).
    - Implementar `Header`.
    - Compor `AppLayout`.

4.  **Integração e Migração**:
    - Refatorar `App.tsx` para usar `AppLayout`.
    - Criar página `/design-system` com exemplos.
    - **Migração de Páginas**: Refatorar `HomePage`, `AdminPage` e outras para usar componentes Shadcn e classes Tailwind, removendo dependências de estilos antigos.
    - **Migração de Ícones**: Substituir todas as ocorrências de Material Symbols por ícones `lucide-react`.

5.  **Limpeza**:
    - Remover CSS legado não utilizado e referências a Material Symbols (CDN ou pacotes).
