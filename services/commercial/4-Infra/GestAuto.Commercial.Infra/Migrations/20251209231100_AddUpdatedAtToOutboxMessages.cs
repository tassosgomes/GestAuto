using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add updated_at column to outbox_messages table (snake_case naming)
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "commercial",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            // Update existing rows to have updated_at = created_at
            migrationBuilder.Sql(@"
                UPDATE commercial.outbox_messages 
                SET updated_at = created_at
                WHERE updated_at = '0001-01-01'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "commercial",
                table: "outbox_messages");
        }
    }
}
