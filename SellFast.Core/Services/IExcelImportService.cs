using System.IO;
using System.Threading.Tasks;

namespace SellFast.Core.Services
{
    public class ImportResult
    {
        public int TotalFilas { get; set; }
        public int RegistrosImportados { get; set; }
        public int RegistrosOmitidos { get; set; }
        public string Mensaje { get; set; } = "";
    }

    public interface IExcelImportService
    {
        Task<ImportResult> ImportarClientesAsync(string filePath);
        Task<ImportResult> ImportarProductosAsync(string filePath);
        Task<string> GenerarPlantillaImportacionAsync(string destinationFolder);
    }
}
