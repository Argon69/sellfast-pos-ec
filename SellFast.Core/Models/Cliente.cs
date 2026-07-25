using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    [Table("Clientes")]
    public class Cliente
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

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required]
        public int TipoClienteId { get; set; }

        [StringLength(50)]
        public string? CodigoInterno { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }

        // Campos de auditoría
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;

        // Navegación
        [ForeignKey("TipoClienteId")]
        public virtual TipoCliente? TipoCliente { get; set; }
        public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
        public virtual ICollection<Ficho> Fichos { get; set; } = new List<Ficho>();

        // Propiedades calculadas
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        public decimal ObtenerPorcentajeDescuento()
        {
            return TipoCliente?.PorcentajeDescuento ?? 0;
        }

        public bool EsSubsidiado()
        {
            return TipoCliente?.EsSubsidiado ?? false;
        }
    }
}
