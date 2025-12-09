using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class TestDriveConfiguration : IEntityTypeConfiguration<TestDrive>
{
    public void Configure(EntityTypeBuilder<TestDrive> builder)
    {
        builder.ToTable("test_drives");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LeadId)
            .HasColumnName("lead_id")
            .IsRequired();

        builder.Property(x => x.ProposalId)
            .HasColumnName("proposal_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ScheduledAt)
            .HasColumnName("scheduled_at")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(x => x.ScheduledBy)
            .HasColumnName("scheduled_by")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(x => x.LeadId).HasDatabaseName("idx_test_drives_lead");
        builder.HasIndex(x => x.ProposalId).HasDatabaseName("idx_test_drives_proposal");
        builder.HasIndex(x => x.ScheduledBy).HasDatabaseName("idx_test_drives_scheduled_by");
        builder.HasIndex(x => x.ScheduledAt).HasDatabaseName("idx_test_drives_scheduled");
    }
}