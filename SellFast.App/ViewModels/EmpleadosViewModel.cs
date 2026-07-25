using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Utils;

namespace SellFast.App.ViewModels
{
    public partial class EmpleadosViewModel : ObservableObject
    {
        private readonly SellFastContext _context;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isEditing = false;

        [ObservableProperty]
        private UsuarioSistema _usuarioActual = new();

        [ObservableProperty]
        private string _nuevoPassword = "";

        public ObservableCollection<UsuarioSistema> Usuarios { get; } = new();

        public Array RolesDisponibles => Enum.GetValues(typeof(RolUsuario));

        public EmpleadosViewModel(SellFastContext context)
        {
            _context = context;
        }

        public async Task CargarUsuariosAsync()
        {
            var query = _context.UsuariosSistema.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string term = SearchQuery.ToLower();
                query = query.Where(u => u.NombreUsuario.ToLower().Contains(term) ||
                                         u.NombreCompleto.ToLower().Contains(term) ||
                                         u.Email.ToLower().Contains(term));
            }

            var result = await query.OrderBy(u => u.NombreCompleto).ToListAsync();
            Usuarios.Clear();
            foreach (var u in result) Usuarios.Add(u);
        }

        partial void OnSearchQueryChanged(string value) => _ = CargarUsuariosAsync();

        [RelayCommand]
        private void NuevoUsuario()
        {
            UsuarioActual = new UsuarioSistema
            {
                NombreUsuario = "",
                NombreCompleto = "",
                Email = "",
                Rol = RolUsuario.Cajero,
                Estado = EstadoUsuarioSistema.Activo,
                AvatarColor = "#6C63FF"
            };
            NuevoPassword = "User123";
            IsEditing = true;
        }

        [RelayCommand]
        private void EditarUsuario(UsuarioSistema usuario)
        {
            UsuarioActual = new UsuarioSistema
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Estado = usuario.Estado,
                Notas = usuario.Notas,
                AvatarColor = usuario.AvatarColor
            };
            NuevoPassword = "";
            IsEditing = true;
        }

        [RelayCommand]
        private async Task GuardarUsuarioAsync()
        {
            if (string.IsNullOrWhiteSpace(UsuarioActual.NombreUsuario) || string.IsNullOrWhiteSpace(UsuarioActual.NombreCompleto))
            {
                Views.ModernDialogWindow.Show("Validación Requerida", "El nombre de usuario y nombre completo son obligatorios.", Views.DialogType.Warning);
                return;
            }

            try
            {
                if (UsuarioActual.Id == 0)
                {
                    if (await _context.UsuariosSistema.AnyAsync(u => u.NombreUsuario == UsuarioActual.NombreUsuario))
                    {
                        Views.ModernDialogWindow.Show("Usuario Existente", "El nombre de usuario ya está registrado en el sistema.", Views.DialogType.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(NuevoPassword))
                    {
                        Views.ModernDialogWindow.Show("Contraseña Requerida", "Debe ingresar una contraseña inicial para el nuevo empleado.", Views.DialogType.Warning);
                        return;
                    }

                    UsuarioActual.PasswordHash = PasswordHelper.HashPassword(NuevoPassword);
                    UsuarioActual.FechaCreacion = DateTime.Now;
                    _context.UsuariosSistema.Add(UsuarioActual);
                }
                else
                {
                    var exist = await _context.UsuariosSistema.FindAsync(UsuarioActual.Id);
                    if (exist != null)
                    {
                        exist.NombreUsuario = UsuarioActual.NombreUsuario;
                        exist.NombreCompleto = UsuarioActual.NombreCompleto;
                        exist.Email = UsuarioActual.Email;
                        exist.Rol = UsuarioActual.Rol;
                        exist.Estado = UsuarioActual.Estado;
                        exist.Notas = UsuarioActual.Notas;

                        if (!string.IsNullOrWhiteSpace(NuevoPassword))
                        {
                            exist.PasswordHash = PasswordHelper.HashPassword(NuevoPassword);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                IsEditing = false;
                await CargarUsuariosAsync();
                Views.ModernDialogWindow.Show("Empleado Guardado", $"El operador {UsuarioActual.NombreCompleto} fue registrado con éxito.", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error", $"No se pudo guardar el empleado: {ex.Message}", Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            IsEditing = false;
        }

        [RelayCommand]
        private async Task ToggleBloqueoAsync(UsuarioSistema usuario)
        {
            var user = await _context.UsuariosSistema.FindAsync(usuario.Id);
            if (user != null)
            {
                if (user.Estado == EstadoUsuarioSistema.Bloqueado)
                    user.DesbloquearUsuario();
                else
                    user.Estado = EstadoUsuarioSistema.Bloqueado;

                await _context.SaveChangesAsync();
                await CargarUsuariosAsync();
            }
        }
    }
}
