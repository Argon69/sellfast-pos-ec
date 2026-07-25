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
    public partial class ComandasViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        public ObservableCollection<Transaccion> ComandasPendientes { get; } = new();

        public ComandasViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarComandasAsync()
        {
            var hoy = DateTime.Today;
            var result = await _context.Transacciones
                .Include(t => t.Cliente)
                .Include(t => t.Mesa)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.FechaHora.Date == hoy && t.Estado == EstadoTransaccion.Completada)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();

            ComandasPendientes.Clear();
            foreach (var c in result) ComandasPendientes.Add(c);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await CargarComandasAsync();
        }
    }
}
