using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;

namespace SellFast.Core.Services
{
    public class HardwareService : IHardwareService
    {
        public List<string> ObtenerImpresorasInstaladas()
        {
            var list = new List<string>();
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    list.Add(printer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo impresoras: {ex.Message}");
            }

            if (list.Count == 0)
            {
                list.Add("Impresora Térmica POS Predeterminada");
            }

            return list;
        }

        public string ObtenerImpresoraPredeterminada()
        {
            try
            {
                var settings = new PrinterSettings();
                return settings.PrinterName ?? "Impresora Predeterminada";
            }
            catch
            {
                return "Impresora Predeterminada";
            }
        }

        public bool ImprimirComprobanteDirecto(string pdfPath, string nombreImpresora)
        {
            if (!File.Exists(pdfPath)) return false;

            try
            {
                // Send PDF directly to system default or target printer using shell printto/print
                var psi = new ProcessStartInfo
                {
                    FileName = pdfPath,
                    Verb = "print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al imprimir comprobante: {ex.Message}");
                return false;
            }
        }
    }
}
