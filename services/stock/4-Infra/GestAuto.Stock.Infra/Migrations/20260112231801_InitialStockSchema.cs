using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Stock.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialStockSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stock");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sales_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bank_deadline_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancel_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    extended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    extended_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    previous_expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    vin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trim = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    year_model = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mileage_km = table.Column<int>(type: "integer", nullable: true),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    demo_purpose = table.Column<string>(type: "text", nullable: true),
                    is_registered = table.Column<bool>(type: "boolean", nullable: false),
                    current_owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "check_ins",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsible_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_ins", x => x.id);
                    table.ForeignKey(
                        name: "FK_check_ins_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "stock",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_outs",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsible_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_outs", x => x.id);
                    table.ForeignKey(
                        name: "FK_check_outs_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "stock",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_drives",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    outcome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_drives", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_drives_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "stock",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_check_ins_vehicle",
                schema: "stock",
                table: "check_ins",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_check_outs_vehicle",
                schema: "stock",
                table: "check_outs",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_pending",
                schema: "stock",
                table: "outbox_messages",
                column: "created_at",
                filter: "processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_status",
                schema: "stock",
                table: "reservations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_reservations_vehicle_active",
                schema: "stock",
                table: "reservations",
                column: "vehicle_id",
                unique: true,
                filter: "status = 'ativa'");

            migrationBuilder.CreateIndex(
                name: "idx_test_drives_vehicle",
                schema: "stock",
                table: "test_drives",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_category",
                schema: "stock",
                table: "vehicles",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_status",
                schema: "stock",
                table: "vehicles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_vehicles_plate",
                schema: "stock",
                table: "vehicles",
                column: "plate",
                unique: true,
                filter: "plate IS NOT NULL AND status NOT IN ('vendido', 'baixado')");

            migrationBuilder.CreateIndex(
                name: "ux_vehicles_vin",
                schema: "stock",
                table: "vehicles",
                column: "vin",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "check_ins",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "check_outs",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "test_drives",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "stock");
        }
    }
}
