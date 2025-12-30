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

        // Qualification as owned entity
        builder.OwnsOne(x => x.Qualification, q =>
        {
            q.Property(p => p.HasTradeInVehicle)
                .HasColumnName("has_trade_in_vehicle");

            q.Property(p => p.PaymentMethod)
                .HasColumnName("payment_method")
                .HasConversion<string>();

            q.Property(p => p.ExpectedPurchaseDate)
                .HasColumnName("expected_purchase_date");

            q.Property(p => p.InterestedInTestDrive)
                .HasColumnName("interested_in_test_drive");

            q.Property(p => p.Notes)
                .HasColumnName("qualification_notes")
                .HasMaxLength(500);

            // TradeInVehicle as nested owned type
            q.OwnsOne(p => p.TradeInVehicle, tv =>
            {
                tv.Property(t => t.Brand)
                    .HasColumnName("trade_in_brand")
                    .HasMaxLength(50);

                tv.Property(t => t.Model)
                    .HasColumnName("trade_in_model")
                    .HasMaxLength(100);

                tv.Property(t => t.Year)
                    .HasColumnName("trade_in_year");

                tv.Property(t => t.Mileage)
                    .HasColumnName("trade_in_mileage");

                tv.Property(t => t.LicensePlate)
                    .HasColumnName("trade_in_license_plate")
                    .HasMaxLength(10);

                tv.Property(t => t.Color)
                    .HasColumnName("trade_in_color")
                    .HasMaxLength(50);

                tv.Property(t => t.GeneralCondition)
                    .HasColumnName("trade_in_general_condition")
                    .HasMaxLength(50);

                tv.Property(t => t.HasDealershipServiceHistory)
                    .HasColumnName("trade_in_has_dealership_service_history");
            });
        });

        // Relacionamento com Interactions (1:N)
        builder.HasMany(x => x.Interactions)
            .WithOne()
            .HasForeignKey(i => i.LeadId)
            .HasConstraintName("FK_interactions_leads_lead_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.SalesPersonId).HasDatabaseName("idx_leads_sales_person");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_leads_status");
        builder.HasIndex(x => x.Score).HasDatabaseName("idx_leads_score");
    }
}