using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingsAndPlannedShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlannedShifts",
                columns: table => new
                {
                    PlannedShiftsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    approx_start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    approx_date = table.Column<DateOnly>(type: "date", nullable: false),
                    approx_duration = table.Column<int>(type: "int", nullable: false),
                    ShiftRequestID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedShifts", x => x.PlannedShiftsID);
                    table.ForeignKey(
                        name: "FK_PlannedShifts_ShiftRequests_ShiftRequestID",
                        column: x => x.ShiftRequestID,
                        principalTable: "ShiftRequests",
                        principalColumn: "ShiftRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftRequestID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_Ratings_ShiftRequests_ShiftRequestID",
                        column: x => x.ShiftRequestID,
                        principalTable: "ShiftRequests",
                        principalColumn: "ShiftRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedShifts_ShiftRequestID",
                table: "PlannedShifts",
                column: "ShiftRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ShiftRequestID",
                table: "Ratings",
                column: "ShiftRequestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlannedShifts");

            migrationBuilder.DropTable(
                name: "Ratings");
        }
    }
}
