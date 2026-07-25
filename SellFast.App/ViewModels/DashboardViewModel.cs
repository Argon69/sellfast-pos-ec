using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;

namespace SellFast.App.ViewModels
{
    public class RecentActivityItem
    {
        public string Time { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Amount { get; set; } = "";
        public string TagColor { get; set; } = "#6C63FF";
    }

    public partial class DashboardViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private decimal _ventasHoy = 0;

        [ObservableProperty]
        private int _fichosPendientes = 0;

        [ObservableProperty]
        private int _productosBajoStock = 0;

        [ObservableProperty]
        private int _transaccionesHoy = 0;

        [ObservableProperty]
        private ConfiguracionNegocio _config = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _greetingText = "Bienvenido a SellFast POS 👋";

        public ObservableCollection<RecentActivityItem> ActividadReciente { get; } = new();

        public DashboardViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarDashboardAsync()
        {
            IsLoading = true;
            try
            {
                var cfg = await _context.Configuracion.FirstOrDefaultAsync();
                if (cfg != null)
                {
                    Config = cfg;
                    GreetingText = $"Bienvenido a {cfg.NombreNegocio} 👋";
                    Converters.CurrencyFormatterConverter.SimboloMoneda = string.IsNullOrWhiteSpace(cfg.SimboloMoneda) ? "$" : cfg.SimboloMoneda;
                }

                var hoy = DateTime.Today;

                // Ventas de hoy
                VentasHoy = await _context.Transacciones
                    .Where(t => t.FechaHora.Date == hoy && t.Estado == Core.Models.EstadoTransaccion.Completada)
                    .SumAsync(t => (decimal?)t.Total) ?? 0;

                TransaccionesHoy = await _context.Transacciones
                    .CountAsync(t => t.FechaHora.Date == hoy && t.Estado == Core.Models.EstadoTransaccion.Completada);

                // Fichos pendientes
                FichosPendientes = await _context.Fichos
                    .CountAsync(f => f.FechaServicio.Date == hoy && f.Estado == Core.Models.EstadoFicho.Pendiente);

                // Productos bajo stock
                ProductosBajoStock = await _context.Productos
                    .CountAsync(p => p.Activo && p.StockDisponible <= p.StockMinimo);

                // Cargar actividad reciente (últimas 5 transacciones)
                ActividadReciente.Clear();
                var ultimasVentas = await _context.Transacciones
                    .Include(t => t.Cliente)
                    .Where(t => t.FechaHora.Date == hoy)
                    .OrderByDescending(t => t.FechaHora)
                    .Take(5)
                    .ToListAsync();

                foreach (var v in ultimasVentas)
                {
                    ActividadReciente.Add(new RecentActivityItem
                    {
                        Time = v.FechaHora.ToString("HH:mm"),
                        Title = $"Venta #{v.NumeroTransaccion}",
                        Subtitle = $"{v.Cliente?.NombreCompleto ?? "Cliente General"} • {v.TipoPago}",
                        Amount = v.Total.ToString("C0"),
                        TagColor = v.TipoPago == Core.Models.TipoPago.Efectivo ? "#00D9A6" : "#6C63FF"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
