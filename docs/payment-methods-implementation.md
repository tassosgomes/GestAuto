# Implementação: Formas de Pagamento Dinâmicas

## Problema Identificado

**Erro:** HTTP 500 ao tentar salvar uma qualificação de lead com forma de pagamento "Leasing"

**Causa Raiz:**
- O enum `PaymentMethod` no backend continha apenas 3 valores: `Cash`, `Financing`, `Consortium`
- O frontend enviava 4 valores, incluindo `LEASING`
- Ao tentar desserializar `LEASING`, o backend lançava exceção: `Requested value 'LEASING' was not found`

**Situação Anterior:**
- Formas de pagamento **hardcoded** no enum backend (3 valores)
- Opções **hardcoded** no frontend (4 valores)
- Inconsistência causando erro 500

---

## Solução Implementada

### 1. Fix Imediato (✅ Concluído)

Adicionado valor `Leasing = 4` ao enum `PaymentMethod.cs`:

```csharp
public enum PaymentMethod
{
    Cash = 1,
    Financing = 2,
    Consortium = 3,
    Leasing = 4  // ← Novo
}
```

**Resultado:** Erro 500 corrigido. Qualificação com Leasing agora funciona perfeitamente.

---

### 2. Infraestrutura para Gestão Dinâmica (✅ Concluído)

#### 2.1 Entidade de Domínio

Criada `PaymentMethodEntity.cs` em `3-Domain/Entities/`:
- `Id` (int) - Chave primária
- `Code` (string, unique) - Código técnico (ex: CASH, FINANCING)
- `Name` (string) - Nome para exibição (ex: "À Vista", "Financiamento")
- `IsActive` (bool) - Controle de ativação/desativação
- `DisplayOrder` (int) - Ordem de exibição
- `CreatedAt`, `UpdatedAt` - Auditoria

#### 2.2 Tabela no Banco de Dados

Migration `20251229205419_AddPaymentMethodsTable` criada e aplicada:

```sql
CREATE TABLE commercial.payment_methods (
    id INT PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    display_order INT NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Dados iniciais (seed)
INSERT INTO commercial.payment_methods VALUES
(1, 'CASH', 'À Vista', true, 1, '2025-12-29', '2025-12-29'),
(2, 'FINANCING', 'Financiamento', true, 2, '2025-12-29', '2025-12-29'),
(3, 'CONSORTIUM', 'Consórcio', true, 3, '2025-12-29', '2025-12-29'),
(4, 'LEASING', 'Leasing', true, 4, '2025-12-29', '2025-12-29');
```

#### 2.3 API Endpoint

Controller `PaymentMethodsController.cs` criado em `1-Services/API/Controllers/`:

**Endpoints:**
- `GET /api/PaymentMethods` - Lista todas as formas de pagamento ativas (ordenadas por `display_order`)
- `GET /api/PaymentMethods/{code}` - Obtém forma de pagamento específica por código

**Response:**
```json
[
  {
    "id": 1,
    "code": "CASH",
    "name": "À Vista",
    "isActive": true,
    "displayOrder": 1
  },
  {
    "id": 2,
    "code": "FINANCING",
    "name": "Financiamento",
    "isActive": true,
    "displayOrder": 2
  },
  ...
]
```

---

## Próximos Passos

### 3. Integração Frontend (⏳ Pendente)

**Objetivo:** Buscar opções de pagamento da API ao invés de usar valores hardcoded

**Arquivos a modificar:**
- `frontend/src/modules/commercial/components/LeadQualificationForm.tsx`
- `frontend/src/modules/commercial/components/proposal/PaymentForm.tsx`
- `frontend/src/modules/commercial/components/LeadOverviewTab.tsx` (labels)

**Implementação:**
```typescript
// Criar hook customizado
const usePaymentMethods = () => {
  return useQuery({
    queryKey: ['payment-methods'],
    queryFn: async () => {
      const response = await api.get('/PaymentMethods');
      return response.data;
    },
    staleTime: 5 * 60 * 1000 // Cache por 5 minutos
  });
};

// Usar no formulário
const { data: paymentMethods, isLoading } = usePaymentMethods();

// Renderizar options dinamicamente
{paymentMethods?.map(pm => (
  <SelectItem key={pm.code} value={pm.code}>
    {pm.name}
  </SelectItem>
))}
```

**Benefícios:**
- ✅ Opções carregadas dinamicamente do banco
- ✅ Possibilidade de adicionar novas formas sem redeploygit- ✅ Controle de ativação/desativação
- ✅ Ordem de exibição configurável

---

### 4. Tela Administrativa (🔮 Futuro)

**Objetivo:** Permitir que usuários administrativos gerenciem formas de pagamento via UI

**Funcionalidades:**
- Listar todas as formas de pagamento (ativas e inativas)
- Criar nova forma de pagamento
- Editar nome e ordem de exibição
- Ativar/desativar formas de pagamento
- Proteção: não permitir excluir formas em uso

**Endpoints adicionais necessários:**
```
POST   /api/PaymentMethods
PUT    /api/PaymentMethods/{id}
DELETE /api/PaymentMethods/{id}
PATCH  /api/PaymentMethods/{id}/activate
PATCH  /api/PaymentMethods/{id}/deactivate
```

**Localização sugerida:**
- Rota: `/admin/payment-methods`
- Componente: `frontend/src/pages/admin/PaymentMethodsAdminPage.tsx`
- RBAC: Requer role `admin` ou `system_admin`

---

## Arquivos Modificados/Criados

### Backend (.NET)

#### Modificados:
- ✅ `services/commercial/3-Domain/.../Enums/PaymentMethod.cs` - Adicionado `Leasing = 4`
- ✅ `services/commercial/4-Infra/.../CommercialDbContext.cs` - Adicionado `DbSet<PaymentMethodEntity>`

#### Criados:
- ✅ `services/commercial/3-Domain/.../Entities/PaymentMethodEntity.cs`
- ✅ `services/commercial/4-Infra/.../EntityConfigurations/PaymentMethodConfiguration.cs`
- ✅ `services/commercial/4-Infra/.../Migrations/20251229205419_AddPaymentMethodsTable.cs`
- ✅ `services/commercial/2-Application/.../DTOs/PaymentMethodDTOs.cs`
- ✅ `services/commercial/1-Services/.../Controllers/PaymentMethodsController.cs`

### Frontend (React/TypeScript)

#### A fazer:
- ⏳ `frontend/src/modules/commercial/hooks/usePaymentMethods.ts` (novo)
- ⏳ `frontend/src/modules/commercial/components/LeadQualificationForm.tsx` (modificar)
- ⏳ `frontend/src/modules/commercial/components/proposal/PaymentForm.tsx` (modificar)
- ⏳ `frontend/src/modules/commercial/components/LeadOverviewTab.tsx` (modificar labels)

---

## Validação

### Teste Realizado ✅

1. Navegado para lead ID `63843024-a8d4-4016-b500-28170a77c3b4`
2. Acessado aba "Qualificação"
3. Selecionado "Leasing" como forma de pagamento
4. Clicado em "Salvar Qualificação"
5. **Resultado:** ✅ Sucesso!
   - Sem erro 500
   - Navegação automática para "Visão Geral"
   - Toast de sucesso exibido
   - Forma de pagamento "Leasing" salva corretamente
   - Exibição correta na aba Overview

---

## Conclusão

**Situação Atual:**
- ✅ Erro 500 com Leasing **corrigido**
- ✅ Infraestrutura de tabela **criada**
- ✅ API endpoint **implementado**
- ⏳ Frontend ainda usa valores hardcoded (funcional, mas não dinâmico)
- 🔮 Tela administrativa planejada para o futuro

**Recomendações:**
1. Implementar integração frontend com API (prioridade média)
2. Criar tela administrativa quando necessário (baixa prioridade)
3. Documentar processo de adição manual de novas formas de pagamento via SQL (caso não haja tela admin)

**Comandos úteis:**
```bash
# Rebuild backend após mudanças
cd /home/tsgomes/github-tassosgomes/GestAuto
docker compose up -d --build commercial-api

# Testar endpoint
curl -k -H "Authorization: Bearer $(cat token.txt)" \
  https://gestauto.tasso.local/api/PaymentMethods | jq .
```

---

Data: 29 de dezembro de 2025  
Autor: GitHub Copilot
