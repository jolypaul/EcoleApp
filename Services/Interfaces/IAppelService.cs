using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Enums;

namespace EcoleApp.Services.Interfaces
{
    public interface IAppelService
    {
        Task<Appel> CreerAppelAsync(int seanceId, string delegueId);
        Task SaisirAppelAsync(int appelId, Dictionary<string, StatutPresence> presences);
        Task<IEnumerable<LigneAppel>> ConsulterAbsencesAsync(DateTime debut, DateTime fin);
    }
}
