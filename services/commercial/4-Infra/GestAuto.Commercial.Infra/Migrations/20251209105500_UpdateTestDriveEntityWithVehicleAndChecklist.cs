using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestDriveEntityWithVehicleAndChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old foreign key and index
            migrationBuilder.DropIndex(
                name: "idx_test_drives_proposal",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropIndex(
                name: "idx_test_drives_scheduled_by",
                schema: "commercial",
                table: "test_drives");

            // Drop the old columns
            migrationBuilder.DropColumn(
                name: "proposal_id",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "scheduled_by",
                schema: "commercial",
                table: "test_drives");

            // Add new columns
            migrationBuilder.AddColumn<Guid>(
                name: "vehicle_id",
                schema: "commercial",
                table: "test_drives",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "sales_person_id",
                schema: "commercial",
                table: "test_drives",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Add checklist columns as owned entity
            migrationBuilder.AddColumn<decimal>(
                name: "checklist_initial_mileage",
                schema: "commercial",
                table: "test_drives",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "checklist_final_mileage",
                schema: "commercial",
                table: "test_drives",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "checklist_fuel_level",
                schema: "commercial",
                table: "test_drives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "checklist_visual_observations",
                schema: "commercial",
                table: "test_drives",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            // Add new columns
            migrationBuilder.AddColumn<string>(
                name: "customer_feedback",
                schema: "commercial",
                table: "test_drives",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                schema: "commercial",
                table: "test_drives",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // Create new indexes
            migrationBuilder.CreateIndex(
                name: "idx_test_drives_vehicle",
                schema: "commercial",
                table: "test_drives",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_test_drives_sales_person",
                schema: "commercial",
                table: "test_drives",
                column: "sales_person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new indexes
            migrationBuilder.DropIndex(
                name: "idx_test_drives_vehicle",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropIndex(
                name: "idx_test_drives_sales_person",
                schema: "commercial",
                table: "test_drives");

            // Drop new columns
            migrationBuilder.DropColumn(
                name: "vehicle_id",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "sales_person_id",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "checklist_initial_mileage",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "checklist_final_mileage",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "checklist_fuel_level",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "checklist_visual_observations",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "customer_feedback",
                schema: "commercial",
                table: "test_drives");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                schema: "commercial",
                table: "test_drives");

            // Add back old columns
            migrationBuilder.AddColumn<Guid>(
                name: "proposal_id",
                schema: "commercial",
                table: "test_drives",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "scheduled_by",
                schema: "commercial",
                table: "test_drives",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Create old indexes
            migrationBuilder.CreateIndex(
                name: "idx_test_drives_proposal",
                schema: "commercial",
                table: "test_drives",
                column: "proposal_id");

            migrationBuilder.CreateIndex(
                name: "idx_test_drives_scheduled_by",
                schema: "commercial",
                table: "test_drives",
                column: "scheduled_by");
        }
    }
}
