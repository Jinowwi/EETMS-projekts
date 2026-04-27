using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorShiftCompanyReason : Migration
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

            migrationBuilder.DropTable(
                name: "CompanyReason");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyID",
                table: "Shifts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompanyReasonID",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CompanyReasons",
                columns: table => new
                {
                    CompanyReasonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompaniesCompanyID = table.Column<int>(type: "int", nullable: false),
                    ReasonsReasonID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyReasons", x => x.CompanyReasonID);
                    table.ForeignKey(
                        name: "FK_CompanyReasons_Companies_CompaniesCompanyID",
                        column: x => x.CompaniesCompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyReasons_Reasons_ReasonsReasonID",
                        column: x => x.ReasonsReasonID,
                        principalTable: "Reasons",
                        principalColumn: "ReasonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CompanyReasonID",
                table: "Shifts",
                column: "CompanyReasonID");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyReasons_CompaniesCompanyID",
                table: "CompanyReasons",
                column: "CompaniesCompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyReasons_ReasonsReasonID",
                table: "CompanyReasons",
                column: "ReasonsReasonID");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Companies_CompanyID",
                table: "Shifts",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_CompanyReasons_CompanyReasonID",
                table: "Shifts",
                column: "CompanyReasonID",
                principalTable: "CompanyReasons",
                principalColumn: "CompanyReasonID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts",
                column: "ReasonID",
                principalTable: "Reasons",
                principalColumn: "ReasonID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Companies_CompanyID",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_CompanyReasons_CompanyReasonID",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "CompanyReasons");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_CompanyReasonID",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CompanyReasonID",
                table: "Shifts");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyID",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyReason",
                columns: table => new
                {
                    CompaniesCompanyID = table.Column<int>(type: "int", nullable: false),
                    ReasonsReasonID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyReason", x => new { x.CompaniesCompanyID, x.ReasonsReasonID });
                    table.ForeignKey(
                        name: "FK_CompanyReason_Companies_CompaniesCompanyID",
                        column: x => x.CompaniesCompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyReason_Reasons_ReasonsReasonID",
                        column: x => x.ReasonsReasonID,
                        principalTable: "Reasons",
                        principalColumn: "ReasonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyReason_ReasonsReasonID",
                table: "CompanyReason",
                column: "ReasonsReasonID");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Companies_CompanyID",
                table: "Shifts",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Reasons_ReasonID",
                table: "Shifts",
                column: "ReasonID",
                principalTable: "Reasons",
                principalColumn: "ReasonID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
