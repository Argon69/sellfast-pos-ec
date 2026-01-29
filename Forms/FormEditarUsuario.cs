using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL.Forms
{
    public partial class FormEditarUsuario : Form
    {
        private readonly IUsuarioService _usuarioService;
        private Usuario? _usuarioActual;
        private readonly bool _esNuevo;

        // Controles del formulario
        private TextBox txtDocumento = null!;
        private TextBox txtNombre = null!;
        private TextBox txtApellido = null!;
        private TextBox txtEmail = null!;
        private TextBox txtTelefono = null!;
        private ComboBox cboTipoUsuario = null!;
        private ComboBox cboModalidadPago = null!;
        private TextBox txtCodigoEstudiante = null!;
        private CheckBox chkActivo = null!;
        private Button btnGuardar = null!;
        private Button btnCancelar = null!;
        private Label lblModalidadPago = null!;
        private Label lblCodigoEstudiante = null!;

        public FormEditarUsuario() : this(null)
        {
        }

        public FormEditarUsuario(int? usuarioId)
        {
            _usuarioService = Program.ServiceProvider.GetRequiredService<IUsuarioService>();
            _esNuevo = !usuarioId.HasValue;
            InitializeComponent();
            ConfigurarFormulario();
            CrearControles();

            if (!_esNuevo)
            {
                _ = CargarUsuarioAsync(usuarioId!.Value);
            }
            else
            {
                ConfigurarFormularioNuevo();
            }
        }

        private void ConfigurarFormulario()
        {
            this.Text = _esNuevo ? "Nuevo Usuario" : "Editar Usuario";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CrearControles()
        {
            this.Controls.Clear();
            this.Padding = new Padding(20);

            var tlpPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10, // 9 campos + 1 para botones
                AutoSize = true
            };
            tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            this.Controls.Add(tlpPrincipal);

            // Controles
            txtDocumento = new TextBox { Dock = DockStyle.Fill };
            txtNombre = new TextBox { Dock = DockStyle.Fill };
            txtApellido = new TextBox { Dock = DockStyle.Fill };
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            txtTelefono = new TextBox { Dock = DockStyle.Fill };
            cboTipoUsuario = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTipoUsuario.Items.AddRange(Enum.GetNames(typeof(TipoUsuario)));
            cboTipoUsuario.SelectedIndexChanged += CboTipoUsuario_SelectedIndexChanged;
            
            cboModalidadPago = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboModalidadPago.Items.AddRange(Enum.GetNames(typeof(ModalidadPagoEstudiante)));
            
            txtCodigoEstudiante = new TextBox { Dock = DockStyle.Fill };
            chkActivo = new CheckBox { Text = "Usuario Activo", Checked = true, Dock = DockStyle.Fill };

            // Añadir filas a tlpPrincipal
            int row = 0;
            tlpPrincipal.Controls.Add(new Label { Text = "Documento:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            tlpPrincipal.Controls.Add(txtDocumento, 1, row++);
            
            tlpPrincipal.Controls.Add(new Label { Text = "Nombre:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            tlpPrincipal.Controls.Add(txtNombre, 1, row++);

            tlpPrincipal.Controls.Add(new Label { Text = "Apellido:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            tlpPrincipal.Controls.Add(txtApellido, 1, row++);

            tlpPrincipal.Controls.Add(new Label { Text = "Email:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            tlpPrincipal.Controls.Add(txtEmail, 1, row++);

            tlpPrincipal.Controls.Add(new Label { Text = "Teléfono:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            tlpPrincipal.Controls.Add(txtTelefono, 1, row++);

            tlpPrincipal.Controls.Add(new Label { Text = "Tipo de Usuario:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            tlpPrincipal.Controls.Add(cboTipoUsuario, 1, row++);

            lblModalidadPago = new Label { Text = "Modalidad Pago:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            tlpPrincipal.Controls.Add(lblModalidadPago, 0, row);
            tlpPrincipal.Controls.Add(cboModalidadPago, 1, row++);

            lblCodigoEstudiante = new Label { Text = "Código Estudiante:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            tlpPrincipal.Controls.Add(lblCodigoEstudiante, 0, row);
            tlpPrincipal.Controls.Add(txtCodigoEstudiante, 1, row++);

            tlpPrincipal.Controls.Add(chkActivo, 1, row++);
            
            // Botones
            var flpBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            tlpPrincipal.Controls.Add(flpBotones, 0, row);
            tlpPrincipal.SetColumnSpan(flpBotones, 2);

            btnGuardar = new Button { Text = "Guardar", Width = 100, Height = 35, BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false };
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 35, BackColor = Color.FromArgb(158, 158, 158), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false };
            btnCancelar.Click += (s, e) => this.Close();
            
            flpBotones.Controls.Add(btnCancelar);
            flpBotones.Controls.Add(btnGuardar);
            
            // Configurar visibilidad inicial
            ActualizarVisibilidadCamposEstudiante();
        }

        private void CboTipoUsuario_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ActualizarVisibilidadCamposEstudiante();
        }

        private void ActualizarVisibilidadCamposEstudiante()
        {
            bool esEstudiante = cboTipoUsuario.SelectedItem?.ToString() == "Estudiante";

            lblModalidadPago.Visible = esEstudiante;
            cboModalidadPago.Visible = esEstudiante;
            lblCodigoEstudiante.Visible = esEstudiante;
            txtCodigoEstudiante.Visible = esEstudiante;

            if (!esEstudiante)
            {
                cboModalidadPago.SelectedIndex = -1;
                txtCodigoEstudiante.Clear();
            }
        }

        private async Task CargarUsuarioAsync(int usuarioId)
        {
            try
            {
                _usuarioActual = await _usuarioService.ObtenerPorIdAsync(usuarioId);
                if (_usuarioActual == null)
                {
                    MessageBox.Show("No se encontró el usuario", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Cargar datos en los controles
                txtDocumento.Text = _usuarioActual.Documento;
                txtNombre.Text = _usuarioActual.Nombre;
                txtApellido.Text = _usuarioActual.Apellido;
                txtEmail.Text = _usuarioActual.Email;
                txtTelefono.Text = _usuarioActual.Telefono ?? "";
                cboTipoUsuario.SelectedItem = _usuarioActual.TipoUsuario.ToString();

                if (_usuarioActual.EsEstudiante && _usuarioActual.ModalidadPago.HasValue)
                {
                    cboModalidadPago.SelectedItem = _usuarioActual.ModalidadPago.Value.ToString();
                    txtCodigoEstudiante.Text = _usuarioActual.CodigoEstudiante ?? "";
                }

                chkActivo.Checked = _usuarioActual.Activo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuario: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void ConfigurarFormularioNuevo()
        {
            cboTipoUsuario.SelectedIndex = 0;
            chkActivo.Checked = true;
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                var usuario = _esNuevo ? new Usuario() : _usuarioActual!;

                usuario.Documento = txtDocumento.Text.Trim();
                usuario.Nombre = txtNombre.Text.Trim();
                usuario.Apellido = txtApellido.Text.Trim();
                usuario.Email = txtEmail.Text.Trim();
                usuario.Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
                usuario.TipoUsuario = (TipoUsuario)Enum.Parse(typeof(TipoUsuario), cboTipoUsuario.SelectedItem!.ToString()!);

                if (usuario.TipoUsuario == TipoUsuario.Estudiante)
                {
                    usuario.ModalidadPago = (ModalidadPagoEstudiante)Enum.Parse(typeof(ModalidadPagoEstudiante),
                        cboModalidadPago.SelectedItem!.ToString()!);
                    usuario.CodigoEstudiante = string.IsNullOrWhiteSpace(txtCodigoEstudiante.Text) ?
                        null : txtCodigoEstudiante.Text.Trim();
                }
                else
                {
                    usuario.ModalidadPago = null;
                    usuario.CodigoEstudiante = null;
                }

                usuario.Activo = chkActivo.Checked;

                if (_esNuevo)
                {
                    await _usuarioService.CrearAsync(usuario);
                    MessageBox.Show("Usuario creado exitosamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _usuarioService.ActualizarAsync(usuario);
                    MessageBox.Show("Usuario actualizado exitosamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("El documento es obligatorio", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDocumento.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("El apellido es obligatorio", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtApellido.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("El email es obligatorio", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (cboTipoUsuario.SelectedItem?.ToString() == "Estudiante" &&
                cboModalidadPago.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar una modalidad de pago para estudiantes", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboModalidadPago.Focus();
                return false;
            }

            return true;
        }
    }
}