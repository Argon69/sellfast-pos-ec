using System;
using System.Collections.Generic;
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
    public partial class FormNuevaVenta : MaterialForm
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IProductoService _productoService;
        private readonly ITransaccionService _transaccionService;

        private Usuario? _usuarioSeleccionado;
        private List<DetalleVenta> _detallesVenta = new();
        
        private MaterialTextBox2 txtBuscarUsuario = null!;
        private MaterialLabel lblUsuarioSeleccionado = null!;
        private MaterialComboBox cboCategoria = null!;
        private MaterialComboBox cboProducto = null!;
        private MaterialTextBox2 nudCantidad = null!;
        private MaterialButton btnAgregar = null!;
        private DataGridView dgvDetalles = null!;
        private MaterialButton btnEliminarItem = null!;
        private MaterialLabel lblSubtotal = null!;
        private MaterialLabel lblDescuentoTotal = null!;
        private MaterialLabel lblTotal = null!;
        private MaterialRadioButton rbEfectivo = null!;
        private MaterialRadioButton rbTarjeta = null!;
        private MaterialRadioButton rbTransferencia = null!;
        private MaterialButton btnGuardarVenta = null!;
        private MaterialButton btnNuevaVenta = null!;

        public FormNuevaVenta()
        {
            _usuarioService = Program.ServiceProvider.GetRequiredService<IUsuarioService>();
            _productoService = Program.ServiceProvider.GetRequiredService<IProductoService>();
            _transaccionService = Program.ServiceProvider.GetRequiredService<ITransaccionService>();

            InitializeComponent();
            CrearControlesModernos();
            CargarDatosIniciales();
        }

        private void CrearControlesModernos()
        {
            this.Text = "Nueva Venta";

            // PANEL PRINCIPAL
            var tlpPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            this.Controls.Add(tlpPrincipal);

            // === COLUMNA IZQUIERDA ===
            var tlpIzquierdo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tlpIzquierdo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpIzquierdo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpIzquierdo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpIzquierdo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.Controls.Add(tlpIzquierdo, 0, 0);

            // CARD USUARIO
            var cardUsuario = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(5) };
            var tlpUsuario = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tlpUsuario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpUsuario.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtBuscarUsuario = new MaterialTextBox2 { Hint = "Buscar documento o nombre de usuario", Dock = DockStyle.Bottom };
            var btnBuscarUsuario = new MaterialButton { Text = "Buscar", Type = MaterialButton.MaterialButtonType.Outlined, Margin = new Padding(5, 0, 0, 0), Dock = DockStyle.Bottom, Height = 36 };
            btnBuscarUsuario.Click += BtnBuscarUsuario_Click;
            lblUsuarioSeleccionado = new MaterialLabel { Text = "Cliente no seleccionado", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, FontType = MaterialSkinManager.fontType.Subtitle2 };

            tlpUsuario.Controls.Add(txtBuscarUsuario, 0, 0);
            tlpUsuario.Controls.Add(btnBuscarUsuario, 1, 0);
            tlpUsuario.Controls.Add(lblUsuarioSeleccionado, 0, 1);
            tlpUsuario.SetColumnSpan(lblUsuarioSeleccionado, 2);
            cardUsuario.Controls.Add(tlpUsuario);
            tlpIzquierdo.Controls.Add(cardUsuario, 0, 0);

            // CARD PRODUCTO
            var cardProducto = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(5) };
            var tlpProducto = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, AutoSize = true };
            tlpProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tlpProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            tlpProducto.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cboCategoria = new MaterialComboBox { Hint = "Categoría", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategoria.SelectedIndexChanged += CboCategoria_SelectedIndexChanged;
            cboProducto = new MaterialComboBox { Hint = "Producto", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            nudCantidad = new MaterialTextBox2 { Hint = "Cant.", Dock = DockStyle.Fill, Width = 80 };
            btnAgregar = new MaterialButton { Text = "Agregar", Type = MaterialButton.MaterialButtonType.Contained, Dock = DockStyle.Right };
            btnAgregar.Click += BtnAgregar_Click;

            tlpProducto.Controls.Add(cboCategoria, 0, 0);
            tlpProducto.Controls.Add(cboProducto, 1, 0);
            tlpProducto.SetColumnSpan(cboProducto, 2);
            tlpProducto.Controls.Add(nudCantidad, 0, 1);
            tlpProducto.Controls.Add(btnAgregar, 2, 1);
            cardProducto.Controls.Add(tlpProducto);
            tlpIzquierdo.Controls.Add(cardProducto, 0, 1);

            // CARD DETALLES (GRID)
            var cardDetalles = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(1), Margin = new Padding(5) };
            dgvDetalles = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BorderStyle = BorderStyle.None, BackgroundColor = this.BackColor };
            EstilizarDataGridView();
            cardDetalles.Controls.Add(dgvDetalles);
            tlpIzquierdo.Controls.Add(cardDetalles, 0, 2);

            btnEliminarItem = new MaterialButton { Text = "Eliminar Item", Type = MaterialButton.MaterialButtonType.Text, Anchor = AnchorStyles.Left, Margin = new Padding(5) };
            btnEliminarItem.Click += BtnEliminarItem_Click;
            tlpIzquierdo.Controls.Add(btnEliminarItem, 0, 3);

            // === COLUMNA DERECHA ===
            var tlpDerecho = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tlpDerecho.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpDerecho.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpDerecho.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpDerecho.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpPrincipal.Controls.Add(tlpDerecho, 1, 0);

            // CARD TOTALES
            var cardTotales = new MaterialCard { Dock = DockStyle.Top, Padding = new Padding(16), Margin = new Padding(5) };
            var tlpTotales = new TableLayoutPanel { ColumnCount = 1, RowCount = 3, Dock = DockStyle.Fill, AutoSize = true };
            lblSubtotal = new MaterialLabel { Text = "Subtotal: $0.00", FontType = MaterialSkinManager.fontType.H6, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            lblDescuentoTotal = new MaterialLabel { Text = "Descuento: $0.00", FontType = MaterialSkinManager.fontType.Body1, ForeColor = Color.Green, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            lblTotal = new MaterialLabel { Text = "TOTAL: $0.00", FontType = MaterialSkinManager.fontType.H4, HighEmphasis = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Height = 60 };
            tlpTotales.Controls.AddRange(new Control[] { lblSubtotal, lblDescuentoTotal, lblTotal });
            cardTotales.Controls.Add(tlpTotales);
            tlpDerecho.Controls.Add(cardTotales, 0, 0);

            // CARD PAGO
            var cardPago = new MaterialCard { Dock = DockStyle.Top, Padding = new Padding(16), Margin = new Padding(5) };
            var tlpPago = new TableLayoutPanel { ColumnCount = 2, RowCount = 3, Dock = DockStyle.Fill, AutoSize = true };
            var lblTipoPago = new MaterialLabel { Text = "Método de Pago", FontType = MaterialSkinManager.fontType.H6, Dock = DockStyle.Fill };
            rbEfectivo = new MaterialRadioButton { Text = "Efectivo", Checked = true, Dock = DockStyle.Fill };
            rbTarjeta = new MaterialRadioButton { Text = "Tarjeta", Dock = DockStyle.Fill };
            rbTransferencia = new MaterialRadioButton { Text = "Transferencia", Dock = DockStyle.Fill };
            tlpPago.Controls.Add(lblTipoPago, 0, 0);
            tlpPago.SetColumnSpan(lblTipoPago, 2);
            tlpPago.Controls.Add(rbEfectivo, 0, 1);
            tlpPago.Controls.Add(rbTarjeta, 1, 1);
            tlpPago.Controls.Add(rbTransferencia, 0, 2);
            cardPago.Controls.Add(tlpPago);
            tlpDerecho.Controls.Add(cardPago, 0, 1);

            // BOTONES INFERIORES
            var flpBotones = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill, Margin = new Padding(5) };
            btnGuardarVenta = new MaterialButton { Text = "Guardar Venta", Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, UseAccentColor = true, Height = 50, Dock = DockStyle.Top };
            btnGuardarVenta.Click += BtnGuardarVenta_Click;
            btnNuevaVenta = new MaterialButton { Text = "Iniciar Nueva Venta", Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = false, Height = 50, Visible = false, Dock = DockStyle.Top };
            btnNuevaVenta.Click += BtnNuevaVenta_Click;
            flpBotones.Controls.AddRange(new Control[] { btnGuardarVenta, btnNuevaVenta });
            tlpDerecho.Controls.Add(flpBotones, 0, 3);
        }
        
        private void EstilizarDataGridView()
        {
            dgvDetalles.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            dgvDetalles.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvDetalles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDetalles.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDetalles.EnableHeadersVisualStyles = false;
            dgvDetalles.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDetalles.GridColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            dgvDetalles.DefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
            dgvDetalles.DefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvDetalles.RowTemplate.Height = 40;
        }

        private async void CargarDatosIniciales()
        {
            try
            {
                cboCategoria.Items.Clear();
                cboCategoria.Items.Add("Todas"); // Opción para todas las categorías
                foreach (var name in Enum.GetNames(typeof(CategoriaProducto)))
                {
                    cboCategoria.Items.Add(name);
                }
                cboCategoria.SelectedIndex = 0;
                await CargarProductosAsync(); // Cargar productos al inicio
                ConfigurarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error");
            }
        }
        
        private void ConfigurarGrid()
        {
            dgvDetalles.Columns.Clear();
            dgvDetalles.Columns.Add("ProductoId", "ID");
            dgvDetalles.Columns.Add("Nombre", "Producto");
            dgvDetalles.Columns.Add("Cantidad", "Cant.");
            dgvDetalles.Columns.Add("PrecioUnitario", "P. Unit.");
            dgvDetalles.Columns.Add("Subtotal", "Subtotal");
            dgvDetalles.Columns["ProductoId"].Visible = false;
            dgvDetalles.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            dgvDetalles.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
        }
        
        private async void BtnBuscarUsuario_Click(object? sender, EventArgs e) => await BuscarUsuarioAsync();
        private async Task BuscarUsuarioAsync() 
        {
            var termino = txtBuscarUsuario.Text.Trim();
            if (string.IsNullOrWhiteSpace(termino)) { MessageBox.Show("Ingrese un término de búsqueda.", "Advertencia"); return; }
            
            var usuarios = await _usuarioService.BuscarAsync(termino);
            if (!usuarios.Any()) { MessageBox.Show("No se encontró usuario.", "Información"); return; }
            
            Usuario? usuario = usuarios.Count == 1 ? usuarios.First() : await SeleccionarUsuarioDesdeListaAsync(usuarios);
            if (usuario != null) SeleccionarUsuario(usuario);
        }

        private async Task<Usuario?> SeleccionarUsuarioDesdeListaAsync(List<Usuario> usuarios)
        {
            await Task.Yield(); // Fix CS1998 warning

            using var form = new MaterialForm
            {
                Text = "Seleccionar Usuario",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var dgv = new DataGridView
            {
                DataSource = usuarios.Select(u => new
                {
                    u.Id,
                    u.Documento,
                    u.NombreCompleto,
                    Tipo = u.TipoUsuario.ToString()
                }).ToList(),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgv.Columns["Id"]!.Visible = false;

            var panel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var btnSeleccionar = new MaterialButton
            {
                Text = "Seleccionar",
                DialogResult = DialogResult.OK,
                Location = new Point(400, 10),
                Size = new Size(80, 30)
            };
            var btnCancelar = new MaterialButton
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(490, 10),
                Size = new Size(80, 30)
            };
            panel.Controls.AddRange(new Control[] { btnSeleccionar, btnCancelar });

            form.Controls.Add(dgv);
            form.Controls.Add(panel);
            
            if (form.ShowDialog() == DialogResult.OK && dgv.SelectedRows.Count > 0)
            {
                var id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
                return usuarios.First(u => u.Id == id);
            }
            return null;
        }

        private void SeleccionarUsuario(Usuario usuario) 
        {
            _usuarioSeleccionado = usuario;
            lblUsuarioSeleccionado.Text = $"Cliente: {usuario.NombreCompleto}";
            ActualizarTotales();
        }

        private async void CboCategoria_SelectedIndexChanged(object? sender, EventArgs e) => await CargarProductosAsync();
        private async Task CargarProductosAsync()
        {
            if (cboCategoria.SelectedItem == null) return; // Añadido para evitar null
            var productos = cboCategoria.SelectedIndex <= 0 
                ? await _productoService.ObtenerActivosAsync() 
                : await _productoService.ObtenerPorCategoriaAsync((CategoriaProducto)Enum.Parse(typeof(CategoriaProducto), cboCategoria.SelectedItem.ToString()!));
            
            cboProducto.DataSource = productos.Where(p => p.StockDisponible > 0)
                .Select(p => new ProductoItem { Id = p.Id, Nombre = p.Nombre, Precio = p.Precio, Stock = p.StockDisponible }).ToList();
            cboProducto.DisplayMember = "Nombre";
            cboProducto.ValueMember = "Id";
        }

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            if (_usuarioSeleccionado == null) { MessageBox.Show("Debe seleccionar un usuario."); return; }
            if (cboProducto.SelectedItem is not ProductoItem producto) { MessageBox.Show("Debe seleccionar un producto."); return; }
            if (!int.TryParse(nudCantidad.Text, out int cantidad) || cantidad <= 0) { MessageBox.Show("Cantidad inválida."); return; }

            var detalleExistente = _detallesVenta.FirstOrDefault(d => d.ProductoId == producto.Id);
            if (detalleExistente != null) { detalleExistente.Cantidad += cantidad; }
            else { _detallesVenta.Add(new DetalleVenta { ProductoId = producto.Id, Nombre = producto.Nombre, Cantidad = cantidad, PrecioUnitario = producto.Precio }); }
            
            ActualizarGrid();
            ActualizarTotales();
        }
        
        private void BtnEliminarItem_Click(object? sender, EventArgs e)
        {
            if (dgvDetalles.SelectedRows.Count == 0) return;
            var productoId = Convert.ToInt32(dgvDetalles.SelectedRows[0].Cells["ProductoId"].Value);
            _detallesVenta.RemoveAll(d => d.ProductoId == productoId);
            ActualizarGrid();
            ActualizarTotales();
        }

        private void ActualizarGrid()
        {
            dgvDetalles.Rows.Clear();
            foreach (var d in _detallesVenta) { dgvDetalles.Rows.Add(d.ProductoId, d.Nombre, d.Cantidad, d.PrecioUnitario, d.Cantidad * d.PrecioUnitario); }
        }

        private void ActualizarTotales()
        {
            var subtotal = _detallesVenta.Sum(d => d.PrecioUnitario * d.Cantidad);
            var descuento = subtotal * ((_usuarioSeleccionado?.ObtenerPorcentajeDescuento() ?? 0) / 100);
            var total = _usuarioSeleccionado?.ModalidadPago == ModalidadPagoEstudiante.Subsidiado ? 0 : subtotal - descuento;

            lblSubtotal.Text = $"Subtotal: {subtotal:C}";
            lblDescuentoTotal.Text = $"Descuento: {descuento:C}";
            lblTotal.Text = $"TOTAL: {total:C}";
        }

        private async void BtnGuardarVenta_Click(object? sender, EventArgs e)
        {
            if (_usuarioSeleccionado == null || !_detallesVenta.Any()) { MessageBox.Show("Venta inválida."); return; }
            var tipoPago = rbTarjeta.Checked ? TipoPago.Tarjeta : rbTransferencia.Checked ? TipoPago.Transferencia : TipoPago.Efectivo;
            var items = _detallesVenta.Select(d => (d.ProductoId, d.Cantidad)).ToList();

            var transaccion = await _transaccionService.CrearTransaccionAsync(_usuarioSeleccionado!.Id, items, tipoPago);
            MessageBox.Show($"Venta {transaccion.NumeroTransaccion} registrada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnGuardarVenta.Visible = false;
            btnNuevaVenta.Visible = true;
        }

        private void BtnNuevaVenta_Click(object? sender, EventArgs e) => LimpiarFormulario();
        private void LimpiarFormulario()
        {
            _usuarioSeleccionado = null;
            _detallesVenta.Clear();
            lblUsuarioSeleccionado.Text = "Cliente no seleccionado";
            txtBuscarUsuario.Clear();
            cboCategoria.SelectedIndex = 0;
            dgvDetalles.Rows.Clear();
            ActualizarTotales();
            btnGuardarVenta.Visible = true;
            btnNuevaVenta.Visible = false;
            txtBuscarUsuario.Focus();
        }

        private class ProductoItem { public int Id { get; set; } public string Nombre { get; set; } = ""; public decimal Precio { get; set; } public int Stock { get; set; } }
        private class DetalleVenta { public int ProductoId { get; set; } public string Nombre { get; set; } = ""; public int Cantidad { get; set; } public decimal PrecioUnitario { get; set; } }
    }
}