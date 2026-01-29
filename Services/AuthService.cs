using CafeteriaUNAL.Data;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public class AuthService : IAuthService
    {
        private readonly CafeteriaContext _context;
        public SesionUsuario? SesionActual { get; private set; }

        public AuthService(CafeteriaContext context)
        {
            _context = context;
        }

        public async Task<SesionUsuario?> AutenticarAsync(string nombreUsuario, string password)
        {
            try
            {
                var usuario = await _context.UsuariosSistema
                    .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

                if (usuario == null)
                    return null;

                // Verificar si está bloqueado
                if (usuario.EstaBloqueado)
                    throw new InvalidOperationException("Usuario bloqueado. Contacte al administrador.");

                // Verificar password
                if (!PasswordHelper.VerifyPassword(password, usuario.PasswordHash))
                {
                    usuario.RegistrarIntentoFallido();
                    await _context.SaveChangesAsync();
                    return null;
                }

                // Autenticación exitosa
                usuario.RegistrarAccesoExitoso();
                await _context.SaveChangesAsync();

                SesionActual = new SesionUsuario
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    Rol = usuario.Rol
                };

                return SesionActual;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string nuevoPassword)
        {
            try
            {
                var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
                if (usuario == null)
                    return false;

                // Verificar password actual (excepto si es primer ingreso)
                if (usuario.Estado != EstadoUsuarioSistema.PrimerIngreso)
                {
                    if (!PasswordHelper.VerifyPassword(passwordActual, usuario.PasswordHash))
                        return false;
                }

                // Validar complejidad del nuevo password
                if (!PasswordHelper.ValidarComplejidadPassword(nuevoPassword))
                    throw new InvalidOperationException("El password debe tener al menos 6 caracteres, una mayúscula, una minúscula y un número.");

                // Cambiar password
                var nuevoHash = PasswordHelper.HashPassword(nuevoPassword);
                usuario.CambiarPassword(nuevoHash);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestablecerPasswordAsync(int usuarioId, string nuevoPassword)
        {
            try
            {
                var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
                if (usuario == null)
                    return false;

                var nuevoHash = PasswordHelper.HashPassword(nuevoPassword);
                usuario.CambiarPassword(nuevoHash);
                usuario.Estado = EstadoUsuarioSistema.PrimerIngreso;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
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
            var usuarioExistente = await _context.UsuariosSistema.FindAsync(usuario.Id);
            if (usuarioExistente == null)
                throw new InvalidOperationException($"No se encontró el usuario con ID {usuario.Id}");

            if (await ExisteNombreUsuarioAsync(usuario.NombreUsuario, usuario.Id))
                throw new InvalidOperationException($"Ya existe otro usuario con el nombre {usuario.NombreUsuario}");

            usuarioExistente.NombreUsuario = usuario.NombreUsuario;
            usuarioExistente.NombreCompleto = usuario.NombreCompleto;
            usuarioExistente.Email = usuario.Email;
            usuarioExistente.Rol = usuario.Rol;
            usuarioExistente.Notas = usuario.Notas;

            await _context.SaveChangesAsync();
            return usuarioExistente;
        }

        public async Task<bool> EliminarUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null)
                return false;

            // No permitir eliminar al último administrador
            var adminCount = await _context.UsuariosSistema
                .CountAsync(u => u.Rol == RolUsuario.Administrador && u.Id != usuarioId);

            if (usuario.Rol == RolUsuario.Administrador && adminCount == 0)
                throw new InvalidOperationException("No se puede eliminar el último administrador del sistema.");

            _context.UsuariosSistema.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UsuarioSistema>> ObtenerTodosLosUsuariosAsync()
        {
            return await _context.UsuariosSistema
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<UsuarioSistema?> ObtenerUsuarioPorIdAsync(int id)
        {
            return await _context.UsuariosSistema.FindAsync(id);
        }

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excludeId = null)
        {
            var query = _context.UsuariosSistema.Where(u => u.NombreUsuario == nombreUsuario);
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> BloquearUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null)
                return false;

            usuario.Estado = EstadoUsuarioSistema.Bloqueado;
            usuario.FechaBloqueo = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesbloquearUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);
            if (usuario == null)
                return false;

            usuario.DesbloquearUsuario();
            await _context.SaveChangesAsync();
            return true;
        }

        public void CerrarSesion()
        {
            SesionActual?.CerrarSesion();
            SesionActual = null;
        }
    }
}