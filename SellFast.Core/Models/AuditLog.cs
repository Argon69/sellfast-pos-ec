using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Usuario { get; set; } = "Sistema";

        [Required]
        [StringLength(100)]
        public string Accion { get; set; } = "";

        [StringLength(500)]
        public string? Detalles { get; set; }

        [StringLength(30)]
        public string TipoModulo { get; set; } = "General";
    }
}
