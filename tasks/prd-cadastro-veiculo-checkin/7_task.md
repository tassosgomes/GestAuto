---
status: pending
parallelizable: false
blocked_by: ["6.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>integration</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis, axios</dependencies>
<unblocks>"8.0"</unblocks>
</task_context>

# Tarefa 7.0: Implementar Serviço de Orquestração e Validação de Avaliação

## Visão Geral

Implementar o método `createVehicleWithCheckIn` no `vehicleService.ts` que orquestra a criação de veículo seguida do check-in. Para veículos seminovos, deve validar a existência do `EvaluationId` antes de prosseguir, consultando o serviço `vehicle-evaluation`.

<requirements>
- Adicionar método `createVehicleWithCheckIn` em `vehicleService.ts`
- Implementar validação de `EvaluationId` para seminovos via chamada ao vehicle-evaluation
- Configurar instância axios para API de avaliações (se não existir)
- Tratar erros de validação e retornar mensagens amigáveis
- Implementar timeout de 5 segundos para chamada de validação
- Permitir fallback com warning se serviço de avaliação indisponível
</requirements>

## Subtarefas

- [ ] 7.1 Verificar se existe instância axios para `vehicle-evaluation` em `lib/api.ts`
- [ ] 7.2 Criar instância axios para `vehicle-evaluation` se necessário
- [ ] 7.3 Criar método `validateEvaluationExists` em `vehicleService.ts`
- [ ] 7.4 Implementar método `createVehicleWithCheckIn` com orquestração
- [ ] 7.5 Adicionar tratamento de erro para avaliação não encontrada
- [ ] 7.6 Adicionar tratamento de fallback para serviço indisponível
- [ ] 7.7 Configurar timeout de 5 segundos na chamada de validação
- [ ] 7.8 Adicionar tipos TypeScript para request/response

## Sequenciamento

- **Bloqueado por**: 6.0 (DynamicVehicleForm)
- **Desbloqueia**: 8.0 (Rotas e Navegação)
- **Paralelizável**: Não — depende do formulário estar pronto

## Detalhes de Implementação

### Configuração da API de Avaliações

```typescript
// frontend/src/lib/api.ts (ou arquivo separado)
import axios from 'axios';
import { getAuthToken } from '@/auth/keycloak';

export const evaluationApi = axios.create({
  baseURL: import.meta.env.VITE_EVALUATION_API_URL || '/api/vehicle-evaluation',
  timeout: 5000, // 5 segundos
});

// Interceptor para adicionar token
evaluationApi.interceptors.request.use(async (config) => {
  const token = await getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

### Método de Orquestração

```typescript
// frontend/src/modules/stock/services/vehicleService.ts
import { stockApi, evaluationApi } from '@/lib/api';
import { VehicleCategory, CheckInSource } from '../types';

export interface CreateVehicleWithCheckInRequest {
  // Dados do veículo
  category: VehicleCategory;
  vin: string;
  make: string;
  model: string;
  yearModel: number;
  color: string;
  plate?: string;
  trim?: string;
  mileageKm?: number;
  evaluationId?: string;
  demoPurpose?: DemoPurpose;
  isRegistered?: boolean;
  // Dados do check-in
  source: CheckInSource;
  notes?: string;
}

export interface CreateVehicleWithCheckInResponse {
  vehicle: VehicleResponse;
  checkIn: CheckInResponse;
}

class VehicleService {
  async createVehicleWithCheckIn(
    request: CreateVehicleWithCheckInRequest
  ): Promise<CreateVehicleWithCheckInResponse> {
    // 1. Se seminovo, validar EvaluationId existe
    if (request.category === VehicleCategory.Used && request.evaluationId) {
      const evaluationExists = await this.validateEvaluationExists(request.evaluationId);
      if (!evaluationExists) {
        throw new ValidationError(
          'Avaliação não encontrada. Verifique o ID informado.',
          'evaluationId'
        );
      }
    }

    // 2. Criar veículo
    const vehicleData = {
      category: request.category,
      vin: request.vin,
      make: request.make,
      model: request.model,
      yearModel: request.yearModel,
      color: request.color,
      plate: request.plate,
      trim: request.trim,
      mileageKm: request.mileageKm,
      evaluationId: request.evaluationId,
      demoPurpose: request.demoPurpose,
      isRegistered: request.isRegistered ?? false,
    };

    const vehicleResponse = await stockApi.post<VehicleResponse>(
      '/api/v1/vehicles',
      vehicleData
    );
    const vehicle = vehicleResponse.data;

    // 3. Registrar check-in
    const checkInData = {
      source: request.source,
      occurredAt: new Date().toISOString(),
      notes: request.notes,
    };

    const checkInResponse = await stockApi.post<CheckInResponse>(
      `/api/v1/vehicles/${vehicle.id}/check-ins`,
      checkInData
    );
    const checkIn = checkInResponse.data;

    return { vehicle, checkIn };
  }

  private async validateEvaluationExists(evaluationId: string): Promise<boolean> {
    try {
      await evaluationApi.get(`/api/v1/evaluations/${evaluationId}`);
      return true;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 404) {
          return false;
        }
        // Serviço indisponível - log warning e permitir prosseguir
        if (error.code === 'ECONNABORTED' || !error.response) {
          console.warn(
            'Serviço de avaliação indisponível. Prosseguindo sem validação.',
            error.message
          );
          return true; // Fallback: permitir
        }
      }
      throw error;
    }
  }
}

export const vehicleService = new VehicleService();
```

### Classe de Erro de Validação

```typescript
// frontend/src/modules/stock/errors/ValidationError.ts
export class ValidationError extends Error {
  constructor(
    message: string,
    public field: string
  ) {
    super(message);
    this.name = 'ValidationError';
  }
}
```

### Fluxo de Orquestração

```
┌─────────────────────────────────────────────────────────────────┐
│                    createVehicleWithCheckIn                      │
│                                                                  │
│  ┌────────────────┐                                             │
│  │ É Seminovo?    │──Não──▶ Pular validação                    │
│  └───────┬────────┘                                             │
│          │ Sim                                                   │
│          ▼                                                       │
│  ┌────────────────┐                                             │
│  │ Validar        │──404──▶ throw ValidationError              │
│  │ EvaluationId   │                                             │
│  └───────┬────────┘──Timeout──▶ Log warning + continuar        │
│          │ OK                                                    │
│          ▼                                                       │
│  ┌────────────────┐                                             │
│  │ POST /vehicles │──Erro──▶ throw (propagar)                  │
│  └───────┬────────┘                                             │
│          │ 201 Created                                          │
│          ▼                                                       │
│  ┌────────────────┐                                             │
│  │ POST /{id}/    │──Erro──▶ throw (veículo órfão - log)       │
│  │ check-ins      │                                             │
│  └───────┬────────┘                                             │
│          │ 201 Created                                          │
│          ▼                                                       │
│  ┌────────────────┐                                             │
│  │ Return         │                                             │
│  │ { vehicle,     │                                             │
│  │   checkIn }    │                                             │
│  └────────────────┘                                             │
└─────────────────────────────────────────────────────────────────┘
```

## Critérios de Sucesso

- [ ] Método `createVehicleWithCheckIn` implementado e funcional
- [ ] Validação de `EvaluationId` funciona para seminovos
- [ ] Erro amigável retornado quando avaliação não existe
- [ ] Fallback funciona quando serviço de avaliação indisponível
- [ ] Timeout de 5 segundos configurado
- [ ] Tipos TypeScript definidos para todas as interfaces
- [ ] Erros de API são tratados e mensagens amigáveis retornadas
