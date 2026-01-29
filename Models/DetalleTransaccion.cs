using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaUNAL.Models
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
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
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

        // Propiedades calculadas
        [NotMapped]
        public string DescripcionCompleta => $"{Cantidad} x {Producto?.Nombre ?? "Producto"}";

        // Métodos de negocio
        public void CalcularSubtotal()
        {
            Subtotal = PrecioUnitario * Cantidad;
        }

        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");

            Cantidad = nuevaCantidad;
            CalcularSubtotal();
        }
    }
}