using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.ValueObjectConverters;

namespace GestAuto.Commercial.Infra.EntityConfigurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("proposals");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LeadId)
            .HasColumnName("lead_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        // Vehicle properties
        builder.Property(x => x.VehicleModel)
            .HasColumnName("vehicle_model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.VehicleTrim)
            .HasColumnName("vehicle_trim")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.VehicleColor)
            .HasColumnName("vehicle_color")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.VehicleYear)
            .HasColumnName("vehicle_year")
            .IsRequired();

        builder.Property(x => x.IsReadyDelivery)
            .HasColumnName("is_ready_delivery")
            .IsRequired();

        // Value properties
        builder.Property(x => x.VehiclePrice)
            .HasColumnName("vehicle_price")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        builder.Property(x => x.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        builder.Property(x => x.DiscountReason)
            .HasColumnName("discount_reason")
            .HasMaxLength(500);

        builder.Property(x => x.DiscountApproverId)
            .HasColumnName("discount_approver_id");

        builder.Property(x => x.TradeInValue)
            .HasColumnName("trade_in_value")
            .HasConversion(new MoneyConverter())
            .IsRequired();

        // Payment properties
        builder.Property(x => x.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.DownPayment)
            .HasColumnName("down_payment")
            .HasConversion(
                v => v != null ? v.Amount : (decimal?)null,
                v => v != null ? new Money(v.Value, "BRL") : null);

        builder.Property(x => x.Installments)
            .HasColumnName("installments");

        builder.Property(x => x.UsedVehicleEvaluationId)
            .HasColumnName("used_vehicle_evaluation_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Calculated field - not mapped to database
        builder.Ignore(x => x.TotalValue);

        // Relacionamento com ProposalItems (1:N)
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("proposal_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.LeadId).HasDatabaseName("idx_proposals_lead");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_proposals_status");
    }
}