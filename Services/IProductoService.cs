using CafeteriaUNAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CafeteriaUNAL.Services
{
    public interface IProductoService
    {
        // Obtener todos los productos
        Task<List<Producto>> ObtenerTodosAsync();

        // Obtener productos activos
        Task<List<Producto>> ObtenerActivosAsync();

        // Obtener producto por ID
        Task<Producto?> ObtenerPorIdAsync(int id);

        // Obtener producto por código
        Task<Producto?> ObtenerPorCodigoAsync(string codigo);

        // Obtener productos por categoría
        Task<List<Producto>> ObtenerPorCategoriaAsync(CategoriaProducto categoria);

        // Obtener menús del día
        Task<List<Producto>> ObtenerMenusDelDiaAsync();

        // Crear nuevo producto
        Task<Producto> CrearAsync(Producto producto);

        // Actualizar producto
        Task<Producto> ActualizarAsync(Producto producto);

        // Actualizar stock
        Task<bool> ActualizarStockAsync(int id, int cantidad, bool esVenta = true);

        // Activar/Desactivar producto
        Task<bool> CambiarEstadoAsync(int id, bool activo);

        // Eliminar producto
        Task<bool> EliminarAsync(int id);

        // Buscar productos
        Task<List<Producto>> BuscarAsync(string termino);

        // Obtener productos con stock bajo
        Task<List<Producto>> ObtenerProductosConStockBajoAsync();

        // Validar si existe código
        Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null);
    }
}