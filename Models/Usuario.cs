using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaUNAL.Models
{
    public enum TipoUsuario
    {
        Estudiante = 1,
        Profesor = 2,
        Administrativo = 3,
        Trabajador = 4,
        Visitante = 5
    }

    public enum ModalidadPagoEstudiante
    {
        Completo = 1,      // Paga 100%
        ConDescuento = 2,  // Paga 50%
        Subsidiado = 3     // No paga (100% subsidiado)
    }

    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El documento es obligatorio")]
        [StringLength(20)]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required]
        public TipoUsuario TipoUsuario { get; set; }

        // Solo aplica para estudiantes
        public ModalidadPagoEstudiante? ModalidadPago { get; set; }

        [StringLength(50)]
        public string? CodigoEstudiante { get; set; }

        // Campos de auditoría
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
        public virtual ICollection<Ficho> Fichos { get; set; } = new List<Ficho>();

        // Propiedades calculadas
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [NotMapped]
        public bool EsEstudiante => TipoUsuario == TipoUsuario.Estudiante;

        // Método para calcular el porcentaje de descuento
        public decimal ObtenerPorcentajeDescuento()
        {
            if (!EsEstudiante || !ModalidadPago.HasValue)
                return 0;

            return ModalidadPago.Value switch
            {
                ModalidadPagoEstudiante.Completo => 0,
                ModalidadPagoEstudiante.ConDescuento => 50,
                ModalidadPagoEstudiante.Subsidiado => 100,
                _ => 0
            };
        }
    }
}