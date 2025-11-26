using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.SeanceDeCours
{
    public class Cours
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;

        public int EnseignantId { get; set; }
        public Enseignant? Enseignant { get; set; }

        public string Semestre { get; set; } = string.Empty;
        public int Annee { get; set; }
        public int VolumeHoraire { get; set; }
    }
}
