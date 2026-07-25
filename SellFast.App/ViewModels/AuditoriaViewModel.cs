using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;

namespace SellFast.App.ViewModels
{
    public partial class AuditoriaViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isLoading = false;

        public ObservableCollection<AuditLog> AuditLogs { get; } = new();

        public AuditoriaViewModel(SellFastContext context)
        {
            _context = context;
        }

        [RelayCommand]
        public async Task BuscarLogsAsync()
        {
            await CargarAuditLogsAsync();
        }

        public async Task CargarAuditLogsAsync()
        {
            IsLoading = true;
            try
            {
                var query = _context.AuditLogs.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    string term = SearchQuery.ToLower();
                    query = query.Where(a => (a.Accion != null && a.Accion.ToLower().Contains(term)) ||
                                             (a.Usuario != null && a.Usuario.ToLower().Contains(term)) ||
                                             (a.TipoModulo != null && a.TipoModulo.ToLower().Contains(term)) ||
                                             (a.Detalles != null && a.Detalles.ToLower().Contains(term)));
                }

                var list = await query.OrderByDescending(a => a.FechaHora).Take(200).ToListAsync();
                AuditLogs.Clear();
                foreach (var log in list)
                {
                    AuditLogs.Add(log);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando audit logs: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            _ = CargarAuditLogsAsync();
        }
    }
}
