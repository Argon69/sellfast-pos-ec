using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SellFast.Core.Data;
using SellFast.Core.Models;

namespace SellFast.Core.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly SellFastContext _context;

        public AuditLogService(SellFastContext context)
        {
            _context = context;
        }

        public async Task RegistrarAccionAsync(string usuario, string accion, string? detalles = null, string tipoModulo = "General")
        {
            try
            {
                var log = new AuditLog
                {
                    FechaHora = DateTime.Now,
                    Usuario = string.IsNullOrWhiteSpace(usuario) ? "admin" : usuario,
                    Accion = accion,
                    Detalles = detalles,
                    TipoModulo = tipoModulo
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registrando auditoría: {ex.Message}");
            }
        }
    }
}
