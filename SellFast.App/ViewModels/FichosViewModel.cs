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
    public partial class FichosViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private DateTime _fechaFiltro = DateTime.Today;

        [ObservableProperty]
        private int _totalEmitidos = 0;

        [ObservableProperty]
        private int _pendientes = 0;

        [ObservableProperty]
        private int _usados = 0;

        [ObservableProperty]
        private int _cancelados = 0;

        [ObservableProperty]
        private Cliente? _selectedClienteParaFicho;

        public ObservableCollection<Ficho> Fichos { get; } = new();
        public ObservableCollection<Cliente> ClientesDisponibles { get; } = new();

        public FichosViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarFichosAsync()
        {
            var hoy = FechaFiltro.Date;
            var list = await _context.Fichos
                .Include(f => f.Cliente)
                .Where(f => f.FechaServicio.Date == hoy)
                .OrderBy(f => f.Numero)
                .ToListAsync();

            Fichos.Clear();
            foreach (var f in list) Fichos.Add(f);

            TotalEmitidos = Fichos.Count(f => f.Estado != EstadoFicho.Cancelado);
            Pendientes = Fichos.Count(f => f.Estado == EstadoFicho.Pendiente);
            Usados = Fichos.Count(f => f.Estado == EstadoFicho.Usado);
            Cancelados = Fichos.Count(f => f.Estado == EstadoFicho.Cancelado);

            var clientes = await _context.Clientes.Where(c => c.Activo).ToListAsync();
            ClientesDisponibles.Clear();
            foreach (var c in clientes) ClientesDisponibles.Add(c);
            if (SelectedClienteParaFicho == null && ClientesDisponibles.Count > 0)
                SelectedClienteParaFicho = ClientesDisponibles.First();
        }

        partial void OnFechaFiltroChanged(DateTime value) => _ = CargarFichosAsync();

        [RelayCommand]
        private async Task EmitirFichoAsync()
        {
            if (SelectedClienteParaFicho == null)
            {
                Views.ModernDialogWindow.Show("Selección Requerida", "Por favor seleccione un cliente para emitir el ficho.", Views.DialogType.Warning);
                return;
            }

            try
            {
                bool existe = await _context.Fichos.AnyAsync(f => f.ClienteId == SelectedClienteParaFicho.Id && f.FechaServicio.Date == FechaFiltro.Date && f.Estado != EstadoFicho.Cancelado);
                if (existe)
                {
                    Views.ModernDialogWindow.Show("Ficho Existente", "El cliente ya tiene un ficho activo emitido para esta fecha.", Views.DialogType.Warning);
                    return;
                }

                var ficho = new Ficho
                {
                    Numero = Ficho.GenerarNumeroFicho(FechaFiltro),
                    ClienteId = SelectedClienteParaFicho.Id,
                    FechaSolicitud = DateTime.Now,
                    FechaServicio = FechaFiltro.Date,
                    Estado = EstadoFicho.Pendiente
                };

                _context.Fichos.Add(ficho);
                await _context.SaveChangesAsync();
                Views.ModernDialogWindow.Show("Ficho Emitido", $"Se generó correctamente el ficho #{ficho.Numero} para {SelectedClienteParaFicho.NombreCompleto}.", Views.DialogType.Success);
                await CargarFichosAsync();
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error", $"No se pudo emitir el ficho: {ex.Message}", Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private async Task MarcarComoUsadoAsync(Ficho ficho)
        {
            var f = await _context.Fichos.FindAsync(ficho.Id);
            if (f != null && f.Estado == EstadoFicho.Pendiente)
            {
                f.Estado = EstadoFicho.Usado;
                f.FechaUso = DateTime.Now;
                await _context.SaveChangesAsync();
                await CargarFichosAsync();
            }
        }

        [RelayCommand]
        private async Task CancelarFichoAsync(Ficho ficho)
        {
            var f = await _context.Fichos.FindAsync(ficho.Id);
            if (f != null && f.Estado == EstadoFicho.Pendiente)
            {
                f.Estado = EstadoFicho.Cancelado;
                await _context.SaveChangesAsync();
                await CargarFichosAsync();
            }
        }
    }
}
