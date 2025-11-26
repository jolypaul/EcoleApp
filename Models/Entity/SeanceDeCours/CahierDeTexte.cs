using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.SeanceDeCours
{
    public class CahierDeTexte
    {
        public int Id { get; set; }

        public int SeanceId { get; set; }

        public Cours? Cours { get; set; }
        public Enseignant? Enseignant { get; set; }

        public string Objectif { get; set; } = string.Empty;
        public string Activites { get; set; } = string.Empty;
        public string Ressources { get; set; } = string.Empty;
        public string Devoirs { get; set; } = string.Empty;
    }
}
