using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;
using SellFast.App.Views;

namespace SellFast.App.ViewModels
{
    public class CountryPreset
    {
        public string Name { get; set; } = "";
        public string Flag { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Symbol { get; set; } = "$";
        public decimal DefaultTax { get; set; } = 19;
    }

    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private int _currentStep = 1;

        [ObservableProperty]
        private string _nombreNegocio = "SellFast POS";

        [ObservableProperty]
        private string _eslogan = "Vende rápido, crece más";

        [ObservableProperty]
        private string _tipoPersona = "Persona Jurídica";

        [ObservableProperty]
        private string _nit = "900.123.456-7";

        [ObservableProperty]
        private string _direccion = "Calle Principal # 12-34";

        [ObservableProperty]
        private string _telefono = "+57 300 123 4567";

        [ObservableProperty]
        private string _email = "contacto@mi-negocio.com";

        // Admin Security Credentials
        [ObservableProperty]
        private string _adminNombreCompleto = "Administrador Principal";

        [ObservableProperty]
        private string _adminNombreUsuario = "admin";

        [ObservableProperty]
        private string _adminPassword = "Admin123";

        [ObservableProperty]
        private string? _logoRuta = null;

        [ObservableProperty]
        private CountryPreset? _selectedCountry;

        [ObservableProperty]
        private string _moneda = "COP";

        [ObservableProperty]
        private string _simboloMoneda = "$";

        [ObservableProperty]
        private decimal _iva = 19;

        [ObservableProperty]
        private bool _ivaIncluido = true;

        [ObservableProperty]
        private bool _mesasHabilitadas = true;

        [ObservableProperty]
        private bool _comandasHabilitadas = true;

        [ObservableProperty]
        private bool _fichosHabilitados = true;

        [ObservableProperty]
        private bool _propinasHabilitadas = true;

        public ObservableCollection<CountryPreset> Countries { get; } = new()
        {
            new CountryPreset { Name = "Colombia", Flag = "🇨🇴", Currency = "COP", Symbol = "$", DefaultTax = 19 },
            new CountryPreset { Name = "México", Flag = "🇲🇽", Currency = "MXN", Symbol = "$", DefaultTax = 16 },
            new CountryPreset { Name = "Perú", Flag = "🇵🇪", Currency = "PEN", Symbol = "S/.", DefaultTax = 18 },
            new CountryPreset { Name = "Chile", Flag = "🇨🇱", Currency = "CLP", Symbol = "$", DefaultTax = 19 },
            new CountryPreset { Name = "Argentina", Flag = "🇦🇷", Currency = "ARS", Symbol = "$", DefaultTax = 21 },
            new CountryPreset { Name = "Estados Unidos", Flag = "🇺🇸", Currency = "USD", Symbol = "$", DefaultTax = 7 },
            new CountryPreset { Name = "España", Flag = "🇪🇸", Currency = "EUR", Symbol = "€", DefaultTax = 21 },
            new CountryPreset { Name = "Otro (Personalizado)", Flag = "🌍", Currency = "USD", Symbol = "$", DefaultTax = 0 }
        };

        public ObservableCollection<string> TiposPersona { get; } = new()
        {
            "Persona Jurídica (NIT / Razón Social)",
            "Persona Natural (Cédula / RUT / DNI)"
        };

        private readonly IExcelImportService _excelImportService;

        [ObservableProperty]
        private string _importSummaryText = "Aún no se han importado archivos.";

        public OnboardingViewModel(SellFastContext context, IExcelImportService excelImportService)
        {
            _context = context;
            _excelImportService = excelImportService;
            SelectedCountry = Countries.First();
        }

        partial void OnSelectedCountryChanged(CountryPreset? value)
        {
            if (value != null)
            {
                Moneda = value.Currency;
                SimboloMoneda = value.Symbol;
                Iva = value.DefaultTax;
            }
        }

        [RelayCommand]
        private void SeleccionarLogo()
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Seleccionar Logo del Negocio",
                Filter = "Imágenes (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|Todos los archivos (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == true)
            {
                LogoRuta = openDialog.FileName;
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
                ModernDialogWindow.Show("Plantilla Generada", $"Se ha generado la plantilla Excel en:\n{filePath}", DialogType.Success);
            }
            catch (Exception ex)
            {
                ModernDialogWindow.Show("Error", $"No se pudo generar la plantilla: {ex.Message}", DialogType.Error);
            }
        }

        [RelayCommand]
        private async Task ImportarClientesExcelAsync()
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Seleccionar Archivo Excel de Clientes",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx"
            };

            if (openDialog.ShowDialog() == true)
            {
                var result = await _excelImportService.ImportarClientesAsync(openDialog.FileName);
                ImportSummaryText = result.Mensaje;
                ModernDialogWindow.Show("Importación de Clientes", result.Mensaje, DialogType.Success);
            }
        }

        [RelayCommand]
        private async Task ImportarProductosExcelAsync()
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Seleccionar Archivo Excel de Productos",
                Filter = "Archivos Excel (*.xlsx)|*.xlsx"
            };

            if (openDialog.ShowDialog() == true)
            {
                var result = await _excelImportService.ImportarProductosAsync(openDialog.FileName);
                ImportSummaryText = result.Mensaje;
                ModernDialogWindow.Show("Importación de Productos", result.Mensaje, DialogType.Success);
            }
        }

        [RelayCommand]
        private void NextStep()
        {
            if (CurrentStep == 1)
            {
                if (string.IsNullOrWhiteSpace(NombreNegocio))
                {
                    ModernDialogWindow.Show("Validación", "El nombre del negocio es obligatorio.", DialogType.Warning);
                    return;
                }
            }

            if (CurrentStep < 4)
            {
                CurrentStep++;
            }
        }

        [RelayCommand]
        private void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        }

        [RelayCommand]
        private async Task FinalizarOnboardingAsync(Window window)
        {
            try
            {
                var config = await _context.Configuracion.FirstOrDefaultAsync();
                if (config == null)
                {
                    config = new ConfiguracionNegocio();
                    _context.Configuracion.Add(config);
                }

                config.NombreNegocio = NombreNegocio.Trim();
                config.Eslogan = Eslogan?.Trim();
                config.TipoPersona = TipoPersona;
                config.NIT = Nit?.Trim();
                config.Direccion = Direccion?.Trim();
                config.Telefono = Telefono?.Trim();
                config.Email = Email?.Trim();
                config.LogoRuta = LogoRuta;
                config.Pais = SelectedCountry?.Name ?? "Colombia";
                config.Moneda = Moneda;
                config.SimboloMoneda = SimboloMoneda;
                config.IVA = Iva;
                config.IVAIncluido = IvaIncluido;

                config.MesasHabilitadas = MesasHabilitadas;
                config.ComandasHabilitadas = ComandasHabilitadas;
                config.FichosHabilitados = FichosHabilitados;
                config.PropinasHabilitadas = PropinasHabilitadas;
                config.IsConfigurado = true;

                await _context.SaveChangesAsync();

                // Save or update Administrator User Account
                if (!string.IsNullOrWhiteSpace(AdminNombreUsuario) && !string.IsNullOrWhiteSpace(AdminPassword))
                {
                    string usernameClean = AdminNombreUsuario.Trim();
                    var adminUser = await _context.UsuariosSistema.FirstOrDefaultAsync(u => u.NombreUsuario == usernameClean);
                    if (adminUser == null)
                    {
                        adminUser = await _context.UsuariosSistema.FirstOrDefaultAsync(u => u.Rol == RolUsuario.Administrador);
                    }

                    if (adminUser == null)
                    {
                        adminUser = new UsuarioSistema
                        {
                            NombreUsuario = usernameClean,
                            NombreCompleto = string.IsNullOrWhiteSpace(AdminNombreCompleto) ? "Administrador Principal" : AdminNombreCompleto.Trim(),
                            Email = Email,
                            Rol = RolUsuario.Administrador,
                            Estado = EstadoUsuarioSistema.Activo,
                            PasswordHash = SellFast.Core.Utils.PasswordHelper.HashPassword(AdminPassword.Trim())
                        };
                        _context.UsuariosSistema.Add(adminUser);
                    }
                    else
                    {
                        adminUser.NombreUsuario = usernameClean;
                        adminUser.NombreCompleto = string.IsNullOrWhiteSpace(AdminNombreCompleto) ? adminUser.NombreCompleto : AdminNombreCompleto.Trim();
                        adminUser.PasswordHash = SellFast.Core.Utils.PasswordHelper.HashPassword(AdminPassword.Trim());
                        adminUser.Estado = EstadoUsuarioSistema.Activo;
                    }

                    await _context.SaveChangesAsync();
                }

                Converters.CurrencyFormatterConverter.SimboloMoneda = string.IsNullOrWhiteSpace(SimboloMoneda) ? "$" : SimboloMoneda;

                try
                {
                    var mainVm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MainViewModel>(App.AppHost.Services);
                    _ = mainVm.CargarConfiguracionGlobalAsync();
                }
                catch { }

                ModernDialogWindow.Show(
                    "¡Configuración Completada!",
                    $"El negocio '{config.NombreNegocio}' ha sido configurado exitosamente.",
                    DialogType.Success);

                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                ModernDialogWindow.Show("Error", $"No se pudo guardar la configuración: {ex.Message}", DialogType.Error);
            }
        }
    }
}
