using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> builder)
    {
        builder.ToTable("interactions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LeadId)
            .HasColumnName("lead_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.InteractionDate)
            .HasColumnName("interaction_date")
            .IsRequired();

        builder.Property(x => x.Result)
            .HasColumnName("result")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(x => x.LeadId).HasDatabaseName("idx_interactions_lead");
        builder.HasIndex(x => x.Type).HasDatabaseName("idx_interactions_type");
        builder.HasIndex(x => x.InteractionDate).HasDatabaseName("idx_interactions_date");
    }
}