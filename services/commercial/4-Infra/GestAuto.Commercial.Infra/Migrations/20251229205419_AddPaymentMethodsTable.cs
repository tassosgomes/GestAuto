using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_methods",
                schema: "commercial",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "commercial",
                table: "payment_methods",
                columns: new[] { "id", "code", "created_at", "display_order", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, "CASH", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "À Vista", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "FINANCING", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Financiamento", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "CONSORTIUM", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "Consórcio", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "LEASING", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, "Leasing", new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_code",
                schema: "commercial",
                table: "payment_methods",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_methods",
                schema: "commercial");
        }
    }
}
