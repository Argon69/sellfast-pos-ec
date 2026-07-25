using Microsoft.EntityFrameworkCore;
using SellFast.Core.Models;

namespace SellFast.Core.Data
{
    public class SellFastContext : DbContext
    {
        public SellFastContext(DbContextOptions<SellFastContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TipoCliente> TiposCliente { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<DetalleTransaccion> DetallesTransaccion { get; set; }
        public DbSet<Ficho> Fichos { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<CajaDiaria> CajasDiarias { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<UsuarioSistema> UsuariosSistema { get; set; } = null!;
        public DbSet<ConfiguracionNegocio> Configuracion { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasIndex(e => e.Documento).IsUnique();
                entity.HasMany(c => c.Transacciones).WithOne(t => t.Cliente).HasForeignKey(t => t.ClienteId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(c => c.Fichos).WithOne(f => f.Cliente).HasForeignKey(f => f.ClienteId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.TipoCliente).WithMany(t => t.Clientes).HasForeignKey(c => c.TipoClienteId).OnDelete(DeleteBehavior.Restrict);
            });

            // Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasMany(p => p.DetallesTransaccion).WithOne(d => d.Producto).HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
            });

            // Transaccion
            modelBuilder.Entity<Transaccion>(entity =>
            {
                entity.HasIndex(e => e.NumeroTransaccion).IsUnique();
                entity.HasMany(t => t.Detalles).WithOne(d => d.Transaccion).HasForeignKey(d => d.TransaccionId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Mesa).WithMany(m => m.Transacciones).HasForeignKey(t => t.MesaId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.CajaDiaria).WithMany(c => c.Transacciones).HasForeignKey(t => t.CajaDiariaId).OnDelete(DeleteBehavior.SetNull);
            });

            // Ficho
            modelBuilder.Entity<Ficho>(entity =>
            {
                entity.HasIndex(e => e.Numero).IsUnique();
            });

            // Mesa
            modelBuilder.Entity<Mesa>(entity =>
            {
                entity.HasIndex(e => e.Numero).IsUnique();
            });

            // UsuarioSistema
            modelBuilder.Entity<UsuarioSistema>(entity =>
            {
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasMany(u => u.Turnos).WithOne(t => t.UsuarioSistema).HasForeignKey(t => t.UsuarioSistemaId).OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TipoCliente>().HasData(
                new TipoCliente { Id = 1, Nombre = "Regular", Descripcion = "Cliente regular", PorcentajeDescuento = 0, ColorHex = "#90A4AE", Orden = 1 },
                new TipoCliente { Id = 2, Nombre = "VIP", Descripcion = "Cliente frecuente con beneficios", PorcentajeDescuento = 10, ColorHex = "#FFD700", Orden = 2 },
                new TipoCliente { Id = 3, Nombre = "Empleado", Descripcion = "Empleado del negocio", PorcentajeDescuento = 50, ColorHex = "#6C63FF", Orden = 3 }
            );

            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Codigo = "COM001", Nombre = "Almuerzo del Día", Descripcion = "Menú completo del día", Categoria = CategoriaProducto.Comida, Precio = 15000, StockDisponible = 50, StockMinimo = 10, EsDestacado = true },
                new Producto { Id = 2, Codigo = "BEB001", Nombre = "Gaseosa 350ml", Descripcion = "Bebida gaseosa", Categoria = CategoriaProducto.Bebida, Precio = 3500, StockDisponible = 100, StockMinimo = 20 },
                new Producto { Id = 3, Codigo = "COM002", Nombre = "Hamburguesa Clásica", Descripcion = "Hamburguesa con papas", Categoria = CategoriaProducto.Comida, Precio = 18000, StockDisponible = 30, StockMinimo = 5, EsDestacado = true }
            );

            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, Documento = "0000000000", Nombre = "Cliente", Apellido = "General", Email = "general@sellfast.app", TipoClienteId = 1 }
            );

            modelBuilder.Entity<ConfiguracionNegocio>().HasData(
                new ConfiguracionNegocio { Id = 1, NombreNegocio = "SellFast POS", Eslogan = "Vende rápido, crece más", Moneda = "COP", SimboloMoneda = "$", TemaOscuro = true }
            );
        }
    }
}
