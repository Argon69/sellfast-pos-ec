using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL.Forms
{
    public partial class FormDetalleVenta : Form
    {
        private readonly ITransaccionService _transaccionService;
        private Transaccion? _transaccion;
        private readonly int _transaccionId;

        private Label lblTitulo = null!;
        private Panel panelInfo = null!;
        private DataGridView dgvDetalles = null!;
        private Panel panelTotales = null!;
        private Button btnImprimir = null!;
        private Button btnCerrar = null!;

        public FormDetalleVenta(int transaccionId)
        {
            _transaccionId = transaccionId;
            _transaccionService = Program.ServiceProvider.GetRequiredService<ITransaccionService>();

            InitializeComponent();
            ConfigurarFormulario();
            CrearControles();
            _ = CargarDetalleAsync();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Detalle de Venta";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CrearControles()
        {
            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(tlpPrincipal);

            // Título
            lblTitulo = new Label
            {
                Text = "DETALLE DE VENTA",
                Dock = DockStyle.Fill,
                Height = 50,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White
            };
            tlpPrincipal.Controls.Add(lblTitulo, 0, 0);

            // Panel de información
            panelInfo = new Panel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(20), BackColor = Color.FromArgb(250, 250, 250) };
            tlpPrincipal.Controls.Add(panelInfo, 0, 1);

            // DataGridView para detalles
            dgvDetalles = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = Color.LightGray, SelectionForeColor = Color.Black, Padding = new Padding(5) }, ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(55, 71, 79), ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) } };
            tlpPrincipal.Controls.Add(dgvDetalles, 0, 2);

            // Panel de totales
            panelTotales = new Panel { Dock = DockStyle.Fill, AutoSize = true, BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(20, 10, 20, 10) };
            tlpPrincipal.Controls.Add(panelTotales, 0, 3);

            // Panel de botones
            var flpBotones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(20, 10, 20, 10) };
            btnImprimir = new Button { Text = "Imprimir", BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(100, 30) };
            btnImprimir.Click += BtnImprimir_Click;
            btnCerrar = new Button { Text = "Cerrar", UseVisualStyleBackColor = true, Size = new Size(100, 30) };
            btnCerrar.Click += (s, e) => this.Close();
            flpBotones.Controls.Add(btnCerrar); // Añadir en orden inverso
            flpBotones.Controls.Add(btnImprimir);
            tlpPrincipal.Controls.Add(flpBotones, 0, 4);
        }

        private async Task CargarDetalleAsync()
        {
            try
            {
                _transaccion = await _transaccionService.ObtenerPorIdConDetallesAsync(_transaccionId);

                if (_transaccion == null)
                {
                    MessageBox.Show("No se encontró la transacción", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                MostrarInformacion();
                MostrarDetalles();
                MostrarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalle: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void MostrarInformacion()
        {
            if (_transaccion == null) return;

            panelInfo.Controls.Clear();
            var tlpInfo = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 4 };
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelInfo.Controls.Add(tlpInfo);

            // Fila 1
            tlpInfo.Controls.Add(new Label { Text = "Número:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            tlpInfo.Controls.Add(new Label { Text = _transaccion.NumeroTransaccion, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 1, 0);
            tlpInfo.Controls.Add(new Label { Text = "Fecha:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 2, 0);
            tlpInfo.Controls.Add(new Label { Text = $"{_transaccion.FechaHora:dd/MM/yyyy HH:mm}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 3, 0);

            // Fila 2
            tlpInfo.Controls.Add(new Label { Text = "Cliente:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            tlpInfo.Controls.Add(new Label { Text = _transaccion.Usuario?.NombreCompleto ?? "N/A", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 1, 1);
            tlpInfo.Controls.Add(new Label { Text = "Documento:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 2, 1);
            tlpInfo.Controls.Add(new Label { Text = _transaccion.Usuario?.Documento ?? "N/A", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 3, 1);

            // Fila 3
            tlpInfo.Controls.Add(new Label { Text = "Tipo:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            tlpInfo.Controls.Add(new Label { Text = _transaccion.Usuario?.TipoUsuario.ToString() ?? "N/A", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 1, 2);
            tlpInfo.Controls.Add(new Label { Text = "Forma de Pago:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 2, 2);
            tlpInfo.Controls.Add(new Label { Text = _transaccion.TipoPago.ToString(), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 3, 2);

            // Fila 4 (Estado)
            if (_transaccion.Observaciones?.StartsWith("ANULADA") == true)
            {
                var lblEstado = new Label { Text = _transaccion.Observaciones, Dock = DockStyle.Fill, ForeColor = Color.Red, Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
                tlpInfo.Controls.Add(lblEstado, 0, 3);
                tlpInfo.SetColumnSpan(lblEstado, 4);
            }
        }

        private void MostrarDetalles()
        {
            if (_transaccion == null) return;

            dgvDetalles.DataSource = _transaccion.Detalles.Select(d => new
            {
                Código = d.Producto?.Codigo ?? "N/A",
                Producto = d.Producto?.Nombre ?? "N/A",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();

            if (dgvDetalles.Columns.Count > 0)
            {
                dgvDetalles.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
                dgvDetalles.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
                dgvDetalles.Columns["PrecioUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvDetalles.Columns["Subtotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvDetalles.Columns["Cantidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void MostrarTotales()
        {
            if (_transaccion == null) return;

            panelTotales.Controls.Clear();
            var tlpTotales = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
            tlpTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpTotales.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panelTotales.Controls.Add(tlpTotales);

            // Nota de subsidiado
            if (_transaccion.EsSubsidiado)
            {
                var lblSubsidiado = new Label { Text = "VENTA 100% SUBSIDIADA", ForeColor = Color.Red, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
                tlpTotales.Controls.Add(lblSubsidiado, 0, 0);
            }

            // Subtotal
            var lblSubtotal = new Label { Text = $"Subtotal: {_transaccion.Subtotal:C}", Font = new Font("Segoe UI", 10F), AutoSize = true, TextAlign = ContentAlignment.MiddleRight };
            tlpTotales.Controls.Add(lblSubtotal, 1, 0);

            // Descuento
            if (_transaccion.MontoDescuento > 0)
            {
                var lblDescuento = new Label { Text = $"Descuento ({_transaccion.PorcentajeDescuento}%): {_transaccion.MontoDescuento:C}", ForeColor = Color.Green, Font = new Font("Segoe UI", 10F), AutoSize = true, TextAlign = ContentAlignment.MiddleRight };
                tlpTotales.Controls.Add(lblDescuento, 1, 1);
            }

            // Total
            var lblTotal = new Label { Text = $"TOTAL: {_transaccion.Total:C}", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.Navy, AutoSize = true, TextAlign = ContentAlignment.MiddleRight };
            tlpTotales.Controls.Add(lblTotal, 1, 2);
        }

        private void BtnImprimir_Click(object? sender, EventArgs e)
        {
            var printDialog = new PrintDialog();
            var printDocument = new PrintDocument();

            printDocument.PrintPage += PrintDocument_PrintPage;
            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (_transaccion == null || e.Graphics == null) return;

            var graphics = e.Graphics;
            var font = new Font("Arial", 10);
            var fontBold = new Font("Arial", 10, FontStyle.Bold);
            var fontTitle = new Font("Arial", 16, FontStyle.Bold);
            var brush = Brushes.Black;

            float x = 50;
            float y = 50;
            float lineHeight = 20;

            // Título
            graphics.DrawString("CAFETERÍA UNIVERSIDAD NACIONAL", fontTitle, brush, x + 150, y);
            y += 30;
            graphics.DrawString("SEDE LA PAZ", fontBold, brush, x + 250, y);
            y += 40;

            // Información de la venta
            graphics.DrawString($"Número: {_transaccion.NumeroTransaccion}", fontBold, brush, x, y);
            graphics.DrawString($"Fecha: {_transaccion.FechaHora:dd/MM/yyyy HH:mm}", font, brush, x + 300, y);
            y += lineHeight;

            graphics.DrawString($"Cliente: {_transaccion.Usuario?.NombreCompleto}", font, brush, x, y);
            y += lineHeight;

            graphics.DrawString($"Documento: {_transaccion.Usuario?.Documento}", font, brush, x, y);
            graphics.DrawString($"Tipo: {_transaccion.Usuario?.TipoUsuario}", font, brush, x + 300, y);
            y += lineHeight * 2;

            // Línea separadora
            graphics.DrawLine(Pens.Black, x, y, x + 700, y);
            y += 10;

            // Encabezados de detalle
            graphics.DrawString("Código", fontBold, brush, x, y);
            graphics.DrawString("Producto", fontBold, brush, x + 100, y);
            graphics.DrawString("Cant", fontBold, brush, x + 400, y);
            graphics.DrawString("P.Unit", fontBold, brush, x + 450, y);
            graphics.DrawString("Subtotal", fontBold, brush, x + 550, y);
            y += lineHeight;

            // Detalles
            foreach (var detalle in _transaccion.Detalles)
            {
                graphics.DrawString(detalle.Producto?.Codigo, font, brush, x, y);
                graphics.DrawString(detalle.Producto?.Nombre, font, brush, x + 100, y);
                graphics.DrawString(detalle.Cantidad.ToString(), font, brush, x + 410, y);
                graphics.DrawString(detalle.PrecioUnitario.ToString("C"), font, brush, x + 450, y);
                graphics.DrawString(detalle.Subtotal.ToString("C"), font, brush, x + 550, y);
                y += lineHeight;
            }

            // Línea separadora
            y += 10;
            graphics.DrawLine(Pens.Black, x, y, x + 700, y);
            y += 20;

            // Totales
            graphics.DrawString($"Subtotal:", font, brush, x + 450, y);
            graphics.DrawString(_transaccion.Subtotal.ToString("C"), font, brush, x + 550, y);
            y += lineHeight;

            if (_transaccion.MontoDescuento > 0)
            {
                graphics.DrawString($"Descuento ({_transaccion.PorcentajeDescuento}%):", font, brush, x + 400, y);
                graphics.DrawString(_transaccion.MontoDescuento.ToString("C"), font, brush, x + 550, y);
                y += lineHeight;
            }

            graphics.DrawString("TOTAL:", fontBold, brush, x + 450, y);
            graphics.DrawString(_transaccion.Total.ToString("C"), fontBold, brush, x + 550, y);
            y += lineHeight * 2;

            // Forma de pago
            graphics.DrawString($"Forma de Pago: {_transaccion.TipoPago}", font, brush, x + 400, y);

            // Nota de subsidiado
            if (_transaccion.EsSubsidiado)
            {
                y += lineHeight * 2;
                graphics.DrawString("VENTA 100% SUBSIDIADA", fontBold, brush, x + 200, y);
            }

            // Estado anulado
            if (_transaccion.Observaciones?.StartsWith("ANULADA") == true)
            {
                y += lineHeight * 2;
                graphics.DrawString(_transaccion.Observaciones, new Font("Arial", 12, FontStyle.Bold), Brushes.Red, x, y);
            }
        }
    }
}
