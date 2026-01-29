using CafeteriaUNAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public interface IAuthService
    {
        Task<SesionUsuario?> AutenticarAsync(string nombreUsuario, string password);
        Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string nuevoPassword);
        Task<bool> RestablecerPasswordAsync(int usuarioId, string nuevoPassword);
        Task<UsuarioSistema> CrearUsuarioAsync(UsuarioSistema usuario, string password);
        Task<UsuarioSistema> ActualizarUsuarioAsync(UsuarioSistema usuario);
        Task<bool> EliminarUsuarioAsync(int usuarioId);
        Task<List<UsuarioSistema>> ObtenerTodosLosUsuariosAsync();
        Task<UsuarioSistema?> ObtenerUsuarioPorIdAsync(int id);
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excludeId = null);
        Task<bool> BloquearUsuarioAsync(int usuarioId);
        Task<bool> DesbloquearUsuarioAsync(int usuarioId);
        void CerrarSesion();
        SesionUsuario? SesionActual { get; }
    }
}