using System;

namespace CafeteriaUNAL.Models
{
    public class SesionUsuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public DateTime FechaInicioSesion { get; set; } = DateTime.Now;
        public DateTime? FechaFinSesion { get; set; }
        public bool SesionActiva => !FechaFinSesion.HasValue;

        // Permisos por rol
        public bool PuedeGestionarUsuarios => Rol == RolUsuario.Administrador;
        public bool PuedeGestionarProductos => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeRealizarVentas => Rol != RolUsuario.Consulta;
        public bool PuedeVerReportes => true; // Todos pueden ver reportes
        public bool PuedeGenerarReportesAvanzados => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeConfigurarSistema => Rol == RolUsuario.Administrador;
        public bool PuedeGestionarFichos => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeAnularTransacciones => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;
        public bool PuedeVerHistorialCompleto => Rol == RolUsuario.Administrador || Rol == RolUsuario.Supervisor;

        public void CerrarSesion()
        {
            FechaFinSesion = DateTime.Now;
        }
    }
}