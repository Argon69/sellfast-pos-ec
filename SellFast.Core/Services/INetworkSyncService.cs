using System.Threading.Tasks;

namespace SellFast.Core.Services
{
    public interface INetworkSyncService
    {
        string ObtenerIpLocal();
        Task<bool> ProbarConexionServidorAsync(string ip, int puerto);
    }
}
