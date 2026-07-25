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
using SellFast.Core.Services;

namespace SellFast.App.ViewModels
{
    public partial class ClientesViewModel : ObservableObject
    {
        private readonly SellFastContext _context;
        private readonly IWhatsAppService _whatsAppService;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isEditing = false;

        [ObservableProperty]
        private Cliente _clienteActual = new();

        public ObservableCollection<Cliente> Clientes { get; } = new();
        public ObservableCollection<TipoCliente> TiposCliente { get; } = new();

        public ClientesViewModel(SellFastContext context, IWhatsAppService whatsAppService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
        }

        public async Task CargarClientesAsync()
        {
            var tipos = await _context.TiposCliente.Where(t => t.Activo).OrderBy(t => t.Orden).ToListAsync();
            TiposCliente.Clear();
            foreach (var t in tipos) TiposCliente.Add(t);

            var query = _context.Clientes.Include(c => c.TipoCliente).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string term = SearchQuery.ToLower();
                query = query.Where(c => c.Documento.ToLower().Contains(term) ||
                                         c.Nombre.ToLower().Contains(term) ||
                                         c.Apellido.ToLower().Contains(term) ||
                                         c.Email.ToLower().Contains(term));
            }

            var result = await query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).ToListAsync();
            Clientes.Clear();
            foreach (var c in result) Clientes.Add(c);
        }

        partial void OnSearchQueryChanged(string value) => _ = CargarClientesAsync();

        [RelayCommand]
        private void NuevoCliente()
        {
            ClienteActual = new Cliente
            {
                TipoClienteId = TiposCliente.FirstOrDefault()?.Id ?? 1,
                Activo = true,
                FechaRegistro = DateTime.Now
            };
            IsEditing = true;
        }

        [RelayCommand]
        private void EditarCliente(Cliente cliente)
        {
            ClienteActual = new Cliente
            {
                Id = cliente.Id,
                Documento = cliente.Documento,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                TipoClienteId = cliente.TipoClienteId,
                CodigoInterno = cliente.CodigoInterno,
                Notas = cliente.Notas,
                Activo = cliente.Activo
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task GuardarClienteAsync()
        {
            if (string.IsNullOrWhiteSpace(ClienteActual.Documento) || string.IsNullOrWhiteSpace(ClienteActual.Nombre))
            {
                Views.ModernDialogWindow.Show("Validación Requerida", "El documento y el nombre son obligatorios.", Views.DialogType.Warning);
                return;
            }

            try
            {
                if (ClienteActual.Id == 0)
                {
                    if (await _context.Clientes.AnyAsync(c => c.Documento == ClienteActual.Documento))
                    {
                        Views.ModernDialogWindow.Show("Documento Registrado", "Ya existe un cliente con ese número de documento.", Views.DialogType.Error);
                        return;
                    }
                    _context.Clientes.Add(ClienteActual);
                }
                else
                {
                    var exist = await _context.Clientes.FindAsync(ClienteActual.Id);
                    if (exist != null)
                    {
                        exist.Documento = ClienteActual.Documento;
                        exist.Nombre = ClienteActual.Nombre;
                        exist.Apellido = ClienteActual.Apellido;
                        exist.Email = ClienteActual.Email;
                        exist.Telefono = ClienteActual.Telefono;
                        exist.TipoClienteId = ClienteActual.TipoClienteId;
                        exist.CodigoInterno = ClienteActual.CodigoInterno;
                        exist.Notas = ClienteActual.Notas;
                    }
                }

                await _context.SaveChangesAsync();
                IsEditing = false;
                await CargarClientesAsync();
                Views.ModernDialogWindow.Show("Cliente Guardado", $"Los datos del cliente {ClienteActual.NombreCompleto} se guardaron con éxito.", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error", $"No se pudo guardar el cliente: {ex.Message}", Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            IsEditing = false;
        }

        [RelayCommand]
        private async Task EliminarClienteAsync(Cliente cliente)
        {
            bool confirmed = Views.ModernDialogWindow.Show(
                "Desactivar Cliente",
                $"¿Desea desactivar al cliente '{cliente.NombreCompleto}'?",
                Views.DialogType.Confirm,
                primaryText: "Sí, Desactivar",
                secondaryText: "Cancelar");

            if (confirmed)
            {
                var cli = await _context.Clientes.FindAsync(cliente.Id);
                if (cli != null)
                {
                    cli.Activo = false;
                    await _context.SaveChangesAsync();
                    await CargarClientesAsync();
                }
            }
        }

        [RelayCommand]
        private async Task EnviarWhatsAppReminderAsync(Cliente cliente)
        {
            if (cliente == null || string.IsNullOrWhiteSpace(cliente.Telefono))
            {
                Views.ModernDialogWindow.Show("WhatsApp", "El cliente seleccionado no tiene número de teléfono registrado.", Views.DialogType.Warning);
                return;
            }

            var cfg = await _context.Configuracion.FirstOrDefaultAsync() ?? new ConfiguracionNegocio();
            _whatsAppService.EnviarRecordatorioPago(cliente.Telefono, cliente.NombreCompleto, 0, cfg.NombreNegocio);
        }
    }
}
