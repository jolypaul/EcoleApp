namespace EcoleApp.Models.Entity.RapportEtStatistique
{
    public class RapportAssiduite
    {
        public int Id { get; set; }
        public DateTime DateGeneration { get; set; }
        public string Type { get; set; } = string.Empty;
        public string FichierUrl { get; set; } = string.Empty;
    }
}
