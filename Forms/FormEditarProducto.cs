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
    public partial class FormEditarProducto : Form
    {
        private readonly IProductoService _productoService;
        private Producto? _productoActual;
        private readonly bool _esNuevo;

        // Controles del formulario
        private TextBox txtCodigo = null!;
        private TextBox txtNombre = null!;
        private TextBox txtDescripcion = null!;
        private ComboBox cboCategoria = null!;
        private NumericUpDown nudPrecio = null!;
        private NumericUpDown nudStock = null!;
        private NumericUpDown nudStockMinimo = null!;
        private CheckBox chkMenuDelDia = null!;
        private CheckBox chkActivo = null!;
        private Button btnGuardar = null!;
        private Button btnCancelar = null!;
        private GroupBox grpInformacion = null!;
        private GroupBox grpInventario = null!;

        public FormEditarProducto() : this(null)
        {
        }

        public FormEditarProducto(int? productoId)
        {
            _productoService = Program.ServiceProvider.GetRequiredService<IProductoService>();
            _esNuevo = !productoId.HasValue;
            InitializeComponent();
            ConfigurarFormulario();
            CrearControles();

            if (!_esNuevo)
            {
                _ = CargarProductoAsync(productoId!.Value);
            }
            else
            {
                ConfigurarFormularioNuevo();
            }
        }

        private void ConfigurarFormulario()
        {
            this.Text = _esNuevo ? "Nuevo Producto" : "Editar Producto";
            this.Size = new Size(550, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CrearControles()
        {
            this.Controls.Clear();
            this.Padding = new Padding(10);

            // ESTRUCTURA PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(tlpPrincipal);

            // === GRUPO INFORMACIÓN GENERAL ===
            grpInformacion = new GroupBox { Text = "Información General", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var tlpInfo = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
            tlpInfo.ColumnStyles.Add(new RowStyle(SizeType.Absolute, 110));
            tlpInfo.ColumnStyles.Add(new RowStyle(SizeType.Percent, 100));
            grpInformacion.Controls.Add(tlpInfo);
            tlpPrincipal.Controls.Add(grpInformacion, 0, 0);

            // Controles de Información
            txtCodigo = new TextBox { Dock = DockStyle.Fill, CharacterCasing = CharacterCasing.Upper };
            txtNombre = new TextBox { Dock = DockStyle.Fill };
            txtDescripcion = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 60 };
            cboCategoria = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategoria.Items.AddRange(Enum.GetNames(typeof(CategoriaProducto)));
            nudPrecio = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 999999.99M, Minimum = 0.01M, ThousandsSeparator = true };

            // Añadir filas a tlpInfo
            tlpInfo.Controls.Add(new Label { Text = "Código:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            tlpInfo.Controls.Add(txtCodigo, 1, 0);
            tlpInfo.Controls.Add(new Label { Text = "Nombre:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            tlpInfo.Controls.Add(txtNombre, 1, 1);
            tlpInfo.Controls.Add(new Label { Text = "Descripción:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopRight, Padding = new Padding(0,5,0,0)}, 0, 2);
            tlpInfo.Controls.Add(txtDescripcion, 1, 2);
            tlpInfo.Controls.Add(new Label { Text = "Categoría:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            tlpInfo.Controls.Add(cboCategoria, 1, 3);
            tlpInfo.Controls.Add(new Label { Text = "Precio:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 4);
            tlpInfo.Controls.Add(nudPrecio, 1, 4);

            // === GRUPO INVENTARIO ===
            grpInventario = new GroupBox { Text = "Control de Inventario", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var tlpInventario = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
            tlpInventario.ColumnStyles.Add(new RowStyle(SizeType.Absolute, 110));
            tlpInventario.ColumnStyles.Add(new RowStyle(SizeType.Percent, 100));
            grpInventario.Controls.Add(tlpInventario);
            tlpPrincipal.Controls.Add(grpInventario, 0, 1);
            
            // Controles de Inventario
            nudStock = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 9999, Minimum = 0 };
            nudStockMinimo = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 9999, Minimum = 0, Value = 5 };
            chkMenuDelDia = new CheckBox { Text = "Es Menú del Día", Dock = DockStyle.Fill };
            chkActivo = new CheckBox { Text = "Producto Activo", Checked = true, Dock = DockStyle.Fill };

            // Añadir filas a tlpInventario
            tlpInventario.Controls.Add(new Label { Text = "Stock Disponible:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            tlpInventario.Controls.Add(nudStock, 1, 0);
            tlpInventario.Controls.Add(new Label { Text = "Stock Mínimo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            tlpInventario.Controls.Add(nudStockMinimo, 1, 1);
            tlpInventario.Controls.Add(chkMenuDelDia, 1, 2);
            tlpInventario.Controls.Add(chkActivo, 1, 3);

            // === BOTONES ===
            var flpBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            tlpPrincipal.Controls.Add(flpBotones, 0, 2);

            btnGuardar = new Button { Text = "Guardar", Width = 100, Height = 35, BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false };
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 35, BackColor = Color.FromArgb(158, 158, 158), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false };
            btnCancelar.Click += (s, e) => this.Close();
            
            // Añadir en orden inverso por RightToLeft
            flpBotones.Controls.Add(btnCancelar);
            flpBotones.Controls.Add(btnGuardar);
        }

        private async Task CargarProductoAsync(int productoId)
        {
            try
            {
                _productoActual = await _productoService.ObtenerPorIdAsync(productoId);
                if (_productoActual == null)
                {
                    MessageBox.Show("No se encontró el producto", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Cargar datos en los controles
                txtCodigo.Text = _productoActual.Codigo;
                txtNombre.Text = _productoActual.Nombre;
                txtDescripcion.Text = _productoActual.Descripcion ?? "";
                cboCategoria.SelectedItem = _productoActual.Categoria.ToString();
                nudPrecio.Value = _productoActual.Precio;
                nudStock.Value = _productoActual.StockDisponible;
                nudStockMinimo.Value = _productoActual.StockMinimo;
                chkMenuDelDia.Checked = _productoActual.EsMenuDelDia;
                chkActivo.Checked = _productoActual.Activo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar producto: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void ConfigurarFormularioNuevo()
        {
            cboCategoria.SelectedIndex = 0;
            chkActivo.Checked = true;
            txtCodigo.Focus();
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                var producto = _esNuevo ? new Producto() : _productoActual!;

                producto.Codigo = txtCodigo.Text.Trim().ToUpper();
                producto.Nombre = txtNombre.Text.Trim();
                producto.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ?
                    null : txtDescripcion.Text.Trim();
                producto.Categoria = (CategoriaProducto)Enum.Parse(typeof(CategoriaProducto),
                    cboCategoria.SelectedItem!.ToString()!);
                producto.Precio = nudPrecio.Value;
                producto.StockDisponible = (int)nudStock.Value;
                producto.StockMinimo = (int)nudStockMinimo.Value;
                producto.EsMenuDelDia = chkMenuDelDia.Checked;
                producto.Activo = chkActivo.Checked;

                if (_esNuevo)
                {
                    await _productoService.CrearAsync(producto);
                    MessageBox.Show("Producto creado exitosamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _productoService.ActualizarAsync(producto);
                    MessageBox.Show("Producto actualizado exitosamente", "Éxito",
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
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("El código es obligatorio", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }

            if (cboCategoria.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar una categoría", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategoria.Focus();
                return false;
            }

            if (nudStockMinimo.Value > nudStock.Value)
            {
                MessageBox.Show("El stock mínimo no puede ser mayor al stock disponible", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudStockMinimo.Focus();
                return false;
            }

            return true;
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