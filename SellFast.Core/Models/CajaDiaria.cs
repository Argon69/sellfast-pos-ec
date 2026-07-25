using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    public enum EstadoCaja
    {
        Abierta = 1,
        Cerrada = 2
    }

    [Table("CajasDiarias")]
    public class CajaDiaria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaApertura { get; set; } = DateTime.Now;

        public DateTime? FechaCierre { get; set; }

        [Required]
        public int UsuarioAperturaId { get; set; }

        public int? UsuarioCierreId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoInicial { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoEsperado { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoReal { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Diferencia { get; set; } = 0;

        [Required]
        public EstadoCaja Estado { get; set; } = EstadoCaja.Abierta;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Navegación
        public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();

        [NotMapped]
        public bool EstaAbierta => Estado == EstadoCaja.Abierta;

        [NotMapped]
        public decimal TotalVentas => Transacciones?.Where(t => t.Estado == EstadoTransaccion.Completada).Sum(t => t.Total) ?? 0;

        public void Cerrar(decimal montoContado, string? observaciones = null)
        {
            MontoEsperado = MontoInicial + TotalVentas;
            MontoReal = montoContado;
            Diferencia = MontoReal - MontoEsperado;
            FechaCierre = DateTime.Now;
            Estado = EstadoCaja.Cerrada;
            Observaciones = observaciones;
        }
    }
}
