using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;

namespace SellFast.App.ViewModels
{
    public partial class ConfiguracionViewModel : ObservableObject
    {
        private readonly SellFastContext _context;
        private readonly IExcelImportService _excelImportService;
        private readonly IHardwareService _hardwareService;
        private readonly INetworkSyncService _networkSyncService;

        [ObservableProperty]
        private ConfiguracionNegocio _config = new();

        [ObservableProperty]
        private string? _selectedImpresora;

        [ObservableProperty]
        private string _ipLocalActual = "127.0.0.1";

        [ObservableProperty]
        private string _rutaBaseDatos = "";

        public System.Collections.ObjectModel.ObservableCollection<string> ImpresorasDisponibles { get; } = new();

        public System.Collections.ObjectModel.ObservableCollection<string> ModosRedDisponibles { get; } = new()
        {
            "Standalone (Terminal Única / Local)",
            "ServidorPrincipal (Caja Maestra Servidor LAN)",
            "ClienteSecundario (Terminal Secundaria / Cocina)"
        };

        public ConfiguracionViewModel(SellFastContext context, IExcelImportService excelImportService, IHardwareService hardwareService, INetworkSyncService networkSyncService)
        {
            _context = context;
            _excelImportService = excelImportService;
            _hardwareService = hardwareService;
            _networkSyncService = networkSyncService;
            IpLocalActual = _networkSyncService.ObtenerIpLocal();
            CargarImpresoras();
        }

        private void CargarImpresoras()
        {
            ImpresorasDisponibles.Clear();
            var prs = _hardwareService.ObtenerImpresorasInstaladas();
            foreach (var p in prs) ImpresorasDisponibles.Add(p);
            SelectedImpresora = _hardwareService.ObtenerImpresoraPredeterminada();
        }

        public async Task CargarConfiguracionAsync()
        {
            var cfg = await _context.Configuracion.FirstOrDefaultAsync();
            if (cfg == null)
            {
                cfg = new ConfiguracionNegocio
                {
                    NombreNegocio = "SellFast POS",
                    Eslogan = "Vende rápido, crece más",
                    Moneda = "COP",
                    SimboloMoneda = "$",
                    IVA = 19,
                    MesasHabilitadas = true,
                    ComandasHabilitadas = true,
                    FichosHabilitados = true,
                    PropinasHabilitadas = true
                };
                _context.Configuracion.Add(cfg);
                await _context.SaveChangesAsync();
            }
            Config = cfg;

            // Cargar ruta DB actual desde dbconfig.txt si existe
            string configFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.txt");
            if (System.IO.File.Exists(configFile))
            {
                RutaBaseDatos = System.IO.File.ReadAllText(configFile).Trim();
            }
            else
            {
                RutaBaseDatos = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SellFast.db");
            }
        }

        [RelayCommand]
        private void SeleccionarRutaBaseDatos()
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar Archivo de Base de Datos (SellFast.db)",
                Filter = "Base de Datos SQLite (*.db)|*.db|Todos los archivos (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == true)
            {
                RutaBaseDatos = openDialog.FileName;
            }
        }

        [RelayCommand]
        private async Task GuardarConfiguracionAsync()
        {
            try
            {
                var cfg = await _context.Configuracion.FirstOrDefaultAsync();
                if (cfg != null)
                {
                    cfg.NombreNegocio = Config.NombreNegocio;
                    cfg.Eslogan = Config.Eslogan;
                    cfg.Direccion = Config.Direccion;
                    cfg.Telefono = Config.Telefono;
                    cfg.Email = Config.Email;
                    cfg.NIT = Config.NIT;
                    cfg.Moneda = Config.Moneda;
                    cfg.SimboloMoneda = Config.SimboloMoneda;
                    cfg.IVA = Config.IVA;
                    cfg.MesasHabilitadas = Config.MesasHabilitadas;
                    cfg.ComandasHabilitadas = Config.ComandasHabilitadas;
                    cfg.FichosHabilitados = Config.FichosHabilitados;
                    cfg.PropinasHabilitadas = Config.PropinasHabilitadas;
                    cfg.MaximoFichosPorDia = Config.MaximoFichosPorDia;
                    cfg.ModoRedTerminal = Config.ModoRedTerminal;
                    cfg.ServidorIP = Config.ServidorIP;
                    cfg.PuertoRed = Config.PuertoRed;

                    await _context.SaveChangesAsync();

                    // Guardar ruta DB personalizada
                    if (!string.IsNullOrWhiteSpace(RutaBaseDatos))
                    {
                        string configFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.txt");
                        System.IO.File.WriteAllText(configFile, RutaBaseDatos.Trim());
                    }

                    Views.ModernDialogWindow.Show("Configuración Guardada", "La configuración general y de red ha sido actualizada con éxito.", Views.DialogType.Success);
                }
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error al Guardar", ex.Message, Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private async Task DescargarPlantillaAsync()
        {
            try
            {
                string docsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string filePath = await _excelImportService.GenerarPlantillaImportacionAsync(docsFolder);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                Views.ModernDialogWindow.Show("Plantilla Generada", $"Se ha generado la plantilla Excel en:\n{filePath}", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error", $"No se pudo generar la plantilla: {ex.Message}", Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private async Task ImportarClientesExcelAsync()
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar Archivo Excel de Clientes",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx"
            };

            if (openDialog.ShowDialog() == true)
            {
                var result = await _excelImportService.ImportarClientesAsync(openDialog.FileName);
                Views.ModernDialogWindow.Show("Importación de Clientes", result.Mensaje, Views.DialogType.Success);
            }
        }

        [RelayCommand]
        private async Task ImportarProductosExcelAsync()
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar Archivo Excel de Productos",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx"
            };

            if (openDialog.ShowDialog() == true)
            {
                var result = await _excelImportService.ImportarProductosAsync(openDialog.FileName);
                Views.ModernDialogWindow.Show("Importación de Productos", result.Mensaje, Views.DialogType.Success);
            }
        }

        [RelayCommand]
        private void CrearBackupDatabase()
        {
            try
            {
                string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SellFast.db");
                if (!System.IO.File.Exists(dbPath))
                {
                    Views.ModernDialogWindow.Show("Backup", "No se encontró la base de datos para respaldar.", Views.DialogType.Warning);
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Guardar Copia de Seguridad de Base de Datos",
                    Filter = "Base de Datos SQLite (*.db)|*.db",
                    FileName = $"SellFast_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(dbPath, saveDialog.FileName, overwrite: true);
                    Views.ModernDialogWindow.Show("Backup Exitoso", $"Copia de seguridad guardada correctamente en:\n{saveDialog.FileName}", Views.DialogType.Success);
                }
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error de Backup", $"No se pudo crear la copia de seguridad: {ex.Message}", Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private async Task ProbarConexionRedAsync()
        {
            bool ok = await _networkSyncService.ProbarConexionServidorAsync(Config.ServidorIP, Config.PuertoRed);
            if (ok)
            {
                Views.ModernDialogWindow.Show("Conexión Exitosa", $"Se estableció conexión correctamente con el Servidor Maestra en:\n{Config.ServidorIP}:{Config.PuertoRed}", Views.DialogType.Success);
            }
            else
            {
                Views.ModernDialogWindow.Show("Sin Conexión", $"No se pudo conectar con la IP {Config.ServidorIP}:{Config.PuertoRed}.\nVerifique que ambas computadoras estén en la misma red Wi-Fi/LAN.", Views.DialogType.Warning);
            }
        }

        [RelayCommand]
        private async Task AbrirOnboardingAsync()
        {
            var onboardingWin = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Views.OnboardingWindow>(App.AppHost.Services);
            if (onboardingWin.ShowDialog() == true)
            {
                await CargarConfiguracionAsync();
            }
        }
    }
}
