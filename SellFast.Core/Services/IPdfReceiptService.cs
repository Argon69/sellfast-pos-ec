using System.Threading.Tasks;
using SellFast.Core.Models;

namespace SellFast.Core.Services
{
    public interface IPdfReceiptService
    {
        Task<string> GenerarComprobantePdfAsync(Transaccion transaccion, ConfiguracionNegocio configuracion);
    }
}
