using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class DeleteForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Companies_CompanyID",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts");

            // Drop old indexes
            migrationBuilder.DropIndex(
                name: "IX_Shifts_CompanyID",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_ReasonID",
                table: "Shifts");

            // DROP OLD COLUMNS - Make sure these lines exist:
            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ReasonID",
                table: "Shifts");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
