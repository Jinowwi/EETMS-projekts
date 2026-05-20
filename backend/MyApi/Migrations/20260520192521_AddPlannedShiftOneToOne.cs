using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedShiftOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlannedShifts_ShiftRequestID",
                table: "PlannedShifts");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedShifts_ShiftRequestID",
                table: "PlannedShifts",
                column: "ShiftRequestID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlannedShifts_ShiftRequestID",
                table: "PlannedShifts");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedShifts_ShiftRequestID",
                table: "PlannedShifts",
                column: "ShiftRequestID");
        }
    }
}
