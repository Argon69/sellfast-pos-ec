using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;
using SellFast.Core.Services;

namespace SellFast.App.ViewModels
{
    public class ReportRow
    {
        public string DateOrCategory { get; set; } = "";
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalFormatted => TotalAmount.ToString("C0");
    }

    public partial class ReportesViewModel : ObservableObject
    {
        private readonly SellFastContext _context;
        private readonly IPdfReceiptService _pdfReceiptService;

        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Today;

        [ObservableProperty]
        private decimal _totalPeriodo = 0;

        [ObservableProperty]
        private int _transaccionesPeriodo = 0;

        public ObservableCollection<ReportRow> FilasReporte { get; } = new();
        public ObservableCollection<Transaccion> TransaccionesLista { get; } = new();

        public ISeries[] SalesSeries { get; set; } = Array.Empty<ISeries>();
        public Axis[] XAxes { get; set; } = Array.Empty<Axis>();

        public ReportesViewModel(SellFastContext context, IPdfReceiptService pdfReceiptService)
        {
            _context = context;
            _pdfReceiptService = pdfReceiptService;
        }

        public async Task CargarReportesAsync()
        {
            var inicio = FechaInicio.Date;
            var fin = FechaFin.Date.AddDays(1).AddTicks(-1);

            var transacciones = await _context.Transacciones
                .Include(t => t.Cliente)
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.FechaHora >= inicio && t.FechaHora <= fin && t.Estado == EstadoTransaccion.Completada)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();

            TotalPeriodo = transacciones.Sum(t => t.Total);
            TransaccionesPeriodo = transacciones.Count;

            TransaccionesLista.Clear();
            foreach (var t in transacciones)
                TransaccionesLista.Add(t);

            var agrupados = transacciones
                .GroupBy(t => t.FechaHora.Date)
                .OrderBy(g => g.Key)
                .ToList();

            FilasReporte.Clear();
            var datesList = new List<string>();
            var valuesList = new List<double>();

            foreach (var g in agrupados)
            {
                string dateStr = g.Key.ToString("dd/MM");
                datesList.Add(dateStr);
                double total = (double)g.Sum(t => t.Total);
                valuesList.Add(total);

                FilasReporte.Add(new ReportRow
                {
                    DateOrCategory = g.Key.ToString("yyyy-MM-dd"),
                    TotalTransactions = g.Count(),
                    TotalAmount = g.Sum(t => t.Total)
                });
            }

            SalesSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = valuesList,
                    Name = "Ventas ($)",
                    Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SkiaSharp.SKColor.Parse("#1C1D2B")) { StrokeThickness = 3 },
                    Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SkiaSharp.SKColor.Parse("#15D2F835"))
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = datesList,
                    LabelsRotation = 15
                }
            };

            OnPropertyChanged(nameof(SalesSeries));
            OnPropertyChanged(nameof(XAxes));
        }

        partial void OnFechaInicioChanged(DateTime value) => _ = CargarReportesAsync();
        partial void OnFechaFinChanged(DateTime value) => _ = CargarReportesAsync();

        [RelayCommand]
        private async Task ReimprimirPdfAsync(Transaccion transaccion)
        {
            try
            {
                var config = await _context.Configuracion.FirstOrDefaultAsync() ?? new ConfiguracionNegocio();
                string pdfPath = await _pdfReceiptService.GenerarComprobantePdfAsync(transaccion, config);

                if (File.Exists(pdfPath))
                {
                    Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error de Reimpresión", ex.Message, Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private void ExportarCsv()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                var sb = new StringBuilder();
                sb.AppendLine("Fecha;Transacciones;Total Ventas");

                foreach (var row in FilasReporte)
                {
                    sb.AppendLine($"{row.DateOrCategory};{row.TotalTransactions};{row.TotalAmount}");
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                Views.ModernDialogWindow.Show("Reporte Exportado", $"El archivo CSV fue guardado exitosamente en el escritorio:\n{filePath}", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error al Exportar", ex.Message, Views.DialogType.Error);
            }
        }

        [RelayCommand]
        private void ExportarExcel()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Reporte de Ventas");

                    // Title Header
                    worksheet.Cell(1, 1).Value = "SELLFAST POS — REPORTE DE VENTAS";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#1C1D2B");

                    worksheet.Cell(2, 1).Value = $"Período: {FechaInicio:yyyy-MM-dd} al {FechaFin:yyyy-MM-dd}";
                    worksheet.Cell(2, 1).Style.Font.Italic = true;

                    // Table Headers
                    worksheet.Cell(4, 1).Value = "Fecha";
                    worksheet.Cell(4, 2).Value = "N° Transacciones";
                    worksheet.Cell(4, 3).Value = "Total Ventas ($)";

                    var headerRange = worksheet.Range(4, 1, 4, 3);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C1D2B");
                    headerRange.Style.Font.FontColor = XLColor.FromHtml("#FFFFFF");

                    int rowIdx = 5;
                    foreach (var row in FilasReporte)
                    {
                        worksheet.Cell(rowIdx, 1).Value = row.DateOrCategory;
                        worksheet.Cell(rowIdx, 2).Value = row.TotalTransactions;
                        worksheet.Cell(rowIdx, 3).Value = row.TotalAmount;
                        worksheet.Cell(rowIdx, 3).Style.NumberFormat.Format = "$ #,##0";
                        rowIdx++;
                    }

                    // Total Summary Row
                    worksheet.Cell(rowIdx, 1).Value = "TOTAL PERÍODO";
                    worksheet.Cell(rowIdx, 1).Style.Font.Bold = true;
                    worksheet.Cell(rowIdx, 2).Value = TransaccionesPeriodo;
                    worksheet.Cell(rowIdx, 2).Style.Font.Bold = true;
                    worksheet.Cell(rowIdx, 3).Value = TotalPeriodo;
                    worksheet.Cell(rowIdx, 3).Style.Font.Bold = true;
                    worksheet.Cell(rowIdx, 3).Style.NumberFormat.Format = "$ #,##0";

                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(filePath);
                }

                Views.ModernDialogWindow.Show("Excel Generado", $"El reporte profesional en Excel fue guardado en el escritorio:\n{filePath}", Views.DialogType.Success);
            }
            catch (Exception ex)
            {
                Views.ModernDialogWindow.Show("Error al Exportar Excel", ex.Message, Views.DialogType.Error);
            }
        }
    }
}
