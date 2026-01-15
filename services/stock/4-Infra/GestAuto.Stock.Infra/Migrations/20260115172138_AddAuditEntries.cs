using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Stock.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsible_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    new_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_audit_entries_occurred_at",
                schema: "stock",
                table: "audit_entries",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "idx_audit_entries_vehicle",
                schema: "stock",
                table: "audit_entries",
                column: "vehicle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entries",
                schema: "stock");
        }
    }
}
