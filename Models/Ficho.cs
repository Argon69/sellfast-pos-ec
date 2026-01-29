using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaUNAL.Models
{
    public enum EstadoFicho
    {
        Pendiente = 1,
        Usado = 2,
        Cancelado = 3
    }

    [Table("Fichos")]
    public class Ficho
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaServicio { get; set; }

        public DateTime? FechaUso { get; set; }

        [Required]
        public EstadoFicho Estado { get; set; } = EstadoFicho.Pendiente;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public bool EstaVencido => Estado == EstadoFicho.Pendiente && FechaServicio.Date < DateTime.Now.Date;

        [NotMapped]
        public bool PuedeUsarse => Estado == EstadoFicho.Pendiente && FechaServicio.Date == DateTime.Now.Date;

        [NotMapped]
        public string EstadoDescripcion
        {
            get
            {
                if (EstaVencido) return "Vencido";

                return Estado switch
                {
                    EstadoFicho.Pendiente => "Pendiente",
                    EstadoFicho.Usado => "Usado",
                    EstadoFicho.Cancelado => "Cancelado",
                    _ => "Desconocido"
                };
            }
        }

        // Métodos de negocio
        public void MarcarComoUsado()
        {
            if (!PuedeUsarse)
                throw new InvalidOperationException("El ficho no puede ser usado. Verifique el estado y la fecha.");

            Estado = EstadoFicho.Usado;
            FechaUso = DateTime.Now;
        }

        public void Cancelar(string motivo)
        {
            if (Estado != EstadoFicho.Pendiente)
                throw new InvalidOperationException("Solo se pueden cancelar fichos pendientes.");

            Estado = EstadoFicho.Cancelado;
            Observaciones = $"Cancelado: {motivo}";
        }

        // Método estático para generar número de ficho
        public static string GenerarNumeroFicho(DateTime fecha)
        {
            // Formato: YYYYMMDD-XXXX
            var fechaStr = fecha.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"{fechaStr}-{random}";
        }
    }
}