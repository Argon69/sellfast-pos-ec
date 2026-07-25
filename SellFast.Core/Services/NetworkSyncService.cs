using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SellFast.Core.Services
{
    public class NetworkSyncService : INetworkSyncService
    {
        public string ObtenerIpLocal()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener IP local: {ex.Message}");
            }
            return "127.0.0.1";
        }

        public async Task<bool> ProbarConexionServidorAsync(string ip, int puerto)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;

            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, puerto);
                var timeoutTask = Task.Delay(2000);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == connectTask && client.Connected)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error probando conexion servidor: {ex.Message}");
            }

            return false;
        }
    }
}
