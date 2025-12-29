using EcoleApp.Models.Entity.SeanceDeCours;

namespace EcoleApp.Services.Interfaces
{
    public interface ISeanceService
    {
        Task<Seance> CreerSeanceAsync(
            int coursId,
            int groupeId,
            DateTime date,
            string salle,
            string type,
            string enseignantId);

        Task ModifierSeanceAsync(
            int seanceId,
            DateTime date,
            string salle,
            string type,
            string enseignantId);

        Task ValiderSeanceAsync(int seanceId, string enseignantId);

        Task<IEnumerable<Seance>> ConsulterSeancesAsync(string utilisateurId);
    }
}
