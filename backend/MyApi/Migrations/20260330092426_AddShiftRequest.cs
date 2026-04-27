using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShiftRequests",
                columns: table => new
                {
                    ShiftRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonID = table.Column<int>(type: "int", nullable: false),
                    RemId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ShopId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftRequests", x => x.ShiftRequestID);
                    table.ForeignKey(
                        name: "FK_ShiftRequests_Administrators_RemId",
                        column: x => x.RemId,
                        principalTable: "Administrators",
                        principalColumn: "RemID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShiftRequests_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShiftRequests_Reasons_ReasonID",
                        column: x => x.ReasonID,
                        principalTable: "Reasons",
                        principalColumn: "ReasonID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftRequests_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_CompanyId",
                table: "ShiftRequests",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_ReasonID",
                table: "ShiftRequests",
                column: "ReasonID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_RemId",
                table: "ShiftRequests",
                column: "RemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_ShopId",
                table: "ShiftRequests",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftRequests");
        }
    }
}
