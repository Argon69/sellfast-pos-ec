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
    public partial class FormProductos : MaterialForm
    {
        private readonly IProductoService _productoService;
        private DataGridView dgvProductos = null!;
        private MaterialTextBox2 txtBuscar = null!;
        private MaterialComboBox cboFiltroCategoria = null!;
        private MaterialCheckbox chkSoloMenuDelDia = null!;
        private MaterialCheckbox chkSoloBajoStock = null!;
        private MaterialButton btnNuevo = null!;
        private MaterialButton btnEditar = null!;
        private MaterialButton btnEliminar = null!;
        private MaterialButton btnBuscar = null!;
        private MaterialButton btnActualizarStock = null!;
        // private MaterialLabel lblTotalProductos = null!; // Eliminado
        // private MaterialLabel lblProductosBajoStock = null!; // Eliminado

        public FormProductos() : this(Program.ServiceProvider.GetRequiredService<IProductoService>())
        {
        }

        public FormProductos(IProductoService productoService)
        {
            _productoService = productoService;
            InitializeComponent();
            ConfigurarFormularioMaterial();
            CrearControlesMaterial();
            _ = CargarProductosAsync();
        }

        private void ConfigurarFormularioMaterial()
        {
            this.Text = "Gestión de Productos";
            this.Size = new Size(1100, 700);
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
            var tlpFiltros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 2, AutoSize = true };
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5f));
            tlpFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5f));
            
            txtBuscar = new MaterialTextBox2 { Hint = "Código, nombre o descripción...", Dock = DockStyle.Fill };
            btnBuscar = new MaterialButton { Text = "Buscar", Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, Dock = DockStyle.Left, Width = 100, Height = 36 };
            btnBuscar.Click += BtnBuscar_Click;
            
            tlpFiltros.Controls.Add(txtBuscar, 0, 0);
            tlpFiltros.Controls.Add(btnBuscar, 1, 0);
            tlpFiltros.SetRowSpan(txtBuscar, 2);
            tlpFiltros.SetRowSpan(btnBuscar, 2);

            cboFiltroCategoria = new MaterialComboBox { Hint = "Categoría", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboFiltroCategoria.Items.Add("Todas");
            cboFiltroCategoria.Items.AddRange(Enum.GetNames(typeof(CategoriaProducto)));
            cboFiltroCategoria.SelectedIndex = 0;
            cboFiltroCategoria.SelectedIndexChanged += FiltrosChanged;

            chkSoloMenuDelDia = new MaterialCheckbox { Text = "Solo Menú del Día", Dock = DockStyle.Fill };
            chkSoloMenuDelDia.CheckedChanged += FiltrosChanged;
            chkSoloBajoStock = new MaterialCheckbox { Text = "Solo Bajo Stock", Dock = DockStyle.Fill };
            chkSoloBajoStock.CheckedChanged += FiltrosChanged;

            tlpFiltros.Controls.Add(cboFiltroCategoria, 2, 0);
            tlpFiltros.Controls.Add(chkSoloMenuDelDia, 3, 0);
            tlpFiltros.Controls.Add(chkSoloBajoStock, 4, 0);

            cardSuperior.Controls.Add(tlpFiltros);
            tlpPrincipal.Controls.Add(cardSuperior, 0, 0);
            
            // === CONTENIDO PRINCIPAL (GRID Y BOTONES) ===
            var tlpContenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpPrincipal.Controls.Add(tlpContenido, 0, 1);

            // DATAGRIDVIEW
            dgvProductos = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BorderStyle = BorderStyle.None };
            EstilizarDataGridView();
            tlpContenido.Controls.Add(dgvProductos, 0, 0);

            // BOTONES LATERALES
            var cardBotones = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(5, 0, 0, 0) };
            var flpBotones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            
            var buttonMargin = new Padding(0, 0, 0, 10);
            btnNuevo = new MaterialButton { Text = "Nuevo", Width=140, Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, UseAccentColor = true, Margin = buttonMargin };
            btnNuevo.Click += BtnNuevo_Click;
            btnEditar = new MaterialButton { Text = "Editar", Width=140, Type = MaterialButton.MaterialButtonType.Contained, Margin = buttonMargin };
            btnEditar.Click += BtnEditar_Click;
            btnActualizarStock = new MaterialButton { Text = "Actualizar Stock", Width=140, Type = MaterialButton.MaterialButtonType.Contained, Margin = buttonMargin };
            btnActualizarStock.Click += BtnActualizarStock_Click;
            btnEliminar = new MaterialButton { Text = "Eliminar", Width=140, Type = MaterialButton.MaterialButtonType.Outlined, Margin = buttonMargin };
            btnEliminar.Click += BtnEliminar_Click;

            flpBotones.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnActualizarStock, btnEliminar });
            cardBotones.Controls.Add(flpBotones);
            tlpContenido.Controls.Add(cardBotones, 1, 0);
        }

        private void EstilizarDataGridView()
        {
            dgvProductos.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvProductos.GridColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;

            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProductos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProductos.EnableHeadersVisualStyles = false;
            
            dgvProductos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProductos.DefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
            dgvProductos.DefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.ColorScheme.TextColor;
            dgvProductos.DefaultCellStyle.Padding = new Padding(5);
            dgvProductos.RowTemplate.Height = 35;
        }

        private async Task CargarProductosAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var productos = await _productoService.ObtenerTodosAsync();

                // Aplicar filtros
                if (cboFiltroCategoria.SelectedIndex > 0 && cboFiltroCategoria.SelectedItem != null)
                {
                    var categoria = (CategoriaProducto)Enum.Parse(typeof(CategoriaProducto),
                        cboFiltroCategoria.SelectedItem.ToString()!);
                    productos = productos.Where(p => p.Categoria == categoria).ToList();
                }

                if (chkSoloMenuDelDia.Checked)
                    productos = productos.Where(p => p.EsMenuDelDia).ToList();

                if (chkSoloBajoStock.Checked)
                    productos = productos.Where(p => p.BajoStock).ToList();

                MostrarProductosEnGrid(productos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void MostrarProductosEnGrid(List<Producto> productos)
        {
            dgvProductos.DataSource = null;
            dgvProductos.DataSource = productos.Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                Categoria = p.Categoria.ToString(),
                Precio = p.Precio.ToString("C"),
                Stock = p.StockDisponible,
                StockMinimo = p.StockMinimo,
                MenuDelDia = p.EsMenuDelDia ? "Sí" : "No",
                Estado = p.Activo ? "Activo" : "Inactivo",
                EstadoStock = p.EstadoStock
            }).ToList();

            if (dgvProductos.Columns["Id"] != null)
                dgvProductos.Columns["Id"].Visible = false;

            if (dgvProductos.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvProductos.Columns)
                {
                    switch (column.Name)
                    {
                        case "Codigo":
                            column.Width = 80;
                            break;
                        case "Nombre":
                            column.Width = 200;
                            break;
                        case "Categoria":
                            column.Width = 100;
                            break;
                        case "Precio":
                            column.Width = 80;
                            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            break;
                        case "Stock":
                            column.Width = 60;
                            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "StockMinimo":
                            column.HeaderText = "Stock Mín.";
                            column.Width = 80;
                            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "MenuDelDia":
                            column.HeaderText = "Menú Día";
                            column.Width = 80;
                            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "Estado":
                            column.Width = 70;
                            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            break;
                        case "EstadoStock":
                            column.HeaderText = "Estado Stock";
                            column.Width = 100;
                            break;
                    }
                }

                // Colorear filas según estado del stock
                foreach (DataGridViewRow row in dgvProductos.Rows)
                {
                    var estadoStock = row.Cells["EstadoStock"]?.Value?.ToString();
                    if (estadoStock == "Sin Stock")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                    else if (estadoStock == "Stock Bajo")
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230);
                }
            }
        }

        private async void BtnBuscar_Click(object? sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var termino = txtBuscar.Text.Trim();
                var productos = await _productoService.BuscarAsync(termino);
                MostrarProductosEnGrid(productos);
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

        private void FiltrosChanged(object? sender, EventArgs e)
        {
            _ = CargarProductosAsync();
        }

        private void BtnNuevo_Click(object? sender, EventArgs e)
        {
            var formEditar = new FormEditarProducto();
            if (formEditar.ShowDialog() == DialogResult.OK)
            {
                _ = CargarProductosAsync();
            }
        }

        private void BtnEditar_Click(object? sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor seleccione un producto para editar", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
            var formEditar = new FormEditarProducto(id);
            if (formEditar.ShowDialog() == DialogResult.OK)
            {
                _ = CargarProductosAsync();
            }
        }

        private async void BtnActualizarStock_Click(object? sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor seleccione un producto para actualizar stock", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
            var nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value?.ToString();
            var stockActual = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Stock"].Value);

            using (var form = new MaterialForm())
            {
                form.Text = "Actualizar Stock";
                form.Size = new Size(400, 250);
                form.Sizable = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var materialSkinManager = MaterialSkinManager.Instance;
                materialSkinManager.AddFormToManage(form);
                materialSkinManager.Theme = MaterialSkinManager.Instance.Theme;
                materialSkinManager.ColorScheme = MaterialSkinManager.Instance.ColorScheme;

                var lblProducto = new MaterialLabel
                {
                    Text = $"Producto: {nombre}",
                    Location = new Point(20, 70),
                    Size = new Size(350, 23),
                    FontType = MaterialSkinManager.fontType.Subtitle1
                };

                var lblStockActual = new MaterialLabel
                {
                    Text = $"Stock Actual: {stockActual}",
                    Location = new Point(20, 100),
                    Size = new Size(150, 23),
                    FontType = MaterialSkinManager.fontType.Body1
                };

                var txtCantidad = new MaterialTextBox2
                {
                    Hint = "Cantidad a ajustar",
                    Location = new Point(20, 130),
                    Size = new Size(180, 40)
                    // REMOVED: ValidationRegex = "^-?\\d*$",
                    // REMOVED: UseAccentColor = true
                };

                var lblNota = new MaterialLabel
                {
                    Text = "(Positivo para agregar, negativo para reducir)",
                    Location = new Point(20, 175),
                    Size = new Size(350, 23),
                    FontType = MaterialSkinManager.fontType.Caption,
                    ForeColor = Color.Gray
                };

                var btnAceptar = new MaterialButton
                {
                    Text = "Aceptar",
                    Location = new Point(100, 200),
                    Size = new Size(100, 36),
                    DialogResult = DialogResult.OK,
                    Type = MaterialButton.MaterialButtonType.Contained,
                    HighEmphasis = true
                };

                var btnCancelar = new MaterialButton
                {
                    Text = "Cancelar",
                    Location = new Point(210, 200),
                    Size = new Size(100, 36),
                    DialogResult = DialogResult.Cancel,
                    Type = MaterialButton.MaterialButtonType.Outlined
                };

                form.Controls.AddRange(new Control[] {
                    lblProducto, lblStockActual, txtCantidad,
                    lblNota, btnAceptar, btnCancelar
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad == 0)
                    {
                        MessageBox.Show("Por favor, ingrese una cantidad válida y diferente de cero.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        var esVenta = cantidad < 0;
                        await _productoService.ActualizarStockAsync(id, Math.Abs(cantidad), esVenta);
                        MessageBox.Show("Stock actualizado correctamente", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CargarProductosAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar stock: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor seleccione un producto para eliminar", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
            var nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value?.ToString();

            var resultado = MessageBox.Show(
                $"¿Está seguro de eliminar el producto {nombre}?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    await _productoService.EliminarAsync(id);
                    MessageBox.Show("Producto eliminado correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await CargarProductosAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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