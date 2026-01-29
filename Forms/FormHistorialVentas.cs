using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.IO;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL.Forms
{
    public partial class FormHistorialVentas : Form
    {
        private readonly ITransaccionService _transaccionService;
        private readonly IUsuarioService _usuarioService;

        // Controles principales
        private GroupBox grpFiltros = null!;
        private DateTimePicker dtpFechaInicio = null!;
        private DateTimePicker dtpFechaFin = null!;
        private ComboBox cboTipoPago = null!;
        private TextBox txtBuscar = null!;
        private Button btnBuscar = null!;
        private Button btnHoy = null!;
        private Button btnAyer = null!;
        private Button btnSemana = null!;
        private Button btnMes = null!;

        private DataGridView dgvVentas = null!;
        private Panel panelDetalles = null!;
        private DataGridView dgvDetallesVenta = null!;

        private GroupBox grpResumen = null!;
        private Label lblTotalVentas = null!;
        private Label lblTotalDescuentos = null!;
        private Label lblTotalSubsidios = null!;
        private Label lblCantidadVentas = null!;

        private Button btnVerDetalle = null!;
        private Button btnAnular = null!;
        private Button btnExportar = null!;
        private Button btnImprimir = null!;

        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatus = null!;

        public FormHistorialVentas()
        {
            _transaccionService = Program.ServiceProvider.GetRequiredService<ITransaccionService>();
            _usuarioService = Program.ServiceProvider.GetRequiredService<IUsuarioService>();

            InitializeComponent();
            ConfigurarFormulario();
            CrearControles();
            _ = CargarVentasHoyAsync();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Historial de Ventas";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private void CrearControles()
        {
            this.Padding = new Padding(10);
            this.Controls.Clear();

            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(tlpPrincipal);
            
            // === FILTROS SUPERIORES ===
            grpFiltros = new GroupBox { Text = "Filtros de Búsqueda", Dock = DockStyle.Fill, Padding = new Padding(10), AutoSize = true };
            var tlpFiltros = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 7, RowCount = 2 };
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grpFiltros.Controls.Add(tlpFiltros);
            tlpPrincipal.Controls.Add(grpFiltros, 0, 0);

            // Controles de Filtros
            dtpFechaInicio = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill };
            dtpFechaFin = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill };
            cboTipoPago = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            cboTipoPago.Items.AddRange(new[] { "Todos", "Efectivo", "Tarjeta", "Transferencia" });
            cboTipoPago.SelectedIndex = 0;
            txtBuscar = new TextBox { PlaceholderText = "Número, usuario, documento...", Dock = DockStyle.Fill };
            btnBuscar = new Button { Text = "Buscar", BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Left, Width = 100 };
            btnBuscar.Click += BtnBuscar_Click;
            
            var flpFiltrosRapidos = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btnHoy = new Button { Text = "Hoy", UseVisualStyleBackColor = true, AutoSize = true };
            btnHoy.Click += (s, e) => CargarVentasPeriodo(DateTime.Today, DateTime.Today);
            btnAyer = new Button { Text = "Ayer", UseVisualStyleBackColor = true, AutoSize = true };
            btnAyer.Click += (s, e) => CargarVentasPeriodo(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
            btnSemana = new Button { Text = "Esta Semana", UseVisualStyleBackColor = true, AutoSize = true };
            btnSemana.Click += (s, e) => CargarVentasPeriodo(DateTime.Today.AddDays(-7), DateTime.Today);
            btnMes = new Button { Text = "Este Mes", UseVisualStyleBackColor = true, AutoSize = true };
            btnMes.Click += (s, e) => CargarVentasPeriodo(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today);
            flpFiltrosRapidos.Controls.AddRange(new Control[] { btnHoy, btnAyer, btnSemana, btnMes });

            tlpFiltros.Controls.Add(new Label { Text = "Desde:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            tlpFiltros.Controls.Add(dtpFechaInicio, 1, 0);
            tlpFiltros.Controls.Add(new Label { Text = "Hasta:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 2, 0);
            tlpFiltros.Controls.Add(dtpFechaFin, 3, 0);
            tlpFiltros.Controls.Add(flpFiltrosRapidos, 0, 1);
            tlpFiltros.SetColumnSpan(flpFiltrosRapidos, 4);

            tlpFiltros.Controls.Add(new Label { Text = "Tipo Pago:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 4, 0);
            tlpFiltros.Controls.Add(cboTipoPago, 5, 0);

            var tlpBusqueda = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tlpBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpBusqueda.Controls.Add(txtBuscar, 0, 0);
            tlpBusqueda.Controls.Add(btnBuscar, 1, 0);
            tlpFiltros.Controls.Add(tlpBusqueda, 6, 0);
            tlpFiltros.SetRowSpan(tlpBusqueda, 2);

            // === CONTENIDO PRINCIPAL ===
            var tlpContenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            tlpPrincipal.Controls.Add(tlpContenido, 0, 1);

            // SPLIT CONTAINER
            var splitter = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350 };
            tlpContenido.Controls.Add(splitter, 0, 0);
            
            dgvVentas = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, GridColor = Color.FromArgb(224, 224, 224), DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = Color.FromArgb(33, 150, 243), SelectionForeColor = Color.White, Padding = new Padding(5)}, ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(55, 71, 79), ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter}, RowTemplate = { Height = 35 } };
            dgvVentas.SelectionChanged += DgvVentas_SelectionChanged;
            splitter.Panel1.Controls.Add(dgvVentas);
            
            panelDetalles = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblDetalles = new Label { Text = "Detalle de la Venta Seleccionada", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            dgvDetallesVenta = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            panelDetalles.Controls.AddRange(new Control[] { dgvDetallesVenta, lblDetalles });
            splitter.Panel2.Controls.Add(panelDetalles);

            // === PANEL LATERAL DERECHO ===
            var tlpLateral = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(5, 0, 0, 0) };
            tlpLateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpLateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpContenido.Controls.Add(tlpLateral, 1, 0);

            // Resumen
            grpResumen = new GroupBox { Text = "Resumen del Período", Dock = DockStyle.Fill, Padding = new Padding(10), AutoSize = true };
            var tlpResumen = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };
            lblCantidadVentas = new Label { Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true };
            lblTotalVentas = new Label { ForeColor = Color.Navy, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true };
            lblTotalDescuentos = new Label { ForeColor = Color.Green, AutoSize = true };
            lblTotalSubsidios = new Label { ForeColor = Color.Red, AutoSize = true };
            tlpResumen.Controls.AddRange(new Control[] { lblCantidadVentas, lblTotalVentas, lblTotalDescuentos, lblTotalSubsidios });
            grpResumen.Controls.Add(tlpResumen);
            tlpLateral.Controls.Add(grpResumen, 0, 0);

            // Botones de Acción
            var flpBotones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(5) };
            btnVerDetalle = new Button { Text = "Ver Detalle", Width = 180, Height = 35, BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnVerDetalle.Click += BtnVerDetalle_Click;
            btnAnular = new Button { Text = "Anular Venta", Width = 180, Height = 35, BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAnular.Click += BtnAnular_Click;
            btnExportar = new Button { Text = "Exportar Excel", Width = 180, Height = 35, BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportar.Click += BtnExportar_Click;
            btnImprimir = new Button { Text = "Imprimir", Width = 180, Height = 35, UseVisualStyleBackColor = true };
            btnImprimir.Click += (s, e) => MessageBox.Show("Imprimir - Por implementar", "Información");
            flpBotones.Controls.AddRange(new Control[] { btnVerDetalle, btnAnular, btnExportar, btnImprimir });
            tlpLateral.Controls.Add(flpBotones, 0, 1);

            // BARRA DE ESTADO
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Listo");
            statusStrip.Items.Add(lblStatus);
            this.Controls.Add(statusStrip);
            tlpPrincipal.BringToFront(); // Asegura que la barra de estado quede detrás
        }

        private async Task CargarVentasHoyAsync()
        {
            dtpFechaInicio.Value = DateTime.Today;
            dtpFechaFin.Value = DateTime.Today;
            await CargarVentasAsync();
        }

        private void CargarVentasPeriodo(DateTime inicio, DateTime fin)
        {
            dtpFechaInicio.Value = inicio;
            dtpFechaFin.Value = fin;
            _ = CargarVentasAsync();
        }

        private async void BtnBuscar_Click(object? sender, EventArgs e)
        {
            await CargarVentasAsync();
        }

        private async Task CargarVentasAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Cargando ventas...";

                var ventas = await _transaccionService.ObtenerPorRangoFechasAsync(
                    dtpFechaInicio.Value.Date,
                    dtpFechaFin.Value.Date);

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
                {
                    var termino = txtBuscar.Text.ToLower();
                    ventas = ventas.Where(v =>
                        v.NumeroTransaccion.ToLower().Contains(termino) ||
                        v.Usuario?.NombreCompleto.ToLower().Contains(termino) == true ||
                        v.Usuario?.Documento.ToLower().Contains(termino) == true).ToList();
                }

                if (cboTipoPago.SelectedIndex > 0)
                {
                    var tipoPago = (TipoPago)cboTipoPago.SelectedIndex;
                    ventas = ventas.Where(v => v.TipoPago == tipoPago).ToList();
                }

                MostrarVentasEnGrid(ventas);
                ActualizarResumen(ventas);

                lblStatus.Text = $"Se encontraron {ventas.Count} ventas";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error al cargar ventas";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void MostrarVentasEnGrid(List<Transaccion> ventas)
        {
            dgvVentas.DataSource = null;
            dgvVentas.DataSource = ventas.Select(v => new
            {
                v.Id,
                Numero = v.NumeroTransaccion,
                Fecha = v.FechaHora,
                Usuario = v.Usuario?.NombreCompleto ?? "N/A",
                Documento = v.Usuario?.Documento ?? "N/A",
                TipoPago = v.TipoPago.ToString(),
                Subtotal = v.Subtotal,
                Descuento = v.MontoDescuento,
                Total = v.Total,
                Subsidiado = v.EsSubsidiado ? "Sí" : "No",
                Items = v.CantidadProductos,
                Estado = v.Observaciones?.StartsWith("ANULADA") == true ? "ANULADA" : "ACTIVA"
            }).ToList();

            // Ocultar columna Id
            if (dgvVentas.Columns["Id"] != null)
                dgvVentas.Columns["Id"].Visible = false;

            // Formatear columnas
            if (dgvVentas.Columns.Count > 0)
            {
                dgvVentas.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                dgvVentas.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
                dgvVentas.Columns["Descuento"].DefaultCellStyle.Format = "C2";
                dgvVentas.Columns["Total"].DefaultCellStyle.Format = "C2";

                dgvVentas.Columns["Numero"].Width = 120;
                dgvVentas.Columns["Fecha"].Width = 130;
                dgvVentas.Columns["Usuario"].Width = 150;
                dgvVentas.Columns["Documento"].Width = 100;
                dgvVentas.Columns["Items"].Width = 60;

                // Colorear filas anuladas
                foreach (DataGridViewRow row in dgvVentas.Rows)
                {
                    if (row.Cells["Estado"]?.Value?.ToString() == "ANULADA")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        row.DefaultCellStyle.Font = new Font(dgvVentas.Font, FontStyle.Strikeout);
                    }
                }
            }
        }

        private void ActualizarResumen(List<Transaccion> ventas)
        {
            var ventasActivas = ventas.Where(v => !v.Observaciones?.StartsWith("ANULADA") == true).ToList();

            lblCantidadVentas.Text = $"Ventas: {ventasActivas.Count}";
            lblTotalVentas.Text = $"Total: {ventasActivas.Sum(v => v.Total):C}";
            lblTotalDescuentos.Text = $"Descuentos: {ventasActivas.Sum(v => v.MontoDescuento):C}";
            lblTotalSubsidios.Text = $"Subsidios: {ventasActivas.Where(v => v.EsSubsidiado).Sum(v => v.Subtotal):C}";
        }

        private async void DgvVentas_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count == 0)
            {
                dgvDetallesVenta.DataSource = null;
                return;
            }

            try
            {
                var id = Convert.ToInt32(dgvVentas.SelectedRows[0].Cells["Id"].Value);
                var transaccion = await _transaccionService.ObtenerPorIdConDetallesAsync(id);

                if (transaccion != null)
                {
                    dgvDetallesVenta.DataSource = transaccion.Detalles.Select(d => new
                    {
                        Codigo = d.Producto?.Codigo ?? "N/A",
                        Producto = d.Producto?.Nombre ?? "N/A",
                        Cantidad = d.Cantidad,
                        PrecioUnit = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList();

                    if (dgvDetallesVenta.Columns.Count > 0)
                    {
                        dgvDetallesVenta.Columns["PrecioUnit"].DefaultCellStyle.Format = "C2";
                        dgvDetallesVenta.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
                        dgvDetallesVenta.Columns["PrecioUnit"].HeaderText = "P. Unitario";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnVerDetalle_Click(object? sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una venta para ver el detalle", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvVentas.SelectedRows[0].Cells["Id"].Value);
            var formDetalle = new FormDetalleVenta(id);
            formDetalle.ShowDialog();
        }

        private async void BtnAnular_Click(object? sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una venta para anular", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var estado = dgvVentas.SelectedRows[0].Cells["Estado"].Value?.ToString();
            if (estado == "ANULADA")
            {
                MessageBox.Show("Esta venta ya está anulada", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var numero = dgvVentas.SelectedRows[0].Cells["Numero"].Value?.ToString();

            var resultado = MessageBox.Show(
                $"¿Está seguro de anular la venta {numero}?\n\n" +
                "Esta acción devolverá el stock de los productos vendidos.",
                "Confirmar Anulación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                // Solicitar motivo
                string motivo = Microsoft.VisualBasic.Interaction.InputBox(
                    "Ingrese el motivo de la anulación:",
                    "Motivo de Anulación",
                    "");

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    MessageBox.Show("Debe ingresar un motivo para anular", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var id = Convert.ToInt32(dgvVentas.SelectedRows[0].Cells["Id"].Value);
                    await _transaccionService.AnularTransaccionAsync(id, motivo);

                    MessageBox.Show("Venta anulada correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await CargarVentasAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al anular venta: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void BtnExportar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvVentas.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar", "Información",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Archivos Excel (*.xlsx)|*.xlsx";
                    saveFileDialog.FileName = $"Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        lblStatus.Text = "Exportando a Excel...";

                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Ventas");

                            // Título
                            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS - CAFETERÍA UNAL";
                            worksheet.Cell(1, 1).Style.Font.Bold = true;
                            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                            worksheet.Range(1, 1, 1, 10).Merge();

                            worksheet.Cell(2, 1).Value = $"Período: {dtpFechaInicio.Value:dd/MM/yyyy} - {dtpFechaFin.Value:dd/MM/yyyy}";
                            worksheet.Range(2, 1, 2, 10).Merge();

                            // Espacio
                            var currentRow = 4;

                            // Encabezados
                            var headers = new[] { "Número", "Fecha", "Usuario", "Documento", "Tipo Pago",
                                                 "Subtotal", "Descuento", "Total", "Subsidiado", "Estado" };

                            for (int i = 0; i < headers.Length; i++)
                            {
                                worksheet.Cell(currentRow, i + 1).Value = headers[i];
                                worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.DarkBlue;
                                worksheet.Cell(currentRow, i + 1).Style.Font.FontColor = XLColor.White;
                                worksheet.Cell(currentRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            currentRow++;

                            // Datos
                            foreach (DataGridViewRow row in dgvVentas.Rows)
                            {
                                worksheet.Cell(currentRow, 1).Value = row.Cells["Numero"].Value?.ToString();
                                worksheet.Cell(currentRow, 2).Value = row.Cells["Fecha"].Value?.ToString();
                                worksheet.Cell(currentRow, 3).Value = row.Cells["Usuario"].Value?.ToString();
                                worksheet.Cell(currentRow, 4).Value = row.Cells["Documento"].Value?.ToString();
                                worksheet.Cell(currentRow, 5).Value = row.Cells["TipoPago"].Value?.ToString();

                                var subtotal = Convert.ToDecimal(row.Cells["Subtotal"].Value);
                                var descuento = Convert.ToDecimal(row.Cells["Descuento"].Value);
                                var total = Convert.ToDecimal(row.Cells["Total"].Value);

                                worksheet.Cell(currentRow, 6).Value = subtotal;
                                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "$#,##0.00";

                                worksheet.Cell(currentRow, 7).Value = descuento;
                                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "$#,##0.00";

                                worksheet.Cell(currentRow, 8).Value = total;
                                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$#,##0.00";

                                worksheet.Cell(currentRow, 9).Value = row.Cells["Subsidiado"].Value?.ToString();
                                worksheet.Cell(currentRow, 10).Value = row.Cells["Estado"].Value?.ToString();

                                // Colorear filas anuladas
                                if (row.Cells["Estado"].Value?.ToString() == "ANULADA")
                                {
                                    worksheet.Range(currentRow, 1, currentRow, 10).Style.Font.FontColor = XLColor.Red;
                                    worksheet.Range(currentRow, 1, currentRow, 10).Style.Font.Strikethrough = true;
                                }

                                currentRow++;
                            }

                            // Resumen
                            currentRow += 2;
                            worksheet.Cell(currentRow, 1).Value = "RESUMEN";
                            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                            worksheet.Range(currentRow, 1, currentRow, 3).Merge();

                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = "Total Ventas:";
                            worksheet.Cell(currentRow, 2).Value = lblCantidadVentas.Text;

                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = "Total Ingresos:";
                            worksheet.Cell(currentRow, 2).Value = lblTotalVentas.Text;

                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = "Total Descuentos:";
                            worksheet.Cell(currentRow, 2).Value = lblTotalDescuentos.Text;

                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = "Total Subsidios:";
                            worksheet.Cell(currentRow, 2).Value = lblTotalSubsidios.Text;

                            // Ajustar anchos de columna
                            worksheet.Columns().AdjustToContents();

                            // Guardar
                            workbook.SaveAs(saveFileDialog.FileName);
                        }

                        lblStatus.Text = "Exportación completada";
                        MessageBox.Show("Archivo exportado exitosamente", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Preguntar si desea abrir el archivo
                        if (MessageBox.Show("¿Desea abrir el archivo exportado?", "Abrir archivo",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error en la exportación";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.F5)
            {
                _ = CargarVentasAsync();
            }
        }
    }
}
