using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaUNAL.Models
{
    public enum RolUsuario
    {
        Administrador = 1,
        Cajero = 2,
        Supervisor = 3,
        Consulta = 4
    }

    public enum EstadoUsuarioSistema
    {
        Activo = 1,
        Inactivo = 2,
        Bloqueado = 3,
        PrimerIngreso = 4
    }

    [Table("UsuariosSistema")]
    public class UsuarioSistema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public RolUsuario Rol { get; set; }

        [Required]
        public EstadoUsuarioSistema Estado { get; set; } = EstadoUsuarioSistema.PrimerIngreso;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaUltimoAcceso { get; set; }
        public DateTime? FechaUltimoCambioPassword { get; set; }
        public int IntentosLoginFallidos { get; set; } = 0;
        public DateTime? FechaBloqueo { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public bool EstaActivo => Estado == EstadoUsuarioSistema.Activo;

        [NotMapped]
        public bool EstaBloqueado => Estado == EstadoUsuarioSistema.Bloqueado ||
            (FechaBloqueo.HasValue && FechaBloqueo.Value.AddHours(24) > DateTime.Now);

        [NotMapped]
        public bool RequiereCambioPassword => Estado == EstadoUsuarioSistema.PrimerIngreso ||
            (FechaUltimoCambioPassword.HasValue && FechaUltimoCambioPassword.Value.AddDays(90) < DateTime.Now);

        // Métodos de negocio
        public void RegistrarAccesoExitoso()
        {
            FechaUltimoAcceso = DateTime.Now;
            IntentosLoginFallidos = 0;
            if (Estado == EstadoUsuarioSistema.PrimerIngreso)
            {
                Estado = EstadoUsuarioSistema.Activo;
            }
        }

        public void RegistrarIntentoFallido()
        {
            IntentosLoginFallidos++;
            if (IntentosLoginFallidos >= 5)
            {
                Estado = EstadoUsuarioSistema.Bloqueado;
                FechaBloqueo = DateTime.Now;
            }
        }

        public void DesbloquearUsuario()
        {
            Estado = EstadoUsuarioSistema.Activo;
            IntentosLoginFallidos = 0;
            FechaBloqueo = null;
        }

        public void CambiarPassword(string nuevoPasswordHash)
        {
            PasswordHash = nuevoPasswordHash;
            FechaUltimoCambioPassword = DateTime.Now;
            if (Estado == EstadoUsuarioSistema.PrimerIngreso)
            {
                Estado = EstadoUsuarioSistema.Activo;
            }
        }
    }
}