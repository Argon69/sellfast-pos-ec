using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellFast.Core.Models
{
    /// <summary>
    /// Configuración global del negocio. Solo debe existir 1 registro.
    /// </summary>
    [Table("ConfiguracionNegocio")]
    public class ConfiguracionNegocio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreNegocio { get; set; } = "Mi Negocio";

        [StringLength(200)]
        public string? Eslogan { get; set; }

        [StringLength(500)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? NIT { get; set; }

        [StringLength(500)]
        public string? LogoRuta { get; set; }

        [StringLength(50)]
        public string TipoPersona { get; set; } = "Persona Jurídica";

        [StringLength(50)]
        public string Pais { get; set; } = "Colombia";

        public bool IsConfigurado { get; set; } = false;

        // Configuración Multi-Terminal LAN
        [StringLength(50)]
        public string ModoRedTerminal { get; set; } = "Standalone"; // Standalone, ServidorPrincipal, ClienteSecundario

        [StringLength(100)]
        public string ServidorIP { get; set; } = "127.0.0.1";

        public int PuertoRed { get; set; } = 8080;

        [StringLength(10)]
        public string Moneda { get; set; } = "COP";

        [StringLength(5)]
        public string SimboloMoneda { get; set; } = "$";

        [Column(TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; } = 0;

        public bool IVAIncluido { get; set; } = true;

        // Módulos habilitados
        public bool MesasHabilitadas { get; set; } = false;
        public bool FichosHabilitados { get; set; } = false;
        public bool PropinasHabilitadas { get; set; } = false;
        public bool ComandasHabilitadas { get; set; } = false;
        public bool TurnosHabilitados { get; set; } = false;

        // Configuración de fichos
        public int MaximoFichosPorDia { get; set; } = 200;

        // Horario
        [StringLength(5)]
        public string HorarioApertura { get; set; } = "07:00";

        [StringLength(5)]
        public string HorarioCierre { get; set; } = "22:00";

        // Tema visual
        public bool TemaOscuro { get; set; } = true;

        [StringLength(7)]
        public string ColorPrimario { get; set; } = "#6C63FF";

        [StringLength(7)]
        public string ColorSecundario { get; set; } = "#00D9A6";
    }
}
