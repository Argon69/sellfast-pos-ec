using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SellFast.Core.Models
{
    public enum TipoPago
    {
        Efectivo = 1,
        Tarjeta = 2,
        Transferencia = 3,
        QR = 4
    }

    public enum EstadoTransaccion
    {
        Completada = 1,
        Anulada = 2,
        Pendiente = 3
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
        public int ClienteId { get; set; }

        public int? MesaId { get; set; }

        [Required]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        public TipoPago TipoPago { get; set; }

        [Required]
        public EstadoTransaccion Estado { get; set; } = EstadoTransaccion.Completada;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoDescuento { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Propina { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public bool EsSubsidiado { get; set; } = false;

        public int? CajaDiariaId { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("MesaId")]
        public virtual Mesa? Mesa { get; set; }

        [ForeignKey("CajaDiariaId")]
        public virtual CajaDiaria? CajaDiaria { get; set; }

        public virtual ICollection<DetalleTransaccion> Detalles { get; set; } = new List<DetalleTransaccion>();

        // Propiedades calculadas
        [NotMapped]
        public int CantidadProductos => Detalles?.Sum(d => d.Cantidad) ?? 0;

        [NotMapped]
        public bool EstaAnulada => Estado == EstadoTransaccion.Anulada;

        public void CalcularTotales()
        {
            Subtotal = Detalles.Sum(d => d.Subtotal);
            if (PorcentajeDescuento > 0)
            {
                MontoDescuento = Subtotal * (PorcentajeDescuento / 100);
                Total = Subtotal - MontoDescuento + Propina;
            }
            else
            {
                MontoDescuento = 0;
                Total = Subtotal + Propina;
            }
            if (EsSubsidiado) Total = Propina;
        }

        public void AgregarDetalle(Producto producto, int cantidad)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));
            if (!producto.PuedeVender(cantidad))
                throw new InvalidOperationException($"No hay suficiente stock de {producto.Nombre}");

            var detalle = new DetalleTransaccion
            {
                ProductoId = producto.Id,
                Producto = producto,
                Cantidad = cantidad,
                PrecioUnitario = producto.Precio,
                Subtotal = producto.Precio * cantidad
            };
            Detalles.Add(detalle);
            CalcularTotales();
        }

        public static string GenerarNumeroTransaccion()
        {
            return $"SF-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}
