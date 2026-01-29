using Microsoft.EntityFrameworkCore;
using CafeteriaUNAL.Models;
using System;
using System.Linq;

namespace CafeteriaUNAL.Data
{
    public class CafeteriaContext : DbContext
    {
        public CafeteriaContext(DbContextOptions<CafeteriaContext> options)
            : base(options)
        {
        }

        // DbSets - Tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<DetalleTransaccion> DetallesTransaccion { get; set; }
        public DbSet<Ficho> Fichos { get; set; }
        public DbSet<UsuarioSistema> UsuariosSistema { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(e => e.Documento).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Configurar la relación con Transacciones
                entity.HasMany(u => u.Transacciones)
                    .WithOne(t => t.Usuario)
                    .HasForeignKey(t => t.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configurar la relación con Fichos
                entity.HasMany(u => u.Fichos)
                    .WithOne(f => f.Usuario)
                    .HasForeignKey(f => f.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(e => e.Codigo).IsUnique();

                // Configurar la relación con DetallesTransaccion
                entity.HasMany(p => p.DetallesTransaccion)
                    .WithOne(d => d.Producto)
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Transaccion
            modelBuilder.Entity<Transaccion>(entity =>
            {
                entity.HasIndex(e => e.NumeroTransaccion).IsUnique();

                // Configurar la relación con DetallesTransaccion
                entity.HasMany(t => t.Detalles)
                    .WithOne(d => d.Transaccion)
                    .HasForeignKey(d => d.TransaccionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración para Ficho
            modelBuilder.Entity<Ficho>(entity =>
            {
                entity.HasIndex(e => e.Numero).IsUnique();

                // Índice compuesto para evitar duplicados por usuario y fecha
                entity.HasIndex(e => new { e.UsuarioId, e.FechaServicio })
                    .IsUnique()
                    .HasFilter("[Estado] != 3"); // Excluir cancelados
            });

            // Datos semilla (seed data) - Opcional
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Agregar algunos productos de ejemplo
            modelBuilder.Entity<Producto>().HasData(
                new Producto
                {
                    Id = 1,
                    Codigo = "ALM001",
                    Nombre = "Almuerzo Ejecutivo",
                    Descripcion = "Sopa, plato principal, jugo y postre",
                    Categoria = CategoriaProducto.Almuerzo,
                    Precio = 12000,
                    StockDisponible = 100,
                    StockMinimo = 20,
                    EsMenuDelDia = true,
                    Activo = true
                },
                new Producto
                {
                    Id = 2,
                    Codigo = "BEB001",
                    Nombre = "Gaseosa 350ml",
                    Descripcion = "Bebida gaseosa en lata",
                    Categoria = CategoriaProducto.Bebida,
                    Precio = 3500,
                    StockDisponible = 50,
                    StockMinimo = 10,
                    Activo = true
                },
                new Producto
                {
                    Id = 3,
                    Codigo = "DES001",
                    Nombre = "Desayuno Completo",
                    Descripcion = "Café, pan, huevos y fruta",
                    Categoria = CategoriaProducto.Desayuno,
                    Precio = 8000,
                    StockDisponible = 50,
                    StockMinimo = 10,
                    Activo = true
                }
            );

            // Agregar un usuario de ejemplo (administrador)
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Documento = "1234567890",
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    Email = "admin@unal.edu.co",
                    Telefono = "3001234567",
                    TipoUsuario = TipoUsuario.Administrativo,
                    Activo = true
                }
            );
        }
    }
}