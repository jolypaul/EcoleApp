using System.Collections.Generic;
using EcoleApp.Models.Entity.AdministrationEtAudit;

namespace EcoleApp.ViewModels.Audit
{
    public class AuditListViewModel
    {
        public IEnumerable<AuditLog> Logs { get; set; } = new List<AuditLog>();
        public AuditFilterViewModel Filtres { get; set; } = new AuditFilterViewModel();

        public int Total { get; set; }
    }
}
