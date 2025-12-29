using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Enums;

namespace EcoleApp.Models.Entity.GestionAppel
{
    public class LigneAppel
    {
        public int Id { get; set; }

        public int AppelId { get; set; }
        public Appel Appel { get; set; } = null!;

        public string EtudiantId { get; set; } = string.Empty;
        public Etudiant Etudiant { get; set; } = null!;

        public StatutPresence Statut { get; set; }
    }
}
