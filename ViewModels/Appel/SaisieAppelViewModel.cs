using EcoleApp.Models.Enums;

namespace EcoleApp.ViewModels.Appel
{
    public class SaisieAppelViewModel
    {
        public int AppelId { get; set; }

        // Key = EtudiantId (string)
        public Dictionary<string, StatutPresence> Presences { get; set; }
            = new();

        // Map student id -> display name
        public Dictionary<string, string> StudentNames { get; set; } = new();
    }
}
