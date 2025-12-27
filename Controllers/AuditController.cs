using EcoleApp.Services;
using EcoleApp.ViewModels.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoleApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly AuditService _auditService;

        public AuditController(AuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(AuditFilterViewModel filtres)
        {
            var (logs, total) = await _auditService.GetAuditsAsync(
                filtres.ActeurEmail,
                filtres.Action,
                filtres.DateDebut,
                filtres.DateFin,
                filtres.Page,
                filtres.PageSize);

            var vm = new AuditListViewModel
            {
                Logs = logs,
                Filtres = filtres,
                Total = total
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? debut, DateTime? fin)
        {
            var data = await _auditService.ExportCsvAsync(debut, fin);
            return File(data, "text/csv", "audit_logs.csv");
        }

        // Endpoint prêt pour PDF
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportPdf(DateTime? debut, DateTime? fin)
        {
            var pdf = await _auditService.ExportPdfAsync(debut, fin);
            return File(pdf, "application/pdf", "audit_logs.pdf");
        }

    }
}
