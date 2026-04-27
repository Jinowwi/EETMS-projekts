using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts");

            migrationBuilder.AlterColumn<int>(
                name: "ReasonID",
                table: "Shifts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts",
                column: "ReasonID",
                principalTable: "Reasons",
                principalColumn: "ReasonID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts");

            migrationBuilder.AlterColumn<int>(
                name: "ReasonID",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts",
                column: "ReasonID",
                principalTable: "Reasons",
                principalColumn: "ReasonID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
