using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.SeanceDeCours;

namespace EcoleApp.Models.Entity.GestionAppel
{
    public class Absence
    {
        public int Id { get; set; }

        public int EtudiantId { get; set; }
        public Etudiant Etudiant { get; set; } = null!;

        public int SeanceId { get; set; }
        public Seance Seance { get; set; } = null!;

        public DateOnly Date { get; set; }

        public string Statut { get; set; } = string.Empty;
    }
}
