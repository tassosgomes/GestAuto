using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Infra.ValueObjectConverters;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class ProposalItemConfiguration : IEntityTypeConfiguration<ProposalItem>
{
    public void Configure(EntityTypeBuilder<ProposalItem> builder)
    {
        builder.ToTable("proposal_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        builder.Property(x => x.IsOptional)
            .HasColumnName("is_optional")
            .IsRequired();

        // Índices
        builder.HasIndex("proposal_id").HasDatabaseName("idx_proposal_items_proposal");
    }
}