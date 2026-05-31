using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOneToONeRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ShiftRequestID",
                table: "Ratings");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Ratings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ShiftRequestID",
                table: "Ratings",
                column: "ShiftRequestID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ShiftRequestID",
                table: "Ratings");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Ratings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ShiftRequestID",
                table: "Ratings",
                column: "ShiftRequestID");
        }
    }
}
