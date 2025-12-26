using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcoleApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesAndAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "NomRole", "Responsabilite" },
                values: new object[,]
                {
                    { 1, "Admin", "Gestion complète du système" },
                    { 2, "Enseignant", "Gestion des séances et présences" },
                    { 3, "Etudiant", "Consultation des présences" },
                    { 4, "Delegue", "Gestion des présences de la classe" }
                });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "Discriminator", "Email", "MotDePasse", "NomComplet", "Poste", "RoleId" },
                values: new object[] { "ADMIN-001", "Admin", "admin@asp.com", "BWawV55i2M4l1tSYbOykgyHH4qslJWo8Bxxg/R9smkI=", "Administrateur Principal", "Administrateur Système", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: "ADMIN-001");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
