using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL.Forms
{
    public partial class FormImprimirFicho : Form
    {
        private readonly IFichoService _fichoService;
        private Ficho? _ficho;
        private readonly int _fichoId;

        private Panel panelPreview = null!;
        private Button btnImprimir = null!;
        private Button btnCerrar = null!;

        public FormImprimirFicho(int fichoId)
        {
            _fichoId = fichoId;
            _fichoService = Program.ServiceProvider.GetRequiredService<IFichoService>();

            InitializeComponent();
            ConfigurarFormulario();
            CrearControles();
            _ = CargarFichoAsync();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Imprimir Ficho";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CrearControles()
        {
            this.Padding = new Padding(20);

            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(tlpPrincipal);

            // Panel de vista previa
            panelPreview = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            panelPreview.Paint += PanelPreview_Paint;
            tlpPrincipal.Controls.Add(panelPreview, 0, 0);

            // Panel de botones
            var flpBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            tlpPrincipal.Controls.Add(flpBotones, 0, 1);

            btnImprimir = new Button
            {
                Text = "Imprimir",
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnImprimir.Click += BtnImprimir_Click;
            
            btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(100, 30),
                UseVisualStyleBackColor = true
            };
            btnCerrar.Click += (s, e) => this.Close();

            flpBotones.Controls.Add(btnCerrar);
            flpBotones.Controls.Add(btnImprimir);
        }

        private async Task CargarFichoAsync()
        {
            try
            {
                _ficho = await _fichoService.ObtenerPorIdAsync(_fichoId);

                if (_ficho == null)
                {
                    MessageBox.Show("No se encontró el ficho", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                panelPreview.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ficho: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void PanelPreview_Paint(object? sender, PaintEventArgs e)
        {
            if (_ficho == null || e.Graphics == null) return;

            DibujarFicho(e.Graphics, panelPreview.ClientRectangle);
        }

        private void DibujarFicho(Graphics g, Rectangle bounds)
        {
            if (_ficho == null) return;

            var font = new Font("Arial", 10);
            var fontBold = new Font("Arial", 12, FontStyle.Bold);
            var fontTitle = new Font("Arial", 14, FontStyle.Bold);
            var brush = Brushes.Black;

            var x = bounds.X + 20;
            var y = bounds.Y + 20;

            // Marco
            g.DrawRectangle(Pens.Black, bounds.X + 10, bounds.Y + 10,
                bounds.Width - 20, bounds.Height - 20);

            // Encabezado
            g.DrawString("UNIVERSIDAD NACIONAL DE COLOMBIA", fontTitle, brush, x, y);
            y += 25;
            g.DrawString("SEDE LA PAZ - CAFETERÍA", fontBold, brush, x + 40, y);
            y += 30;

            // Línea separadora
            g.DrawLine(Pens.Black, x, y, bounds.Right - 20, y);
            y += 20;

            // Título del ficho
            g.DrawString("FICHO DE ALMUERZO", fontTitle, brush, x + 60, y);
            y += 40;

            // Número de ficho
            g.DrawString("NÚMERO:", font, brush, x, y);
            g.DrawString(_ficho.Numero, fontBold, brush, x + 80, y);
            y += 30;

            // Fecha
            g.DrawString("FECHA:", font, brush, x, y);
            g.DrawString(_ficho.FechaServicio.ToString("dd/MM/yyyy"), fontBold, brush, x + 80, y);
            y += 30;

            // Usuario
            g.DrawString("USUARIO:", font, brush, x, y);
            y += 20;
            g.DrawString(_ficho.Usuario?.NombreCompleto ?? "N/A", fontBold, brush, x + 20, y);
            y += 30;

            // Documento
            g.DrawString("DOCUMENTO:", font, brush, x, y);
            g.DrawString(_ficho.Usuario?.Documento ?? "N/A", fontBold, brush, x + 100, y);
            y += 30;

            // Tipo de usuario
            g.DrawString("TIPO:", font, brush, x, y);
            g.DrawString(_ficho.Usuario?.TipoUsuario.ToString() ?? "N/A", fontBold, brush, x + 80, y);
            y += 40;

            // Estado
            var estadoBrush = _ficho.Estado switch
            {
                EstadoFicho.Usado => Brushes.Green,
                EstadoFicho.Cancelado => Brushes.Red,
                _ => Brushes.Black
            };

            g.DrawString("ESTADO:", font, brush, x, y);
            g.DrawString(_ficho.EstadoDescripcion, fontBold, estadoBrush, x + 80, y);
            y += 30;

            // Hora de emisión
            g.DrawString($"Emitido: {_ficho.FechaSolicitud:dd/MM/yyyy HH:mm}",
                new Font("Arial", 8), Brushes.Gray, x, bounds.Bottom - 40);
        }

        private void BtnImprimir_Click(object? sender, EventArgs e)
        {
            var printDialog = new PrintDialog();
            var printDocument = new PrintDocument();

            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Ficho", 300, 400);

            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
                MessageBox.Show("Ficho impreso correctamente", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null) return;

            var bounds = new Rectangle(0, 0, 300, 400);
            DibujarFicho(e.Graphics, bounds);
        }
    }
}