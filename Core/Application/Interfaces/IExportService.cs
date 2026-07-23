using System.Collections.Generic;

namespace Application.Interfaces
{
    public interface IExportService
    {
        byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName);
        byte[] ExportToPdf<T>(IEnumerable<T> data, string title, string qrCodeUrl = null);
        byte[] ExportToWordHtml<T>(IEnumerable<T> data, string title, string qrCodeUrl = null);
    }
}
