using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;

namespace SellFast.App.ViewModels
{
    public partial class CartItem : ObservableObject
    {
        public Producto Producto { get; set; } = null!;

        [ObservableProperty]
        private int _cantidad = 1;

        public decimal PrecioUnitario => Producto.Precio;
        public decimal Subtotal => PrecioUnitario * Cantidad;

        partial void OnCantidadChanged(int value)
        {
            OnPropertyChanged(nameof(Subtotal));
        }
    }

    public partial class PosViewModel : ObservableObject
    {
        private readonly SellFastContext _context;
        private readonly IPdfReceiptService _pdfReceiptService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IAuditLogService _auditLogService;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private CategoriaProducto? _selectedCategoriaFilter = null;

        [ObservableProperty]
        private Cliente? _selectedCliente;

        [ObservableProperty]
        private Mesa? _selectedMesa;

        [ObservableProperty]
        private string _nombreSubcuenta = "Cuenta Principal";

        [ObservableProperty]
        private TipoPago _selectedTipoPago = TipoPago.Efectivo;

        [ObservableProperty]
        private decimal _descuentoPorcentaje = 0;

        [ObservableProperty]
        private decimal _propina = 0;

        [ObservableProperty]
        private bool _isProcessing = false;

        [ObservableProperty]
        private bool _isSplitBillActive = false;

        [ObservableProperty]
        private int _partesSplitBill = 2;

        [ObservableProperty]
        private decimal _totalParteDividida = 0;

        public ObservableCollection<Producto> ProductosFiltrados { get; } = new();
        public ObservableCollection<Cliente> ClientesDisponibles { get; } = new();
        public ObservableCollection<Mesa> MesasDisponibles { get; } = new();
        public ObservableCollection<string> SubcuentasMesa { get; } = new();
        public ObservableCollection<CartItem> Cart { get; } = new();

        public decimal CartSubtotal => Cart.Sum(i => i.Subtotal);
        public decimal CartMontoDescuento => CartSubtotal * (DescuentoPorcentaje / 100);
        public decimal CartTotal => Math.Max(0, CartSubtotal - CartMontoDescuento + Propina);

        public PosViewModel(SellFastContext context, IPdfReceiptService pdfReceiptService, IWhatsAppService whatsAppService, IAuditLogService auditLogService)
        {
            _context = context;
            _pdfReceiptService = pdfReceiptService;
            _whatsAppService = whatsAppService;
            _auditLogService = auditLogService;
            Cart.CollectionChanged += (s, e) => RecalcularTotales();
        }

        public async Task CargarDatosAsync()
        {
            var productos = await _context.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ProductosFiltrados.Clear();
            foreach (var p in productos)
                ProductosFiltrados.Add(p);

            var clientes = await _context.Clientes
                .Include(c => c.TipoCliente)
                .Where(c => c.Activo)
                .ToListAsync();

            ClientesDisponibles.Clear();
            foreach (var c in clientes)
                ClientesDisponibles.Add(c);

            if (SelectedCliente == null && ClientesDisponibles.Count > 0)
                SelectedCliente = ClientesDisponibles.First();

            var mesas = await _context.Mesas
                .Where(m => m.Activa)
                .OrderBy(m => m.Numero)
                .ToListAsync();

            MesasDisponibles.Clear();
            foreach (var m in mesas)
                MesasDisponibles.Add(m);
        }

        partial void OnSearchQueryChanged(string value) => _ = FiltrarProductosAsync();
        partial void OnSelectedCategoriaFilterChanged(CategoriaProducto? value) => _ = FiltrarProductosAsync();

        [RelayCommand]
        private async Task EscanearCodigoAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;

            string term = SearchQuery.Trim().ToLower();
            var exactMatch = await _context.Productos.FirstOrDefaultAsync(p => p.Activo && p.Codigo.ToLower() == term);

            if (exactMatch != null)
            {
                AddToCart(exactMatch);
                SearchQuery = "";
            }
        }

        partial void OnSelectedClienteChanged(Cliente? value)
        {
            if (value != null && value.TipoCliente != null)
            {
                DescuentoPorcentaje = value.TipoCliente.PorcentajeDescuento;
            }
            RecalcularTotales();
        }

        partial void OnDescuentoPorcentajeChanged(decimal value) => RecalcularTotales();
        partial void OnPropinaChanged(decimal value) => RecalcularTotales();

        private async Task FiltrarProductosAsync()
        {
            var query = _context.Productos.Where(p => p.Activo);

            if (SelectedCategoriaFilter.HasValue)
                query = query.Where(p => p.Categoria == SelectedCategoriaFilter.Value);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string term = SearchQuery.ToLower();
                query = query.Where(p => p.Codigo.ToLower().Contains(term) || p.Nombre.ToLower().Contains(term));
            }

            var result = await query.ToListAsync();
            ProductosFiltrados.Clear();
            foreach (var p in result) ProductosFiltrados.Add(p);
        }

        [RelayCommand]
        private void AddToCart(Producto producto)
        {
            var existing = Cart.FirstOrDefault(i => i.Producto.Id == producto.Id);
            if (existing != null)
            {
                if (existing.Cantidad < producto.StockDisponible)
                    existing.Cantidad++;
                else
                    Views.ModernDialogWindow.Show("Límite de Stock", $"Se ha alcanzado el stock disponible para {producto.Nombre}", Views.DialogType.Warning);
            }
            else
            {
                if (producto.StockDisponible > 0)
                    Cart.Add(new CartItem { Producto = producto, Cantidad = 1 });
                else
                    Views.ModernDialogWindow.Show("Sin Stock", $"El producto {producto.Nombre} no tiene unidades disponibles", Views.DialogType.Warning);
            }
            RecalcularTotales();
        }

        [RelayCommand]
        private void IncrementCartItem(CartItem item)
        {
            if (item.Cantidad < item.Producto.StockDisponible)
                item.Cantidad++;
            RecalcularTotales();
        }

        [RelayCommand]
        private void DecrementCartItem(CartItem item)
        {
            if (item.Cantidad > 1)
                item.Cantidad--;
            else
                Cart.Remove(item);
            RecalcularTotales();
        }

        [RelayCommand]
        private void RemoveFromCart(CartItem item)
        {
            Cart.Remove(item);
            RecalcularTotales();
        }

        [RelayCommand]
        private void ClearCart()
        {
            Cart.Clear();
            RecalcularTotales();
        }

        [RelayCommand]
        private async Task ProcesarPagoAsync()
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("El carrito está vacío", "Venta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCliente == null)
            {
                MessageBox.Show("Seleccione un cliente", "Venta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsProcessing = true;
            try
            {
                var transaccion = new Transaccion
                {
                    NumeroTransaccion = Transaccion.GenerarNumeroTransaccion(),
                    ClienteId = SelectedCliente.Id,
                    MesaId = SelectedMesa?.Id,
                    NombreSubcuenta = string.IsNullOrWhiteSpace(NombreSubcuenta) ? "Cuenta Principal" : NombreSubcuenta.Trim(),
                    FechaHora = DateTime.Now,
                    TipoPago = SelectedTipoPago,
                    Estado = EstadoTransaccion.Completada,
                    PorcentajeDescuento = DescuentoPorcentaje,
                    Propina = Propina,
                    EsSubsidiado = SelectedCliente.EsSubsidiado()
                };

                foreach (var item in Cart)
                {
                    var producto = await _context.Productos.FindAsync(item.Producto.Id);
                    if (producto != null)
                    {
                        transaccion.AgregarDetalle(producto, item.Cantidad);
                        producto.ActualizarStock(item.Cantidad, esVenta: true);
                    }
                }

                transaccion.CalcularTotales();
                _context.Transacciones.Add(transaccion);
                await _context.SaveChangesAsync();

                // Get business config for PDF receipt
                var config = await _context.Configuracion.FirstOrDefaultAsync() ?? new ConfiguracionNegocio();

                // Generate PDF receipt
                string pdfPath = await _pdfReceiptService.GenerarComprobantePdfAsync(transaccion, config);

                // Audit Log
                _ = _auditLogService.RegistrarAccionAsync(
                    "admin", 
                    "Venta Completada", 
                    $"Transacción #{transaccion.NumeroTransaccion} por ${transaccion.Total:N0} (Cliente: {SelectedCliente.NombreCompleto})", 
                    "POS");

                var clienteTel = SelectedCliente.Telefono;
                var clienteNombre = SelectedCliente.NombreCompleto;
                var trxNum = transaccion.NumeroTransaccion;
                var totalVal = transaccion.Total;
                var negocioNom = config.NombreNegocio;

                ClearCart();
                await CargarDatosAsync();

                bool sendWa = Views.ModernDialogWindow.Show(
                    "¡Venta Procesada con Éxito!",
                    $"La venta #{trxNum} por ${totalVal:N0} fue registrada con éxito.\n\nSe ha generado el comprobante PDF.\n¿Desea enviar una notificación por WhatsApp al cliente?",
                    Views.DialogType.ReceiptSuccess,
                    primaryText: "💬 Enviar WhatsApp",
                    secondaryText: "📄 Abrir PDF",
                    pdfPath: pdfPath);

                if (sendWa)
                {
                    _whatsAppService.EnviarComprobanteVenta(clienteTel, trxNum, totalVal, clienteNombre, negocioNom);
                }
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error de Venta", $"No se pudo procesar la venta: {ex.Message}", Views.DialogType.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private void ToggleSplitBill()
        {
            IsSplitBillActive = !IsSplitBillActive;
            RecalcularTotales();
        }

        [RelayCommand]
        private void AplicarDivisionPartes(string partesStr)
        {
            if (int.TryParse(partesStr, out int partes) && partes >= 2)
            {
                PartesSplitBill = partes;
                IsSplitBillActive = true;
                RecalcularTotales();
                Views.ModernDialogWindow.Show("División Aplicada", $"Cuenta dividida en {partes} partes iguales de ${TotalParteDividida:N0} c/u.", Views.DialogType.Success);
            }
        }

        public void RecalcularTotales()
        {
            OnPropertyChanged(nameof(CartSubtotal));
            OnPropertyChanged(nameof(CartMontoDescuento));
            OnPropertyChanged(nameof(CartTotal));
            TotalParteDividida = PartesSplitBill > 0 ? Math.Round(CartTotal / Math.Max(1, PartesSplitBill), 2) : CartTotal;
            OnPropertyChanged(nameof(TotalParteDividida));
        }
    }
}
