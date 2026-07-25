using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;
using SellFast.App.Views;

namespace SellFast.App.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly SellFastContext _context;

        [ObservableProperty]
        private string _username = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy = false;

        public LoginViewModel(IAuthService authService, SellFastContext context)
        {
            _authService = authService;
            _context = context;
        }

        [RelayCommand]
        private async Task LoginAsync(Window? window)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Ingrese usuario y contraseña";
                return;
            }

            IsBusy = true;
            ErrorMessage = "";

            try
            {
                var sesion = await _authService.AutenticarAsync(Username.Trim(), Password);
                if (sesion != null)
                {
                    // Check if initial business onboarding is required
                    var config = await _context.Configuracion.FirstOrDefaultAsync();
                    if (config == null || !config.IsConfigurado)
                    {
                        var onboardingWin = App.AppHost.Services.GetRequiredService<OnboardingWindow>();
                        onboardingWin.ShowDialog();
                    }

                    var mainVm = App.AppHost.Services.GetRequiredService<MainViewModel>();
                    mainVm.Inicializar(sesion);

                    var mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
                    mainWindow.Show();

                    window?.Close();
                }
                else
                {
                    ErrorMessage = "Usuario o contraseña incorrectos";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
