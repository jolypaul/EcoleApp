using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Enums;
using EcoleApp.ViewModels.Appel;

namespace EcoleApp.Services.Interfaces
{
    public interface IAppelService
    {
        Task<int> CreerAppelAsync(int seanceId, string delegueId);

        Task<SaisieAppelViewModel> GetSaisieAppelAsync(int appelId);

        Task EnregistrerSaisieAsync(
            int appelId,
            Dictionary<string, StatutPresence> presences);

        Task<IEnumerable<LigneAppel>> ConsulterAbsencesAsync(
            DateTime debut,
            DateTime fin);
    }
}
