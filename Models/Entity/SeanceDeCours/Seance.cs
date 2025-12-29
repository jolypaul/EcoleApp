using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Enums;

namespace EcoleApp.Models.Entity.SeanceDeCours
{
    public class Seance
    {
        public int Id { get; set; }

        public int CoursId { get; set; }
        public Cours Cours { get; set; } = null!;

        public int GroupeId { get; set; }
        public Groupe Groupe { get; set; } = null!;

        public DateTime Date { get; set; }
        public string Salle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public EtatSeance Etat { get; set; } = EtatSeance.Brouillon;

        public Appel? Appel { get; set; }
        public CahierDeTexte? CahierDeTexte { get; set; }
    }
}
