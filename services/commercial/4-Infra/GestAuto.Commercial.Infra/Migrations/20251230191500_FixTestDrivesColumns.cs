using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAuto.Commercial.Infra.Migrations;

public partial class FixTestDrivesColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The test_drives table exists in some environments but is missing columns expected by
        // the current EF Core model (see TestDriveConfiguration).
        // Use IF NOT EXISTS to keep this migration safe across slightly divergent schemas.

        migrationBuilder.Sql(@"
            ALTER TABLE commercial.test_drives
            ADD COLUMN IF NOT EXISTS customer_feedback character varying(1000);
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE commercial.test_drives
            ADD COLUMN IF NOT EXISTS cancellation_reason character varying(500);
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE commercial.test_drives
            ADD COLUMN IF NOT EXISTS checklist_fuel_level text;
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE commercial.test_drives
            ADD COLUMN IF NOT EXISTS checklist_visual_observations character varying(1000);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep Down resilient as well.
        migrationBuilder.Sql(@"ALTER TABLE commercial.test_drives DROP COLUMN IF EXISTS checklist_visual_observations;");
        migrationBuilder.Sql(@"ALTER TABLE commercial.test_drives DROP COLUMN IF EXISTS checklist_fuel_level;");
        migrationBuilder.Sql(@"ALTER TABLE commercial.test_drives DROP COLUMN IF EXISTS cancellation_reason;");
        migrationBuilder.Sql(@"ALTER TABLE commercial.test_drives DROP COLUMN IF EXISTS customer_feedback;");
    }
}
