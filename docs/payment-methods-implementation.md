# Guia de Implementação de Métodos de Pagamento

## Visão Geral
Este documento descreve a implementação do suporte a múltiplos métodos de pagamento no módulo Financeiro/Vendas da GestAuto. O sistema suporta pagamentos via cartão de crédito, débito, boleto, PIX e financiamento.

## Estrutura de Dados

### Enum `PaymentMethodType`

Localizado em: `src/Domain/Enums/PaymentMethodType.cs`

```csharp
public enum PaymentMethodType
{
    CreditCard = 1,
    DebitCard = 2,
    Boleto = 3,
    Pix = 4,
    Financing = 5,
    Cash = 6,
    TradeIn = 7 // Veículo como entrada
}
```

### Entidade `PaymentMethod`

A entidade base para métodos de pagamento. Armazena configurações globais.

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| Name | String | Nome de exibição (ex: "Cartão Visa") |
| Type | PaymentMethodType | O tipo do método |
| IsActive | Boolean | Se o método está habilitado |
| FeePercentage | Decimal | Taxa administrativa cobrada (ex: 3.5%) |
| HandlerService | String | Nome do serviço que processa este pagamento |

## Implementação no Frontend

O frontend deve renderizar formulários dinâmicos baseados no `PaymentMethodType` selecionado.

### Mapeamento de Componentes (React)

```tsx
const paymentComponents = {
  [PaymentMethodType.CreditCard]: CreditCardForm,
  [PaymentMethodType.DebitCard]: DebitCardForm,
  [PaymentMethodType.Boleto]: BoletoForm,
  [PaymentMethodType.Pix]: PixGenerator,
  [PaymentMethodType.Financing]: FinancingSimulator,
};
```

### Validações Específicas

1. **Cartão de Crédito/Débito**:
   - Algoritmo de Luhn para número do cartão.
   - Data de validade futura.
   - CVV com 3 ou 4 dígitos.

2. **PIX**:
   - Geração de QR Code dinâmico.
   - Verificação de status de pagamento em tempo real (WebSocket ou Polling).

3. **Financiamento**:
   - Integração com API de bancos parceiros para simulação.
   - Upload de documentos obrigatórios.

## Fluxo de Processamento (Backend)

O padrão Strategy é utilizado para selecionar o processador correto.

1. `PaymentProcessorFactory` recebe o tipo de pagamento.
2. Retorna a implementação correta de `IPaymentProcessor`.
3. O método `ProcessPaymentAsync` é executado.

### Exemplo de Interface

```csharp
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<bool> ValidateAsync(PaymentRequest request);
}
```

## Considerações de Segurança

- Dados sensíveis de cartão **NUNCA** devem ser armazenados no banco de dados local. Use tokenização do gateway de pagamento.
- Todas as transações devem ser registradas em log de auditoria seguro.
- Comunicação com gateways externos deve usar TLS 1.2+.

## Próximos Passos

- [ ] Implementar integração com Gateway Stripe.
- [ ] Criar testes unitários para cada estratégia de pagamento.
- [ ] Desenvolver UI para administração de taxas por método.
