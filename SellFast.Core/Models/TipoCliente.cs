using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    /// <summary>
    /// Tipo de cliente configurable. Reemplaza los enums fijos del sistema anterior.
    /// Permite al negocio definir sus propias categorías (Estudiante, VIP, Regular, Empleado, etc.)
    /// </summary>
    [Table("TiposCliente")]
    public class TipoCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        public bool EsSubsidiado { get; set; } = false;

        [StringLength(7)]
        public string ColorHex { get; set; } = "#6C63FF";

        public bool Activo { get; set; } = true;

        public int Orden { get; set; } = 0;

        // Navegación
        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
