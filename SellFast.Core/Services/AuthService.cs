using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace SellFast.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly SellFastContext _context;
        public SesionUsuario? SesionActual { get; private set; }

        public AuthService(SellFastContext context) { _context = context; }

        public async Task<SesionUsuario?> AutenticarAsync(string nombreUsuario, string password)
        {
            var usuario = await _context.UsuariosSistema.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
            if (usuario == null) return null;
            if (usuario.EstaBloqueado) throw new InvalidOperationException("Usuario bloqueado. Contacte al administrador.");
            if (!PasswordHelper.VerifyPassword(password, usuario.PasswordHash))
            {
                usuario.RegistrarIntentoFallido();
                await _context.SaveChangesAsync();
                return null;
            }
            usuario.RegistrarAccesoExitoso();
            await _context.SaveChangesAsync();
            SesionActual = new SesionUsuario
            {
                Id = usuario.Id, NombreUsuario = usuario.NombreUsuario, NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email, Rol = usuario.Rol, AvatarColor = usuario.AvatarColor
            };
            return SesionActual;
        }

        public async Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string nuevoPassword)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null) return false;
            if (usuario.Estado != EstadoUsuarioSistema.PrimerIngreso && !PasswordHelper.VerifyPassword(passwordActual, usuario.PasswordHash)) return false;
            if (!PasswordHelper.ValidarComplejidadPassword(nuevoPassword))
                throw new InvalidOperationException("El password debe tener al menos 6 caracteres, una mayúscula, una minúscula y un número.");
            usuario.CambiarPassword(PasswordHelper.HashPassword(nuevoPassword));
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestablecerPasswordAsync(int usuarioId, string nuevoPassword)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null) return false;
            usuario.CambiarPassword(PasswordHelper.HashPassword(nuevoPassword));
            usuario.Estado = EstadoUsuarioSistema.PrimerIngreso;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UsuarioSistema> CrearUsuarioAsync(UsuarioSistema usuario, string password)
        {
            if (await ExisteNombreUsuarioAsync(usuario.NombreUsuario))
                throw new InvalidOperationException($"Ya existe un usuario con el nombre {usuario.NombreUsuario}");
            if (!PasswordHelper.ValidarComplejidadPassword(password))
                throw new InvalidOperationException("El password debe tener al menos 6 caracteres, una mayúscula, una minúscula y un número.");
            usuario.PasswordHash = PasswordHelper.HashPassword(password);
            usuario.FechaCreacion = DateTime.Now;
            usuario.Estado = EstadoUsuarioSistema.PrimerIngreso;
            _context.UsuariosSistema.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<UsuarioSistema> ActualizarUsuarioAsync(UsuarioSistema usuario)
        {
            var existente = await _context.UsuariosSistema.FindAsync(usuario.Id) ?? throw new InvalidOperationException("Usuario no encontrado");
            if (await ExisteNombreUsuarioAsync(usuario.NombreUsuario, usuario.Id))
                throw new InvalidOperationException($"Ya existe otro usuario con el nombre {usuario.NombreUsuario}");
            existente.NombreUsuario = usuario.NombreUsuario;
            existente.NombreCompleto = usuario.NombreCompleto;
            existente.Email = usuario.Email;
            existente.Rol = usuario.Rol;
            existente.Notas = usuario.Notas;
            await _context.SaveChangesAsync();
            return existente;
        }

        public async Task<bool> EliminarUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null) return false;
            var adminCount = await _context.UsuariosSistema.CountAsync(u => u.Rol == RolUsuario.Administrador && u.Id != usuarioId);
            if (usuario.Rol == RolUsuario.Administrador && adminCount == 0)
                throw new InvalidOperationException("No se puede eliminar el último administrador.");
            _context.UsuariosSistema.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UsuarioSistema>> ObtenerTodosLosUsuariosAsync() =>
            await _context.UsuariosSistema.OrderBy(u => u.NombreCompleto).ToListAsync();

        public async Task<UsuarioSistema?> ObtenerUsuarioPorIdAsync(int id) =>
            await _context.UsuariosSistema.FindAsync(id);

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excludeId = null)
        {
            var query = _context.UsuariosSistema.Where(u => u.NombreUsuario == nombreUsuario);
            if (excludeId.HasValue) query = query.Where(u => u.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> BloquearUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null) return false;
            usuario.Estado = EstadoUsuarioSistema.Bloqueado;
            usuario.FechaBloqueo = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesbloquearUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null) return false;
            usuario.DesbloquearUsuario();
            await _context.SaveChangesAsync();
            return true;
        }

        public void CerrarSesion() { SesionActual?.CerrarSesion(); SesionActual = null; }
    }
}
