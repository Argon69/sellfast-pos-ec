using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL.Forms
{
    public partial class FormReportes : MaterialForm
    {
        private readonly ITransaccionService _transaccionService;
        private readonly IProductoService _productoService;
        private readonly IUsuarioService _usuarioService;

        private MaterialComboBox cmbTipoReporte = null!;
        private DateTimePicker dtpFechaInicio = null!; // Mantener el control estándar por falta de Material
        private DateTimePicker dtpFechaFin = null!; // Mantener el control estándar
        private MaterialButton btnGenerar = null!;
        private MaterialButton btnExportar = null!;
        private DataGridView dgvReportes = null!;
        private MaterialTabControl tabControl = null!;
        private Chart chartPrincipal = null!;
        private Chart chartSecundario = null!;
        private MaterialLabel lblEstadisticas = null!;
        private MaterialLabel lblDetalles = null!;
        private MaterialLabel lblFeedbackStatus = null!; // Nuevo para mensajes de estado

        public FormReportes()
        {
            _transaccionService = Program.ServiceProvider.GetRequiredService<ITransaccionService>();
            _productoService = Program.ServiceProvider.GetRequiredService<IProductoService>();
            _usuarioService = Program.ServiceProvider.GetRequiredService<IUsuarioService>();

            InitializeComponent();
            ConfigurarFormularioMaterial();
            CrearControlesMaterial();
            CargarDatosIniciales();
        }

        private void ConfigurarFormularioMaterial()
        {
            this.Text = "📊 Sistema de Reportes con Gráficos";
            this.Size = new Size(1400, 800);
            this.Sizable = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.Padding = new Padding(14, 60, 14, 14); // Espacio para el título de MaterialForm
        }

        private void CrearControlesMaterial()
        {
            this.Controls.Clear();

            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(tlpPrincipal);

            // === FILTROS SUPERIORES ===
            var cardFiltros = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(10), Margin = new Padding(0,0,0,5), AutoSize=true };
            var tlpFiltros = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 8 };
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220)); // ComboBox
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // DateTimePicker
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // DateTimePicker
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Spacer
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Buttons
            cardFiltros.Controls.Add(tlpFiltros);
            tlpPrincipal.Controls.Add(cardFiltros, 0, 0);

            // Controles de Filtros
            cmbTipoReporte = new MaterialComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipoReporte.Items.AddRange(new string[] { "Ventas del Día", "Ventas por Período", "Resumen de Ventas", "Productos Más Vendidos", "Estadísticas por Usuario", "Productos con Stock Bajo", "Transacciones por Tipo de Pago" });
            cmbTipoReporte.SelectedIndexChanged += CmbTipoReporte_SelectedIndexChanged;
            dtpFechaInicio = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
            dtpFechaFin = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
            btnGenerar = new MaterialButton { Text = "Generar", Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, AutoSize=true, Margin = new Padding(5,0,5,0) };
            btnGenerar.Click += BtnGenerar_Click;
            btnExportar = new MaterialButton { Text = "Exportar", Type = MaterialButton.MaterialButtonType.Outlined, AutoSize=true };
            btnExportar.Click += BtnExportar_Click;
            lblFeedbackStatus = new MaterialLabel { Text = "Listo para generar reportes", Dock = DockStyle.Fill, FontType = MaterialSkinManager.fontType.Caption, HighEmphasis = false, TextAlign = ContentAlignment.MiddleLeft };
            
            tlpFiltros.Controls.Add(new MaterialLabel { Text = "Tipo de Reporte:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, FontType = MaterialSkinManager.fontType.Subtitle1}, 0, 0);
            tlpFiltros.Controls.Add(cmbTipoReporte, 1, 0);
            tlpFiltros.Controls.Add(new MaterialLabel { Text = "Fecha Inicio:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, FontType = MaterialSkinManager.fontType.Subtitle1 }, 2, 0);
            tlpFiltros.Controls.Add(dtpFechaInicio, 3, 0);
            tlpFiltros.Controls.Add(new MaterialLabel { Text = "Fecha Fin:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, FontType = MaterialSkinManager.fontType.Subtitle1 }, 4, 0);
            tlpFiltros.Controls.Add(dtpFechaFin, 5, 0);
            
            var flpBotones = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Anchor = AnchorStyles.Right };
            flpBotones.Controls.AddRange(new Control[] { btnGenerar, btnExportar });
            tlpFiltros.Controls.Add(flpBotones, 7, 0);
            
            tlpFiltros.Controls.Add(lblFeedbackStatus, 0, 1);
            tlpFiltros.SetColumnSpan(lblFeedbackStatus, 8);


            // === TAB CONTROL ===
            tabControl = new MaterialTabControl { Dock = DockStyle.Fill };
            var tabDatos = new TabPage("Datos");
            var tabGraficos = new TabPage("Gráficos");
            tabControl.Controls.Add(tabDatos);
            tabControl.Controls.Add(tabGraficos);
            tlpPrincipal.Controls.Add(tabControl, 0, 1);

            // Contenido Tab Datos
            dgvReportes = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BorderStyle = BorderStyle.None };
            EstilizarDataGridView();
            tabDatos.Controls.Add(dgvReportes);

            // Contenido Tab Gráficos
            var tlpGraficos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tlpGraficos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            tlpGraficos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            chartPrincipal = new Chart { Dock = DockStyle.Fill };
            ConfigurarChart(chartPrincipal);
            chartSecundario = new Chart { Dock = DockStyle.Fill };
            ConfigurarChart(chartSecundario);
            tlpGraficos.Controls.Add(chartPrincipal, 0, 0);
            tlpGraficos.Controls.Add(chartSecundario, 1, 0);
            tabGraficos.Controls.Add(tlpGraficos);

            // === ESTADÍSTICAS INFERIORES ===
            var cardEstadisticas = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(10), Margin = new Padding(0,5,0,0), AutoSize = true };
            var tlpEstadisticas = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };
            lblEstadisticas = new MaterialLabel { Text = "📊 Seleccione un tipo de reporte para ver las estadísticas", FontType = MaterialSkinManager.fontType.H6, AutoSize = true };
            lblDetalles = new MaterialLabel { Text = "", FontType = MaterialSkinManager.fontType.Body2, AutoSize = true, Margin = new Padding(0,5,0,0) };
            tlpEstadisticas.Controls.Add(lblEstadisticas, 0, 0);
            tlpEstadisticas.Controls.Add(lblDetalles, 0, 1);
            cardEstadisticas.Controls.Add(tlpEstadisticas);
            tlpPrincipal.Controls.Add(cardEstadisticas, 0, 2);
        }

        private void EstilizarDataGridView()
        {
            dgvReportes.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvReportes.GridColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;

            dgvReportes.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            dgvReportes.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvReportes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvReportes.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvReportes.EnableHeadersVisualStyles = false;
            
            dgvReportes.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvReportes.DefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
            dgvReportes.DefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvReportes.DefaultCellStyle.Padding = new Padding(5);
            dgvReportes.RowTemplate.Height = 35;
        }

        private void ConfigurarChart(Chart chart)
        {
            chart.ChartAreas.Add(new ChartArea("MainArea"));
            chart.ChartAreas[0].BackColor = Color.Transparent;
            chart.ChartAreas[0].BorderColor = MaterialSkinManager.Instance.ColorScheme.PrimaryColor;
            chart.ChartAreas[0].BorderWidth = 1;
            chart.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;

            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            chart.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            chart.ChartAreas[0].AxisX.TitleForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            chart.ChartAreas[0].AxisY.TitleForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;


            chart.Legends.Add(new Legend("MainLegend"));
            chart.Legends[0].BackColor = Color.Transparent;
            chart.Legends[0].Font = new Font("Segoe UI", 9F);
            chart.Legends[0].ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
        }

        private void CargarDatosIniciales()
        {
            cmbTipoReporte.SelectedIndex = 0;
            dtpFechaInicio.Value = DateTime.Today;
            dtpFechaFin.Value = DateTime.Today;
        }

        private void CmbTipoReporte_SelectedIndexChanged(object? sender, EventArgs e)
        {
            switch (cmbTipoReporte.SelectedItem?.ToString())
            {
                case "Ventas del Día":
                    dtpFechaInicio.Value = DateTime.Today;
                    dtpFechaFin.Value = DateTime.Today;
                    dtpFechaFin.Enabled = false;
                    break;
                case "Ventas por Período":
                case "Resumen de Ventas":
                    dtpFechaInicio.Value = DateTime.Today.AddDays(-7);
                    dtpFechaFin.Value = DateTime.Today;
                    dtpFechaFin.Enabled = true;
                    break;
                case "Productos con Stock Bajo":
                    dtpFechaInicio.Enabled = false;
                    dtpFechaFin.Enabled = false;
                    break;
                default:
                    dtpFechaInicio.Enabled = true;
                    dtpFechaFin.Enabled = true;
                    break;
            }
        }

        private async void BtnGenerar_Click(object? sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblFeedbackStatus.Text = "Generando reporte...";

                string tipoReporte = cmbTipoReporte.SelectedItem?.ToString() ?? "";
                if (string.IsNullOrEmpty(tipoReporte))
                {
                    MessageBox.Show("Por favor, seleccione un tipo de reporte.", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime fechaInicio = dtpFechaInicio.Value.Date;
                DateTime fechaFin = dtpFechaFin.Value.Date;

                chartPrincipal.Series.Clear();
                chartSecundario.Series.Clear();
                chartPrincipal.Titles.Clear();
                chartSecundario.Titles.Clear();

                switch (tipoReporte)
                {
                    case "Ventas del Día":
                        await GenerarReporteVentasDiaAsync(fechaInicio);
                        break;
                    case "Ventas por Período":
                        await GenerarReporteVentasPeriodoAsync(fechaInicio, fechaFin);
                        break;
                    case "Resumen de Ventas":
                        await GenerarReporteResumenVentasAsync(fechaInicio, fechaFin);
                        break;
                    case "Productos Más Vendidos":
                        await GenerarReporteProductosMasVendidosAsync(fechaInicio, fechaFin);
                        break;
                    case "Estadísticas por Usuario":
                        await GenerarReporteEstadisticasUsuarioAsync(fechaInicio, fechaFin);
                        break;
                    case "Productos con Stock Bajo":
                        await GenerarReporteProductosStockBajoAsync();
                        break;
                    case "Transacciones por Tipo de Pago":
                        await GenerarReporteTransaccionesTipoPagoAsync(fechaInicio, fechaFin);
                        break;
                }

                lblFeedbackStatus.Text = "Reporte generado exitosamente";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblFeedbackStatus.Text = "Error al generar reporte";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task GenerarReporteVentasDiaAsync(DateTime fecha)
        {
            var transacciones = await _transaccionService.ObtenerDelDiaAsync(fecha);
            var datos = new List<object>();

            foreach (var transaccion in transacciones)
            {
                foreach (var detalle in transaccion.Detalles)
                {
                    datos.Add(new
                    {
                        NumeroTransaccion = transaccion.NumeroTransaccion,
                        Cliente = transaccion.Usuario?.NombreCompleto ?? "N/A",
                        Documento = transaccion.Usuario?.Documento ?? "N/A",
                        TipoUsuario = transaccion.Usuario?.TipoUsuario.ToString() ?? "N/A",
                        Producto = detalle.Producto?.Nombre ?? "N/A",
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal,
                        DescuentoPorcentaje = transaccion.PorcentajeDescuento,
                        MontoDescuento = transaccion.MontoDescuento,
                        TotalTransaccion = transaccion.Total,
                        TipoPago = transaccion.TipoPago.ToString(),
                        Hora = transaccion.FechaHora.ToString("HH:mm:ss"),
                        EsSubsidiado = transaccion.EsSubsidiado ? "Sí" : "No"
                    });
                }
            }

            dgvReportes.DataSource = datos;
            ConfigurarColumnasMoneda();

            GenerarGraficosVentasDiaAsync(transacciones, fecha);

            var totalVentas = transacciones.Sum(t => t.Total);
            var totalDescuentos = transacciones.Sum(t => t.MontoDescuento);
            var totalSubsidios = transacciones.Where(t => t.EsSubsidiado).Sum(t => t.Subtotal);
            var cantidadTransacciones = transacciones.Count;

            lblEstadisticas.Text = $"📊 Resumen de Ventas del {fecha:dd/MM/yyyy}";
            lblDetalles.Text = $"Total Transacciones: {cantidadTransacciones} | " +
                              $"Monto Total: {totalVentas:C} | " +
                              $"Total Descuentos: {totalDescuentos:C} | " +
                              $"Total Subsidios: {totalSubsidios:C} | " +
                              $"Promedio por Transacción: {(cantidadTransacciones > 0 ? totalVentas / cantidadTransacciones : 0):C}";
        }

        private void GenerarGraficosVentasDiaAsync(List<Transaccion> transacciones, DateTime fecha)
        {
            chartPrincipal.Titles.Add($"Ventas por Hora - {fecha:dd/MM/yyyy}");
            var ventasPorHora = transacciones
                .GroupBy(t => t.FechaHora.Hour)
                .Select(g => new { Hora = g.Key, Total = g.Sum(t => t.Total) })
                .OrderBy(v => v.Hora)
                .ToList();

            var serieHoras = new Series("Ventas por Hora")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(54, 162, 235),
                BorderWidth = 2
            };

            foreach (var venta in ventasPorHora)
            {
                serieHoras.Points.AddXY($"{venta.Hora}:00", venta.Total);
            }
            chartPrincipal.Series.Add(serieHoras);

            chartSecundario.Titles.Add("Distribución por Tipo de Pago");
            var pagosPorTipo = transacciones
                .GroupBy(t => t.TipoPago.ToString())
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToList();

            var seriePagos = new Series("Tipos de Pago")
            {
                ChartType = SeriesChartType.Pie
            };

            var colores = new Color[] {
                Color.FromArgb(255, 99, 132),
                Color.FromArgb(54, 162, 235),
                Color.FromArgb(255, 205, 86)
            };

            for (int i = 0; i < pagosPorTipo.Count; i++)
            {
                var punto = seriePagos.Points.AddXY(pagosPorTipo[i].Tipo, pagosPorTipo[i].Cantidad);
                seriePagos.Points[punto].Color = colores[i % colores.Length];
                seriePagos.Points[punto].Label = $"{pagosPorTipo[i].Tipo}\n({pagosPorTipo[i].Cantidad})";
            }
            chartSecundario.Series.Add(seriePagos);
        }

        private async Task GenerarReporteVentasPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var transacciones = await _transaccionService.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            var ventasPorDia = transacciones
                .GroupBy(t => t.FechaHora.Date)
                .Select(g => new
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    CantidadTransacciones = g.Count(),
                    TotalVentas = g.Sum(t => t.Total),
                    TotalDescuentos = g.Sum(t => t.MontoDescuento),
                    TotalSubsidios = g.Where(t => t.EsSubsidiado).Sum(t => t.Subtotal),
                    PromedioVenta = g.Average(t => t.Total),
                    VentasEfectivo = g.Count(t => t.TipoPago == TipoPago.Efectivo),
                    VentasTarjeta = g.Count(t => t.TipoPago == TipoPago.Tarjeta),
                    VentasTransferencia = g.Count(t => t.TipoPago == TipoPago.Transferencia),
                    FechaOrden = g.Key
                })
                .OrderByDescending(v => v.FechaOrden)
                .ToList();

            dgvReportes.DataSource = ventasPorDia;
            ConfigurarColumnasMoneda();

            GenerarGraficosVentasPeriodoAsync(ventasPorDia.Cast<object>().ToList(), fechaInicio, fechaFin);

            var totalPeriodo = transacciones.Sum(t => t.Total);
            var totalTransacciones = transacciones.Count;
            var diasConVentas = ventasPorDia.Count;

            lblEstadisticas.Text = $"📊 Ventas del Período {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            lblDetalles.Text = $"Días con Ventas: {diasConVentas} | " +
                              $"Total Transacciones: {totalTransacciones} | " +
                              $"Monto Total: {totalPeriodo:C} | " +
                              $"Promedio Diario: {(diasConVentas > 0 ? totalPeriodo / diasConVentas : 0):C}";
        }

        private void GenerarGraficosVentasPeriodoAsync(List<object> ventasPorDia, DateTime fechaInicio, DateTime fechaFin)
        {
            chartPrincipal.Titles.Add($"Evolución de Ventas - {fechaInicio:dd/MM/yyyy} a {fechaFin:dd/MM/yyyy}");

            var serieVentas = new Series("Total Ventas")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(75, 192, 192),
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6
            };

            var serieTransacciones = new Series("Cantidad Transacciones")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(255, 159, 64),
                YAxisType = AxisType.Secondary
            };

            var ventasData = ventasPorDia.Cast<dynamic>().ToList();
            foreach (var venta in ventasData)
            {
                serieVentas.Points.AddXY((string)venta.Fecha, (double)(decimal)venta.TotalVentas);
                serieTransacciones.Points.AddXY((string)venta.Fecha, (int)venta.CantidadTransacciones);
            }

            chartPrincipal.Series.Add(serieVentas);
            chartPrincipal.Series.Add(serieTransacciones);

            chartPrincipal.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartPrincipal.ChartAreas[0].AxisY2.Title = "Cantidad Transacciones";
            chartPrincipal.ChartAreas[0].AxisY.Title = "Monto Ventas";

            chartSecundario.Titles.Add("Métodos de Pago por Día");

            var serieEfectivo = new Series("Efectivo") { ChartType = SeriesChartType.StackedColumn, Color = Color.FromArgb(255, 99, 132) };
            var serieTarjeta = new Series("Tarjeta") { ChartType = SeriesChartType.StackedColumn, Color = Color.FromArgb(54, 162, 235) };
            var serieTransferencia = new Series("Transferencia") { ChartType = SeriesChartType.StackedColumn, Color = Color.FromArgb(255, 205, 86) };

            foreach (var venta in ventasData)
            {
                serieEfectivo.Points.AddXY((string)venta.Fecha, (int)venta.VentasEfectivo);
                serieTarjeta.Points.AddXY((string)venta.Fecha, (int)venta.VentasTarjeta);
                serieTransferencia.Points.AddXY((string)venta.Fecha, (int)venta.VentasTransferencia);
            }

            chartSecundario.Series.Add(serieEfectivo);
            chartSecundario.Series.Add(serieTarjeta);
            chartSecundario.Series.Add(serieTransferencia);
        }

        private async Task GenerarReporteResumenVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var resumen = await _transaccionService.ObtenerResumenDelDiaAsync(fechaInicio);

            var datos = new List<object>
            {
                new { Concepto = "Total Transacciones", Valor = resumen.TotalTransacciones.ToString(), Monto = "-" },
                new { Concepto = "Total Ventas", Valor = "-", Monto = resumen.TotalVentas.ToString("C") },
                new { Concepto = "Total Descuentos", Valor = "-", Monto = resumen.TotalDescuentos.ToString("C") },
                new { Concepto = "Total Subsidios", Valor = "-", Monto = resumen.TotalSubsidios.ToString("C") },
                new { Concepto = "Pagos en Efectivo", Valor = resumen.TransaccionesEfectivo.ToString(), Monto = "-" },
                new { Concepto = "Pagos con Tarjeta", Valor = resumen.TransaccionesTarjeta.ToString(), Monto = "-" },
                new { Concepto = "Transferencias", Valor = resumen.TransaccionesTransferencia.ToString(), Monto = "-" }
            };

            dgvReportes.DataSource = datos;

            GenerarGraficosResumenAsync(resumen, fechaInicio);

            lblEstadisticas.Text = $"📊 Resumen General - {fechaInicio:dd/MM/yyyy}";
            lblDetalles.Text = $"Resumen completo de todas las operaciones del día seleccionado";
        }

        private void GenerarGraficosResumenAsync(ResumenVentas resumen, DateTime fecha)
        {
            chartPrincipal.Titles.Add($"Resumen Financiero - {fecha:dd/MM/yyyy}");

            var serieTotales = new Series("Montos")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(54, 162, 235)
            };

            serieTotales.Points.AddXY("Ventas Total", (double)resumen.TotalVentas);
            serieTotales.Points.AddXY("Descuentos", (double)resumen.TotalDescuentos);
            serieTotales.Points.AddXY("Subsidios", (double)resumen.TotalSubsidios);

            chartPrincipal.Series.Add(serieTotales);

            chartSecundario.Titles.Add("Métodos de Pago");

            var seriePagos = new Series("Métodos de Pago")
            {
                ChartType = SeriesChartType.Doughnut
            };

            var colores = new Color[] {
                Color.FromArgb(255, 99, 132),
                Color.FromArgb(54, 162, 235),
                Color.FromArgb(255, 205, 86)
            };

            for (int i = 0; i < seriePagos.Points.Count; i++)
            {
                seriePagos.Points[i].Color = colores[i % colores.Length];
                seriePagos.Points[i].Label = $"{seriePagos.Points[i].AxisLabel}\n{seriePagos.Points[i].YValues[0]}";
            }

            chartSecundario.Series.Add(seriePagos);
        }

        private async Task GenerarReporteProductosMasVendidosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var transacciones = await _transaccionService.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            var productosMasVendidos = transacciones
                .SelectMany(t => t.Detalles)
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre, d.Producto.Categoria })
                .Select(g => new
                {
                    Producto = g.Key.Nombre,
                    Categoria = g.Key.Categoria.ToString(),
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.Subtotal),
                    PrecioPromedio = g.Average(d => d.PrecioUnitario),
                    TransaccionesParticipadas = g.Select(d => d.TransaccionId).Distinct().Count()
                })
                .OrderByDescending(p => p.CantidadVendida)
                .ToList();

            dgvReportes.DataSource = productosMasVendidos;
            ConfigurarColumnasMoneda();

            GenerarGraficosProductosMasVendidosAsync(productosMasVendidos.Cast<object>().ToList(), fechaInicio, fechaFin);

            var totalProductos = productosMasVendidos.Count;
            var totalUnidades = productosMasVendidos.Sum(p => p.CantidadVendida);
            var totalIngresos = productosMasVendidos.Sum(p => p.TotalVentas);

            lblEstadisticas.Text = $"📊 Productos Más Vendidos {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            lblDetalles.Text = $"Productos Vendidos: {totalProductos} | " +
                              $"Total Unidades: {totalUnidades} | " +
                              $"Ingresos Totales: {totalIngresos:C}";
        }

        private void GenerarGraficosProductosMasVendidosAsync(List<object> productos, DateTime fechaInicio, DateTime fechaFin)
        {
            chartPrincipal.Titles.Add($"Top 10 Productos Más Vendidos - {fechaInicio:dd/MM/yyyy} a {fechaFin:dd/MM/yyyy}");

            var serieProductos = new Series("Cantidad Vendida")
            {
                ChartType = SeriesChartType.Bar,
                Color = Color.FromArgb(75, 192, 192)
            };

            var productosData = productos.Cast<dynamic>().Take(10).ToList();
            foreach (var producto in productosData)
            {
                string nombre = (string)producto.Producto;
                var nombreCorto = nombre.Length > 15 ? nombre.Substring(0, 12) + "..." : nombre;
                serieProductos.Points.AddXY(nombreCorto, (int)producto.CantidadVendida);
            }

            chartPrincipal.Series.Add(serieProductos);

            chartSecundario.Titles.Add("Ventas por Categoría");

            var ventasPorCategoria = productos.Cast<dynamic>()
                .GroupBy(p => (string)p.Categoria)
                .Select(g => new {
                    Categoria = g.Key,
                    TotalVentas = g.Sum(d => (decimal)d.TotalVentas),
                    CantidadProductos = g.Count()
                })
                .OrderByDescending(c => c.TotalVentas)
                .ToList();

            var serieCategorias = new Series("Ventas por Categoría")
            {
                ChartType = SeriesChartType.Pie
            };

            var coloresCategorias = new Color[] {
                Color.FromArgb(255, 99, 132), Color.FromArgb(54, 162, 235),
                Color.FromArgb(255, 205, 86), Color.FromArgb(75, 192, 192),
                Color.FromArgb(153, 102, 255), Color.FromArgb(255, 159, 64)
            };

            for (int i = 0; i < ventasPorCategoria.Count; i++)
            {
                var punto = serieCategorias.Points.AddXY(ventasPorCategoria[i].Categoria, (double)ventasPorCategoria[i].TotalVentas);
                serieCategorias.Points[punto].Color = coloresCategorias[i % coloresCategorias.Length];
                serieCategorias.Points[punto].Label = $"{ventasPorCategoria[i].Categoria}\n{ventasPorCategoria[i].TotalVentas:C}";
            }

            chartSecundario.Series.Add(serieCategorias);
        }

        private async Task GenerarReporteEstadisticasUsuarioAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var transacciones = await _transaccionService.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            var estadisticasUsuario = transacciones
                .GroupBy(t => new { t.Usuario!.TipoUsuario, ModalidadPago = t.Usuario.ModalidadPago?.ToString() ?? "N/A" })
                .Select(g => new
                {
                    TipoUsuario = g.Key.TipoUsuario.ToString(),
                    ModalidadPago = g.Key.ModalidadPago,
                    CantidadUsuarios = g.Select(t => t.UsuarioId).Distinct().Count(),
                    TotalCompras = g.Count(),
                    TotalGastado = g.Sum(t => t.Total),
                    TotalSubsidios = g.Sum(t => t.MontoDescuento),
                    PromedioCompra = g.Average(t => t.Total),
                    PromedioDescuento = g.Average(t => t.PorcentajeDescuento)
                })
                .OrderByDescending(e => e.TotalCompras)
                .ToList();

            dgvReportes.DataSource = estadisticasUsuario;
            ConfigurarColumnasMoneda();

            GenerarGraficosEstadisticasUsuarioAsync(estadisticasUsuario.Cast<object>().ToList(), fechaInicio, fechaFin);

            var totalUsuarios = estadisticasUsuario.Sum(e => e.CantidadUsuarios);
            var totalCompras = estadisticasUsuario.Sum(e => e.TotalCompras);
            var totalGastado = estadisticasUsuario.Sum(e => e.TotalGastado);

            lblEstadisticas.Text = $"📊 Estadísticas por Tipo de Usuario {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            lblDetalles.Text = $"Total Usuarios: {totalUsuarios} | " +
                              $"Total Compras: {totalCompras} | " +
                              $"Total Gastado: {totalGastado:C}";
        }

        private void GenerarGraficosEstadisticasUsuarioAsync(List<object> estadisticas, DateTime fechaInicio, DateTime fechaFin)
        {
            chartPrincipal.Titles.Add($"Compras por Tipo de Usuario - {fechaInicio:dd/MM/yyyy} a {fechaFin:dd/MM/yyyy}");

            var serieCompras = new Series("Total Compras")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(54, 162, 235)
            };

            var serieUsuarios = new Series("Cantidad Usuarios")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(255, 99, 132),
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                YAxisType = AxisType.Secondary
            };

            var estadisticasData = estadisticas.Cast<dynamic>().ToList();
            foreach (var stat in estadisticasData)
            {
                string modalidadPago = (string)stat.ModalidadPago;
                string tipoUsuario = (string)stat.TipoUsuario;
                var etiqueta = modalidadPago != "N/A" ?
                    $"{tipoUsuario}\n({modalidadPago})" : tipoUsuario;
                serieCompras.Points.AddXY(etiqueta, (int)stat.TotalCompras);
                serieUsuarios.Points.AddXY(etiqueta, (int)stat.CantidadUsuarios);
            }

            chartPrincipal.Series.Add(serieCompras);
            chartPrincipal.Series.Add(serieUsuarios);

            chartPrincipal.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartPrincipal.ChartAreas[0].AxisY2.Title = "Cantidad Usuarios";
            chartPrincipal.ChartAreas[0].AxisY.Title = "Total Compras";

            chartSecundario.Titles.Add("Distribución de Gastos");

            var serieGastos = new Series("Total Gastado")
            {
                ChartType = SeriesChartType.Doughnut
            };

            var colores = new Color[] {
                Color.FromArgb(255, 99, 132), Color.FromArgb(54, 162, 235),
                Color.FromArgb(255, 205, 86), Color.FromArgb(75, 192, 192)
            };

            for (int i = 0; i < estadisticasData.Count; i++)
            {
                string modalidadPago = (string)estadisticasData[i].ModalidadPago;
                string tipoUsuario = (string)estadisticasData[i].TipoUsuario;
                var etiqueta = modalidadPago != "N/A" ?
                    $"{tipoUsuario} ({modalidadPago})" : tipoUsuario;
                var punto = serieGastos.Points.AddXY(etiqueta, (double)(decimal)estadisticasData[i].TotalGastado);
                serieGastos.Points[punto].Color = colores[i % colores.Length];
                serieGastos.Points[punto].Label = $"{etiqueta}\n{(decimal)estadisticasData[i].TotalGastado:C}";
            }

            chartSecundario.Series.Add(serieGastos);
        }

        private async Task GenerarReporteProductosStockBajoAsync()
        {
            var productosStockBajo = await _productoService.ObtenerProductosConStockBajoAsync();

            var datos = productosStockBajo.Select(p => new
            {
                p.Codigo,
                p.Nombre,
                Categoria = p.Categoria.ToString(),
                StockActual = p.StockDisponible,
                StockMinimo = p.StockMinimo,
                DeficitStock = p.StockMinimo - p.StockDisponible,
                EstadoStock = p.EstadoStock,
                p.Precio,
                ValorInventario = p.Precio * p.StockDisponible,
                UltimaModificacion = p.FechaUltimaModificacion?.ToString("dd/MM/yyyy") ?? "N/A",
                Estado = p.Activo ? "Activo" : "Inactivo"
            }).ToList();

            dgvReportes.DataSource = datos;
            ConfigurarColumnasMoneda();

            foreach (DataGridViewRow row in dgvReportes.Rows)
            {
                var estadoStock = row.Cells["EstadoStock"]?.Value?.ToString();
                if (estadoStock == "Sin Stock")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                else if (estadoStock == "Stock Bajo")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230);
            }

            GenerarGraficosStockBajoAsync(datos.Cast<object>().ToList());

            var totalProductos = productosStockBajo.Count;
            var sinStock = productosStockBajo.Count(p => p.StockDisponible == 0);
            var stockBajo = productosStockBajo.Count(p => p.StockDisponible > 0);

            lblEstadisticas.Text = "⚠️ Productos con Stock Bajo";
            lblDetalles.Text = $"Total Productos: {totalProductos} | " +
                              $"Sin Stock: {sinStock} | " +
                              $"Stock Bajo: {stockBajo} | " +
                              $"Requieren Reabastecimiento Inmediato";
        }

        private void GenerarGraficosStockBajoAsync(List<object> productos)
        {
            chartPrincipal.Titles.Add("Estado de Stock por Categoría");

            var productosData = productos.Cast<dynamic>().ToList();
            var stockPorCategoria = productosData
                .GroupBy(p => (string)p.Categoria)
                .Select(g => new {
                    Categoria = g.Key,
                    SinStock = g.Count(p => (int)p.StockActual == 0),
                    StockBajo = g.Count(p => (int)p.StockActual > 0)
                })
                .ToList();

            var serieSinStock = new Series("Sin Stock")
            {
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.FromArgb(255, 99, 132)
            };

            var serieStockBajo = new Series("Stock Bajo")
            {
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.FromArgb(255, 205, 86)
            };

            foreach (var categoria in stockPorCategoria)
            {
                serieSinStock.Points.AddXY(categoria.Categoria, categoria.SinStock);
                serieStockBajo.Points.AddXY(categoria.Categoria, categoria.StockBajo);
            }

            chartPrincipal.Series.Add(serieSinStock);
            chartPrincipal.Series.Add(serieStockBajo);

            chartSecundario.Titles.Add("Productos Más Críticos");

            var productosCriticos = productosData
                .Where(p => (int)p.DeficitStock > 0)
                .OrderByDescending(p => (int)p.DeficitStock)
                .Take(8)
                .ToList();

            var serieCriticos = new Series("Déficit de Stock")
            {
                ChartType = SeriesChartType.Bar,
                Color = Color.FromArgb(255, 99, 132)
            };

            foreach (var producto in productosCriticos)
            {
                string nombre = (string)producto.Nombre;
                var nombreCorto = nombre.Length > 12 ? nombre.Substring(0, 9) + "..." : nombre;
                serieCriticos.Points.AddXY(nombreCorto, (int)producto.DeficitStock);
            }

            chartSecundario.Series.Add(serieCriticos);
        }

        private async Task GenerarReporteTransaccionesTipoPagoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var transacciones = await _transaccionService.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            var transaccionesPorTipo = transacciones
                .GroupBy(t => t.TipoPago)
                .Select(g => new
                {
                    TipoPago = g.Key.ToString(),
                    CantidadTransacciones = g.Count(),
                    MontoTotal = g.Sum(t => t.Total),
                    PromedioTransaccion = g.Average(t => t.Total),
                    PorcentajeTransacciones = Math.Round((g.Count() * 100.0 / transacciones.Count), 2),
                    PorcentajeMonto = Math.Round((g.Sum(t => t.Total) * 100m / transacciones.Sum(t => t.Total)), 2)
                })
                .OrderByDescending(t => t.MontoTotal)
                .ToList();

            dgvReportes.DataSource = transaccionesPorTipo;
            ConfigurarColumnasMoneda();

            GenerarGraficosTransaccionesTipoPagoAsync(transaccionesPorTipo.Cast<object>().ToList(), fechaInicio, fechaFin);

            var totalTransacciones = transacciones.Count;
            var montoTotal = transacciones.Sum(t => t.Total);

            lblEstadisticas.Text = $"💳 Transacciones por Tipo de Pago {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            lblDetalles.Text = $"Total Transacciones: {totalTransacciones} | " +
                              $"Monto Total: {montoTotal:C} | " +
                              $"Distribución de métodos de pago utilizados";
        }

        private void GenerarGraficosTransaccionesTipoPagoAsync(List<object> tiposPago, DateTime fechaInicio, DateTime fechaFin)
        {
            chartPrincipal.Titles.Add($"Montos por Tipo de Pago - {fechaInicio:dd/MM/yyyy} a {fechaFin:dd/MM/yyyy}");

            var serieMontos = new Series("Monto Total")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(54, 162, 235)
            };

            var serieCantidad = new Series("Cantidad Transacciones")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(255, 99, 132),
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 10,
                YAxisType = AxisType.Secondary
            };

            var tiposPagoData = tiposPago.Cast<dynamic>().ToList();
            foreach (var tipo in tiposPagoData)
            {
                serieMontos.Points.AddXY((string)tipo.TipoPago, (double)(decimal)tipo.MontoTotal);
                serieCantidad.Points.AddXY((string)tipo.TipoPago, (int)tipo.CantidadTransacciones);
            }

            chartPrincipal.Series.Add(serieMontos);
            chartPrincipal.Series.Add(serieCantidad);

            chartPrincipal.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartPrincipal.ChartAreas[0].AxisY2.Title = "Cantidad";
            chartPrincipal.ChartAreas[0].AxisY.Title = "Monto";

            chartSecundario.Titles.Add("Distribución Porcentual por Tipo");

            var seriePorcentaje = new Series("Porcentaje de Transacciones")
            {
                ChartType = SeriesChartType.Pie
            };

            var colores = new Color[] {
                Color.FromArgb(255, 99, 132),
                Color.FromArgb(54, 162, 235),
                Color.FromArgb(255, 205, 86)
            };

            for (int i = 0; i < tiposPagoData.Count; i++)
            {
                var punto = seriePorcentaje.Points.AddXY((string)tiposPagoData[i].TipoPago, (double)tiposPagoData[i].PorcentajeTransacciones);
                seriePorcentaje.Points[punto].Color = colores[i % colores.Length];
                seriePorcentaje.Points[punto].Label = $"{(string)tiposPagoData[i].TipoPago}\n{(double)tiposPagoData[i].PorcentajeTransacciones}%";
            }

            chartSecundario.Series.Add(seriePorcentaje);
        }

        private void ConfigurarColumnasMoneda()
        {
            foreach (DataGridViewColumn column in dgvReportes.Columns)
            {
                if (column.Name.Contains("Precio") || column.Name.Contains("Total") ||
                    column.Name.Contains("Monto") || column.Name.Contains("Subtotal") ||
                    column.Name.Contains("Promedio") || column.Name.Contains("Valor"))
                {
                    column.DefaultCellStyle.Format = "C2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Porcentaje"))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Cantidad") || column.Name.Contains("Stock"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void BtnExportar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvReportes.DataSource == null)
                {
                    MessageBox.Show("No hay datos para exportar. Genere un reporte primero.", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Archivos CSV (*.csv)|*.csv|Archivos de Texto (*.txt)|*.txt";
                    saveDialog.DefaultExt = "csv";
                    saveDialog.FileName = $"Reporte_{cmbTipoReporte.SelectedItem}_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportarACSV(saveDialog.FileName);
                        MessageBox.Show($"Reporte exportado exitosamente a:\n{saveDialog.FileName}", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarACSV(string rutaArchivo)
        {
            var csv = new StringBuilder();

            csv.AppendLine($"Reporte: {cmbTipoReporte.SelectedItem}");
            csv.AppendLine($"Fecha de Generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            csv.AppendLine($"Período: {dtpFechaInicio.Value:dd/MM/yyyy} - {dtpFechaFin.Value:dd/MM/yyyy}");
            csv.AppendLine("");

            csv.AppendLine(lblEstadisticas.Text.Replace("📊 ", "").Replace("⚠️ ", "").Replace("💳 ", ""));
            csv.AppendLine(lblDetalles.Text);
            csv.AppendLine("");

            var columnNames = new List<string>();
            foreach (DataGridViewColumn column in dgvReportes.Columns)
            {
                if (column.Visible)
                    columnNames.Add('"' + column.HeaderText.Replace("\"", "\"\"") + '"'); 
            }
            csv.AppendLine(string.Join(",", columnNames));

            foreach (DataGridViewRow row in dgvReportes.Rows)
            {
                if (row.IsNewRow) continue;

                var values = new List<string>();
                foreach (DataGridViewColumn column in dgvReportes.Columns)
                {
                    if (column.Visible)
                    {
                        var cellValue = row.Cells[column.Index].Value?.ToString() ?? "";
                        values.Add('"' + cellValue.Replace("\"", "\"\"") + '"'); 
                    }
                }
                csv.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(rutaArchivo, csv.ToString(), Encoding.UTF8);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F5)
            {
                BtnGenerar_Click(this, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Tab && e.Control)
            {
                int nextTab = (tabControl.SelectedIndex + 1) % tabControl.TabCount;
                tabControl.SelectedIndex = nextTab;
            }
        }
    }
}
