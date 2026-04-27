using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    public partial class FixCompanyReasonRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys only if they exist
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shifts_Companies_CompanyID')
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_Shifts_Companies_CompanyID];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shifts_Reasons_ReasonID')
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_Shifts_Reasons_ReasonID];
");

            // Drop indexes only if they exist
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shifts_CompanyID' AND object_id = OBJECT_ID(N'[dbo].[Shifts]'))
    DROP INDEX [IX_Shifts_CompanyID] ON [dbo].[Shifts];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shifts_ReasonID' AND object_id = OBJECT_ID(N'[dbo].[Shifts]'))
    DROP INDEX [IX_Shifts_ReasonID] ON [dbo].[Shifts];
");

            // Drop columns only if they exist
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Shifts', N'CompanyID') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP COLUMN [CompanyID];
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Shifts', N'ReasonID') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP COLUMN [ReasonID];
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate columns (only if missing)
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Shifts', N'CompanyID') IS NULL
    ALTER TABLE [dbo].[Shifts] ADD [CompanyID] int NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Shifts', N'ReasonID') IS NULL
    ALTER TABLE [dbo].[Shifts] ADD [ReasonID] int NULL;
");

            // Recreate indexes if missing
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shifts_CompanyID' AND object_id = OBJECT_ID(N'[dbo].[Shifts]'))
    CREATE INDEX [IX_Shifts_CompanyID] ON [dbo].[Shifts]([CompanyID]);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shifts_ReasonID' AND object_id = OBJECT_ID(N'[dbo].[Shifts]'))
    CREATE INDEX [IX_Shifts_ReasonID] ON [dbo].[Shifts]([ReasonID]);
");

            // Recreate foreign keys if missing (and only if columns exist)
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Shifts', N'CompanyID') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shifts_Companies_CompanyID')
BEGIN
    ALTER TABLE [dbo].[Shifts]
    ADD CONSTRAINT [FK_Shifts_Companies_CompanyID]
    FOREIGN KEY ([CompanyID]) REFERENCES [dbo].[Companies]([CompanyID]);
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Shifts', N'ReasonID') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shifts_Reasons_ReasonID')
BEGIN
    ALTER TABLE [dbo].[Shifts]
    ADD CONSTRAINT [FK_Shifts_Reasons_ReasonID]
    FOREIGN KEY ([ReasonID]) REFERENCES [dbo].[Reasons]([ReasonID]);
END
");
        }
    }
}
