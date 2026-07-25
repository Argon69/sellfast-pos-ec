using System.Collections.Generic;

namespace SellFast.Core.Services
{
    public interface IHardwareService
    {
        List<string> ObtenerImpresorasInstaladas();
        string ObtenerImpresoraPredeterminada();
        bool ImprimirComprobanteDirecto(string pdfPath, string nombreImpresora);
    }
}
