using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    public enum EstadoMesa
    {
        Libre = 1,
        Ocupada = 2,
        Reservada = 3,
        CuentaPedida = 4,
        Mantenimiento = 5
    }

    [Table("Mesas")]
    public class Mesa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Zona { get; set; }

        public int Capacidad { get; set; } = 4;

        [Required]
        public EstadoMesa Estado { get; set; } = EstadoMesa.Libre;

        public bool Activa { get; set; } = true;

        public int PosicionX { get; set; } = 0;
        public int PosicionY { get; set; } = 0;

        // Navegación
        public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();

        [NotMapped]
        public bool EstaDisponible => Estado == EstadoMesa.Libre;

        [NotMapped]
        public string ColorEstado => Estado switch
        {
            EstadoMesa.Libre => "#00D9A6",
            EstadoMesa.Ocupada => "#FFB74D",
            EstadoMesa.Reservada => "#90A4AE",
            EstadoMesa.CuentaPedida => "#FF5252",
            EstadoMesa.Mantenimiento => "#616161",
            _ => "#9E9E9E"
        };
    }
}
