using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.SeanceDeCours
{
    public class Groupe
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;

        public ICollection<Etudiant> Etudiants { get; set; }
            = new List<Etudiant>();
    }

}
