using GestAuto.Commercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethodEntity>
{
    public void Configure(EntityTypeBuilder<PaymentMethodEntity> builder)
    {
        builder.ToTable("payment_methods", "commercial");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Valor manual (1, 2, 3, 4)

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("ix_payment_methods_code");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Seed inicial dos dados
        builder.HasData(
            new
            {
                Id = 1,
                Code = "CASH",
                Name = "À Vista",
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = 2,
                Code = "FINANCING",
                Name = "Financiamento",
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = 3,
                Code = "CONSORTIUM",
                Name = "Consórcio",
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = 4,
                Code = "LEASING",
                Name = "Leasing",
                IsActive = true,
                DisplayOrder = 4,
                CreatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
