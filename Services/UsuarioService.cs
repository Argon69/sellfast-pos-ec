using CafeteriaUNAL.Data;
using CafeteriaUNAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly CafeteriaContext _context;

        public UsuarioService(CafeteriaContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<List<Usuario>> ObtenerActivosAsync()
        {
            return await _context.Usuarios
                .Where(u => u.Activo)
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObtenerPorDocumentoAsync(string documento)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Documento == documento);
        }

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            // Validar que no exista documento duplicado
            if (await ExisteDocumentoAsync(usuario.Documento))
                throw new InvalidOperationException($"Ya existe un usuario con el documento {usuario.Documento}");

            // Validar que no exista email duplicado
            if (await ExisteEmailAsync(usuario.Email))
                throw new InvalidOperationException($"Ya existe un usuario con el email {usuario.Email}");

            // Si es estudiante, debe tener modalidad de pago
            if (usuario.TipoUsuario == TipoUsuario.Estudiante && !usuario.ModalidadPago.HasValue)
                throw new InvalidOperationException("Los estudiantes deben tener una modalidad de pago");

            usuario.FechaRegistro = DateTime.Now;
            usuario.Activo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario> ActualizarAsync(Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(usuario.Id);
            if (usuarioExistente == null)
                throw new InvalidOperationException($"No se encontró el usuario con ID {usuario.Id}");

            // Validar documento único
            if (await ExisteDocumentoAsync(usuario.Documento, usuario.Id))
                throw new InvalidOperationException($"Ya existe otro usuario con el documento {usuario.Documento}");

            // Validar email único
            if (await ExisteEmailAsync(usuario.Email, usuario.Id))
                throw new InvalidOperationException($"Ya existe otro usuario con el email {usuario.Email}");

            // Actualizar propiedades
            usuarioExistente.Documento = usuario.Documento;
            usuarioExistente.Nombre = usuario.Nombre;
            usuarioExistente.Apellido = usuario.Apellido;
            usuarioExistente.Email = usuario.Email;
            usuarioExistente.Telefono = usuario.Telefono;
            usuarioExistente.TipoUsuario = usuario.TipoUsuario;
            usuarioExistente.ModalidadPago = usuario.ModalidadPago;
            usuarioExistente.CodigoEstudiante = usuario.CodigoEstudiante;

            await _context.SaveChangesAsync();

            return usuarioExistente;
        }

        public async Task<bool> CambiarEstadoAsync(int id, bool activo)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            usuario.Activo = activo;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            // Verificar si tiene transacciones
            var tieneTransacciones = await _context.Transacciones
                .AnyAsync(t => t.UsuarioId == id);

            if (tieneTransacciones)
                throw new InvalidOperationException("No se puede eliminar un usuario que tiene transacciones registradas");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Usuario>> BuscarAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodosAsync();

            termino = termino.ToLower();

            return await _context.Usuarios
                .Where(u => u.Documento.ToLower().Contains(termino) ||
                           u.Nombre.ToLower().Contains(termino) ||
                           u.Apellido.ToLower().Contains(termino) ||
                           u.Email.ToLower().Contains(termino))
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteDocumentoAsync(string documento, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.Documento == documento);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteEmailAsync(string email, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.Email == email);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}