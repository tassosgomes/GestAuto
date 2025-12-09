using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Infra.ValueObjectConverters;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class UsedVehicleConfiguration : IEntityTypeConfiguration<UsedVehicle>
{
    public void Configure(EntityTypeBuilder<UsedVehicle> builder)
    {
        builder.ToTable("used_vehicles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Brand)
            .HasColumnName("brand")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(x => x.Mileage)
            .HasColumnName("mileage")
            .IsRequired();

        builder.Property(x => x.LicensePlate)
            .HasColumnName("license_plate")
            .HasMaxLength(10)
            .HasConversion(new LicensePlateConverter())
            .IsRequired();

        builder.Property(x => x.Condition)
            .HasColumnName("condition")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.HasServiceHistory)
            .HasColumnName("has_service_history")
            .IsRequired();

        builder.Property(x => x.EstimatedValue)
            .HasColumnName("estimated_value")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        builder.Property(x => x.LeadId)
            .HasColumnName("lead_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Índices
        builder.HasIndex(x => x.LicensePlate).HasDatabaseName("idx_used_vehicles_license_plate").IsUnique();
        builder.HasIndex(x => new { x.Brand, x.Model, x.Year }).HasDatabaseName("idx_used_vehicles_brand_model_year");
        builder.HasIndex(x => x.LeadId).HasDatabaseName("idx_used_vehicles_lead");
    }
}