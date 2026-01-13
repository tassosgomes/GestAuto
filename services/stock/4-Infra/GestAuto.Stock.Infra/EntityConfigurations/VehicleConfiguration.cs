using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Stock.Infra.EntityConfigurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    private static string CategoryToDb(VehicleCategory category)
    {
        return category switch
        {
            VehicleCategory.New => "novo",
            VehicleCategory.Used => "seminovo",
            VehicleCategory.Demonstration => "demonstracao",
            _ => throw new InvalidOperationException("Categoria de veículo inválida")
        };
    }

    private static VehicleCategory DbToCategory(string value)
    {
        return value switch
        {
            "novo" => VehicleCategory.New,
            "seminovo" => VehicleCategory.Used,
            "demonstracao" => VehicleCategory.Demonstration,
            _ => throw new InvalidOperationException($"Categoria de veículo inválida: {value}")
        };
    }

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

    private static string? DemoPurposeToDb(DemoPurpose? purpose)
    {
        return purpose.HasValue
            ? purpose.Value switch
            {
                DemoPurpose.TestDrive => "test_drive",
                DemoPurpose.InternalFleet => "frota_interna",
                _ => throw new InvalidOperationException("Finalidade demo inválida")
            }
            : null;
    }

    private static DemoPurpose? DbToDemoPurpose(string? value)
    {
        return value is null
            ? null
            : value switch
            {
                "test_drive" => DemoPurpose.TestDrive,
                "frota_interna" => DemoPurpose.InternalFleet,
                _ => throw new InvalidOperationException($"Finalidade demo inválida: {value}")
            };
    }

    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.UseXminAsConcurrencyToken();

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion(v => CategoryToDb(v), v => DbToCategory(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CurrentStatus)
            .HasColumnName("status")
            .HasConversion(v => StatusToDb(v), v => DbToStatus(v))
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Vin)
            .HasColumnName("vin")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Plate)
            .HasColumnName("plate")
            .HasMaxLength(20);

        builder.Property(x => x.Make)
            .HasColumnName("make")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Trim)
            .HasColumnName("trim")
            .HasMaxLength(100);

        builder.Property(x => x.YearModel)
            .HasColumnName("year_model")
            .IsRequired();

        builder.Property(x => x.Color)
            .HasColumnName("color")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MileageKm)
            .HasColumnName("mileage_km");

        builder.Property(x => x.EvaluationId)
            .HasColumnName("evaluation_id");

        builder.Property(x => x.DemoPurpose)
            .HasColumnName("demo_purpose")
            .HasConversion(v => DemoPurposeToDb(v), v => DbToDemoPurpose(v));

        builder.Property(x => x.IsRegistered)
            .HasColumnName("is_registered")
            .IsRequired();

        builder.Property(x => x.CurrentOwnerUserId)
            .HasColumnName("current_owner_user_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(x => x.CheckIns)
            .WithOne()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CheckOuts)
            .WithOne()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TestDrives)
            .WithOne()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.CheckIns)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.CheckOuts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.TestDrives)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Vin)
            .IsUnique()
            .HasDatabaseName("ux_vehicles_vin");

        builder.HasIndex(x => x.Plate)
            .IsUnique()
            .HasDatabaseName("ux_vehicles_plate")
            .HasFilter("plate IS NOT NULL AND status NOT IN ('vendido', 'baixado')");

        builder.HasIndex(x => x.CurrentStatus)
            .HasDatabaseName("idx_vehicles_status");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("idx_vehicles_category");

        builder.Ignore(x => x.DomainEvents);
    }
}
