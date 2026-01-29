using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CafeteriaUNAL.Forms;
using CafeteriaUNAL.Models;
using CafeteriaUNAL.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CafeteriaUNAL
{
    public partial class MainForm : MaterialForm
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;
        private readonly ITransaccionService _transaccionService;
        private readonly IProductoService _productoService;
        private readonly IFichoService _fichoService;

        // --- Controles de la Interfaz Principal ---
        // Panel principal que alojará todos los formularios (Arquitectura SDI)
        private Panel contentPanel = null!;
        private Panel panelBienvenida = null!;
        private MaterialDrawer drawer = null!;
        
        // Tarjetas del Dashboard
        private MaterialCard cardVentas = null!, cardFichos = null!, cardStock = null!;

        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _authService = _serviceProvider.GetRequiredService<IAuthService>();
            _transaccionService = _serviceProvider.GetRequiredService<ITransaccionService>();
            _productoService = _serviceProvider.GetRequiredService<IProductoService>();
            _fichoService = _serviceProvider.GetRequiredService<IFichoService>();

            // Se suscribe al evento 'Shown' para realizar tareas asíncronas de forma segura
            // después de que el formulario se haya cargado y mostrado por completo.
            this.Shown += MainForm_Shown;

            ConfigurarEstiloMaterial();
            CrearControlesSDI();
            CrearDrawerDeNavegacion();
            MostrarPanelBienvenida();
        }

        // Evento que se dispara cuando el formulario se muestra por primera vez.
        // Es el lugar seguro para iniciar operaciones asíncronas que cargan datos para la UI.
        private async void MainForm_Shown(object? sender, EventArgs e)
        {
            await CargarDatosDashboardAsync();
        }

        private void ConfigurarEstiloMaterial()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,
                Primary.BlueGrey900,
                Primary.BlueGrey500,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            this.Text = "Sistema de Cafetería UNAL";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // Crea el panel de contenido principal para la arquitectura de Interfaz de Documento Único (SDI).
        private void CrearControlesSDI()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 239, 241)
            };
            this.Controls.Add(contentPanel);
        }

        // Método de ayuda para crear botones del menú de navegación de forma consistente.
        private MaterialButton CrearBotonMenu(string texto, EventHandler onClick)
        {
            var btn = new MaterialButton
            {
                Text = texto,
                Type = MaterialButton.MaterialButtonType.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 40,
                Margin = new Padding(10, 2, 10, 2)
            };
            btn.Click += onClick;
            return btn;
        }

        // Construye el menú de navegación lateral (Drawer).
        // Se utiliza un TableLayoutPanel de 3 filas para asegurar una estructura robusta:
        // 1. Cabecera (AutoSize)
        // 2. Contenido de módulos (ocupa el 100% del espacio restante y tiene scroll)
        // 3. Pie de página con botón de salir (AutoSize)
        private void CrearDrawerDeNavegacion()
        {
            var sesion = _authService.SesionActual;
            if (sesion == null) return;

            drawer = new MaterialDrawer { Dock = DockStyle.Left, UseColors = true, Width = 280, AutoSize = false };
            contentPanel.Parent.Controls.Add(drawer);

            var tlpDrawer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            tlpDrawer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpDrawer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpDrawer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            drawer.Controls.Add(tlpDrawer);

            // 1. Cabecera
            var headerPanel = new Panel { Height = 120, Dock = DockStyle.Fill, BackColor = MaterialSkinManager.Instance.ColorScheme.PrimaryColor };
            var headerLabel = new MaterialLabel { Text = sesion.NombreCompleto, FontType = MaterialSkinManager.fontType.Subtitle1, Location = new Point(20, 75), BackColor = Color.Transparent };
            var subHeaderLabel = new MaterialLabel { Text = sesion.Rol.ToString(), FontType = MaterialSkinManager.fontType.Body2, Location = new Point(20, 95), BackColor = Color.Transparent };
            headerPanel.Controls.AddRange(new Control[] { headerLabel, subHeaderLabel });
            tlpDrawer.Controls.Add(headerPanel, 0, 0);

            // 2. Contenido de Módulos (con scroll)
            var scrollablePanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tlpDrawer.Controls.Add(scrollablePanel, 0, 1);

            var menuContentPanel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Width = scrollablePanel.ClientSize.Width };
            menuContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            scrollablePanel.Controls.Add(menuContentPanel);

            int menuRow = 0;
            if (sesion.PuedeRealizarVentas)
            {
                var panelVentas = new MaterialExpansionPanel { Title = "Ventas", UseAccentColor = true, Padding = new Padding(24, 64, 24, 16), Dock = DockStyle.Top };
                panelVentas.Controls.Add(CrearBotonMenu("Nueva Venta", MenuNuevaVenta_Click));
                panelVentas.Controls.Add(CrearBotonMenu("Historial de Ventas", MenuHistorialVentas_Click));
                menuContentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                menuContentPanel.Controls.Add(panelVentas, 0, menuRow++);
            }

            if (sesion.PuedeGestionarUsuarios || sesion.PuedeGestionarProductos || sesion.PuedeGestionarFichos)
            {
                var panelGestion = new MaterialExpansionPanel { Title = "Gestión", UseAccentColor = true, Padding = new Padding(24, 64, 24, 16), Dock = DockStyle.Top };
                if (sesion.PuedeGestionarProductos) panelGestion.Controls.Add(CrearBotonMenu("Productos", MenuProductos_Click));
                panelGestion.Controls.Add(CrearBotonMenu("Usuarios (Clientes)", MenuUsuarios_Click));
                if (sesion.PuedeGestionarUsuarios) panelGestion.Controls.Add(CrearBotonMenu("Usuarios del Sistema", MenuUsuariosSistema_Click));
                if (sesion.PuedeGestionarFichos) panelGestion.Controls.Add(CrearBotonMenu("Fichos del Día", MenuFichos_Click));
                menuContentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                menuContentPanel.Controls.Add(panelGestion, 0, menuRow++);
            }

            if (sesion.PuedeVerReportes || sesion.PuedeConfigurarSistema)
            {
                var panelSistema = new MaterialExpansionPanel { Title = "Análisis y Sistema", UseAccentColor = true, Padding = new Padding(24, 64, 24, 16), Dock = DockStyle.Top };
                if (sesion.PuedeVerReportes) panelSistema.Controls.Add(CrearBotonMenu("Reportes", MenuSistemaReportes_Click));
                if (sesion.PuedeConfigurarSistema) panelSistema.Controls.Add(CrearBotonMenu("Configuración", MenuConfiguracion_Click));
                menuContentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                menuContentPanel.Controls.Add(panelSistema, 0, menuRow++);
            }

            // 3. Pie de página (Footer)
            var footerPanel = new Panel { Dock = DockStyle.Fill, Height = 50, Padding = new Padding(0, 5, 0, 5) };
            footerPanel.Controls.Add(new MaterialDivider { Dock = DockStyle.Top });
            var itemCerrarSesion = CrearBotonMenu("Cerrar Sesión", MenuCerrarSesion_Click);
            itemCerrarSesion.Dock = DockStyle.Fill;
            footerPanel.Controls.Add(itemCerrarSesion);
            tlpDrawer.Controls.Add(footerPanel, 0, 2);
        }

        // Prepara el panel de bienvenida (Dashboard) con su estructura estática.
        private void MostrarPanelBienvenida()
        {
            var sesion = _authService.SesionActual;
            if (sesion == null) return;

            contentPanel.Controls.Clear();

            panelBienvenida = new Panel { Dock = DockStyle.Fill, Name = "PanelBienvenida", Padding = new Padding(25) };

            var tlpDashboard = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
            tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panelBienvenida.Controls.Add(tlpDashboard);

            cardVentas = CreateKpiCard("VENTAS DE HOY", "---", Color.FromArgb(33, 150, 243));
            cardFichos = CreateKpiCard("FICHOS PENDIENTES", "---", Color.FromArgb(255, 152, 0));
            cardStock = CreateKpiCard("PRODUCTOS BAJO STOCK", "---", Color.FromArgb(244, 67, 54));

            tlpDashboard.Controls.Add(cardVentas, 0, 0);
            tlpDashboard.Controls.Add(cardFichos, 1, 0);
            tlpDashboard.Controls.Add(cardStock, 2, 0);

            contentPanel.Controls.Add(panelBienvenida);
            panelBienvenida.BringToFront();
        }

        // Carga los datos para las tarjetas del Dashboard de forma asíncrona.
        // Se ejecuta después de que el formulario es visible para no bloquear la UI.
        private async Task CargarDatosDashboardAsync()
        {
            (cardVentas.Controls[1] as MaterialLabel)!.Text = "Cargando...";
            (cardFichos.Controls[1] as MaterialLabel)!.Text = "Cargando...";
            (cardStock.Controls[1] as MaterialLabel)!.Text = "Cargando...";

            try
            {
                var ventasHoyTask = _transaccionService.ObtenerDelDiaAsync(DateTime.Today);
                var statsFichosTask = _fichoService.ObtenerEstadisticasAsync(DateTime.Today);
                var productosBajoStockTask = _productoService.ObtenerProductosConStockBajoAsync();

                await Task.WhenAll(ventasHoyTask, statsFichosTask, productosBajoStockTask);

                var ventasHoy = await ventasHoyTask;
                var statsFichos = await statsFichosTask;
                var productosBajoStock = await productosBajoStockTask;

                if (cardVentas.IsHandleCreated) cardVentas.BeginInvoke((MethodInvoker)delegate { (cardVentas.Controls[1] as MaterialLabel)!.Text = ventasHoy.Sum(v => v.Total).ToString("C"); });
                if (cardFichos.IsHandleCreated) cardFichos.BeginInvoke((MethodInvoker)delegate { (cardFichos.Controls[1] as MaterialLabel)!.Text = statsFichos.Pendientes.ToString(); });
                if (cardStock.IsHandleCreated) cardStock.BeginInvoke((MethodInvoker)delegate { (cardStock.Controls[1] as MaterialLabel)!.Text = productosBajoStock.Count.ToString(); });
            }
            catch
            {
                if (cardVentas.IsHandleCreated) cardVentas.BeginInvoke((MethodInvoker)delegate { (cardVentas.Controls[1] as MaterialLabel)!.Text = "Error"; });
                if (cardFichos.IsHandleCreated) cardFichos.BeginInvoke((MethodInvoker)delegate { (cardFichos.Controls[1] as MaterialLabel)!.Text = "Error"; });
                if (cardStock.IsHandleCreated) cardStock.BeginInvoke((MethodInvoker)delegate { (cardStock.Controls[1] as MaterialLabel)!.Text = "Error"; });
            }
        }

        private MaterialCard CreateKpiCard(string titulo, string valorInicial, Color color)
        {
            var card = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(10), Padding = new Padding(15) };
            var lblTitulo = new MaterialLabel { Text = titulo, FontType = MaterialSkinManager.fontType.Button, ForeColor = color, Dock = DockStyle.Top, HighEmphasis = true };
            var lblValor = new MaterialLabel { Text = valorInicial, FontType = MaterialSkinManager.fontType.H3, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            card.Controls.Add(lblValor);
            card.Controls.Add(lblTitulo);
            return card;
        }

        // Carga un formulario dentro del panel de contenido principal.
        // Esta es la implementación clave de la arquitectura SDI (Single Document Interface).
        private void AbrirFormulario(Form form)
        {
            contentPanel.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(form);
            form.Show();
        }

        private void MenuCerrarSesion_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Está seguro de que desea cerrar sesión?", "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) { Application.Restart(); }
        }

        private void MenuUsuariosSistema_Click(object? sender, EventArgs e) => MessageBox.Show("Gestión de usuarios del sistema - Por implementar.", "Información");
        private void MenuConfiguracion_Click(object? sender, EventArgs e) => MessageBox.Show("Configuración del sistema - Por implementar.", "Información");
        private void MenuUsuarios_Click(object? sender, EventArgs e) => AbrirFormulario(_serviceProvider.GetRequiredService<FormUsuarios>());
        private void MenuProductos_Click(object? sender, EventArgs e) => AbrirFormulario(_serviceProvider.GetRequiredService<FormProductos>());
        private void MenuNuevaVenta_Click(object? sender, EventArgs e) => AbrirFormulario(_serviceProvider.GetRequiredService<FormNuevaVenta>());
        private void MenuHistorialVentas_Click(object? sender, EventArgs e) => AbrirFormulario(_serviceProvider.GetRequiredService<FormHistorialVentas>());
        private void MenuFichos_Click(object? sender, EventArgs e) => AbrirFormulario(_serviceProvider.GetRequiredService<FormGestionFichos>());
        private void MenuSistemaReportes_Click(object? sender, EventArgs e) => AbrirFormulario(_serviceProvider.GetRequiredService<FormReportes>());
    }
}