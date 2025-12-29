using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.SeanceDeCours;

namespace EcoleApp.Models.Entity.GestionAppel
{
    public class Appel
    {
        public int Id { get; set; }

        public int SeanceId { get; set; }
        public Seance Seance { get; set; } = null!;

        public string DelegueId { get; set; } = string.Empty;
        public Delegue Delegue { get; set; } = null!;

        public bool EstVerrouille { get; set; }

        public DateTime DateSaisie { get; set; } = DateTime.UtcNow;

        public ICollection<LigneAppel> LignesAppel { get; set; } = new List<LigneAppel>();
    }
}
