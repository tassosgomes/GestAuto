using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.ValueObjectConverters;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // Value Object - Email
        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .HasConversion(new EmailConverter())
            .IsRequired();

        // Value Object - Phone
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .HasConversion(new PhoneConverter())
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnName("score")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.SalesPersonId)
            .HasColumnName("sales_person_id")
            .IsRequired();

        builder.Property(x => x.InterestedModel)
            .HasColumnName("interested_model")
            .HasMaxLength(100);

        builder.Property(x => x.InterestedTrim)
            .HasColumnName("interested_trim")
            .HasMaxLength(100);

        builder.Property(x => x.InterestedColor)
            .HasColumnName("interested_color")
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Ignore complex value objects for now
        builder.Ignore(x => x.Qualification);

        // Relacionamento com Interactions (1:N)
        builder.HasMany(x => x.Interactions)
            .WithOne()
            .HasForeignKey("lead_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.SalesPersonId).HasDatabaseName("idx_leads_sales_person");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_leads_status");
        builder.HasIndex(x => x.Score).HasDatabaseName("idx_leads_score");
    }
}