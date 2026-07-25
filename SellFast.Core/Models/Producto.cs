using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    public enum CategoriaProducto
    {
        Comida = 1,
        Bebida = 2,
        Postre = 3,
        Snack = 4,
        Combo = 5,
        Otro = 6
    }

    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public CategoriaProducto Categoria { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockDisponible { get; set; }

        [Range(0, int.MaxValue)]
        public int StockMinimo { get; set; } = 5;

        public bool EsDestacado { get; set; } = false;

        [StringLength(500)]
        public string? ImagenRuta { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<DetalleTransaccion> DetallesTransaccion { get; set; } = new List<DetalleTransaccion>();

        // Propiedades calculadas
        [NotMapped]
        public bool BajoStock => StockDisponible <= StockMinimo;

        [NotMapped]
        public string EstadoStock => StockDisponible == 0 ? "Sin Stock" : BajoStock ? "Stock Bajo" : "Disponible";

        public bool PuedeVender(int cantidad) => Activo && StockDisponible >= cantidad;

        public void ActualizarStock(int cantidad, bool esVenta = true)
        {
            if (esVenta)
            {
                if (StockDisponible < cantidad)
                    throw new InvalidOperationException("Stock insuficiente");
                StockDisponible -= cantidad;
            }
            else
            {
                StockDisponible += cantidad;
            }
            FechaUltimaModificacion = DateTime.Now;
        }
    }
}
