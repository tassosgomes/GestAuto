using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Stock.Infra.EntityConfigurations;

public class CheckInRecordConfiguration : IEntityTypeConfiguration<CheckInRecord>
{
    private static string SourceToDb(CheckInSource source)
    {
        return source switch
        {
            CheckInSource.Manufacturer => "montadora",
            CheckInSource.CustomerUsedPurchase => "compra_cliente_seminovo",
            CheckInSource.StoreTransfer => "transferencia_entre_lojas",
            CheckInSource.InternalFleet => "frota_interna",
            _ => throw new InvalidOperationException("Origem de check-in inválida")
        };
    }

    private static CheckInSource DbToSource(string value)
    {
        return value switch
        {
            "montadora" => CheckInSource.Manufacturer,
            "compra_cliente_seminovo" => CheckInSource.CustomerUsedPurchase,
            "transferencia_entre_lojas" => CheckInSource.StoreTransfer,
            "frota_interna" => CheckInSource.InternalFleet,
            _ => throw new InvalidOperationException($"Origem de check-in inválida: {value}")
        };
    }

    public void Configure(EntityTypeBuilder<CheckInRecord> builder)
    {
        builder.ToTable("check_ins");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasConversion(v => SourceToDb(v), v => DbToSource(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(x => x.ResponsibleUserId)
            .HasColumnName("responsible_user_id")
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("idx_check_ins_vehicle");

        builder.Ignore(x => x.DomainEvents);
    }
}
