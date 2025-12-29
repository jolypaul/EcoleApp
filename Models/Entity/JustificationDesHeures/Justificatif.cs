using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.JustificationDesHeures
{
    public class Justificatif
    {
        public int Id { get; set; }

        public string EtudiantId { get; set; } = string.Empty;

        public Etudiant? Etudiant { get; set; }

        public string FichierUrl { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty; // valide, refusé, en cours de traitement

        public DateTime DateDepot { get; set; } = DateTime.UtcNow;
    }
}
