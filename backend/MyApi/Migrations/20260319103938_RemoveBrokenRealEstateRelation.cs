using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrokenRealEstateRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Administrators_RemID",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_RemID",
                table: "Companies");

            migrationBuilder.AddColumn<int>(
                name: "AdministrationRemID",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_AdministrationRemID",
                table: "Companies",
                column: "AdministrationRemID");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Administrators_AdministrationRemID",
                table: "Companies",
                column: "AdministrationRemID",
                principalTable: "Administrators",
                principalColumn: "RemID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Administrators_AdministrationRemID",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_AdministrationRemID",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AdministrationRemID",
                table: "Companies");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_RemID",
                table: "Companies",
                column: "RemID");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Administrators_RemID",
                table: "Companies",
                column: "RemID",
                principalTable: "Administrators",
                principalColumn: "RemID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
