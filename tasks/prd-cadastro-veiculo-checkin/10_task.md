---
status: pending
parallelizable: false
blocked_by: ["9.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>implementation</type>
<scope>performance</scope>
<complexity>medium</complexity>
<dependencies>react, wcag</dependencies>
<unblocks>"11.0"</unblocks>
</task_context>

# Tarefa 10.0: Garantir Acessibilidade e Refinamentos de UX

## Visão Geral

Revisar todos os componentes implementados para garantir conformidade com WCAG 2.1 AA, incluindo navegação por teclado, labels adequados, mensagens de erro acessíveis, e contraste de cores. Também realizar refinamentos de UX baseados em feedback visual e comportamental.

<requirements>
- Garantir labels associados a todos os campos de formulário
- Implementar `aria-live` para mensagens de erro e feedback
- Verificar navegação completa por teclado
- Validar contraste mínimo WCAG 2.1 AA (4.5:1 para texto normal)
- Adicionar indicadores de foco visíveis
- Implementar skip links se necessário
- Testar com screen reader (VoiceOver/NVDA)
</requirements>

## Subtarefas

- [ ] 10.1 Auditar OriginSelector para acessibilidade
- [ ] 10.2 Auditar CategorySelector para acessibilidade
- [ ] 10.3 Auditar DynamicVehicleForm para acessibilidade
- [ ] 10.4 Adicionar `aria-live="polite"` em áreas de feedback
- [ ] 10.5 Verificar e corrigir ordem de tabulação
- [ ] 10.6 Adicionar `aria-describedby` para campos com mensagens de erro
- [ ] 10.7 Testar contraste de cores com ferramenta automatizada
- [ ] 10.8 Testar navegação completa apenas com teclado
- [ ] 10.9 Testar com screen reader (pelo menos um)
- [ ] 10.10 Documentar quaisquer limitações conhecidas

## Sequenciamento

- **Bloqueado por**: 9.0 (Testes Vitest)
- **Desbloqueia**: 11.0 (Documentação)
- **Paralelizável**: Não — revisão final antes da documentação

## Detalhes de Implementação

### Checklist de Acessibilidade por Componente

#### OriginSelector

```typescript
// Atributos de acessibilidade necessários
<div 
  role="radiogroup"
  aria-label="Selecione a origem do veículo"
>
  <Card
    role="radio"
    aria-checked={isSelected}
    aria-label={option.title}
    tabIndex={0}
    onKeyDown={handleKeyDown} // Enter/Space
  >
    ...
  </Card>
</div>
```

#### CategorySelector

```typescript
// Atributos de acessibilidade necessários
<div 
  role="radiogroup"
  aria-label="Selecione a categoria do veículo"
>
  <Button
    role="radio"
    aria-checked={isSelected}
  >
    ...
  </Button>
</div>
```

#### DynamicVehicleForm

```typescript
// Padrão para campos de formulário
<FormField
  name="vin"
  render={({ field, fieldState }) => (
    <FormItem>
      <FormLabel htmlFor="vin">VIN/Chassi</FormLabel>
      <FormControl>
        <Input 
          id="vin"
          aria-describedby={fieldState.error ? 'vin-error' : undefined}
          aria-invalid={!!fieldState.error}
          {...field} 
        />
      </FormControl>
      {fieldState.error && (
        <FormMessage 
          id="vin-error"
          role="alert"
          aria-live="polite"
        >
          {fieldState.error.message}
        </FormMessage>
      )}
    </FormItem>
  )}
/>
```

### Área de Feedback com aria-live

```typescript
// Componente para feedback de ações
function FeedbackArea({ message, type }: FeedbackProps) {
  return (
    <div 
      role="status"
      aria-live="polite"
      aria-atomic="true"
      className={cn(
        'p-4 rounded-lg',
        type === 'success' && 'bg-green-100 text-green-800',
        type === 'error' && 'bg-red-100 text-red-800',
      )}
    >
      {message}
    </div>
  );
}
```

### Indicadores de Foco

```css
/* Garantir focus rings visíveis */
:focus-visible {
  outline: 2px solid hsl(var(--ring));
  outline-offset: 2px;
}

/* Para cards clicáveis */
.card-selectable:focus-visible {
  ring: 2px;
  ring-color: hsl(var(--primary));
  ring-offset: 2px;
}
```

### Testes de Acessibilidade

| Teste | Ferramenta | Resultado Esperado |
|-------|------------|-------------------|
| Contraste de cores | axe DevTools | 0 erros de contraste |
| Estrutura de headings | Wave | Hierarquia correta |
| Labels de formulário | axe DevTools | 100% dos inputs com label |
| Navegação por teclado | Manual | Todos elementos acessíveis |
| Screen reader | NVDA/VoiceOver | Anúncios corretos |

### Refinamentos de UX

| Item | Descrição | Impacto |
|------|-----------|---------|
| Loading skeleton | Mostrar skeleton enquanto carrega | Percepção de velocidade |
| Transições suaves | Animar mudança de steps | Feedback visual |
| Erro inline vs toast | Erros de campo inline, erros de API como toast | Clareza |
| Confirmação de sucesso | Mostrar resumo após conclusão | Confiança |
| Autopreenchimento | Sugerir marcas/modelos populares | Eficiência |

### Padrões WCAG 2.1 AA

| Critério | Descrição | Status |
|----------|-----------|--------|
| 1.1.1 | Conteúdo não-textual | Labels em ícones |
| 1.3.1 | Info e relacionamentos | Estrutura semântica |
| 1.4.3 | Contraste mínimo | 4.5:1 |
| 2.1.1 | Teclado | Navegação completa |
| 2.4.3 | Ordem de foco | Sequência lógica |
| 2.4.6 | Headings e labels | Descritivos |
| 3.3.1 | Identificação de erro | Mensagens claras |
| 3.3.2 | Labels ou instruções | Presentes |
| 4.1.2 | Nome, função, valor | ARIA correto |

## Critérios de Sucesso

- [ ] 0 erros em auditoria automatizada (axe DevTools)
- [ ] Navegação completa apenas com teclado funciona
- [ ] Todos os campos têm labels visíveis e associados
- [ ] Mensagens de erro são anunciadas por screen readers
- [ ] Contraste mínimo de 4.5:1 em todo texto
- [ ] Focus rings visíveis em todos elementos interativos
- [ ] Teste com pelo menos um screen reader realizado
