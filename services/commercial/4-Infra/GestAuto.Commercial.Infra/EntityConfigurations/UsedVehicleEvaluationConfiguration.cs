using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Infra.ValueObjectConverters;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class UsedVehicleEvaluationConfiguration : IEntityTypeConfiguration<UsedVehicleEvaluation>
{
    public void Configure(EntityTypeBuilder<UsedVehicleEvaluation> builder)
    {
        builder.ToTable("used_vehicle_evaluations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProposalId)
            .HasColumnName("proposal_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.EvaluatedValue)
            .HasColumnName("evaluated_value")
            .HasConversion(new MoneyConverter())
            .IsRequired(false);

        builder.Property(x => x.EvaluationNotes)
            .HasColumnName("evaluation_notes")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.RequestedAt)
            .HasColumnName("requested_at")
            .IsRequired();

        builder.Property(x => x.RespondedAt)
            .HasColumnName("responded_at")
            .IsRequired(false);

        builder.Property(x => x.CustomerAccepted)
            .HasColumnName("customer_accepted")
            .IsRequired(false);

        builder.Property(x => x.CustomerRejectionReason)
            .HasColumnName("customer_rejection_reason")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.RequestedBy)
            .HasColumnName("requested_by")
            .IsRequired();

        // Configurar o Value Object Vehicle como propriedade complexa
        builder.OwnsOne(x => x.Vehicle, vehicleBuilder =>
        {
            vehicleBuilder.Property(v => v.Brand)
                .HasColumnName("vehicle_brand")
                .HasMaxLength(50)
                .IsRequired();

            vehicleBuilder.Property(v => v.Model)
                .HasColumnName("vehicle_model")
                .HasMaxLength(100)
                .IsRequired();

            vehicleBuilder.Property(v => v.Year)
                .HasColumnName("vehicle_year")
                .IsRequired();

            vehicleBuilder.Property(v => v.Mileage)
                .HasColumnName("vehicle_mileage")
                .IsRequired();

            vehicleBuilder.Property(v => v.LicensePlate)
                .HasColumnName("vehicle_license_plate")
                .HasConversion(new LicensePlateConverter())
                .IsRequired();

            vehicleBuilder.Property(v => v.Color)
                .HasColumnName("vehicle_color")
                .HasMaxLength(50)
                .IsRequired();

            vehicleBuilder.Property(v => v.GeneralCondition)
                .HasColumnName("vehicle_general_condition")
                .HasMaxLength(200)
                .IsRequired();

            vehicleBuilder.Property(v => v.HasDealershipServiceHistory)
                .HasColumnName("vehicle_has_dealership_service_history")
                .IsRequired();
        });

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(x => x.ProposalId).HasDatabaseName("idx_evaluations_proposal");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_evaluations_status");
        builder.HasIndex(x => x.RequestedAt).HasDatabaseName("idx_evaluations_requested_at");
    }
}