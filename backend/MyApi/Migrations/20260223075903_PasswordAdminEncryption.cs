using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class PasswordAdminEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                table: "Administrators",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Administrators",
                newName: "PasswordHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Administrators",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Administrators",
                newName: "Password");
        }
    }
}
