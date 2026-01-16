# PRD - Modernização de UI com Shadcn e Tailwind

## Visão Geral

Este projeto visa modernizar a interface do frontend do GestAuto, migrando de uma estilização básica para um Design System robusto baseado em **Shadcn UI** e **Tailwind CSS**. O objetivo é replicar a identidade visual profissional definida no protótipo `Modelos/code.html`, estabelecendo um layout padrão (Sidebar e Header) e uma biblioteca de componentes reutilizáveis para acelerar o desenvolvimento futuro e garantir consistência visual.

## Objetivos

- **Estabelecer Identidade Visual:** Implementar a paleta de cores (Azul #135bec), tipografia (Inter) e espaçamentos definidos no modelo de referência.
- **Padronizar Layout:** Criar uma estrutura de navegação consistente (Sidebar responsiva + Header) para todas as páginas autenticadas.
- **Acelerar Desenvolvimento:** Disponibilizar uma biblioteca de componentes (Shadcn) pré-configurados e estilizados.
- **Documentação Viva:** Criar uma rota `/design-system` que sirva como catálogo visual dos componentes implementados.

## Histórias de Usuário

- **Como Desenvolvedor**, quero ter o Tailwind CSS e Shadcn UI configurados no projeto para poder construir interfaces complexas rapidamente sem escrever CSS do zero.
- **Como Usuário**, quero uma navegação lateral e superior consistente em todas as telas para que eu possa me localizar facilmente no sistema.
- **Como Desenvolvedor**, quero acessar uma página `/design-system` para visualizar todos os componentes disponíveis (botões, inputs, cards) e seus estados, facilitando a reutilização.
- **Como Usuário**, quero que a interface seja responsiva e se adapte a dispositivos móveis, mantendo a usabilidade.

## Funcionalidades Principais

### 1. Configuração da Base de Estilos
- **O que faz:** Instalação e configuração do Tailwind CSS e Shadcn UI.
- **Por que é importante:** Fundamenta toda a estilização do projeto.
- **Requisitos Funcionais:**
    1. Instalar Tailwind CSS e suas dependências.
    2. Inicializar Shadcn UI.
    3. Configurar a fonte "Inter" como padrão.
    4. Configurar as variáveis de cor CSS (`--primary`, `--background`, etc.) para corresponder aos valores hexadecimais do `code.html` (ex: Primary `#135bec`).

### 2. Layout Principal (AppLayout)
- **O que faz:** Componente estrutural que envolve o conteúdo das páginas.
- **Por que é importante:** Garante consistência de navegação.
- **Requisitos Funcionais:**
    1. Criar componente `Sidebar` com navegação vertical (links para Home, Admin, etc.).
    2. Criar componente `Header` com título da página, busca (visual) e perfil do usuário.
    3. Implementar comportamento responsivo (Sidebar colapsável ou menu hambúrguer em mobile).
    4. Substituir o layout atual das páginas existentes (`HomePage`, `AdminPage`, etc.) por este novo `AppLayout`.

### 3. Biblioteca de Componentes (Shadcn)
- **O que faz:** Conjunto de componentes de UI reutilizáveis.
- **Por que é importante:** Evita duplicação de código e inconsistências.
- **Requisitos Funcionais:**
    1. Instalar componentes essenciais do Shadcn: `Button`, `Card`, `Input`, `Label`, `Separator`, `Sheet` (para menu mobile), `Avatar`.
    2. Customizar estilos base dos componentes para alinhar com o `code.html` (arredondamento, sombras).
    3. Substituir ícones *Material Symbols* por **Lucide React** equivalentes.

### 4. Página de Design System
- **O que faz:** Uma página de demonstração estática.
- **Por que é importante:** Serve como referência visual e teste de integração dos componentes.
- **Requisitos Funcionais:**
    1. Criar rota `/design-system`.
    2. Implementar seções visuais copiando a estrutura do `code.html`:
        - Cores (exibição da paleta).
        - Tipografia (exibição de H1, H2, Body).
        - Componentes (exibição de Botões, Inputs, Cards).
    3. A página deve usar os componentes React reais implementados, não apenas HTML estático.

## Experiência do Usuário

- **Estética:** Limpa, corporativa, com bom uso de espaço em branco (whitespace) e contraste adequado.
- **Navegação:** Intuitiva, com feedback visual de estado ativo nos menus.
- **Responsividade:** A Sidebar deve se comportar adequadamente em telas menores (ex: transformar-se em um drawer/sheet).

## Restrições Técnicas de Alto Nível

- **Stack:** React 19, Vite, TypeScript.
- **Bibliotecas:** Tailwind CSS, Shadcn UI, Lucide React.
- **Compatibilidade:** Manter compatibilidade com o sistema de rotas (`react-router-dom`) e autenticação (`keycloak-js`) existentes.
- **Ícones:** Migração obrigatória para Lucide React (não usar Material Symbols via CDN para evitar requisições extras e manter padrão React).

## Não-Objetivos (Fora de Escopo)

- Alterações no backend ou APIs.
- Implementação funcional dos widgets da página de Design System (ex: o botão de busca não precisa realizar buscas reais, o gráfico de vendas não precisa ser dinâmico).
- Redesign completo de fluxos de negócio complexos (apenas aplicar o layout base).

## Questões em Aberto

- Nenhuma no momento. O escopo visual está bem definido pelo arquivo de referência.
