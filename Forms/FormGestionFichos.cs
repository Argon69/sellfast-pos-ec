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
    public partial class FormGestionFichos : Form
    {
        private readonly IFichoService _fichoService;
        private readonly IUsuarioService _usuarioService;

        // Controles principales
        private GroupBox grpFecha = null!;
        private DateTimePicker dtpFecha = null!;
        private Button btnCargar = null!;
        private Label lblFichosDisponibles = null!;

        private GroupBox grpNuevoFicho = null!;
        private TextBox txtBuscarUsuario = null!;
        private Button btnBuscarUsuario = null!;
        private Label lblUsuarioSeleccionado = null!;
        private Button btnGenerarFicho = null!;

        private GroupBox grpEstadisticas = null!;
        private Label lblTotalEmitidos = null!;
        private Label lblPendientes = null!;
        private Label lblUsados = null!;
        private Label lblCancelados = null!;

        private GroupBox grpFiltros = null!;
        private ComboBox cboFiltroEstado = null!;
        private TextBox txtBuscarFicho = null!;
        private Button btnBuscar = null!;

        private DataGridView dgvFichos = null!;
        private Button btnMarcarUsado = null!;
        private Button btnCancelar = null!;
        private Button btnImprimir = null!;
        private Button btnRefrescar = null!;

        private Usuario? _usuarioSeleccionado;

        public FormGestionFichos()
        {
            _fichoService = Program.ServiceProvider.GetRequiredService<IFichoService>();
            _usuarioService = Program.ServiceProvider.GetRequiredService<IUsuarioService>();

            InitializeComponent();
            ConfigurarFormulario();
            CrearControles();
            _ = CargarFichosHoyAsync();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Gestión de Fichos - Control de Almuerzo";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CrearControles()
        {
            this.Controls.Clear();
            this.Padding = new Padding(10);

            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(tlpPrincipal);

            // === FILA SUPERIOR DE GRUPOS ===
            var tlpSuperior = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3 };
            tlpSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            tlpSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
            tlpPrincipal.Controls.Add(tlpSuperior, 0, 0);

            // Grupo Fecha
            grpFecha = new GroupBox { Text = "Fecha de Servicio", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var flpFecha = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            dtpFecha = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120 };
            btnCargar = new Button { Text = "Cargar", BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = dtpFecha.Height + 4 };
            btnCargar.Click += BtnCargar_Click;
            lblFichosDisponibles = new Label { Text = "Disponibles: 0", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Green, TextAlign = ContentAlignment.MiddleCenter, Height = dtpFecha.Height };
            flpFecha.Controls.AddRange(new Control[] { dtpFecha, btnCargar, lblFichosDisponibles });
            grpFecha.Controls.Add(flpFecha);
            tlpSuperior.Controls.Add(grpFecha, 0, 0);

            // Grupo Nuevo Ficho
            grpNuevoFicho = new GroupBox { Text = "Generar Nuevo Ficho", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var tlpNuevo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
            tlpNuevo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpNuevo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpNuevo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtBuscarUsuario = new TextBox { PlaceholderText = "Documento...", Dock = DockStyle.Fill };
            txtBuscarUsuario.KeyPress += TxtBuscarUsuario_KeyPress;
            btnBuscarUsuario = new Button { Text = "Buscar", UseVisualStyleBackColor = true, AutoSize=true };
            btnBuscarUsuario.Click += BtnBuscarUsuario_Click;
            btnGenerarFicho = new Button { Text = "Generar Ficho", BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false, AutoSize=true };
            btnGenerarFicho.Click += BtnGenerarFicho_Click;
            lblUsuarioSeleccionado = new Label { Text = "Ningún usuario seleccionado", ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            tlpNuevo.Controls.Add(txtBuscarUsuario, 0, 0);
            tlpNuevo.Controls.Add(btnBuscarUsuario, 1, 0);
            tlpNuevo.Controls.Add(btnGenerarFicho, 2, 0);
            tlpNuevo.Controls.Add(lblUsuarioSeleccionado, 0, 1);
            tlpNuevo.SetColumnSpan(lblUsuarioSeleccionado, 3);
            grpNuevoFicho.Controls.Add(tlpNuevo);
            tlpSuperior.Controls.Add(grpNuevoFicho, 1, 0);

            // Grupo Estadísticas
            grpEstadisticas = new GroupBox { Text = "Estadísticas del Día", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var tlpStats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            lblTotalEmitidos = new Label { Text = "Total: 0", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Dock=DockStyle.Fill };
            lblPendientes = new Label { Text = "Pendientes: 0", ForeColor = Color.Orange, Dock=DockStyle.Fill };
            lblUsados = new Label { Text = "Usados: 0", ForeColor = Color.Green, Dock=DockStyle.Fill };
            lblCancelados = new Label { Text = "Cancelados: 0", ForeColor = Color.Red, Dock=DockStyle.Fill };
            tlpStats.Controls.AddRange(new Control[] { lblTotalEmitidos, lblPendientes, lblUsados, lblCancelados });
            grpEstadisticas.Controls.Add(tlpStats);
            tlpSuperior.Controls.Add(grpEstadisticas, 2, 0);

            // === FILA DE FILTROS ===
            grpFiltros = new GroupBox { Text = "Filtros de Lista", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8) };
            var flpFiltros = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            cboFiltroEstado = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            cboFiltroEstado.Items.AddRange(new[] { "Todos", "Pendiente", "Usado", "Cancelado" });
            cboFiltroEstado.SelectedIndex = 0;
            cboFiltroEstado.SelectedIndexChanged += Filtros_Changed;
            txtBuscarFicho = new TextBox { PlaceholderText = "Número, usuario, documento...", Width = 200 };
            btnBuscar = new Button { Text = "Buscar", UseVisualStyleBackColor = true, AutoSize = true };
            btnBuscar.Click += BtnBuscar_Click;
            btnRefrescar = new Button { Text = "Refrescar", UseVisualStyleBackColor = true, AutoSize = true };
            btnRefrescar.Click += (s, e) => _ = CargarFichosAsync();
            flpFiltros.Controls.AddRange(new Control[] { new Label { Text="Estado:", AutoSize=true, TextAlign=ContentAlignment.MiddleCenter }, cboFiltroEstado, new Label { Text="Buscar:", AutoSize=true, TextAlign=ContentAlignment.MiddleCenter }, txtBuscarFicho, btnBuscar, btnRefrescar });
            grpFiltros.Controls.Add(flpFiltros);
            tlpPrincipal.Controls.Add(grpFiltros, 0, 1);

            // === CONTENIDO PRINCIPAL ===
            var tlpContenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
            tlpPrincipal.Controls.Add(tlpContenido, 0, 2);

            // DataGridView
            dgvFichos = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D };
            tlpContenido.Controls.Add(dgvFichos, 0, 0);

            // === BARRA LATERAL DERECHA ===
            var tlpLateral = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlpLateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpLateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpContenido.Controls.Add(tlpLateral, 1, 0);
            
            // Botones laterales
            var grpBotones = new GroupBox { Text = "Acciones", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(8) };
            var flpBotones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoSize = true };
            btnMarcarUsado = new Button { Text = "Marcar como Usado", BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F), Height = 40, Width = 250 };
            btnMarcarUsado.Click += BtnMarcarUsado_Click;
            btnCancelar = new Button { Text = "Cancelar Ficho", BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F), Height = 40, Width = 250 };
            btnCancelar.Click += BtnCancelar_Click;
            btnImprimir = new Button { Text = "Imprimir Ficho", UseVisualStyleBackColor = true, Font = new Font("Segoe UI", 10F), Height = 40, Width = 250 };
            btnImprimir.Click += BtnImprimir_Click;
            flpBotones.Controls.AddRange(new Control[] { btnMarcarUsado, btnCancelar, btnImprimir });
            grpBotones.Controls.Add(flpBotones);
            tlpLateral.Controls.Add(grpBotones, 0, 0);

            // Panel de información adicional
            var panelInfo = new GroupBox { Text = "Distribución por Tipo de Usuario", Dock = DockStyle.Fill, Padding = new Padding(8) };
            tlpLateral.Controls.Add(panelInfo, 0, 1);
        }

        private async Task CargarFichosHoyAsync()
        {
            dtpFecha.Value = DateTime.Today;
            await CargarFichosAsync();
        }

        private async void BtnCargar_Click(object? sender, EventArgs e)
        {
            await CargarFichosAsync();
        }

        private async Task CargarFichosAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                var fecha = dtpFecha.Value.Date;
                var fichos = await _fichoService.ObtenerDelDiaAsync(fecha);

                // Aplicar filtros
                if (cboFiltroEstado.SelectedIndex > 0)
                {
                    var estadoFiltro = (EstadoFicho)cboFiltroEstado.SelectedIndex;
                    fichos = fichos.Where(f => f.Estado == estadoFiltro).ToList();
                }

                if (!string.IsNullOrWhiteSpace(txtBuscarFicho.Text))
                {
                    var termino = txtBuscarFicho.Text.ToLower();
                    fichos = fichos.Where(f =>
                        f.Numero.ToLower().Contains(termino) ||
                        f.Usuario?.NombreCompleto.ToLower().Contains(termino) == true ||
                        f.Usuario?.Documento.ToLower().Contains(termino) == true).ToList();
                }

                MostrarFichosEnGrid(fichos);
                await ActualizarEstadisticasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar fichos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void MostrarFichosEnGrid(List<Ficho> fichos)
        {
            dgvFichos.DataSource = null;
            dgvFichos.DataSource = fichos.Select(f => new
            {
                f.Id,
                Número = f.Numero,
                Usuario = f.Usuario?.NombreCompleto ?? "N/A",
                Documento = f.Usuario?.Documento ?? "N/A",
                TipoUsuario = f.Usuario?.TipoUsuario.ToString() ?? "N/A",
                HoraSolicitud = f.FechaSolicitud,
                Estado = f.EstadoDescripcion,
                HoraUso = f.FechaUso?.ToString("HH:mm") ?? "-"
            }).ToList();

            // Ocultar columna Id
            if (dgvFichos.Columns["Id"] != null)
                dgvFichos.Columns["Id"].Visible = false;

            // Formatear columnas
            if (dgvFichos.Columns.Count > 0)
            {
                dgvFichos.Columns["HoraSolicitud"].DefaultCellStyle.Format = "HH:mm";
                dgvFichos.Columns["Número"].Width = 120;
                dgvFichos.Columns["Usuario"].Width = 180;
                dgvFichos.Columns["Documento"].Width = 100;
                dgvFichos.Columns["TipoUsuario"].Width = 100;

                // Colorear según estado
                foreach (DataGridViewRow row in dgvFichos.Rows)
                {
                    var estado = row.Cells["Estado"]?.Value?.ToString();
                    switch (estado)
                    {
                        case "Usado":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233);
                            break;
                        case "Cancelado":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                            row.DefaultCellStyle.ForeColor = Color.Gray;
                            break;
                        case "Vencido":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                            break;
                    }
                }
            }
        }
        private async Task ActualizarEstadisticasAsync()
        {
            try
            {
                var fecha = dtpFecha.Value.Date;
                var estadisticas = await _fichoService.ObtenerEstadisticasAsync(fecha);

                lblFichosDisponibles.Text = $"Fichos disponibles: {estadisticas.Disponibles}";
                lblFichosDisponibles.ForeColor = estadisticas.Disponibles > 0 ? Color.Green : Color.Red;

                lblTotalEmitidos.Text = $"Total: {estadisticas.TotalEmitidos}";
                lblPendientes.Text = $"Pendientes: {estadisticas.Pendientes}";
                lblUsados.Text = $"Usados: {estadisticas.Usados}";
                lblCancelados.Text = $"Cancelados: {estadisticas.Cancelados}";

                // Actualizar panel de distribución
                ActualizarDistribucionTipoUsuario(estadisticas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar estadísticas: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarDistribucionTipoUsuario(EstadisticasFichos estadisticas)
        {
            var panel = this.Controls.OfType<Panel>().LastOrDefault();
            if (panel != null)
            {
                // Limpiar controles anteriores excepto el título
                var titulo = panel.Controls[0];
                panel.Controls.Clear();
                panel.Controls.Add(titulo);

                var y = 40;
                foreach (var tipo in estadisticas.PorTipoUsuario.OrderByDescending(t => t.Value))
                {
                    var lbl = new Label
                    {
                        Text = $"{tipo.Key}: {tipo.Value}",
                        Location = new Point(20, y),
                        Size = new Size(240, 20),
                        Font = new Font("Segoe UI", 9F)
                    };
                    panel.Controls.Add(lbl);
                    y += 25;
                }

                if (!estadisticas.PorTipoUsuario.Any())
                {
                    var lblSinDatos = new Label
                    {
                        Text = "Sin fichos emitidos",
                        Location = new Point(20, y),
                        Size = new Size(240, 20),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    panel.Controls.Add(lblSinDatos);
                }
            }
        }

        private async void TxtBuscarUsuario_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                await BuscarUsuarioAsync();
            }
        }

        private async void BtnBuscarUsuario_Click(object? sender, EventArgs e)
        {
            await BuscarUsuarioAsync();
        }

        private async Task BuscarUsuarioAsync()
        {
            try
            {
                var termino = txtBuscarUsuario.Text.Trim();
                if (string.IsNullOrEmpty(termino))
                {
                    MessageBox.Show("Ingrese un documento para buscar", "Información",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var usuario = await _usuarioService.ObtenerPorDocumentoAsync(termino);

                if (usuario == null)
                {
                    MessageBox.Show("Usuario no encontrado", "Información",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Verificar si ya tiene ficho
                var fecha = dtpFecha.Value.Date;
                var tieneFicho = await _fichoService.UsuarioTieneFichoAsync(usuario.Id, fecha);

                if (tieneFicho)
                {
                    MessageBox.Show($"{usuario.NombreCompleto} ya tiene un ficho para esta fecha",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _usuarioSeleccionado = usuario;
                lblUsuarioSeleccionado.Text = $"{usuario.NombreCompleto} - {usuario.TipoUsuario}";
                lblUsuarioSeleccionado.ForeColor = Color.Black;
                btnGenerarFicho.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar usuario: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGenerarFicho_Click(object? sender, EventArgs e)
        {
            if (_usuarioSeleccionado == null) return;

            try
            {
                var fecha = dtpFecha.Value.Date;
                var ficho = await _fichoService.CrearFichoAsync(_usuarioSeleccionado.Id, fecha);

                MessageBox.Show($"Ficho generado exitosamente\n\nNúmero: {ficho.Numero}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Limpiar selección
                _usuarioSeleccionado = null;
                txtBuscarUsuario.Clear();
                lblUsuarioSeleccionado.Text = "Ningún usuario seleccionado";
                lblUsuarioSeleccionado.ForeColor = Color.Gray;
                btnGenerarFicho.Enabled = false;

                // Recargar lista
                await CargarFichosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar ficho: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Filtros_Changed(object? sender, EventArgs e)
        {
            _ = CargarFichosAsync();
        }

        private async void BtnBuscar_Click(object? sender, EventArgs e)
        {
            await CargarFichosAsync();
        }

        private async void BtnMarcarUsado_Click(object? sender, EventArgs e)
        {
            if (dgvFichos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un ficho para marcar como usado", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var estado = dgvFichos.SelectedRows[0].Cells["Estado"].Value?.ToString();
            if (estado != "Pendiente")
            {
                MessageBox.Show($"El ficho está en estado: {estado}", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var numero = dgvFichos.SelectedRows[0].Cells["Número"].Value?.ToString();
            var usuario = dgvFichos.SelectedRows[0].Cells["Usuario"].Value?.ToString();

            var resultado = MessageBox.Show(
                $"¿Marcar como usado el ficho {numero} de {usuario}?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    var id = Convert.ToInt32(dgvFichos.SelectedRows[0].Cells["Id"].Value);
                    await _fichoService.MarcarComoUsadoAsync(id);

                    MessageBox.Show("Ficho marcado como usado", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await CargarFichosAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnCancelar_Click(object? sender, EventArgs e)
        {
            if (dgvFichos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un ficho para cancelar", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var estado = dgvFichos.SelectedRows[0].Cells["Estado"].Value?.ToString();
            if (estado != "Pendiente")
            {
                MessageBox.Show($"Solo se pueden cancelar fichos pendientes", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var numero = dgvFichos.SelectedRows[0].Cells["Número"].Value?.ToString();

            string motivo = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el motivo de la cancelación:", "Cancelar Ficho", "");

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("Debe ingresar un motivo", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var id = Convert.ToInt32(dgvFichos.SelectedRows[0].Cells["Id"].Value);
                await _fichoService.CancelarFichoAsync(id, motivo);

                MessageBox.Show("Ficho cancelado", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await CargarFichosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImprimir_Click(object? sender, EventArgs e)
        {
            if (dgvFichos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un ficho para imprimir", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvFichos.SelectedRows[0].Cells["Id"].Value);
            var formImpresion = new FormImprimirFicho(id);
            formImpresion.ShowDialog();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.F5)
            {
                _ = CargarFichosAsync();
            }
        }
    }
}