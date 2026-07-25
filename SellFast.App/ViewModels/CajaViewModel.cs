using System;
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
    public partial class CajaViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private CajaDiaria? _cajaActual;

        [ObservableProperty]
        private decimal _montoInicialApertura = 50000;

        [ObservableProperty]
        private decimal _montoContadoCierre = 0;

        [ObservableProperty]
        private string _observacionesCierre = "";

        [ObservableProperty]
        private decimal _ventasEfectivoHoy = 0;

        [ObservableProperty]
        private decimal _ventasTarjetaHoy = 0;

        [ObservableProperty]
        private decimal _ventasOtrosHoy = 0;

        [ObservableProperty]
        private decimal _totalVentasHoy = 0;

        [ObservableProperty]
        private bool _isCajaAbierta = false;

        [ObservableProperty]
        private string _textoEstadoCaja = "Caja Cerrada";

        public CajaViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarCajaAsync()
        {
            var hoy = DateTime.Today;

            CajaActual = await _context.CajasDiarias
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.FechaApertura.Date == hoy && c.Estado == EstadoCaja.Abierta);

            IsCajaAbierta = CajaActual != null;
            TextoEstadoCaja = IsCajaAbierta ? "Caja del Día Abierta" : "Caja del Día Cerrada";

            var transacciones = await _context.Transacciones
                .Where(t => t.FechaHora.Date == hoy && t.Estado == EstadoTransaccion.Completada)
                .ToListAsync();

            VentasEfectivoHoy = transacciones.Where(t => t.TipoPago == TipoPago.Efectivo).Sum(t => t.Total);
            VentasTarjetaHoy = transacciones.Where(t => t.TipoPago == TipoPago.Tarjeta).Sum(t => t.Total);
            VentasOtrosHoy = transacciones.Where(t => t.TipoPago != TipoPago.Efectivo && t.TipoPago != TipoPago.Tarjeta).Sum(t => t.Total);
            TotalVentasHoy = transacciones.Sum(t => t.Total);

            if (CajaActual != null)
            {
                MontoContadoCierre = CajaActual.MontoInicial + VentasEfectivoHoy;
            }
            else
            {
                MontoContadoCierre = 0;
            }
        }

        [RelayCommand]
        private async Task AbrirCajaAsync()
        {
            try
            {
                var nuevaCaja = new CajaDiaria
                {
                    FechaApertura = DateTime.Now,
                    UsuarioAperturaId = 1,
                    MontoInicial = Math.Max(0, MontoInicialApertura),
                    Estado = EstadoCaja.Abierta
                };

                _context.CajasDiarias.Add(nuevaCaja);
                await _context.SaveChangesAsync();
                Views.ModernDialogWindow.Show("Caja Diaria Abierta", $"La caja fue abierta correctamente con una base inicial de {nuevaCaja.MontoInicial:C0}.", Views.DialogType.Success);
                await CargarCajaAsync();
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error al Abrir Caja", ex.Message, Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private async Task CerrarCajaAsync()
        {
            if (CajaActual == null) return;

            bool confirmed = Views.ModernDialogWindow.Show(
                "Arqueo y Cierre de Caja",
                "¿Está seguro de que desea realizar el cierre y arqueo de la caja diaria?",
                Views.DialogType.Confirm,
                primaryText: "Sí, Cerrar Caja",
                secondaryText: "Cancelar");

            if (confirmed)
            {
                try
                {
                    var caja = await _context.CajasDiarias.FindAsync(CajaActual.Id);
                    if (caja != null)
                    {
                        caja.Cerrar(MontoContadoCierre, ObservacionesCierre);
                        await _context.SaveChangesAsync();
                        Views.ModernDialogWindow.Show("Caja Cerrada", $"El arqueo de caja fue completado.\n\nDiferencia calculada: {caja.Diferencia:C0}", Views.DialogType.Success);
                        await CargarCajaAsync();
                    }
                }
                catch (Exception ex)
                {
                    Views.ModernDialogWindow.Show("Error de Cierre", ex.Message, Views.DialogType.Error);
                }
            }
        }
    }
}
