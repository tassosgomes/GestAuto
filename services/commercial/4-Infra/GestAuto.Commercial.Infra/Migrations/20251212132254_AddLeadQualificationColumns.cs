using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadQualificationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE commercial.leads
                    ADD COLUMN IF NOT EXISTS expected_purchase_date timestamp with time zone NULL,
                    ADD COLUMN IF NOT EXISTS has_trade_in_vehicle boolean NULL,
                    ADD COLUMN IF NOT EXISTS interested_in_test_drive boolean NULL,
                    ADD COLUMN IF NOT EXISTS payment_method text NULL,
                    ADD COLUMN IF NOT EXISTS qualification_notes character varying(500) NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_brand character varying(50) NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_color character varying(50) NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_general_condition character varying(50) NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_has_dealership_service_history boolean NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_license_plate character varying(10) NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_mileage integer NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_model character varying(100) NULL,
                    ADD COLUMN IF NOT EXISTS trade_in_year integer NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE commercial.leads
                    DROP COLUMN IF EXISTS trade_in_year,
                    DROP COLUMN IF EXISTS trade_in_model,
                    DROP COLUMN IF EXISTS trade_in_mileage,
                    DROP COLUMN IF EXISTS trade_in_license_plate,
                    DROP COLUMN IF EXISTS trade_in_has_dealership_service_history,
                    DROP COLUMN IF EXISTS trade_in_general_condition,
                    DROP COLUMN IF EXISTS trade_in_color,
                    DROP COLUMN IF EXISTS trade_in_brand,
                    DROP COLUMN IF EXISTS qualification_notes,
                    DROP COLUMN IF EXISTS payment_method,
                    DROP COLUMN IF EXISTS interested_in_test_drive,
                    DROP COLUMN IF EXISTS has_trade_in_vehicle,
                    DROP COLUMN IF EXISTS expected_purchase_date;
            ");
        }
    }
}
