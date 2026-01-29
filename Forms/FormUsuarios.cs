using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL.Forms
{
    public partial class FormUsuarios : MaterialForm
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IServiceProvider _serviceProvider; // Agregado
        private DataGridView dgvUsuarios = null!;
        private MaterialTextBox2 txtBuscar = null!;
        private MaterialButton btnNuevo = null!;
        private MaterialButton btnEditar = null!;
        private MaterialButton btnEliminar = null!;
        private MaterialButton btnBuscar = null!;

        public FormUsuarios() : this(Program.ServiceProvider.GetRequiredService<IUsuarioService>(), Program.ServiceProvider)
        {
        }

        public FormUsuarios(IUsuarioService usuarioService, IServiceProvider serviceProvider) // serviceProvider agregado
        {
            _usuarioService = usuarioService;
            _serviceProvider = serviceProvider; // Inicializado
            InitializeComponent();
            ConfigurarFormularioMaterial();
            CrearControlesMaterial();
            _ = CargarUsuariosAsync(); // Mantener _ = para el constructor async
        }

        private void ConfigurarFormularioMaterial()
        {
            this.Text = "Gestión de Usuarios";
            this.Size = new Size(1000, 650);
            this.Sizable = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
        }

        private void CrearControlesMaterial()
        {
            this.Controls.Clear();
            this.Padding = new Padding(14, 70, 14, 14); // Padding superior para el título

            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(tlpPrincipal);

            // === FILTROS SUPERIORES ===
            var cardSuperior = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(0, 0, 0, 5) };
            var tlpFiltros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true };
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            
            txtBuscar = new MaterialTextBox2 { Hint = "Nombre, apellido, documento o email...", Dock = DockStyle.Fill };
            btnBuscar = new MaterialButton { Text = "Buscar", Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, Dock = DockStyle.Left, Width = 100, Height = 36, Margin = new Padding(8,0,0,0) };
            btnBuscar.Click += BtnBuscar_Click;
            
            tlpFiltros.Controls.Add(txtBuscar, 0, 0);
            tlpFiltros.Controls.Add(btnBuscar, 1, 0);

            cardSuperior.Controls.Add(tlpFiltros);
            tlpPrincipal.Controls.Add(cardSuperior, 0, 0);
            
            // === CONTENIDO PRINCIPAL (GRID Y BOTONES) ===
            var tlpContenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpPrincipal.Controls.Add(tlpContenido, 0, 1);

            // DATAGRIDVIEW
            dgvUsuarios = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BorderStyle = BorderStyle.None };
            EstilizarDataGridView();
            tlpContenido.Controls.Add(dgvUsuarios, 0, 0);

            // BOTONES LATERALES
            var cardBotones = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(5, 0, 0, 0) };
            var flpBotones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
            
            var buttonMargin = new Padding(0, 0, 0, 10);
            btnNuevo = new MaterialButton { Text = "Nuevo", Width=120, Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, UseAccentColor = true, Margin = buttonMargin };
            btnNuevo.Click += BtnNuevo_Click;
            btnEditar = new MaterialButton { Text = "Editar", Width=120, Type = MaterialButton.MaterialButtonType.Contained, Margin = buttonMargin };
            btnEditar.Click += BtnEditar_Click;
            btnEliminar = new MaterialButton { Text = "Eliminar", Width=120, Type = MaterialButton.MaterialButtonType.Outlined, Margin = buttonMargin };
            btnEliminar.Click += BtnEliminar_Click;

            flpBotones.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnEliminar });
            cardBotones.Controls.Add(flpBotones);
            tlpContenido.Controls.Add(cardBotones, 1, 0);
        }

        private void EstilizarDataGridView()
        {
            dgvUsuarios.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvUsuarios.GridColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;

            dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvUsuarios.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvUsuarios.EnableHeadersVisualStyles = false;
            
            dgvUsuarios.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvUsuarios.DefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
            dgvUsuarios.DefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvUsuarios.DefaultCellStyle.Padding = new Padding(5);
            dgvUsuarios.RowTemplate.Height = 35;
        }

        private async Task CargarUsuariosAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var usuarios = await _usuarioService.ObtenerTodosAsync();
                MostrarUsuariosEnGrid(usuarios);
            }
            catch (Exception)
            {
                //MessageBox.Show($"Error al cargar usuarios:\n\n{ex.Message}\n\n{ex.StackTrace}", "Error",
                //MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void MostrarUsuariosEnGrid(List<Usuario> usuarios)
        {
            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = usuarios.Select(u => new
            {
                u.Id,
                u.Documento,
                u.Nombre,
                u.Apellido,
                u.Email,
                u.Telefono,
                Tipo = u.TipoUsuario.ToString(),
                Modalidad = u.EsEstudiante ? u.ModalidadPago?.ToString() ?? "N/A" : "N/A",
                Estado = u.Activo ? "Activo" : "Inactivo"
            }).ToList();

            if (dgvUsuarios.Columns["Id"] != null)
                dgvUsuarios.Columns["Id"].Visible = false;

            if (dgvUsuarios.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvUsuarios.Columns)
                {
                    switch (column.Name)
                    {
                        case "Documento":
                            column.Width = 100;
                            break;
                        case "Nombre":
                            column.Width = 120;
                            break;
                        case "Apellido":
                            column.Width = 120;
                            break;
                        case "Email":
                            column.Width = 180;
                            break;
                        case "Telefono":
                            column.Width = 100;
                            break;
                        case "Tipo":
                            column.Width = 100;
                            break;
                        case "Modalidad":
                            column.Width = 100;
                            break;
                        case "Estado":
                            column.Width = 80;
                            break;
                    }
                }
            }
        }

        private async void BtnBuscar_Click(object? sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var termino = txtBuscar.Text.Trim();
                var usuarios = await _usuarioService.BuscarAsync(termino);
                MostrarUsuariosEnGrid(usuarios);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnNuevo_Click(object? sender, EventArgs e)
        {
            var formEditar = new FormEditarUsuario(); // Instancia directa para nuevo registro
            if (formEditar.ShowDialog() == DialogResult.OK)
            {
                _ = CargarUsuariosAsync();
            }
        }

        private void BtnEditar_Click(object? sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor seleccione un usuario para editar", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["Id"].Value);
            var formEditar = new FormEditarUsuario(id); // Instancia directa para edición con ID
            
            if (formEditar.ShowDialog() == DialogResult.OK)
            {
                _ = CargarUsuariosAsync();
            }
        }

        private async void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor seleccione un usuario para eliminar", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["Id"].Value);
            var nombre = dgvUsuarios.SelectedRows[0].Cells["Nombre"].Value?.ToString();
            var apellido = dgvUsuarios.SelectedRows[0].Cells["Apellido"].Value?.ToString();

            var resultado = MessageBox.Show(
                $"¿Está seguro de eliminar al usuario {nombre} {apellido}?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    await _usuarioService.EliminarAsync(id);
                    MessageBox.Show("Usuario eliminado correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await CargarUsuariosAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}