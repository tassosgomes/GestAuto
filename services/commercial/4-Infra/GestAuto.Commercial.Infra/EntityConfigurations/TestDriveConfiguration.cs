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

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
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

        builder.Property(x => x.SalesPersonId)
            .HasColumnName("sales_person_id")
            .IsRequired();

        // Checklist configuration
        builder.OwnsOne(x => x.Checklist, checklist =>
        {
            checklist.Property(c => c.InitialMileage)
                .HasColumnName("checklist_initial_mileage");

            checklist.Property(c => c.FinalMileage)
                .HasColumnName("checklist_final_mileage");

            checklist.Property(c => c.FuelLevel)
                .HasColumnName("checklist_fuel_level")
                .HasConversion<string>();

            checklist.Property(c => c.VisualObservations)
                .HasColumnName("checklist_visual_observations")
                .HasMaxLength(1000)
                .IsRequired(false);
        });

        builder.Navigation(x => x.Checklist).IsRequired(false);

        builder.Property(x => x.CustomerFeedback)
            .HasColumnName("customer_feedback")
            .HasMaxLength(1000);

        builder.Property(x => x.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(x => x.LeadId).HasDatabaseName("idx_test_drives_lead");
        builder.HasIndex(x => x.VehicleId).HasDatabaseName("idx_test_drives_vehicle");
        builder.HasIndex(x => x.SalesPersonId).HasDatabaseName("idx_test_drives_sales_person");
        builder.HasIndex(x => x.ScheduledAt).HasDatabaseName("idx_test_drives_scheduled");
    }
}