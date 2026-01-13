using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Stock.Infra.EntityConfigurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    private static string TypeToDb(ReservationType type)
    {
        return type switch
        {
            ReservationType.Standard => "padrao",
            ReservationType.PaidDeposit => "entrada_paga",
            ReservationType.WaitingBank => "aguardando_banco",
            _ => throw new InvalidOperationException("Tipo de reserva inválido")
        };
    }

    private static ReservationType DbToType(string value)
    {
        return value switch
        {
            "padrao" => ReservationType.Standard,
            "entrada_paga" => ReservationType.PaidDeposit,
            "aguardando_banco" => ReservationType.WaitingBank,
            _ => throw new InvalidOperationException($"Tipo de reserva inválido: {value}")
        };
    }

    private static string StatusToDb(ReservationStatus status)
    {
        return status switch
        {
            ReservationStatus.Active => "ativa",
            ReservationStatus.Cancelled => "cancelada",
            ReservationStatus.Completed => "concluida",
            ReservationStatus.Expired => "expirada",
            _ => throw new InvalidOperationException("Status de reserva inválido")
        };
    }

    private static ReservationStatus DbToStatus(string value)
    {
        return value switch
        {
            "ativa" => ReservationStatus.Active,
            "cancelada" => ReservationStatus.Cancelled,
            "concluida" => ReservationStatus.Completed,
            "expirada" => ReservationStatus.Expired,
            _ => throw new InvalidOperationException($"Status de reserva inválido: {value}")
        };
    }

    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.UseXminAsConcurrencyToken();

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion(v => TypeToDb(v), v => DbToType(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => StatusToDb(v), v => DbToStatus(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SalesPersonId)
            .HasColumnName("sales_person_id")
            .IsRequired();

        builder.Property(x => x.ContextType)
            .HasColumnName("context_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ContextId)
            .HasColumnName("context_id");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .HasColumnName("expires_at_utc");

        builder.Property(x => x.BankDeadlineAtUtc)
            .HasColumnName("bank_deadline_at_utc");

        builder.Property(x => x.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc");

        builder.Property(x => x.CancelledByUserId)
            .HasColumnName("cancelled_by_user_id");

        builder.Property(x => x.CancelReason)
            .HasColumnName("cancel_reason")
            .HasMaxLength(500);

        builder.Property(x => x.ExtendedAtUtc)
            .HasColumnName("extended_at_utc");

        builder.Property(x => x.ExtendedByUserId)
            .HasColumnName("extended_by_user_id");

        builder.Property(x => x.PreviousExpiresAtUtc)
            .HasColumnName("previous_expires_at_utc");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("idx_reservations_vehicle");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_reservations_status");

        // Constraint: reserva ativa única por veículo (Postgres partial unique index)
        builder.HasIndex(x => x.VehicleId)
            .IsUnique()
            .HasDatabaseName("ux_reservations_vehicle_active")
            .HasFilter("status = 'ativa'");

        builder.Ignore(x => x.DomainEvents);
    }
}
