using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;
using SellFast.App.Views;
using Microsoft.EntityFrameworkCore;

namespace SellFast.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private SesionUsuario? _sesion;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _activeSectionTitle = "Dashboard Principal";

        [ObservableProperty]
        private bool _isSidebarExpanded = true;

        [ObservableProperty]
        private ConfiguracionNegocio _config = new();

        public MainViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task CargarConfiguracionGlobalAsync()
        {
            try
            {
                using var scope = App.AppHost.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SellFastContext>();
                var cfg = await dbContext.Configuracion.FirstOrDefaultAsync();
                if (cfg != null)
                {
                    Config = cfg;
                    Converters.CurrencyFormatterConverter.SimboloMoneda = string.IsNullOrWhiteSpace(cfg.SimboloMoneda) ? "$" : cfg.SimboloMoneda;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando configuracion global: {ex.Message}");
            }
        }

        public void Inicializar(SesionUsuario sesion)
        {
            Sesion = sesion;
            _ = CargarConfiguracionGlobalAsync();
            NavegarADashboard();
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
        }

        [RelayCommand]
        public void NavegarADashboard()
        {
            ActiveSectionTitle = "Dashboard Principal";
            var vm = App.AppHost.Services.GetRequiredService<DashboardViewModel>();
            _ = vm.CargarDashboardAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAPos()
        {
            ActiveSectionTitle = "Punto de Venta (POS)";
            var vm = App.AppHost.Services.GetRequiredService<PosViewModel>();
            _ = vm.CargarDatosAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAProductos()
        {
            ActiveSectionTitle = "Catálogo de Productos e Inventario";
            var vm = App.AppHost.Services.GetRequiredService<ProductosViewModel>();
            _ = vm.CargarProductosAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAClientes()
        {
            ActiveSectionTitle = "Gestión de Clientes";
            var vm = App.AppHost.Services.GetRequiredService<ClientesViewModel>();
            _ = vm.CargarClientesAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAMesas()
        {
            ActiveSectionTitle = "Gestión de Mesas y Salón";
            var vm = App.AppHost.Services.GetRequiredService<MesasViewModel>();
            _ = vm.CargarMesasAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAComandas()
        {
            ActiveSectionTitle = "Pantalla de Cocina / Comandas";
            var vm = App.AppHost.Services.GetRequiredService<ComandasViewModel>();
            _ = vm.CargarComandasAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarACaja()
        {
            ActiveSectionTitle = "Caja Diaria y Arqueo";
            var vm = App.AppHost.Services.GetRequiredService<CajaViewModel>();
            _ = vm.CargarCajaAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAReportes()
        {
            ActiveSectionTitle = "Reportes y Analítica";
            var vm = App.AppHost.Services.GetRequiredService<ReportesViewModel>();
            _ = vm.CargarReportesAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAConfiguracion()
        {
            ActiveSectionTitle = "Configuración del Sistema";
            var vm = App.AppHost.Services.GetRequiredService<ConfiguracionViewModel>();
            _ = vm.CargarConfiguracionAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAEmpleados()
        {
            ActiveSectionTitle = "Gestión de Empleados y Permisos";
            var vm = App.AppHost.Services.GetRequiredService<EmpleadosViewModel>();
            _ = vm.CargarUsuariosAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAFichos()
        {
            ActiveSectionTitle = "Gestión de Fichos de Almuerzo";
            var vm = App.AppHost.Services.GetRequiredService<FichosViewModel>();
            _ = vm.CargarFichosAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        public void NavegarAAuditoria()
        {
            ActiveSectionTitle = "Registro de Auditoría & Bitácora";
            var vm = App.AppHost.Services.GetRequiredService<AuditoriaViewModel>();
            _ = vm.CargarAuditLogsAsync();
            CurrentView = vm;
        }

        [RelayCommand]
        private void CerrarSesion(Window? window)
        {
            bool confirmed = Views.ModernDialogWindow.Show(
                "Cerrar Sesión",
                "¿Está seguro de que desea cerrar sesión y volver a la pantalla de acceso?",
                Views.DialogType.Confirm,
                primaryText: "Sí, Cerrar",
                secondaryText: "Cancelar");

            if (confirmed)
            {
                _authService.CerrarSesion();
                var loginWindow = App.AppHost.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                window?.Close();
            }
        }
    }
}
