using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "commercial");

            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<string>(type: "text", nullable: false),
                    sales_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interested_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    interested_trim = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    interested_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposals",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    vehicle_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    vehicle_trim = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    vehicle_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vehicle_year = table.Column<int>(type: "integer", nullable: false),
                    is_ready_delivery = table.Column<bool>(type: "boolean", nullable: false),
                    vehicle_price = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    discount_approver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trade_in_value = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_method = table.Column<string>(type: "text", nullable: false),
                    down_payment = table.Column<decimal>(type: "numeric", nullable: true),
                    installments = table.Column<int>(type: "integer", nullable: true),
                    used_vehicle_evaluation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "interactions",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    interaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    lead_id1 = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_interactions_leads_lead_id1",
                        column: x => x.lead_id1,
                        principalSchema: "commercial",
                        principalTable: "leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proposal_items",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    is_optional = table.Column<bool>(type: "boolean", nullable: false),
                    proposal_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposal_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_proposal_items_proposals_proposal_id",
                        column: x => x.proposal_id,
                        principalSchema: "commercial",
                        principalTable: "proposals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_audit_created",
                schema: "commercial",
                table: "audit_entries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_audit_entity",
                schema: "commercial",
                table: "audit_entries",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "idx_interactions_date",
                schema: "commercial",
                table: "interactions",
                column: "interaction_date");

            migrationBuilder.CreateIndex(
                name: "idx_interactions_lead",
                schema: "commercial",
                table: "interactions",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "idx_interactions_type",
                schema: "commercial",
                table: "interactions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_interactions_lead_id1",
                schema: "commercial",
                table: "interactions",
                column: "lead_id1");

            migrationBuilder.CreateIndex(
                name: "idx_leads_sales_person",
                schema: "commercial",
                table: "leads",
                column: "sales_person_id");

            migrationBuilder.CreateIndex(
                name: "idx_leads_score",
                schema: "commercial",
                table: "leads",
                column: "score");

            migrationBuilder.CreateIndex(
                name: "idx_leads_status",
                schema: "commercial",
                table: "leads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_pending",
                schema: "commercial",
                table: "outbox_messages",
                column: "created_at",
                filter: "processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_proposal_items_proposal",
                schema: "commercial",
                table: "proposal_items",
                column: "proposal_id");

            migrationBuilder.CreateIndex(
                name: "idx_proposals_lead",
                schema: "commercial",
                table: "proposals",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "idx_proposals_status",
                schema: "commercial",
                table: "proposals",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entries",
                schema: "commercial");

            migrationBuilder.DropTable(
                name: "interactions",
                schema: "commercial");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "commercial");

            migrationBuilder.DropTable(
                name: "proposal_items",
                schema: "commercial");

            migrationBuilder.DropTable(
                name: "leads",
                schema: "commercial");

            migrationBuilder.DropTable(
                name: "proposals",
                schema: "commercial");
        }
    }
}
