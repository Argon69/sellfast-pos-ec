using CafeteriaUNAL.Data;
using CafeteriaUNAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public class FichoService : IFichoService
    {
        private readonly CafeteriaContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _maximoFichosPorDia;

        public FichoService(CafeteriaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _maximoFichosPorDia = _configuration.GetValue<int>("AppSettings:MaximoFichosPorDia", 200);
        }

        public async Task<List<Ficho>> ObtenerTodosAsync()
        {
            return await _context.Fichos
                .Include(f => f.Usuario)
                .OrderByDescending(f => f.FechaServicio)
                .ThenBy(f => f.Numero)
                .ToListAsync();
        }

        public async Task<List<Ficho>> ObtenerDelDiaAsync(DateTime? fecha = null)
        {
            var fechaBusqueda = fecha?.Date ?? DateTime.Now.Date;

            return await _context.Fichos
                .Include(f => f.Usuario)
                .Where(f => f.FechaServicio.Date == fechaBusqueda)
                .OrderBy(f => f.Numero)
                .ToListAsync();
        }

        public async Task<Ficho?> ObtenerPorIdAsync(int id)
        {
            return await _context.Fichos
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<Ficho>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Fichos
                .Include(f => f.Usuario)
                .Where(f => f.UsuarioId == usuarioId)
                .OrderByDescending(f => f.FechaServicio)
                .ToListAsync();
        }

        public async Task<Ficho?> ObtenerFichoPendienteUsuarioAsync(int usuarioId, DateTime fecha)
        {
            return await _context.Fichos
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(f =>
                    f.UsuarioId == usuarioId &&
                    f.FechaServicio.Date == fecha.Date &&
                    f.Estado == EstadoFicho.Pendiente);
        }

        public async Task<Ficho> CrearFichoAsync(int usuarioId, DateTime fechaServicio)
        {
            // Validar que no se exceda el límite diario
            var fichosEmitidos = await ObtenerFichosEmitidosAsync(fechaServicio);
            if (fichosEmitidos >= _maximoFichosPorDia)
            {
                throw new InvalidOperationException($"Se ha alcanzado el límite máximo de {_maximoFichosPorDia} fichos para esta fecha");
            }

            // Validar que el usuario no tenga ya un ficho para esa fecha
            if (await UsuarioTieneFichoAsync(usuarioId, fechaServicio))
            {
                throw new InvalidOperationException("El usuario ya tiene un ficho para esta fecha");
            }

            // Obtener el usuario
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario no encontrado");
            }

            // Validar que la fecha no sea pasada
            if (fechaServicio.Date < DateTime.Now.Date)
            {
                throw new InvalidOperationException("No se pueden crear fichos para fechas pasadas");
            }

            // Crear el ficho
            var ficho = new Ficho
            {
                Numero = Ficho.GenerarNumeroFicho(fechaServicio),
                UsuarioId = usuarioId,
                FechaSolicitud = DateTime.Now,
                FechaServicio = fechaServicio.Date,
                Estado = EstadoFicho.Pendiente,
                Usuario = usuario
            };

            _context.Fichos.Add(ficho);
            await _context.SaveChangesAsync();

            return ficho;
        }

        public async Task<bool> MarcarComoUsadoAsync(int id)
        {
            var ficho = await _context.Fichos.FindAsync(id);
            if (ficho == null)
                return false;

            try
            {
                ficho.MarcarComoUsado();
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelarFichoAsync(int id, string motivo)
        {
            var ficho = await _context.Fichos.FindAsync(id);
            if (ficho == null)
                return false;

            try
            {
                ficho.Cancelar(motivo);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> ObtenerFichosDisponiblesAsync(DateTime fecha)
        {
            var emitidos = await ObtenerFichosEmitidosAsync(fecha);
            return Math.Max(0, _maximoFichosPorDia - emitidos);
        }

        public async Task<int> ObtenerFichosEmitidosAsync(DateTime fecha)
        {
            return await _context.Fichos
                .Where(f => f.FechaServicio.Date == fecha.Date && f.Estado != EstadoFicho.Cancelado)
                .CountAsync();
        }

        public async Task<bool> UsuarioTieneFichoAsync(int usuarioId, DateTime fecha)
        {
            return await _context.Fichos
                .AnyAsync(f =>
                    f.UsuarioId == usuarioId &&
                    f.FechaServicio.Date == fecha.Date &&
                    f.Estado != EstadoFicho.Cancelado);
        }

        public async Task<EstadisticasFichos> ObtenerEstadisticasAsync(DateTime fecha)
        {
            var fichos = await ObtenerDelDiaAsync(fecha);

            var estadisticas = new EstadisticasFichos
            {
                TotalEmitidos = fichos.Count(f => f.Estado != EstadoFicho.Cancelado),
                Pendientes = fichos.Count(f => f.Estado == EstadoFicho.Pendiente),
                Usados = fichos.Count(f => f.Estado == EstadoFicho.Usado),
                Cancelados = fichos.Count(f => f.Estado == EstadoFicho.Cancelado),
                Disponibles = await ObtenerFichosDisponiblesAsync(fecha)
            };

            // Agrupar por tipo de usuario
            estadisticas.PorTipoUsuario = fichos
                .Where(f => f.Estado != EstadoFicho.Cancelado && f.Usuario != null)
                .GroupBy(f => f.Usuario!.TipoUsuario.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return estadisticas;
        }
    }
}