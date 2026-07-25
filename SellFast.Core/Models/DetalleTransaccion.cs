using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    [Table("DetallesTransaccion")]
    public class DetalleTransaccion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TransaccionId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }

        // Navegación
        [ForeignKey("TransaccionId")]
        public virtual Transaccion? Transaccion { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }

        [NotMapped]
        public string DescripcionCompleta => $"{Cantidad} x {Producto?.Nombre ?? "Producto"}";

        public void CalcularSubtotal() => Subtotal = PrecioUnitario * Cantidad;
    }
}
