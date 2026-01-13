using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Stock.Infra.EntityConfigurations;

public class CheckOutRecordConfiguration : IEntityTypeConfiguration<CheckOutRecord>
{
    private static string ReasonToDb(CheckOutReason reason)
    {
        return reason switch
        {
            CheckOutReason.Sale => "venda",
            CheckOutReason.TestDrive => "test_drive",
            CheckOutReason.Transfer => "transferencia",
            CheckOutReason.TotalLoss => "baixa_sinistro_perda_total",
            _ => throw new InvalidOperationException("Motivo de check-out inválido")
        };
    }

    private static CheckOutReason DbToReason(string value)
    {
        return value switch
        {
            "venda" => CheckOutReason.Sale,
            "test_drive" => CheckOutReason.TestDrive,
            "transferencia" => CheckOutReason.Transfer,
            "baixa_sinistro_perda_total" => CheckOutReason.TotalLoss,
            _ => throw new InvalidOperationException($"Motivo de check-out inválido: {value}")
        };
    }

    public void Configure(EntityTypeBuilder<CheckOutRecord> builder)
    {
        builder.ToTable("check_outs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasConversion(v => ReasonToDb(v), v => DbToReason(v))
            .HasMaxLength(60)
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
            .HasDatabaseName("idx_check_outs_vehicle");

        builder.Ignore(x => x.DomainEvents);
    }
}
