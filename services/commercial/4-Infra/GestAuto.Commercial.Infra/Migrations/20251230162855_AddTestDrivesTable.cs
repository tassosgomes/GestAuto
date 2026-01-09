using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddTestDrivesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "test_drives",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sales_person_id = table.Column<Guid>(type: "uuid", nullable: false),

                    checklist_initial_mileage = table.Column<decimal>(type: "numeric", nullable: true),
                    checklist_final_mileage = table.Column<decimal>(type: "numeric", nullable: true),
                    checklist_fuel_level = table.Column<string>(type: "text", nullable: true),
                    checklist_visual_observations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),

                    customer_feedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),

                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_drives", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_test_drives_lead",
                schema: "commercial",
                table: "test_drives",
                column: "lead_id");

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

            migrationBuilder.CreateIndex(
                name: "idx_test_drives_scheduled",
                schema: "commercial",
                table: "test_drives",
                column: "scheduled_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_drives",
                schema: "commercial");
        }
    }
}
