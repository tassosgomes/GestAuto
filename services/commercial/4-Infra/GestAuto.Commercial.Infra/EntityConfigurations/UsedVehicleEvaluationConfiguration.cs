using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;
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
            .IsRequired();

        builder.Property(x => x.MarketValue)
            .HasColumnName("market_value")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        builder.Property(x => x.TradeInValue)
            .HasColumnName("trade_in_value")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        builder.Property(x => x.EvaluationNotes)
            .HasColumnName("evaluation_notes")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.EvaluatedBy)
            .HasColumnName("evaluated_by")
            .IsRequired();

        builder.Property(x => x.EvaluatedAt)
            .HasColumnName("evaluated_at")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(x => x.ProposalId).HasDatabaseName("idx_evaluations_proposal");
        builder.HasIndex(x => x.LicensePlate).HasDatabaseName("idx_evaluations_license_plate");
        builder.HasIndex(x => x.EvaluatedAt).HasDatabaseName("idx_evaluations_date");
    }
}