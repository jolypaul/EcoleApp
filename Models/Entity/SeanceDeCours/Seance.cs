using System;
using EcoleApp.Models.Entity.CahierDAppel;
using EcoleApp.Models.Entity.GestionAppel;

namespace EcoleApp.Models.Entity.SeanceDeCours
{
    public class Seance
    {
        public int Id { get; set; }

        public int CoursId { get; set; }
        public Cours? Cours { get; set; }

        public int GroupeId { get; set; }
        public Groupe? Groupe { get; set; }

        public DateTime Date { get; set; }
        public string Salle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public bool ValideEnseignant { get; set; }
        public bool ValideDelegue { get; set; }
        public CahierDeTexte? CahierDeTexte { get; set; }
    }
}
