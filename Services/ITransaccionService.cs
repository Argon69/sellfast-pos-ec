using CafeteriaUNAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public interface ITransaccionService
    {
        // Obtener todas las transacciones
        Task<List<Transaccion>> ObtenerTodasAsync();

        // Obtener transacciones del día
        Task<List<Transaccion>> ObtenerDelDiaAsync(DateTime? fecha = null);

        // Obtener transacción por ID con detalles
        Task<Transaccion?> ObtenerPorIdConDetallesAsync(int id);

        // Obtener transacciones por usuario
        Task<List<Transaccion>> ObtenerPorUsuarioAsync(int usuarioId);

        // Obtener transacciones por rango de fechas
        Task<List<Transaccion>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        // Crear nueva transacción
        Task<Transaccion> CrearTransaccionAsync(int usuarioId, List<(int productoId, int cantidad)> items, TipoPago tipoPago);

        // Anular transacción (devolver stock)
        Task<bool> AnularTransaccionAsync(int id, string motivo);

        // Obtener resumen de ventas del día
        Task<ResumenVentas> ObtenerResumenDelDiaAsync(DateTime? fecha = null);

        // Buscar transacciones
        Task<List<Transaccion>> BuscarAsync(string termino);
    }

    public class ResumenVentas
    {
        public int TotalTransacciones { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal TotalSubsidios { get; set; }
        public int TransaccionesEfectivo { get; set; }
        public int TransaccionesTarjeta { get; set; }
        public int TransaccionesTransferencia { get; set; }
        public Dictionary<string, int> VentasPorCategoria { get; set; } = new();
        public List<ProductoMasVendido> ProductosMasVendidos { get; set; } = new();
    }

    public class ProductoMasVendido
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }
}
