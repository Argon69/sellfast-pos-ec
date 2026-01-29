using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using Microsoft.Extensions.DependencyInjection;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CafeteriaUNAL.Forms
{
    public partial class FormLogin : MaterialForm
    {
        private readonly IAuthService _authService;

        // Controles del formulario
        private MaterialTextBox2 txtUsuario = null!;
        private MaterialTextBox2 txtPassword = null!;
        private MaterialButton btnLogin = null!;
        private MaterialButton btnSalir = null!;
        private MaterialCheckbox chkMostrarPassword = null!;
        private Label lblEstado = null!;

        public SesionUsuario? SesionUsuario { get; private set; }

        public FormLogin() : this(Program.ServiceProvider.GetRequiredService<IAuthService>())
        {
        }

        public FormLogin(IAuthService authService)
        {
            _authService = authService;
            InitializeComponent();
            ConfigurarEstiloMaterial();
            CrearControlesMaterial();
        }

        private void ConfigurarEstiloMaterial()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,
                Primary.BlueGrey900,
                Primary.BlueGrey500,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            this.Text = "Acceso al Sistema";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Sizable = false;
        }

        private void CrearControlesMaterial()
        {
            this.Controls.Clear();

            // Panel principal para centrar la tarjeta
            var tlpCentrador = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
            };
            this.Controls.Add(tlpCentrador);

            var card = new MaterialCard
            {
                Width = 380,
                Height = 400,
                Padding = new Padding(20, 25, 20, 20),
                Anchor = AnchorStyles.None // Centrar la tarjeta en la celda del TableLayoutPanel
            };
            tlpCentrador.Controls.Add(card, 0, 0);

            // Panel de flujo para organizar los controles dentro de la tarjeta
            var flpContenido = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            card.Controls.Add(flpContenido);
            
            var lblTitulo = new MaterialLabel
            {
                Text = "Acceso al Sistema",
                FontType = MaterialSkinManager.fontType.H6,
                Width = flpContenido.ClientSize.Width,
                TextAlign = ContentAlignment.MiddleCenter,
                HighEmphasis = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            flpContenido.Controls.Add(lblTitulo);

            txtUsuario = new MaterialTextBox2
            {
                Hint = "Nombre de usuario",
                Width = flpContenido.ClientSize.Width,
                Margin = new Padding(0, 0, 0, 10)
            };
            txtUsuario.KeyPress += TxtUsuario_KeyPress;
            flpContenido.Controls.Add(txtUsuario);

            txtPassword = new MaterialTextBox2
            {
                Hint = "Contraseña",
                UseSystemPasswordChar = true,
                Width = flpContenido.ClientSize.Width,
                Margin = new Padding(0, 0, 0, 5)
            };
            txtPassword.KeyPress += TxtPassword_KeyPress;
            flpContenido.Controls.Add(txtPassword);

            chkMostrarPassword = new MaterialCheckbox
            {
                Text = "Mostrar contraseña",
                Width = flpContenido.ClientSize.Width,
                Margin = new Padding(0, 0, 0, 15)
            };
            chkMostrarPassword.CheckedChanged += ChkMostrarPassword_CheckedChanged;
            flpContenido.Controls.Add(chkMostrarPassword);
            
            lblEstado = new Label
            {
                Text = "Ingrese sus credenciales",
                Width = flpContenido.ClientSize.Width,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            flpContenido.Controls.Add(lblEstado);

            btnLogin = new MaterialButton
            {
                Text = "Iniciar Sesión",
                Type = MaterialButton.MaterialButtonType.Contained,
                Density = MaterialButton.MaterialButtonDensity.Default,
                Width = flpContenido.ClientSize.Width,
                Height = 40,
                HighEmphasis = true,
                Margin = new Padding(0, 10, 0, 5)
            };
            btnLogin.Click += BtnLogin_Click;
            flpContenido.Controls.Add(btnLogin);

            btnSalir = new MaterialButton
            {
                Text = "Salir",
                Type = MaterialButton.MaterialButtonType.Text,
                Density = MaterialButton.MaterialButtonDensity.Default,
                Width = flpContenido.ClientSize.Width,
                Height = 40
            };
            btnSalir.Click += (s, e) => Application.Exit();
            flpContenido.Controls.Add(btnSalir);
            
            this.ClientSize = new Size(450, 520);
        }

        private void ChkMostrarPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkMostrarPassword.Checked;
        }

        private void TxtUsuario_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                txtPassword.Focus();
            }
        }

        private async void TxtPassword_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                await IniciarSesionAsync();
            }
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            await IniciarSesionAsync();
        }

        private async Task IniciarSesionAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                {
                    MostrarMensaje("Por favor ingrese su nombre de usuario", Color.Red);
                    txtUsuario.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MostrarMensaje("Por favor ingrese su contraseña", Color.Red);
                    txtPassword.Focus();
                    return;
                }

                this.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                MostrarMensaje("Verificando...", Color.Blue);

                var sesion = await _authService.AutenticarAsync(txtUsuario.Text.Trim(), txtPassword.Text);

                if (sesion != null)
                {
                    SesionUsuario = sesion;
                    MostrarMensaje($"¡Bienvenido, {sesion.NombreCompleto}!", Color.Green);
                    await Task.Delay(1000);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MostrarMensaje("Usuario o contraseña incorrectos", Color.Red);
                    LimpiarCampos();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}", Color.Red);
                LimpiarCampos();
            }
            finally
            {
                this.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void MostrarMensaje(string mensaje, Color color)
        {
            lblEstado.Text = mensaje;
            lblEstado.ForeColor = color;
        }

        private void LimpiarCampos()
        {
            txtPassword.Clear();
            txtUsuario.Focus();
            chkMostrarPassword.Checked = false;
        }
    }
}
