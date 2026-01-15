using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Stock.Infra.EntityConfigurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    private static string StatusToDb(VehicleStatus status)
    {
        return status switch
        {
            VehicleStatus.InTransit => "em_transito",
            VehicleStatus.InStock => "em_estoque",
            VehicleStatus.Reserved => "reservado",
            VehicleStatus.InTestDrive => "em_test_drive",
            VehicleStatus.InPreparation => "em_preparacao",
            VehicleStatus.Sold => "vendido",
            VehicleStatus.WrittenOff => "baixado",
            _ => throw new InvalidOperationException("Status de veículo inválido")
        };
    }

    private static VehicleStatus DbToStatus(string value)
    {
        return value switch
        {
            "em_transito" => VehicleStatus.InTransit,
            "em_estoque" => VehicleStatus.InStock,
            "reservado" => VehicleStatus.Reserved,
            "em_test_drive" => VehicleStatus.InTestDrive,
            "em_preparacao" => VehicleStatus.InPreparation,
            "vendido" => VehicleStatus.Sold,
            "baixado" => VehicleStatus.WrittenOff,
            _ => throw new InvalidOperationException($"Status de veículo inválido: {value}")
        };
    }

    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.UseXminAsConcurrencyToken();

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        builder.Property(x => x.ResponsibleUserId)
            .HasColumnName("responsible_user_id")
            .IsRequired();

        builder.Property(x => x.PreviousStatus)
            .HasColumnName("previous_status")
            .HasConversion(v => StatusToDb(v), v => DbToStatus(v))
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.NewStatus)
            .HasColumnName("new_status")
            .HasConversion(v => StatusToDb(v), v => DbToStatus(v))
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("idx_audit_entries_vehicle");

        builder.HasIndex(x => x.OccurredAtUtc)
            .HasDatabaseName("idx_audit_entries_occurred_at");

        builder.Ignore(x => x.DomainEvents);
    }
}
