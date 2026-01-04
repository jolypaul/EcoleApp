using EcoleApp.Models.Entity.SeanceDeCours;

namespace EcoleApp.Models.Entity.Auth
{
    public class Etudiant : Utilisateur
    {
        public string Matricule { get; set; } = string.Empty;
        public string Filiere { get; set; } = string.Empty;
        public string Niveau { get; set; } = string.Empty;

        // Clé étrangère (optionnelle)
        public int? GroupeId { get; set; }

        // Navigation vers Groupe (OBLIGATOIRE pour EF)
        public Groupe? Groupe { get; set; }
    }
}
