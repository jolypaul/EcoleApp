using System;

namespace EcoleApp.ViewModels.Audit
{
    public class AuditFilterViewModel
    {
        public string? ActeurEmail { get; set; }
        public string? Action { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
