---
status: pending
parallelizable: false
blocked_by: ["8.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>vitest, testing-library, msw</dependencies>
<unblocks>"10.0"</unblocks>
</task_context>

# Tarefa 9.0: Criar Testes Vitest para VehicleCheckInPage

## Visão Geral

Implementar testes automatizados para a página de cadastro de veículo com check-in usando Vitest e Testing Library. Os testes devem cobrir navegação entre steps, validação de formulário, e integração com APIs (usando MSW para mocking).

<requirements>
- Criar arquivo de testes em `frontend/tests/vehicle-checkin-page.test.tsx`
- Testar renderização inicial da página
- Testar navegação entre steps (origem → categoria → formulário)
- Testar validação de campos obrigatórios por categoria
- Testar comportamento de submissão com sucesso
- Testar comportamento de erro na validação de avaliação
- Usar MSW para mock de APIs
</requirements>

## Subtarefas

- [ ] 9.1 Criar arquivo `vehicle-checkin-page.test.tsx` em `frontend/tests/`
- [ ] 9.2 Configurar MSW handlers para APIs de stock e evaluation
- [ ] 9.3 Implementar teste de renderização inicial
- [ ] 9.4 Implementar teste de seleção de origem
- [ ] 9.5 Implementar teste de auto-seleção de categoria
- [ ] 9.6 Implementar teste de validação de formulário (campos obrigatórios)
- [ ] 9.7 Implementar teste de erro quando avaliação não existe
- [ ] 9.8 Implementar teste de submissão com sucesso
- [ ] 9.9 Implementar teste de navegação voltar
- [ ] 9.10 Executar todos os testes e garantir aprovação

## Sequenciamento

- **Bloqueado por**: 8.0 (Rotas e Navegação)
- **Desbloqueia**: 10.0 (Acessibilidade)
- **Paralelizável**: Não — depende da funcionalidade completa

## Detalhes de Implementação

### Estrutura do Arquivo de Testes

```typescript
// frontend/tests/vehicle-checkin-page.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { MemoryRouter } from 'react-router-dom';
import { VehicleCheckInPage } from '@/modules/stock/pages/VehicleCheckInPage';
import { AuthProvider } from '@/auth/AuthProvider';

// Setup MSW
const server = setupServer(
  // Handler padrão para criação de veículo
  http.post('/api/v1/vehicles', () => {
    return HttpResponse.json({
      id: 'vehicle-123',
      vin: 'ABC123',
      category: 'Used',
      status: 'Pending',
    }, { status: 201 });
  }),
  
  // Handler padrão para check-in
  http.post('/api/v1/vehicles/:id/check-ins', () => {
    return HttpResponse.json({
      id: 'checkin-456',
      vehicleId: 'vehicle-123',
      currentStatus: 'InStock',
    }, { status: 201 });
  }),
  
  // Handler padrão para validação de avaliação
  http.get('/api/v1/evaluations/:id', () => {
    return HttpResponse.json({
      id: 'eval-789',
      status: 'Completed',
    });
  }),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

// Helper para renderizar com providers
function renderWithProviders(component: React.ReactNode) {
  return render(
    <MemoryRouter>
      <AuthProvider>
        {component}
      </AuthProvider>
    </MemoryRouter>
  );
}

describe('VehicleCheckInPage', () => {
  // Testes aqui
});
```

### Testes Implementados

```typescript
describe('VehicleCheckInPage', () => {
  describe('Renderização inicial', () => {
    it('should render origin selector on initial load', () => {
      renderWithProviders(<VehicleCheckInPage />);
      
      expect(screen.getByText('Registrar Entrada de Veículo')).toBeInTheDocument();
      expect(screen.getByText('Montadora')).toBeInTheDocument();
      expect(screen.getByText('Compra Cliente/Seminovo')).toBeInTheDocument();
    });
  });

  describe('Navegação entre steps', () => {
    it('should navigate to category selector after origin selection', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Compra Cliente/Seminovo'));
      
      // Seminovo tem auto-seleção, então vai direto para formulário
      await waitFor(() => {
        expect(screen.getByLabelText(/placa/i)).toBeInTheDocument();
      });
    });

    it('should show back button when not on first step', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Transferência'));
      
      expect(screen.getByRole('button', { name: /voltar/i })).toBeInTheDocument();
    });

    it('should navigate back when clicking back button', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Transferência'));
      await user.click(screen.getByRole('button', { name: /voltar/i }));
      
      expect(screen.getByText('Montadora')).toBeInTheDocument();
    });
  });

  describe('Validação de formulário', () => {
    it('should disable submit when required fields are missing for Used category', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Compra Cliente/Seminovo'));
      
      await waitFor(() => {
        const submitButton = screen.getByRole('button', { name: /registrar entrada/i });
        expect(submitButton).toBeDisabled();
      });
    });

    it('should show validation error for missing plate on Used vehicle', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Compra Cliente/Seminovo'));
      
      await waitFor(() => {
        expect(screen.getByLabelText(/placa/i)).toBeInTheDocument();
      });
      
      // Preencher outros campos exceto placa e tentar submeter
      await user.type(screen.getByLabelText(/vin/i), 'WVWZZZ3CZWE123456');
      await user.type(screen.getByLabelText(/marca/i), 'Toyota');
      // ... preencher demais campos
      
      await user.click(screen.getByRole('button', { name: /registrar entrada/i }));
      
      expect(screen.getByText(/placa é obrigatória/i)).toBeInTheDocument();
    });
  });

  describe('Validação de avaliação', () => {
    it('should show error when evaluationId is invalid', async () => {
      // Override handler para retornar 404
      server.use(
        http.get('/api/v1/evaluations/:id', () => {
          return HttpResponse.json(
            { title: 'Not Found' },
            { status: 404 }
          );
        })
      );
      
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Compra Cliente/Seminovo'));
      
      // Preencher formulário completo com evaluationId inválido
      // ... preencher campos
      
      await user.click(screen.getByRole('button', { name: /registrar entrada/i }));
      
      await waitFor(() => {
        expect(screen.getByText(/avaliação não encontrada/i)).toBeInTheDocument();
      });
    });
  });

  describe('Submissão com sucesso', () => {
    it('should show success message after successful submission', async () => {
      const user = userEvent.setup();
      renderWithProviders(<VehicleCheckInPage />);
      
      await user.click(screen.getByText('Montadora'));
      
      // Preencher formulário de veículo novo
      await user.type(screen.getByLabelText(/vin/i), 'WVWZZZ3CZWE123456');
      await user.type(screen.getByLabelText(/marca/i), 'Toyota');
      await user.type(screen.getByLabelText(/modelo/i), 'Corolla');
      // ... preencher demais campos
      
      await user.click(screen.getByRole('button', { name: /registrar entrada/i }));
      
      await waitFor(() => {
        expect(screen.getByText(/entrada registrada com sucesso/i)).toBeInTheDocument();
      });
    });
  });
});
```

### Cenários de Teste

| # | Cenário | Tipo | Prioridade |
|---|---------|------|------------|
| 1 | Renderização inicial | Smoke | Alta |
| 2 | Seleção de origem navega para categoria | Funcional | Alta |
| 3 | Auto-seleção de categoria funciona | Funcional | Alta |
| 4 | Botão voltar aparece/funciona | Navegação | Média |
| 5 | Validação de campos obrigatórios | Validação | Alta |
| 6 | Erro quando avaliação não existe | Integração | Alta |
| 7 | Submissão com sucesso | E2E | Alta |
| 8 | Loading state durante submissão | UX | Média |

## Critérios de Sucesso

- [ ] Todos os testes passando
- [ ] Cobertura de cenários críticos > 80%
- [ ] MSW configurado corretamente para mocking
- [ ] Testes executam em < 30 segundos no total
- [ ] Nenhum teste flaky (intermitente)
- [ ] Cenários de erro cobertos
