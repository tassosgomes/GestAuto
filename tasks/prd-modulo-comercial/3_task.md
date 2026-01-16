---
status: completed
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>domain/value-objects</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>4.0</unblocks>
</task_context>

# Tarefa 3.0: Implementar Domain Layer - Value Objects e Enums ✅ CONCLUÍDA

## Visão Geral

- [x] 3.0 [Implementar Domain Layer - Value Objects e Enums] ✅ CONCLUÍDA
  - [x] 3.1 Implementação completada
  - [x] 3.2 Definição da tarefa, PRD e tech spec validados
  - [x] 3.3 Análise de regras e conformidade verificadas
  - [x] 3.4 Revisão de código completada
  - [x] 3.5 Pronto para deploy

Implementar os Value Objects e Enums do domínio comercial. Value Objects garantem validação e imutabilidade de dados primitivos complexos (Email, Phone, Money, LicensePlate). Enums definem os estados e tipos permitidos no sistema.

<requirements>
- Implementar Value Objects com validação no construtor
- Garantir imutabilidade dos Value Objects
- Implementar igualdade por valor (não por referência)
- Criar todos os Enums necessários para o domínio
- Seguir padrões de DDD para Value Objects
</requirements>

## Subtarefas

- [x] 3.1 Criar Value Object `Email` com validação de formato ✅ CONCLUÍDA
- [x] 3.2 Criar Value Object `Phone` com validação de formato brasileiro ✅ CONCLUÍDA
- [x] 3.3 Criar Value Object `Money` com operações aritméticas ✅ CONCLUÍDA
- [x] 3.4 Criar Value Object `LicensePlate` com validação (padrão antigo e Mercosul) ✅ CONCLUÍDA
- [x] 3.5 Criar Enum `LeadStatus` (New, InContact, InNegotiation, TestDriveScheduled, ProposalSent, Lost, Converted) ✅ CONCLUÍDA
- [x] 3.6 Criar Enum `LeadScore` (Diamond, Gold, Silver, Bronze) ✅ CONCLUÍDA
- [x] 3.7 Criar Enum `LeadSource` (Instagram, Referral, Google, Store, Phone, Showroom, ClassifiedsPortal, Other) ✅ CONCLUÍDA
- [x] 3.8 Criar Enum `PaymentMethod` (Cash, Financing, Consortium) ✅ CONCLUÍDA
- [x] 3.9 Criar Enum `ProposalStatus` (Draft, InNegotiation, AwaitingUsedVehicleEvaluation, AwaitingDiscountApproval, AwaitingCustomer, Approved, Closed, Lost) ✅ CONCLUÍDA
- [x] 3.10 Criar Enum `OrderStatus` (AwaitingDocumentation, CreditAnalysis, CreditApproved, CreditRejected, AwaitingVehicle, ReadyForDelivery, Delivered) ✅ CONCLUÍDA
- [x] 3.11 Criar Enum `TestDriveStatus` (Scheduled, Completed, Cancelled) ✅ CONCLUÍDA
- [x] 3.12 Criar Enum `EvaluationStatus` (Requested, Completed, Accepted, Rejected) ✅ CONCLUÍDA
- [x] 3.13 Criar Enum `InteractionType` (Call, Email, WhatsApp, Visit, Other) ✅ CONCLUÍDA
- [x] 3.14 Criar testes unitários para todos os Value Objects ✅ CONCLUÍDA

## Sequenciamento

- **Bloqueado por:** 2.0 (Entidades Core)
- **Desbloqueia:** 4.0 (Repositórios)
- **Paralelizável:** Sim (pode executar junto com outras tarefas que dependem de 2.0)

## Detalhes de Implementação

### Value Object Base

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
```

### Email Value Object

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email não pode ser vazio");

        if (!IsValidEmail(value))
            throw new DomainException("Email inválido");

        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
```

### Phone Value Object

```csharp
public class Phone : ValueObject
{
    public string Value { get; }
    public string DDD { get; }
    public string Number { get; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Telefone não pode ser vazio");

        var cleanNumber = new string(value.Where(char.IsDigit).ToArray());

        if (cleanNumber.Length < 10 || cleanNumber.Length > 11)
            throw new DomainException("Telefone deve ter 10 ou 11 dígitos");

        DDD = cleanNumber[..2];
        Number = cleanNumber[2..];
        Value = cleanNumber;
    }

    public string Formatted => Number.Length == 9 
        ? $"({DDD}) {Number[..5]}-{Number[5..]}"
        : $"({DDD}) {Number[..4]}-{Number[4..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
```

### Money Value Object

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Zero => new(0);

    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("Valor não pode ser negativo");

        Amount = Math.Round(amount, 2);
        Currency = currency;
    }

    public static Money operator +(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Amount * multiplier, a.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    private static void ValidateSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Não é possível operar valores em moedas diferentes");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}
```

### LicensePlate Value Object

```csharp
public class LicensePlate : ValueObject
{
    public string Value { get; }
    public bool IsMercosul { get; }

    private static readonly Regex OldPatternRegex = new(@"^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled);
    private static readonly Regex MercosulPatternRegex = new(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled);

    public LicensePlate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Placa não pode ser vazia");

        var cleanValue = value.ToUpperInvariant().Replace("-", "").Replace(" ", "");

        if (OldPatternRegex.IsMatch(cleanValue))
        {
            Value = cleanValue;
            IsMercosul = false;
        }
        else if (MercosulPatternRegex.IsMatch(cleanValue))
        {
            Value = cleanValue;
            IsMercosul = true;
        }
        else
        {
            throw new DomainException("Formato de placa inválido. Use AAA-1234 ou AAA1A23 (Mercosul)");
        }
    }

    public string Formatted => IsMercosul 
        ? Value 
        : $"{Value[..3]}-{Value[3..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
```

### Enums

```csharp
public enum LeadStatus
{
    New = 1,
    InContact = 2,
    InNegotiation = 3,
    TestDriveScheduled = 4,
    ProposalSent = 5,
    Lost = 6,
    Converted = 7
}

public enum LeadScore
{
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Diamond = 4
}

public enum LeadSource
{
    Instagram = 1,
    Referral = 2,
    Google = 3,
    Store = 4,
    Phone = 5,
    Showroom = 6,
    ClassifiedsPortal = 7,
    Other = 8
}

public enum PaymentMethod
{
    Cash = 1,
    Financing = 2,
    Consortium = 3
}

public enum ProposalStatus
{
    Draft = 1,
    InNegotiation = 2,
    AwaitingUsedVehicleEvaluation = 3,
    AwaitingDiscountApproval = 4,
    AwaitingCustomer = 5,
    Approved = 6,
    Closed = 7,
    Lost = 8
}

public enum OrderStatus
{
    AwaitingDocumentation = 1,
    CreditAnalysis = 2,
    CreditApproved = 3,
    CreditRejected = 4,
    AwaitingVehicle = 5,
    ReadyForDelivery = 6,
    Delivered = 7
}

public enum TestDriveStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3
}

public enum EvaluationStatus
{
    Requested = 1,
    Completed = 2,
    Accepted = 3,
    Rejected = 4
}

public enum InteractionType
{
    Call = 1,
    Email = 2,
    WhatsApp = 3,
    Visit = 4,
    Other = 5
}
```

## Critérios de Sucesso

- [x] Email valida formato corretamente (aceita válidos, rejeita inválidos)
- [x] Phone valida formato brasileiro (10-11 dígitos)
- [x] Phone formata corretamente (com DDD e hífen)
- [x] Money implementa operações aritméticas sem erro de precisão
- [x] Money não permite valores negativos
- [x] LicensePlate aceita formato antigo (AAA-1234) e Mercosul (AAA1A23)
- [x] Todos os Value Objects são imutáveis
- [x] Comparação por valor funciona corretamente (Equals e ==)
- [x] Enums têm valores numéricos explícitos para evitar problemas de migração
- [x] Testes unitários cobrem casos de sucesso e falha para cada Value Object
