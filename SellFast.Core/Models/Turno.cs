using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    [Table("Turnos")]
    public class Turno
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioSistemaId { get; set; }

        [Required]
        public DateTime HoraInicio { get; set; } = DateTime.Now;

        public DateTime? HoraFin { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(200)]
        public string? Notas { get; set; }

        // Navegación
        [ForeignKey("UsuarioSistemaId")]
        public virtual UsuarioSistema? UsuarioSistema { get; set; }

        [NotMapped]
        public bool EstaActivo => Activo && !HoraFin.HasValue;

        [NotMapped]
        public TimeSpan Duracion => (HoraFin ?? DateTime.Now) - HoraInicio;

        public void Finalizar(string? notas = null)
        {
            HoraFin = DateTime.Now;
            Activo = false;
            if (notas != null) Notas = notas;
        }
    }
}
