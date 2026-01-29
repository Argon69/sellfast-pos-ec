using CafeteriaUNAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public interface IUsuarioService
    {
        // Obtener todos los usuarios
        Task<List<Usuario>> ObtenerTodosAsync();

        // Obtener usuarios activos
        Task<List<Usuario>> ObtenerActivosAsync();

        // Obtener usuario por ID
        Task<Usuario?> ObtenerPorIdAsync(int id);

        // Obtener usuario por documento
        Task<Usuario?> ObtenerPorDocumentoAsync(string documento);

        // Crear nuevo usuario
        Task<Usuario> CrearAsync(Usuario usuario);

        // Actualizar usuario
        Task<Usuario> ActualizarAsync(Usuario usuario);

        // Activar/Desactivar usuario
        Task<bool> CambiarEstadoAsync(int id, bool activo);

        // Eliminar usuario
        Task<bool> EliminarAsync(int id);

        // Buscar usuarios
        Task<List<Usuario>> BuscarAsync(string termino);

        // Validar si existe documento
        Task<bool> ExisteDocumentoAsync(string documento, int? excludeId = null);

        // Validar si existe email
        Task<bool> ExisteEmailAsync(string email, int? excludeId = null);
    }
}