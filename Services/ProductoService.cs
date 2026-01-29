using CafeteriaUNAL.Data;
using CafeteriaUNAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public class ProductoService : IProductoService
    {
        private readonly CafeteriaContext _context;

        public ProductoService(CafeteriaContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerActivosAsync()
        {
            return await _context.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Producto?> ObtenerPorCodigoAsync(string codigo)
        {
            return await _context.Productos
                .FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<List<Producto>> ObtenerPorCategoriaAsync(CategoriaProducto categoria)
        {
            return await _context.Productos
                .Where(p => p.Categoria == categoria && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerMenusDelDiaAsync()
        {
            return await _context.Productos
                .Where(p => p.EsMenuDelDia && p.Activo && p.StockDisponible > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto> CrearAsync(Producto producto)
        {
            // Validar que no exista código duplicado
            if (await ExisteCodigoAsync(producto.Codigo))
                throw new InvalidOperationException($"Ya existe un producto con el código {producto.Codigo}");

            producto.FechaCreacion = DateTime.Now;
            producto.Activo = true;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return producto;
        }

        public async Task<Producto> ActualizarAsync(Producto producto)
        {
            var productoExistente = await _context.Productos.FindAsync(producto.Id);
            if (productoExistente == null)
                throw new InvalidOperationException($"No se encontró el producto con ID {producto.Id}");

            // Validar código único
            if (await ExisteCodigoAsync(producto.Codigo, producto.Id))
                throw new InvalidOperationException($"Ya existe otro producto con el código {producto.Codigo}");

            // Actualizar propiedades
            productoExistente.Codigo = producto.Codigo;
            productoExistente.Nombre = producto.Nombre;
            productoExistente.Descripcion = producto.Descripcion;
            productoExistente.Categoria = producto.Categoria;
            productoExistente.Precio = producto.Precio;
            productoExistente.StockDisponible = producto.StockDisponible;
            productoExistente.StockMinimo = producto.StockMinimo;
            productoExistente.EsMenuDelDia = producto.EsMenuDelDia;
            productoExistente.FechaUltimaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return productoExistente;
        }

        public async Task<bool> ActualizarStockAsync(int id, int cantidad, bool esVenta = true)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return false;

            try
            {
                producto.ActualizarStock(cantidad, esVenta);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CambiarEstadoAsync(int id, bool activo)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return false;

            producto.Activo = activo;
            producto.FechaUltimaModificacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return false;

            // Verificar si tiene transacciones
            var tieneTransacciones = await _context.DetallesTransaccion
                .AnyAsync(d => d.ProductoId == id);

            if (tieneTransacciones)
                throw new InvalidOperationException("No se puede eliminar un producto que tiene ventas registradas");

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Producto>> BuscarAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodosAsync();

            termino = termino.ToLower();

            return await _context.Productos
                .Where(p => p.Codigo.ToLower().Contains(termino) ||
                           p.Nombre.ToLower().Contains(termino) ||
                           (p.Descripcion != null && p.Descripcion.ToLower().Contains(termino)))
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerProductosConStockBajoAsync()
        {
            return await _context.Productos
                .Where(p => p.Activo && p.StockDisponible <= p.StockMinimo)
                .OrderBy(p => p.StockDisponible)
                .ThenBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null)
        {
            var query = _context.Productos.Where(p => p.Codigo == codigo);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}