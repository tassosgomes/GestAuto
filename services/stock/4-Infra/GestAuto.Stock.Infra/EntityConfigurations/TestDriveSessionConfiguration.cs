using GestAuto.Stock.Domain.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Stock.Infra.EntityConfigurations;

public class TestDriveSessionConfiguration : IEntityTypeConfiguration<TestDriveSession>
{
    private static string? OutcomeToDb(TestDriveOutcome? outcome)
    {
        return outcome.HasValue
            ? outcome.Value switch
            {
                TestDriveOutcome.ReturnedToStock => "returned_to_stock",
                TestDriveOutcome.ConvertedToReservation => "converted_to_reservation",
                _ => throw new InvalidOperationException("Outcome de test-drive inválido")
            }
            : null;
    }

    private static TestDriveOutcome? DbToOutcome(string? value)
    {
        return value is null
            ? null
            : value switch
            {
                "returned_to_stock" => TestDriveOutcome.ReturnedToStock,
                "converted_to_reservation" => TestDriveOutcome.ConvertedToReservation,
                _ => throw new InvalidOperationException($"Outcome de test-drive inválido: {value}")
            };
    }

    public void Configure(EntityTypeBuilder<TestDriveSession> builder)
    {
        builder.ToTable("test_drives");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.UseXminAsConcurrencyToken();

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(x => x.SalesPersonId)
            .HasColumnName("sales_person_id")
            .IsRequired();

        builder.Property(x => x.CustomerRef)
            .HasColumnName("customer_ref")
            .HasMaxLength(200);

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(x => x.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(x => x.Outcome)
            .HasColumnName("outcome")
            .HasConversion(v => OutcomeToDb(v), v => DbToOutcome(v))
            .HasMaxLength(40);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("idx_test_drives_vehicle");

        builder.Ignore(x => x.DomainEvents);
    }
}
