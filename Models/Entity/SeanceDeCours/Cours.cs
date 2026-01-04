using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.SeanceDeCours
{
    public class Cours
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;

        public string EnseignantId { get; set; } = string.Empty;
        public Enseignant Enseignant { get; set; } = null!;

        public string Semestre { get; set; } = string.Empty;
        public int Annee { get; set; }
        public int VolumeHoraire { get; set; }
    }
}
