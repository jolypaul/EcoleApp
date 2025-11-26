namespace EcoleApp.Models.Entity.Auth
{
    public class Enseignant : Utilisateur
    {
        public string Specialite { get; set; } = string.Empty;
    }
}
