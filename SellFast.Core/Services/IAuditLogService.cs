using System.Threading.Tasks;
using SellFast.Core.Models;

namespace SellFast.Core.Services
{
    public interface IAuditLogService
    {
        Task RegistrarAccionAsync(string usuario, string accion, string? detalles = null, string tipoModulo = "General");
    }
}
