using CafeteriaUNAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public interface IFichoService
    {
        // Obtener todos los fichos
        Task<List<Ficho>> ObtenerTodosAsync();

        // Obtener fichos del día
        Task<List<Ficho>> ObtenerDelDiaAsync(DateTime? fecha = null);

        // Obtener ficho por ID
        Task<Ficho?> ObtenerPorIdAsync(int id);

        // Obtener fichos por usuario
        Task<List<Ficho>> ObtenerPorUsuarioAsync(int usuarioId);

        // Obtener ficho pendiente de un usuario para una fecha
        Task<Ficho?> ObtenerFichoPendienteUsuarioAsync(int usuarioId, DateTime fecha);

        // Crear nuevo ficho
        Task<Ficho> CrearFichoAsync(int usuarioId, DateTime fechaServicio);

        // Marcar ficho como usado
        Task<bool> MarcarComoUsadoAsync(int id);

        // Cancelar ficho
        Task<bool> CancelarFichoAsync(int id, string motivo);

        // Obtener cantidad de fichos disponibles para una fecha
        Task<int> ObtenerFichosDisponiblesAsync(DateTime fecha);

        // Obtener cantidad de fichos emitidos para una fecha
        Task<int> ObtenerFichosEmitidosAsync(DateTime fecha);

        // Verificar si un usuario ya tiene ficho para una fecha
        Task<bool> UsuarioTieneFichoAsync(int usuarioId, DateTime fecha);

        // Obtener estadísticas de fichos
        Task<EstadisticasFichos> ObtenerEstadisticasAsync(DateTime fecha);
    }

    public class EstadisticasFichos
    {
        public int TotalEmitidos { get; set; }
        public int Pendientes { get; set; }
        public int Usados { get; set; }
        public int Cancelados { get; set; }
        public int Disponibles { get; set; }
        public Dictionary<string, int> PorTipoUsuario { get; set; } = new();
    }
}
