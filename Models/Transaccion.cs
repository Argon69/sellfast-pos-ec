using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CafeteriaUNAL.Models
{
    public enum TipoPago
    {
        Efectivo = 1,
        Tarjeta = 2,
        Transferencia = 3
    }

    [Table("Transacciones")]
    public class Transaccion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroTransaccion { get; set; } = string.Empty;

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        public TipoPago TipoPago { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoDescuento { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public bool EsSubsidiado { get; set; } = false;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<DetalleTransaccion> Detalles { get; set; } = new List<DetalleTransaccion>();

        // Propiedades calculadas
        [NotMapped]
        public int CantidadProductos => Detalles?.Sum(d => d.Cantidad) ?? 0;

        [NotMapped]
        public string DescripcionPago
        {
            get
            {
                if (EsSubsidiado)
                    return "Subsidiado 100%";
                else if (PorcentajeDescuento > 0)
                    return $"Descuento {PorcentajeDescuento}%";
                else
                    return "Pago Completo";
            }
        }

        // Métodos de negocio
        public void CalcularTotales()
        {
            Subtotal = Detalles.Sum(d => d.Subtotal);

            if (PorcentajeDescuento > 0)
            {
                MontoDescuento = Subtotal * (PorcentajeDescuento / 100);
                Total = Subtotal - MontoDescuento;
            }
            else
            {
                MontoDescuento = 0;
                Total = Subtotal;
            }

            // Si es subsidiado, el total a pagar es 0
            if (EsSubsidiado)
            {
                Total = 0;
            }
        }

        public void AgregarDetalle(Producto producto, int cantidad)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            if (!producto.PuedeVender(cantidad))
                throw new InvalidOperationException($"No hay suficiente stock de {producto.Nombre}");

            var detalle = new DetalleTransaccion
            {
                ProductoId = producto.Id,
                Cantidad = cantidad,
                PrecioUnitario = producto.Precio,
                Subtotal = producto.Precio * cantidad
            };

            Detalles.Add(detalle);
            CalcularTotales();
        }

        // Método estático para generar número de transacción
        public static string GenerarNumeroTransaccion()
        {
            // Formato: TRX-YYYYMMDD-XXXX
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"TRX-{fecha}-{random}";
        }
    }
}