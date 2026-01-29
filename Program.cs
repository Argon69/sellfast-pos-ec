using CafeteriaUNAL.Data;
using CafeteriaUNAL.Forms;
using CafeteriaUNAL.Services;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeteriaUNAL
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public static IConfiguration Configuration { get; private set; } = null!;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Cargar configuración desde appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Configurar servicios (Dependency Injection)
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Aplicar migraciones y crear base de datos si no existe
            AplicarMigraciones(ServiceProvider);

            // Crear usuario inicial si no existe
            await CrearUsuarioInicialSiNoExisteAsync(ServiceProvider);

            // Configurar cultura para Colombia
            System.Globalization.CultureInfo cultura = new System.Globalization.CultureInfo("es-CO");
            System.Threading.Thread.CurrentThread.CurrentCulture = cultura;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultura;

            // Mostrar formulario de login primero
            var loginForm = new FormLogin();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Si el login es exitoso, mostrar el formulario principal
                Application.Run(ServiceProvider.GetRequiredService<MainForm>());
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Registrar la configuración
            services.AddSingleton<IConfiguration>(Configuration);

            // Registrar DbContext
            services.AddDbContext<CafeteriaContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            // Registrar servicios
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<ITransaccionService, TransaccionService>();
            services.AddScoped<IFichoService, FichoService>();
            services.AddScoped<IAuthService, AuthService>();

            // Registrar formularios
            services.AddTransient<MainForm>();
            services.AddTransient<FormUsuarios>();
            services.AddTransient<FormEditarUsuario>();
            services.AddTransient<FormProductos>();
            services.AddTransient<FormEditarProducto>();
            services.AddTransient<FormNuevaVenta>();
            services.AddTransient<FormHistorialVentas>();
            services.AddTransient<FormDetalleVenta>();
            services.AddTransient<FormGestionFichos>();
            services.AddTransient<FormImprimirFicho>();
            services.AddTransient<FormReportes>();
        }

        private static void AplicarMigraciones(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CafeteriaContext>();
                try
                {
                    context.Database.EnsureCreated(); // Crea la BD si no existe
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear/actualizar la base de datos: {ex.Message}",
                        "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static async Task CrearUsuarioInicialSiNoExisteAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                var adminInicial = new UsuarioSistema
                {
                    NombreUsuario = "admin",
                    NombreCompleto = "Administrador del Sistema",
                    Email = "admin@cafeteria.unal.edu.co",
                    Rol = RolUsuario.Administrador,
                    Estado = EstadoUsuarioSistema.Activo
                };

                await authService.CrearUsuarioAsync(adminInicial, "Admin123");
            }
            catch (Exception)
            {
                // Usuario ya existe o error, ignorar silenciosamente
            }
        }
    } 
  }
