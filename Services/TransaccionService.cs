using CafeteriaUNAL.Data;
using CafeteriaUNAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public class TransaccionService : ITransaccionService
    {
        private readonly CafeteriaContext _context;
        private readonly IUsuarioService _usuarioService;
        private readonly IProductoService _productoService;

        public TransaccionService(CafeteriaContext context, IUsuarioService usuarioService, IProductoService productoService)
        {
            _context = context;
            _usuarioService = usuarioService;
            _productoService = productoService;
        }

        public async Task<List<Transaccion>> ObtenerTodasAsync()
        {
            return await _context.Transacciones
                .Include(t => t.Usuario)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<List<Transaccion>> ObtenerDelDiaAsync(DateTime? fecha = null)
        {
            var fechaBusqueda = fecha?.Date ?? DateTime.Now.Date;

            return await _context.Transacciones
                .Include(t => t.Usuario)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.FechaHora.Date == fechaBusqueda)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<Transaccion?> ObtenerPorIdConDetallesAsync(int id)
        {
            return await _context.Transacciones
                .Include(t => t.Usuario)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Transaccion>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Transacciones
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.UsuarioId == usuarioId)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<List<Transaccion>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Transacciones
                .Include(t => t.Usuario)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.FechaHora.Date >= fechaInicio.Date && t.FechaHora.Date <= fechaFin.Date)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<Transaccion> CrearTransaccionAsync(int usuarioId, List<(int productoId, int cantidad)> items, TipoPago tipoPago)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener usuario
                var usuario = await _usuarioService.ObtenerPorIdAsync(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException("Usuario no encontrado");

                // Crear transacción
                var transaccion = new Transaccion
                {
                    NumeroTransaccion = Transaccion.GenerarNumeroTransaccion(),
                    UsuarioId = usuarioId,
                    FechaHora = DateTime.Now,
                    TipoPago = tipoPago,
                    Usuario = usuario
                };

                // Aplicar descuento si es estudiante
                if (usuario.EsEstudiante)
                {
                    transaccion.PorcentajeDescuento = usuario.ObtenerPorcentajeDescuento();
                    transaccion.EsSubsidiado = usuario.ModalidadPago == ModalidadPagoEstudiante.Subsidiado;
                }

                // Agregar productos a la transacción
                foreach (var (productoId, cantidad) in items)
                {
                    var producto = await _productoService.ObtenerPorIdAsync(productoId);
                    if (producto == null)
                        throw new InvalidOperationException($"Producto con ID {productoId} no encontrado");

                    if (!producto.PuedeVender(cantidad))
                        throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}");

                    // Agregar detalle
                    transaccion.AgregarDetalle(producto, cantidad);

                    // Actualizar stock
                    await _productoService.ActualizarStockAsync(productoId, cantidad, esVenta: true);
                }

                // Calcular totales finales
                transaccion.CalcularTotales();

                // Guardar transacción
                _context.Transacciones.Add(transaccion);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return transaccion;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AnularTransaccionAsync(int id, string motivo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var transaccion = await ObtenerPorIdConDetallesAsync(id);
                if (transaccion == null)
                    return false;

                // Devolver stock
                foreach (var detalle in transaccion.Detalles)
                {
                    await _productoService.ActualizarStockAsync(
                        detalle.ProductoId,
                        detalle.Cantidad,
                        esVenta: false);
                }

                // Marcar como anulada (agregando una nota)
                transaccion.Observaciones = $"ANULADA: {motivo} - {DateTime.Now:dd/MM/yyyy HH:mm}";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ResumenVentas> ObtenerResumenDelDiaAsync(DateTime? fecha = null)
        {
            var fechaBusqueda = fecha?.Date ?? DateTime.Now.Date;
            var transacciones = await ObtenerDelDiaAsync(fechaBusqueda);

            var resumen = new ResumenVentas
            {
                TotalTransacciones = transacciones.Count,
                TotalVentas = transacciones.Sum(t => t.Total),
                TotalDescuentos = transacciones.Sum(t => t.MontoDescuento),
                TotalSubsidios = transacciones.Where(t => t.EsSubsidiado).Sum(t => t.Subtotal),
                TransaccionesEfectivo = transacciones.Count(t => t.TipoPago == TipoPago.Efectivo),
                TransaccionesTarjeta = transacciones.Count(t => t.TipoPago == TipoPago.Tarjeta),
                TransaccionesTransferencia = transacciones.Count(t => t.TipoPago == TipoPago.Transferencia)
            };

            // Ventas por categoría
            var ventasPorCategoria = transacciones
                .SelectMany(t => t.Detalles)
                .GroupBy(d => d.Producto!.Categoria.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

            resumen.VentasPorCategoria = ventasPorCategoria;

            // Productos más vendidos
            var productosMasVendidos = transacciones
                .SelectMany(t => t.Detalles)
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre })
                .Select(g => new ProductoMasVendido
                {
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(5)
                .ToList();

            resumen.ProductosMasVendidos = productosMasVendidos;

            return resumen;
        }

        public async Task<List<Transaccion>> BuscarAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodasAsync();

            termino = termino.ToLower();

            return await _context.Transacciones
                .Include(t => t.Usuario)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.NumeroTransaccion.ToLower().Contains(termino) ||
                           t.Usuario!.Nombre.ToLower().Contains(termino) ||
                           t.Usuario.Apellido.ToLower().Contains(termino) ||
                           t.Usuario.Documento.ToLower().Contains(termino))
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }
    }
}