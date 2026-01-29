using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaUNAL.Models
{
    public enum CategoriaProducto
    {
        Desayuno = 1,
        Almuerzo = 2,
        Cena = 3,
        Bebida = 4,
        Snack = 5,
        Postre = 6
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
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int StockDisponible { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; } = 5;

        public bool EsMenuDelDia { get; set; } = false;

        // Campos de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<DetalleTransaccion> DetallesTransaccion { get; set; } = new List<DetalleTransaccion>();


        // Propiedades calculadas
        [NotMapped]
        public bool BajoStock => StockDisponible <= StockMinimo;

        [NotMapped]
        public string EstadoStock
        {
            get
            {
                if (StockDisponible == 0) return "Sin Stock";
                if (BajoStock) return "Stock Bajo";
                return "Disponible";
            }
        }

        // Métodos de negocio
        public bool PuedeVender(int cantidad)
        {
            return Activo && StockDisponible >= cantidad;
        }

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