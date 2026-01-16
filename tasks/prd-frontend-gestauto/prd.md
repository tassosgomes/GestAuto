# PRD - Frontend GestAuto (Login e Menus por Perfil)

## Visão Geral

Atualmente o GestAuto possui serviços backend (ex.: `commercial` e `vehicle-evaluation`) com autenticação e autorização via Keycloak, mas **não existe um frontend para usuários finais**. Isso obriga usuários a depender de ferramentas técnicas (ex.: Swagger/Postman) ou processos manuais, reduzindo adoção e aumentando risco operacional.

Esta iniciativa cria um **frontend web** que permite:
- Autenticar usuários via Keycloak.
- Exibir **menus e rotas** conforme as roles disponíveis na claim `roles` do token.

O objetivo desta entrega é fornecer um “ponto de entrada” (entry point) seguro e simples para o ecossistema GestAuto, com **estrutura de login e navegação por perfil**, sem implementar fluxos de negócio (CRUD/ações) nesta fase.

## Objetivos

- Fornecer uma aplicação web para acesso de usuários ao GestAuto.
- Garantir que apenas usuários autenticados acessem rotas protegidas.
- Garantir que a navegação (menus) respeite as roles fornecidas pelo Keycloak.
- Estabelecer um baseline consistente de RBAC alinhado ao catálogo de roles oficial.

**Métricas de sucesso (qualitativas nesta fase):**
- Usuário consegue fazer login com Keycloak e acessar a aplicação.
- Usuário vê apenas os menus compatíveis com suas roles.
- Usuário sem permissão não consegue acessar rotas diretamente via URL.

## Histórias de Usuário

### Vendedor (`SALES_PERSON`)
- Como vendedor, quero fazer login no GestAuto para acessar o menu Comercial.
- Como vendedor, quero que menus não relacionados ao meu perfil não apareçam, para reduzir confusão.

### Gerente Comercial (`SALES_MANAGER`)
- Como gerente comercial, quero fazer login e visualizar o menu Comercial com as mesmas opções base do vendedor.

### Avaliador (`VEHICLE_EVALUATOR`)
- Como avaliador, quero fazer login no GestAuto para acessar o menu Avaliações.

### Gerente de Avaliações (`EVALUATION_MANAGER`)
- Como gerente de avaliações, quero fazer login e visualizar o menu Avaliações.

### Gestor Cross-Service (`MANAGER`)
- Como gestor, quero ver os menus Comercial e Avaliações para navegar entre módulos, sem ter que usar múltiplos logins.

### Visualizador (`VIEWER`)
- Como visualizador, quero fazer login e acessar somente o menu Avaliações em modo de consulta (nesta fase, apenas navegação).

### Administrador (`ADMIN`)
- Como administrador, quero fazer login e ver todos os menus para fins de administração e suporte.

## Funcionalidades Principais

### F1. Autenticação via Keycloak

**O que faz:** autentica usuários usando o Realm `gestauto` do Keycloak e cria uma sessão de usuário no frontend.

**Por que é importante:** sem autenticação não existe controle de acesso seguro nem experiência de usuário consistente.

**Como funciona (alto nível):** o usuário é redirecionado ao Keycloak, autentica, retorna ao frontend com um token, e a aplicação passa a considerar o usuário “logado”.

**Requisitos funcionais:**
1. O sistema deve permitir login via Keycloak usando a base URL `http://keycloak.tasso.local`.
2. O sistema deve suportar realms diferentes por ambiente (dev/hml/prod).
3. O sistema deve considerar o usuário autenticado apenas quando existir token válido.
4. O sistema deve permitir logout encerrando a sessão do usuário.
5. O sistema deve lidar com token expirado/ausente redirecionando o usuário para login.

### F2. Autorização baseada na claim `roles`

**O que faz:** interpreta a claim `roles` do token para determinar permissões e visibilidade de menus.

**Por que é importante:** o backend já usa RBAC; o frontend deve refletir as mesmas regras para reduzir erros de uso e risco de exposição.

**Requisitos funcionais:**
1. O sistema deve ler as roles do usuário a partir da claim `roles` do token.
2. O sistema deve seguir o catálogo oficial de roles em `SCREAMING_SNAKE_CASE`.
3. O sistema deve tratar ausência de roles como “sem acesso”, exibindo estado apropriado (ex.: “Sem permissões configuradas”).

### F3. Visibilidade de menus por perfil (RBAC de navegação)

**O que faz:** mostra/oculta menus de navegação conforme as roles.

**Requisitos funcionais:**
1. O sistema deve exibir o menu **Comercial** quando o usuário tiver pelo menos uma das roles: `SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`.
2. O sistema deve exibir o menu **Avaliações** quando o usuário tiver pelo menos uma das roles: `VEHICLE_EVALUATOR`, `EVALUATION_MANAGER`, `MANAGER`, `VIEWER`, `ADMIN`.
3. O sistema deve exibir o menu **Admin** apenas quando o usuário tiver a role `ADMIN`.
4. O sistema não deve exibir menus que o usuário não possui permissão para acessar.

### F4. Proteção de rotas (route guarding)

**O que faz:** impede que o usuário acesse rotas protegidas diretamente pela URL quando não tiver permissão.

**Requisitos funcionais:**
1. O sistema deve bloquear acesso a rotas do módulo Comercial quando o usuário não possuir uma role que habilite o menu Comercial.
2. O sistema deve bloquear acesso a rotas do módulo Avaliações quando o usuário não possuir uma role que habilite o menu Avaliações.
3. O sistema deve bloquear acesso a rotas de administração quando o usuário não possuir a role `ADMIN`.
4. Ao bloquear acesso, o sistema deve exibir uma tela de “Acesso negado” (sem detalhes sensíveis).

### F5. Estrutura mínima de páginas (MVP)

**O que faz:** fornece as páginas mínimas para navegação e validação do RBAC.

**Requisitos funcionais:**
1. O sistema deve ter uma página inicial pós-login (Home) com navegação para os menus disponíveis.
2. O sistema deve ter páginas “placeholder” para:
   - Comercial
   - Avaliações
   - Administração
3. As páginas “placeholder” devem deixar claro que não há operações de negócio nesta fase.

## Experiência do Usuário

### Fluxo principal
1. Usuário acessa o frontend GestAuto.
2. Usuário clica em “Entrar”.
3. Usuário autentica no Keycloak.
4. Usuário retorna autenticado e visualiza a Home.
5. O sistema exibe apenas os menus permitidos pelas roles.
6. Usuário navega para o módulo permitido e visualiza a página correspondente.
7. Usuário pode sair (logout).

### Estados e mensagens
- Não autenticado: apresentar ação clara de login.
- Sem permissão: apresentar “Acesso negado” e opção de voltar.
- Sem roles: apresentar mensagem orientando a procurar um administrador.

### Acessibilidade
- Navegação por teclado deve ser suportada (foco visível e ordem lógica).
- Textos e rótulos devem ser claros e não depender apenas de cor para significado.

## Restrições Técnicas de Alto Nível

- **Identidade e RBAC**: autenticação e roles devem ser obtidas via Keycloak, com roles na claim `roles` (multivalued) conforme a configuração padronizada.
- **Conformidade (LGPD)**:
  - Minimizar coleta e armazenamento local de dados pessoais.
  - Não persistir dados além do necessário para manter a sessão.
  - Evitar registrar tokens ou dados sensíveis em logs do frontend.
- **Segurança**:
  - Somente rotas autorizadas devem ser acessíveis.
  - O frontend não deve assumir que “ocultar menu” equivale a autorização; deve haver proteção de rotas.

## Referências de UI

- Existe um exemplo de componentes/base visual em [model-ui/code.html](model-ui/code.html). O frontend deve se inspirar nesses componentes para manter consistência.

## Não-Objetivos (Fora de Escopo)

- Implementar operações de negócio (CRUD) dos módulos Comercial e Avaliações.
- Consumir APIs dos serviços (`commercial` e `vehicle-evaluation`) nesta fase.
- Implementar gestão de usuários/roles no frontend (isso permanece no Keycloak).
- Criar aplicativo mobile nativo.
- Definir detalhes de stack/infra (isso pertence à Tech Spec).
- Implementar SLAs de performance específicos nesta entrega.

## Questões em Aberto

- Keycloak: a base URL será `http://keycloak.tasso.local` para dev/hml/prod; cada ambiente usará um realm diferente. Ainda é necessário criar uma task para configurar realms/clients/mappers/redirecionamentos.
- O frontend será publicado em `http://gestauto.tasso.local`. Ainda é necessário definir políticas de CORS e redirect URIs/Web Origins no Keycloak e no proxy.

Decisão registrada: o menu **Admin** pode existir no MVP como página placeholder (visível apenas para `ADMIN`).
