using System;

namespace SellFast.Core.Models
{
    public class SesionUsuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public string AvatarColor { get; set; } = "#6C63FF";
        public DateTime FechaInicioSesion { get; set; } = DateTime.Now;
        public DateTime? FechaFinSesion { get; set; }
        public bool SesionActiva => !FechaFinSesion.HasValue;
        public string Iniciales => string.Join("", NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(s => s[0])).ToUpper();

        // Permisos por rol
        public bool PuedeGestionarUsuarios => Rol == RolUsuario.Administrador;
        public bool PuedeGestionarProductos => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeRealizarVentas => Rol != RolUsuario.Consulta && Rol != RolUsuario.Cocina;
        public bool PuedeVerReportes => true;
        public bool PuedeGenerarReportesAvanzados => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeConfigurarSistema => Rol == RolUsuario.Administrador;
        public bool PuedeGestionarFichos => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeAnularTransacciones => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeVerHistorialCompleto => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeGestionarMesas => Rol != RolUsuario.Consulta && Rol != RolUsuario.Cocina;
        public bool PuedeVerCocina => Rol == RolUsuario.Administrador || Rol == RolUsuario.Cocina || Rol == RolUsuario.Supervisor;
        public bool PuedeGestionarCaja => Rol == RolUsuario.Administrador || Rol == RolUsuario.Cajero || Rol == RolUsuario.Supervisor;
        public bool PuedeGestionarTurnos => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;

        public void CerrarSesion()
        {
            FechaFinSesion = DateTime.Now;
        }
    }
}
