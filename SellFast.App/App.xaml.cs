using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;
using SellFast.Core.Utils;
using SellFast.App.ViewModels;
using SellFast.App.Views;

namespace SellFast.App
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            DispatcherUnhandledException += (s, e) =>
            {
                string log = $"[DispatcherUnhandledException] {DateTime.Now}\n{e.Exception}\n\n";
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), log);
                MessageBox.Show($"Error no controlado:\n{e.Exception.Message}\n\nVer crash.log", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                string log = $"[AppDomainUnhandledException] {DateTime.Now}\n{e.ExceptionObject}\n\n";
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), log);
                MessageBox.Show($"Error grave:\n{e.ExceptionObject}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // DB Context (Dynamic path from dbconfig.txt or local default)
                    string defaultDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SellFast.db");
                    string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.txt");
                    string dbPath = defaultDbPath;
                    if (File.Exists(configFile))
                    {
                        string customPath = File.ReadAllText(configFile).Trim();
                        if (!string.IsNullOrWhiteSpace(customPath)) dbPath = customPath;
                    }
                    services.AddDbContext<SellFastContext>(options =>
                        options.UseSqlite($"Data Source={dbPath}"));

                    // Services
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IPdfReceiptService, PdfReceiptService>();
                    services.AddScoped<IWhatsAppService, WhatsAppService>();
                    services.AddScoped<IAuditLogService, AuditLogService>();
                    services.AddScoped<IExcelImportService, ExcelImportService>();
                    services.AddScoped<IHardwareService, HardwareService>();
                    services.AddScoped<INetworkSyncService, NetworkSyncService>();

                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<PosViewModel>();
                    services.AddTransient<ProductosViewModel>();
                    services.AddTransient<ClientesViewModel>();
                    services.AddTransient<MesasViewModel>();
                    services.AddTransient<ComandasViewModel>();
                    services.AddTransient<CajaViewModel>();
                    services.AddTransient<EmpleadosViewModel>();
                    services.AddTransient<ReportesViewModel>();
                    services.AddTransient<ConfiguracionViewModel>();
                    services.AddTransient<FichosViewModel>();
                    services.AddTransient<OnboardingViewModel>();
                    services.AddTransient<AuditoriaViewModel>();

                    // Windows & Views
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<OnboardingWindow>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await AppHost.StartAsync();

            // Ensure Database is created and Seeded
            using (var scope = AppHost.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SellFastContext>();
                dbContext.Database.EnsureCreated();

                // Ensure newly added columns exist in SQLite database
                try
                {
                    dbContext.Database.ExecuteSqlRaw("ALTER TABLE ConfiguracionNegocio ADD COLUMN TipoPersona TEXT DEFAULT 'Persona Jurídica';");
                }
                catch { }

                try
                {
                    dbContext.Database.ExecuteSqlRaw("ALTER TABLE ConfiguracionNegocio ADD COLUMN Pais TEXT DEFAULT 'Colombia';");
                }
                catch { }

                try
                {
                    dbContext.Database.ExecuteSqlRaw("ALTER TABLE ConfiguracionNegocio ADD COLUMN IsConfigurado INTEGER NOT NULL DEFAULT 0;");
                }
                catch { }

                try
                {
                    dbContext.Database.ExecuteSqlRaw("ALTER TABLE ConfiguracionNegocio ADD COLUMN ModoRedTerminal TEXT DEFAULT 'Standalone';");
                    dbContext.Database.ExecuteSqlRaw("ALTER TABLE ConfiguracionNegocio ADD COLUMN ServidorIP TEXT DEFAULT '127.0.0.1';");
                    dbContext.Database.ExecuteSqlRaw("ALTER TABLE ConfiguracionNegocio ADD COLUMN PuertoRed INTEGER DEFAULT 8080;");
                }
                catch { }

                try
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS AuditLogs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FechaHora TEXT NOT NULL,
                            Usuario TEXT NOT NULL,
                            Accion TEXT NOT NULL,
                            Detalles TEXT NULL,
                            TipoModulo TEXT NOT NULL
                        );");

                    // Seed an initial system log if empty
                    if (!await dbContext.AuditLogs.AnyAsync())
                    {
                        dbContext.AuditLogs.Add(new Core.Models.AuditLog
                        {
                            FechaHora = DateTime.Now,
                            Usuario = "admin",
                            Accion = "Inicio del Sistema",
                            Detalles = "El sistema SellFast POS inició correctamente.",
                            TipoModulo = "Sistema"
                        });
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch { }

                // Create initial admin user if not exists
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                if (!await authService.ExisteNombreUsuarioAsync("admin"))
                {
                    var admin = new UsuarioSistema
                    {
                        NombreUsuario = "admin",
                        NombreCompleto = "Administrador del Sistema",
                        Email = "admin@sellfast.app",
                        Rol = RolUsuario.Administrador,
                        Estado = EstadoUsuarioSistema.Activo,
                        AvatarColor = "#6C63FF"
                    };
                    await authService.CrearUsuarioAsync(admin, "Admin123");
                }
            }

            var loginWindow = AppHost.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            base.OnExit(e);
        }
    }
}
