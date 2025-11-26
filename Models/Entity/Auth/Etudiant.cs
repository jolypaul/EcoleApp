namespace EcoleApp.Models.Entity.Auth
{
    public class Etudiant : Utilisateur
    {
        public string Matricule { get; set; } = string.Empty;
        public string Filiere { get; set; } = string.Empty;
        public string Niveau { get; set; } = string.Empty;
    }
}
