namespace EcoleApp.Services.Interfaces
{
    public interface IExportAbsenceService
    {
        Task<byte[]> ExportExcelAsync(DateTime debut, DateTime fin);
        Task<byte[]> ExportPdfAsync(DateTime debut, DateTime fin);
    }
}
