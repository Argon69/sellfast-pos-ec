using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;

namespace SellFast.App.ViewModels
{
    public partial class ProductosViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _soloStockBajo = false;

        [ObservableProperty]
        private bool _isEditing = false;

        [ObservableProperty]
        private Producto _productoActual = new();

        public ObservableCollection<Producto> Productos { get; } = new();

        public Array CategoriasDisponibles => Enum.GetValues(typeof(CategoriaProducto));

        public ProductosViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarProductosAsync()
        {
            var query = _context.Productos.AsQueryable();

            if (SoloStockBajo)
                query = query.Where(p => p.StockDisponible <= p.StockMinimo);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string term = SearchQuery.ToLower();
                query = query.Where(p => p.Codigo.ToLower().Contains(term) || p.Nombre.ToLower().Contains(term));
            }

            var result = await query.OrderBy(p => p.Nombre).ToListAsync();
            Productos.Clear();
            foreach (var p in result) Productos.Add(p);
        }

        partial void OnSearchQueryChanged(string value) => _ = CargarProductosAsync();
        partial void OnSoloStockBajoChanged(bool value) => _ = CargarProductosAsync();

        [RelayCommand]
        private void NuevoProducto()
        {
            ProductoActual = new Producto
            {
                Codigo = $"PROD-{new Random().Next(1000, 9999)}",
                StockMinimo = 5,
                Activo = true
            };
            IsEditing = true;
        }

        [RelayCommand]
        private void EditarProducto(Producto producto)
        {
            ProductoActual = new Producto
            {
                Id = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Categoria = producto.Categoria,
                Precio = producto.Precio,
                StockDisponible = producto.StockDisponible,
                StockMinimo = producto.StockMinimo,
                EsDestacado = producto.EsDestacado,
                Activo = producto.Activo
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task GuardarProductoAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductoActual.Codigo) || string.IsNullOrWhiteSpace(ProductoActual.Nombre))
            {
                Views.ModernDialogWindow.Show("Validación requerida", "El código y el nombre del producto son obligatorios.", Views.DialogType.Warning);
                return;
            }

            if (ProductoActual.Precio <= 0)
            {
                Views.ModernDialogWindow.Show("Validación de Precio", "El precio del producto debe ser mayor a cero.", Views.DialogType.Warning);
                return;
            }

            try
            {
                if (ProductoActual.Id == 0)
                {
                    if (await _context.Productos.AnyAsync(p => p.Codigo == ProductoActual.Codigo))
                    {
                        Views.ModernDialogWindow.Show("Código Duplicado", "Ya existe un producto registrado con ese código.", Views.DialogType.Error);
                        return;
                    }
                    _context.Productos.Add(ProductoActual);
                }
                else
                {
                    var exist = await _context.Productos.FindAsync(ProductoActual.Id);
                    if (exist != null)
                    {
                        exist.Codigo = ProductoActual.Codigo;
                        exist.Nombre = ProductoActual.Nombre;
                        exist.Descripcion = ProductoActual.Descripcion;
                        exist.Categoria = ProductoActual.Categoria;
                        exist.Precio = ProductoActual.Precio;
                        exist.StockDisponible = ProductoActual.StockDisponible;
                        exist.StockMinimo = ProductoActual.StockMinimo;
                        exist.EsDestacado = ProductoActual.EsDestacado;
                        exist.FechaUltimaModificacion = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                IsEditing = false;
                await CargarProductosAsync();
                Views.ModernDialogWindow.Show("Producto Guardado", $"El producto {ProductoActual.Nombre} fue guardado correctamente.", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error", $"No se pudo guardar el producto: {ex.Message}", Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            IsEditing = false;
        }

        [RelayCommand]
        private async Task AdjustStockAsync(object[] parameters)
        {
            if (parameters.Length == 2 && parameters[0] is Producto p && parameters[1] is string deltaStr && int.TryParse(deltaStr, out int delta))
            {
                var prod = await _context.Productos.FindAsync(p.Id);
                if (prod != null)
                {
                    prod.StockDisponible = Math.Max(0, prod.StockDisponible + delta);
                    prod.FechaUltimaModificacion = DateTime.Now;
                    await _context.SaveChangesAsync();
                    await CargarProductosAsync();
                }
            }
        }

        [RelayCommand]
        private async Task EliminarProductoAsync(Producto producto)
        {
            bool confirmed = Views.ModernDialogWindow.Show(
                "Desactivar Producto",
                $"¿Desea desactivar el producto '{producto.Nombre}'?",
                Views.DialogType.Confirm,
                primaryText: "Sí, Desactivar",
                secondaryText: "Cancelar");

            if (confirmed)
            {
                var prod = await _context.Productos.FindAsync(producto.Id);
                if (prod != null)
                {
                    prod.Activo = false;
                    await _context.SaveChangesAsync();
                    await CargarProductosAsync();
                }
            }
        }
    }
}
