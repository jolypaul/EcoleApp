namespace EcoleApp.Models.Entity.Auth
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Enseignant = "Enseignant";
        public const string Delegue = "Delegue";
        public const string Etudiant = "Etudiant";

        // Alias pour garder le métier
        public const string Administrateur = Admin;
        public const string Responsable = Admin;
    }
}
