using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoleApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMustChangePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Utilisateurs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: "ADMIN-001",
                columns: new[] { "MotDePasseHash", "MustChangePassword" },
                values: new object[] { "+n//ic1KGURUKbebJgD9og==.JNzH0fQczqulV90tqsXXAEOCT6fdI4Jj7Ylgp3LBAqc=", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Utilisateurs");

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: "ADMIN-001",
                column: "MotDePasseHash",
                value: "uaCC25mkO69Wq3kYe7yksg==.QX/poa93n8valy4SX9XTWzJNP6IESiX8odRohplt5D4=");
        }
    }
}
