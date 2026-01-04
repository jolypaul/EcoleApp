namespace EcoleApp.ViewModels.Seance
{
    public class CreateSeanceViewModel
    {
        public int CoursId { get; set; }
        public int GroupeId { get; set; }
        public DateTime Date { get; set; }
        public string Salle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
