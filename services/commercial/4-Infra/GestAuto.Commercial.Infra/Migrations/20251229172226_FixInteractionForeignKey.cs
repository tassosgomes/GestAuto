using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations
{
    /// <inheritdoc />
    public partial class FixInteractionForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_interactions_leads_lead_id1",
                schema: "commercial",
                table: "interactions");

            migrationBuilder.DropIndex(
                name: "IX_interactions_lead_id1",
                schema: "commercial",
                table: "interactions");

            migrationBuilder.DropColumn(
                name: "lead_id1",
                schema: "commercial",
                table: "interactions");

            migrationBuilder.AddForeignKey(
                name: "FK_interactions_leads_lead_id",
                schema: "commercial",
                table: "interactions",
                column: "lead_id",
                principalSchema: "commercial",
                principalTable: "leads",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_interactions_leads_lead_id",
                schema: "commercial",
                table: "interactions");

            migrationBuilder.AddColumn<Guid>(
                name: "lead_id1",
                schema: "commercial",
                table: "interactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_interactions_lead_id1",
                schema: "commercial",
                table: "interactions",
                column: "lead_id1");

            migrationBuilder.AddForeignKey(
                name: "FK_interactions_leads_lead_id1",
                schema: "commercial",
                table: "interactions",
                column: "lead_id1",
                principalSchema: "commercial",
                principalTable: "leads",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
