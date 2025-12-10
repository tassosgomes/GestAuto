# Documento de Requisitos de Produto - Sistema de Avaliação de Veículos Seminovos

## Visão Geral

O Sistema de Avaliação de Veículos Seminovos é uma solução digital para padronizar e automatizar o processo de avaliação de veículos usados recebidos como troca na concessionária. O sistema visa substituir o processo atual baseado em planilhas e e-mails por um fluxo integrado que inclui documentação fotográfica obrigatória, checklist técnico padronizado, cálculos automáticos baseados na tabela FIPE, e um fluxo de aprovação gerencial com geração de laudos.

O problema principal resolvido é a falta de padronização nas avaliações, que resulta em inconsistências, perda de informações, e dificuldade na análise de rentabilidade das operações de "troca". O sistema fornecerá dados concretos para comparar a rentabilidade da venda direta versus outros canais de venda.

## Objetivos

### Objetivos de Negócio
- **Padronizar Processos**: Criar um fluxo único e replicável para todas as avaliações
- **Aumentar Rentabilidade**: Identificar veículos com melhor margem de lucro através de análise precisa
- **Reduzir Riscos**: Minimizar perdas financeiras em avaliações mal executadas
- **Agilizar Decisões**: Reduzir o tempo de avaliação de dias para horas através de automação
- **Melhorar Controle**: Ter visibilidade 100% do status de cada avaliação em tempo real

### Métricas de Sucesso
- Redução de 70% no tempo médio de avaliação (de 24h para 7h)
- Aumento de 15% na precisão das avaliações comparado com mercado
- Redução de 50% em contestações de clientes
- Taxa de aprovação de avaliações > 85%
- Adocão de 100% pelos avaliadores em 3 meses
- Geração de laudos PDF em < 2 minutos

## Histórias de Usuário

### Avaliador de Veículos
- **Como** avaliador, **eu quero** iniciar uma avaliação informando dados básicos do veículo (placa, KM, cor, acessórios) **para que** o sistema crie uma avaliação padronizada
- **Como** avaliador, **eu quero** fazer upload das 15 fotos obrigatórias seguindo o padrão definido (externa, interna, motor, etc) **para que** tenha documentação visual completa do veículo
- **Como** avaliador, **eu quero** preencher um checklist técnico padronizado (estado de conservação, pneus, documentos, etc) **para que** a avaliação seja consistente
- **Como** avaliador, **eu quero** que o sistema calcule automaticamente o preço baseado na FIPE aplicando as depreciações **para que** eu tenha uma sugestão de preço fundamentada
- **Como** avaliador, **eu quero** adicionar observações importantes sobre o veículo **para que** o gerente tenha contexto completo na aprovação

### Gerente de Usados
- **Como** gerente, **eu quero** ver todas as avaliações pendentes em um dashboard **para que** possa priorizar minhas aprovações
- **Como** gerente, **eu quero** analisar cada avaliação com todas as fotos, checklist e cálculos **para que** possa tomar decisões informadas
- **Como** gerente, **eu quero** aprovar ou reprovar avaliações com justificativa **para que** haja um registro claro da decisão
- **Como** gerente, **eu quero** visualizar relatórios com métricas de aprovação, valores médios e rentabilidade **para que** possa identificar padrões e oportunidades
- **Como** gerente, **eu quero** ser notificado quando há avaliações urgentes ou acima de um valor específico **para que** possa dar atenção prioritária

### Vendedor
- **Como** vendedor, **eu quero** consultar o status de uma avaliação que solicitei **para que** possa informar o cliente
- **Como** vendedor, **eu quero** receber uma notificação quando uma avaliação for aprovada **para que** possa fechar negócio com o cliente
- **Como** vendedor, **eu quero** acessar o laudo final aprovado **para que** possa apresentar ao cliente como justificativa do valor

### Administrador do Sistema
- **Como** administrador, **eu quero** gerenciar a tabela de depreciação por marca/modelo/ano **para que** os cálculos sempre reflitam o mercado atual
- **Como** administrador, **eu quero** configurar os percentuais de segurança e lucro **para que** a política comercial seja aplicada consistentemente
- **Como** administrador, **eu quero** gerenciar usuários e permissões **para que** apenas pessoas autorizadas possam avaliar e aprovar

## Funcionalidades Principais

### 1. Gestão de Avaliações
**O que faz**: Cria, edita e gerencia o ciclo de vida completo das avaliações

**Por que é importante**: É o core do sistema que padroniza todo o processo

**Como funciona**:
- Cadastro inicial com dados básicos do veículo
- Progressão através de status: Rascunho → Em Análise → Aguardando Aprovação → Aprovada/Reprovada
- Histórico completo de todas as alterações
- Associação com vendedor solicitante e avaliador responsável

**Requisitos funcionais**:
1. O sistema deve permitir iniciar uma avaliação com placa, KM, cor, opcionais e observações iniciais
2. O sistema deve buscar automaticamente dados do veículo pela placa (marca, modelo, ano)
3. O sistema deve validar que todos os campos obrigatórios foram preenchidos antes de prosseguir
4. O sistema deve registrar data/hora e usuário em cada mudança de status
5. O sistema deve impedir alterações em avaliações já aprovadas

### 2. Documentação Fotográfica
**O que faz**: Gerencia o upload e organização das fotos obrigatórias

**Por que é importante**: Garante documentação visual completa e padronizada

**Como funciona**:
- Upload em lote das 15 fotos obrigatórias
- Validação automática se todos os ângulos foram cobertos
- Armazenamento em Cloudflare R2 otimizado para acesso rápido
- Visualização em carrossel para análise

**Requisitos funcionais**:
1. O sistema deve exigir exatamente 15 fotos: 4 externas, 4 internas, 3 painel, 2 motor, 2 porta-malas
2. O sistema deve validar qualidade mínima da imagem (resolução, nitidez)
3. O sistema deve permitir trocar fotos individuais antes da finalização
4. O sistema deve gerar automaticamente um thumbnail de cada foto
5. O sistema deve armazenar metadados (data/hora, dispositivo) de cada foto

### 3. Checklist Técnico
**O que faz**: Coleta informações técnicas padronizadas sobre o estado do veículo

**Por que é importante**: Garante avaliação consistente e completa

**Como funciona**:
- Formulário estruturado com seções (lataria, pneus, interior, mecânica, documentos)
- Validações por seção impedindo avanço se itens críticos não marcados
- Cálculo automático de score de conservação
- Geração de resumo automático

**Requisitos funcionais**:
1. O sistema deve ter seções específicas para lataria, pintura, pneus, interior, eletrônica, mecânica e documentação
2. O sistema deve marcar itens críticos que impossibilitam aprovação
3. O sistema deve calcular um score geral de 0-100 baseado nas respostas
4. O sistema deve permitir adicionar observações detalhadas em cada item
5. O sistema deve identificar automaticamente itens que impactam o valor

### 4. Cálculo de Valoração
**O que faz**: Calcula o valor recomendado baseado na FIPE e regras de negócio

**Por que é importante**: Fornece uma sugestão de preço objetiva e defensável

**Como funciona**:
- Busca automática do valor FIPE baseado em marca/modelo/ano
- Aplicação de percentuais de depreciação pré-configurados
- Adição de percentuais de segurança e margem
- Ajuste manual com justificativa obrigatória

**Requisitos funcionais**:
1. O sistema deve integrar com API FIPE para obter valor de mercado
2. O sistema deve aplicar tabela de depreciação por marca/ano/configuração
3. O sistema deve adicionar percentuais configuráveis de segurança e lucro
4. O sistema deve mostrar detalhadamente o cálculo passo a passo
5. O sistema deve permitir ajuste manual不超过 10% com aprovação do gerente

### 5. Geração de Laudo
**O que faz**: Cria documento PDF completo com todos os dados da avaliação

**Por que é importante**: Gera documento oficial para apresentação ao cliente

**Como funciona**:
- Compila todas as informações em template padronizado
- Inclui fotos em miniatura e checklist completo
- Adiciona cálculo detalhado e valor final
- Gera QR code para validação online

**Requisitos funcionais**:
1. O sistema deve gerar PDF com marca d'água "APROVADO" ou "REPROVADO"
2. O sistema deve incluir todas as 15 fotos organizadas por categoria
3. O sistema deve mostrar o cálculo completo com FIPE e deduções
4. O sistema deve incluir observações do avaliador e do gerente
5. O sistema deve gerar um link único para validação online do laudo

### 6. Workflow de Aprovação
**O que faz**: Gerencia o fluxo de aprovação gerencial obrigatório

**Por que é importante**: Garante controle e qualidade nas avaliações

**Como funciona**:
- Lista de avaliações pendentes para o gerente
- Visualização completa de todos os dados
- Opções de aprovar, reprovar ou solicitar correção
- Notificações automáticas para o avaliador
- Todas as avaliações passam por aprovação manual (sem automação)

**Requisitos funcionais**:
1. O sistema deve mostrar lista priorizada por data e valor
2. O sistema deve exigir justificativa obrigatória para rejeições
3. O sistema deve permitir aprovação parcial com condições
4. O sistema deve notificar automaticamente o avaliador da decisão
5. O sistema deve registrar histórico completo de aprovações
6. O sistema não deve permitir aprovações automáticas

### 7. Dashboard Gerencial
**O que faz**: Fornece visão estratégica das operações

**Por que é importante**: Permite tomada de decisão baseada em dados

**Como funciona**:
- KPIs principais em tempo real
- Gráficos de tendências e comparações
- Filtros por período, avaliador, marca
- Exportação para Excel

**Requisitos funcionais**:
1. O sistema deve mostrar: avaliações no mês, taxa aprovação, ticket médio, tempo médio
2. O sistema deve ter gráficos de evolução mensal e distribuição por marca
3. O sistema deve permitir filtrar por avaliador, período e status
4. O sistema deve exportar relatórios em PDF/Excel
5. O sistema deve atualizar dados em tempo real

## Experiência do Usuário

### Personas

**João - Avaliador Sênior (35 anos)**
- Experiente em mecânica e avaliação de veículos
- Necessidade de agilidade e precisão
- Familiar com tecnologia mobile
- Realiza 5-10 avaliações por dia

**Maria - Gerente de Usados (42 anos)**
- 15 anos de experiência no mercado
- Focada em rentabilidade e controle
- Precisa de visão rápida do status geral
- Tomadora de decisão

**Pedro - Vendedor (28 anos)**
- Focado em vendas e atendimento
- Necessita de respostas rápidas para clientes
- Usa principalmente mobile
- Precisa de simplicidade

### Fluxos Principais

**Fluxo de Avaliação Completo**:
1. Login no sistema (mobile ou web)
2. Nova avaliação → preencher dados básicos
3. Upload das 15 fotos (validação automática)
4. Preencher checklist técnico
5. Sistema calcula valor automaticamente
6. Revisar e submeter para aprovação
7. Aguardar decisão do gerente
8. Receber notificação com resultado
9. Gerar laudo PDF se aprovado

**Fluxo de Aprovação Gerencial**:
1. Acessar dashboard de pendências
2. Filtrar por prioridade (valor/urgência)
3. Abrir avaliação → revisar fotos
4. Analisar checklist e observações
5. Verificar cálculo e valor sugerido
6. Decidir: aprovar/reprovar/solicitar ajuste
7. Adicionar justificativa se necessário
8. Confirmar decisão
9. Sistema notifica avaliador automaticamente

### Requisitos de UI/UX

**Mobile-First Design**:
- Interface otimizada para tablets no pátio
- Captura de fotos direto da câmera
- Upload progressivo para não perder dados
- Modo offline para áreas sem internet

**Acessibilidade**:
- Contraste WCAG AA compliance
- Navegação 100% por teclado
- Leitores de tela compatíveis
- Texto alternativo para todas as imagens

**Performance**:
- Carregamento < 3 segundos
- Upload de fotos com progresso visual
- Cache inteligente para evitar reloads
- Notificações push para decisões

## Restrições Técnicas de Alto Nível

### Integrações Externas
- **API FIPE**: Integração obrigatória com tabela Fipe (https://deividfortuna.github.io/fipe/v2/)
- **Sistema Estoque**: Integração com módulo de estoque GestAuto para veículos aprovados
- **SNG/Checkauto**: Futura integração para consulta de histórico (fora do escopo inicial)
- **RabbitMQ**: Eventos de integração com o bounded context Commercial

### Requisitos de Performance
- Suporte para 50 avaliações simultâneas
- Tempo resposta < 2s para operações CRUD
- Geração PDF < 30 segundos
- Upload fotos com progresso real-time

### Segurança e Conformidade
- LGPD compliance para dados de clientes
- Criptografia de dados sensíveis (placas, documentos)
- Audit trail completo de todas as operações
- Role-based access control (RBAC)

### Escalabilidade
- Arquitetura baseada em microserviços
- Banco de dados PostgreSQL horizontalmente escalável
- Armazenamento de imagens em Cloudflare R2 (compatível com S3)
- Cache Redis para consultas frequentes

## Não-Objetivos (Fora de Escopo)

### Funcionalidades Excluídas
- Aplicativo mobile nativo (versão web mobile apenas)
- OCR automático para documentos (fora do escopo inicial)
- Integração com SNG/Checkauto (será tratado em implementação futura)
- Compressão automática de imagens
- Simulador de financiamento
- Cálculo de IPVA e taxas
- Contestação de valores pelo cliente
- Rate limiting para APIs externas
- Integração com redes sociais para marketing
- Leilão online entre departamentos
- Multiplas filiais (escopo é concessionária única)

### Limitações Conhecidas
- Dependência de internet para validações em tempo real
- Fotos qualidade dependente do dispositivo
- Sem integração com sistemas de terceiros inicialmente
- Todas as avaliações exigem aprovação manual (sem automação)

### Considerações Futuras
- Machine learning para precificação baseada em histórico
- App mobile com realidade aumentada
- Integração com consultório de funilaria
- API pública para parceiros
- Dashboard para clientes acompanharem avaliação

## Decisões Resolvidas

### Decisões de Negócio
1. **Aprovações Automáticas**: Não haverá aprovações automáticas por enquanto - Todas as avaliações precisarão de aprovação manual do gerente
2. **Política de Reavaliação**: Pode haver reavaliação - Mesmo veículo pode ser reavaliado em menos de 72h
3. **Escopo Geográfico**: Não vamos trabalhar com filiais no momento - Escopo é apenas para concessionária única
4. **Contestação de Valores**: Não vamos tratar no momento - Contestação de valores pelo cliente está fora de escopo

### Aspectos Técnicos
1. **Storage de Fotos**: Vamos usar Cloudflare R2 (compatível com S3) para armazenamento das fotos
2. **Compressão de Imagens**: Não vamos cobrir no momento - Compressão automática de imagens está fora de escopo
3. **Backup de Laudos**: Não vamos cobrir no momento - Backup específico dos laudos segue padrão do sistema
4. **Rate Limiting**: Não vamos cobrir no momento - Rate limiting para API externa está fora de escopo

### Integrações
1. **APIs Externas (SNG/Checkauto)**: Task @11_task.md define implementação futura - Fora do escopo inicial
2. **Integração com Estoque**: Vai ser tratado na criação do microserviço de estoque - Veículo aprovado entra automaticamente no estoque
3. **Perfis de Avaliador**: Não vamos tratar nesse momento - Todos os avaliadores terão mesmo nível de permissão
4. **Integração Financeira**: Vai ser tratado no módulo financeiro - Contabilização do valor da troca

## Questões em Aberto

### Processos
1. **Treinamento**: Como será o treinamento dos avaliadores no novo sistema?
2. **Transição**: Como migrar avaliações em andamento para o novo sistema?
3. **Suporte**: Canal de suporte para problemas urgentes?
4. **Validação**: Como validar se a avaliação foi feita corretamente?