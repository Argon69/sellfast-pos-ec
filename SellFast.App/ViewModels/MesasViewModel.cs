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
    public partial class MesasViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private Mesa? _selectedMesa;

        [ObservableProperty]
        private string _nuevaMesaNumero = "";

        [ObservableProperty]
        private string _nuevaMesaZona = "Principal";

        [ObservableProperty]
        private int _nuevaMesaCapacidad = 4;

        [ObservableProperty]
        private bool _isAddingMesa = false;

        public ObservableCollection<Mesa> Mesas { get; } = new();
        public ObservableCollection<string> SubcuentasMesa { get; } = new();

        [ObservableProperty]
        private string? _selectedSubcuenta = "Cuenta Principal";

        [ObservableProperty]
        private string _nuevaSubcuentaNombre = "";

        [ObservableProperty]
        private bool _isAddingSubcuenta = false;

        public Array EstadosMesaDisponibles => Enum.GetValues(typeof(EstadoMesa));

        public MesasViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarMesasAsync()
        {
            var result = await _context.Mesas.Where(m => m.Activa).OrderBy(m => m.Numero).ToListAsync();

            if (result.Count == 0)
            {
                // Seed initial tables if none exist
                for (int i = 1; i <= 8; i++)
                {
                    _context.Mesas.Add(new Mesa
                    {
                        Numero = $"Mesa {i}",
                        Zona = i <= 4 ? "Salón Principal" : "Terraza",
                        Capacidad = i % 2 == 0 ? 4 : 2,
                        Estado = i == 2 ? EstadoMesa.Ocupada : i == 4 ? EstadoMesa.CuentaPedida : EstadoMesa.Libre
                    });
                }
                await _context.SaveChangesAsync();
                result = await _context.Mesas.Where(m => m.Activa).OrderBy(m => m.Numero).ToListAsync();
            }

            Mesas.Clear();
            foreach (var m in result) Mesas.Add(m);

            if (SelectedMesa == null && Mesas.Count > 0)
                SelectedMesa = Mesas.First();

            await CargarSubcuentasDeMesaAsync();
        }

        partial void OnSelectedMesaChanged(Mesa? value)
        {
            _ = CargarSubcuentasDeMesaAsync();
        }

        public async Task CargarSubcuentasDeMesaAsync()
        {
            SubcuentasMesa.Clear();
            SubcuentasMesa.Add("Cuenta Principal");

            if (SelectedMesa != null)
            {
                var subcuentas = await _context.Transacciones
                    .Where(t => t.MesaId == SelectedMesa.Id && !string.IsNullOrEmpty(t.NombreSubcuenta))
                    .Select(t => t.NombreSubcuenta!)
                    .Distinct()
                    .ToListAsync();

                foreach (var sc in subcuentas)
                {
                    if (!SubcuentasMesa.Contains(sc))
                        SubcuentasMesa.Add(sc);
                }
            }
            SelectedSubcuenta = SubcuentasMesa.FirstOrDefault();
        }

        [RelayCommand]
        private void SelectMesa(Mesa mesa)
        {
            SelectedMesa = mesa;
        }

        [RelayCommand]
        private void ToggleAddSubcuenta()
        {
            NuevaSubcuentaNombre = $"Cuenta {SubcuentasMesa.Count + 1}";
            IsAddingSubcuenta = !IsAddingSubcuenta;
        }

        [RelayCommand]
        private void GuardarNuevaSubcuenta()
        {
            if (string.IsNullOrWhiteSpace(NuevaSubcuentaNombre))
            {
                Views.ModernDialogWindow.Show("Validación Requerida", "El nombre de la subcuenta es obligatorio (ej. Cuenta 2 o Nombre del Comensal).", Views.DialogType.Warning);
                return;
            }

            string nombre = NuevaSubcuentaNombre.Trim();
            if (!SubcuentasMesa.Contains(nombre))
            {
                SubcuentasMesa.Add(nombre);
            }
            SelectedSubcuenta = nombre;
            IsAddingSubcuenta = false;
            Views.ModernDialogWindow.Show("Subcuenta Creada", $"Subcuenta '{nombre}' agregada a {SelectedMesa?.Numero}.", Views.DialogType.Success);
        }

        [RelayCommand]
        private async Task CambiarEstadoMesaAsync(EstadoMesa nuevoEstado)
        {
            if (SelectedMesa != null)
            {
                var mesa = await _context.Mesas.FindAsync(SelectedMesa.Id);
                if (mesa != null)
                {
                    mesa.Estado = nuevoEstado;
                    await _context.SaveChangesAsync();
                    await CargarMesasAsync();
                }
            }
        }

        [RelayCommand]
        private void ToggleAddMesa()
        {
            NuevaMesaNumero = $"Mesa {Mesas.Count + 1}";
            IsAddingMesa = !IsAddingMesa;
        }

        [RelayCommand]
        private async Task GuardarNuevaMesaAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevaMesaNumero))
            {
                Views.ModernDialogWindow.Show("Validación Requerida", "El número o identificador de la mesa es obligatorio.", Views.DialogType.Warning);
                return;
            }

            try
            {
                var mesa = new Mesa
                {
                    Numero = NuevaMesaNumero.Trim(),
                    Zona = string.IsNullOrWhiteSpace(NuevaMesaZona) ? "Salón" : NuevaMesaZona.Trim(),
                    Capacidad = Math.Max(1, NuevaMesaCapacidad),
                    Estado = EstadoMesa.Libre,
                    Activa = true
                };

                _context.Mesas.Add(mesa);
                await _context.SaveChangesAsync();
                IsAddingMesa = false;
                await CargarMesasAsync();
                Views.ModernDialogWindow.Show("Mesa Registrada", $"La mesa '{mesa.Numero}' se agregó correctamente al plano.", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error", $"No se pudo crear la mesa: {ex.Message}", Views.DialogType.Error);
            }
        }
    }
}
