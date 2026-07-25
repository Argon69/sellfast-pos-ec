using System;
using System.IO;
using System.Threading.Tasks;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SellFast.Core.Models;

namespace SellFast.Core.Services
{
    public class PdfReceiptService : IPdfReceiptService
    {
        public PdfReceiptService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<string> GenerarComprobantePdfAsync(Transaccion transaccion, ConfiguracionNegocio configuracion)
        {
            return Task.Run(() =>
            {
                string receiptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Comprobantes");
                Directory.CreateDirectory(receiptsFolder);

                string fileName = $"Comprobante_{transaccion.NumeroTransaccion}.pdf";
                string filePath = Path.Combine(receiptsFolder, fileName);

                string pais = (configuracion.Pais ?? "Colombia").Trim();
                string simbolo = string.IsNullOrWhiteSpace(configuracion.SimboloMoneda) ? "$" : configuracion.SimboloMoneda;

                // Determine Header Badge & Legal Footer based on Country Fiscal Rules
                string headerTitle = "COMPROBANTE DE VENTA";
                string legalFooter = "Documento equivalente generado electrónicamente con código QR";
                string regFiscalText = "";

                if (pais.Equals("Colombia", StringComparison.OrdinalIgnoreCase))
                {
                    headerTitle = "FACTURA EQUIVALENTE POS";
                    legalFooter = "Documento Equivalente de Venta POS — Ley 2010 DIAN Colombia. Generado por SellFast POS.";
                    regFiscalText = $"Régimen: {configuracion.TipoPersona}";
                }
                else if (pais.Equals("México", StringComparison.OrdinalIgnoreCase) || pais.Equals("Mexico", StringComparison.OrdinalIgnoreCase))
                {
                    headerTitle = "TICKET SIMPLIFICADO SAT";
                    legalFooter = "Comprobante de Venta al Público en General según normativa del SAT México. Sistema SellFast POS.";
                    regFiscalText = $"RFC: {configuracion.NIT ?? "XAXX010101000"}";
                }

                // Generate QR Code bytes using QRCoder
                byte[] qrCodeBytes;
                using (var qrGenerator = new QRCodeGenerator())
                {
                    string qrPayload = $"SELLFAST|PAIS:{pais}|NIT:{configuracion.NIT}|TRX:{transaccion.NumeroTransaccion}|TOTAL:{transaccion.Total:F0}|FECHA:{transaccion.FechaHora:yyyyMMddHHmm}";
                    var qrData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new PngByteQRCode(qrData);
                    qrCodeBytes = qrCode.GetGraphic(5);
                }

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A5);
                        page.Margin(20, Unit.Point);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                        page.Header().Element(headerContainer =>
                        {
                            headerContainer.Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(configuracion.NombreNegocio).FontSize(16).Bold().FontColor("#1C1D2B");
                                    if (!string.IsNullOrEmpty(configuracion.Eslogan))
                                        col.Item().Text(configuracion.Eslogan).FontSize(9).Italic().FontColor("#6B7280");
                                    if (!string.IsNullOrEmpty(configuracion.NIT))
                                        col.Item().Text($"NIT/RFC: {configuracion.NIT}").FontSize(8).FontColor("#6B7280");
                                    if (!string.IsNullOrEmpty(regFiscalText))
                                        col.Item().Text(regFiscalText).FontSize(8).FontColor("#6B7280");
                                    if (!string.IsNullOrEmpty(configuracion.Direccion))
                                        col.Item().Text(configuracion.Direccion).FontSize(8).FontColor("#6B7280");
                                    if (!string.IsNullOrEmpty(configuracion.Telefono))
                                        col.Item().Text($"Tel: {configuracion.Telefono}").FontSize(8).FontColor("#6B7280");
                                });

                                row.ConstantItem(160).AlignRight().Column(col =>
                                {
                                    col.Item().Background("#1C1D2B").Padding(4).AlignCenter().Text(headerTitle).FontSize(10).Bold().FontColor("#D2F835");
                                    col.Item().AlignRight().Text($"Folio: #{transaccion.NumeroTransaccion}").FontSize(9).Bold().FontColor("#1C1D2B");
                                    col.Item().AlignRight().Text(transaccion.FechaHora.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor("#6B7280");
                                });
                            });
                        });

                        page.Content().PaddingVertical(10, Unit.Point).Column(col =>
                        {
                            col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor("#E2E8F0");

                            // Client & Order Info
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Cliente: {transaccion.Cliente?.NombreCompleto ?? "Público en General"}").Bold();
                                    c.Item().Text($"Doc/NIT: {transaccion.Cliente?.Documento ?? "222222222222"}").FontColor("#6B7280");
                                });
                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text($"Forma de Pago: {transaccion.TipoPago}").Bold();
                                    if (transaccion.Mesa != null)
                                        c.Item().Text($"Mesa / Salón: #{transaccion.Mesa.Numero}").FontColor("#6B7280");
                                });
                            });

                            col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor("#E2E8F0");

                            // Items Table
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(75);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Cant").Bold();
                                    header.Cell().Text("Descripción").Bold();
                                    header.Cell().AlignRight().Text("P. Unit").Bold();
                                    header.Cell().AlignRight().Text("Subtotal").Bold();
                                });

                                foreach (var d in transaccion.Detalles)
                                {
                                    table.Cell().Text(d.Cantidad.ToString());
                                    table.Cell().Text(d.Producto?.Nombre ?? "Producto");
                                    table.Cell().AlignRight().Text($"{simbolo} {d.PrecioUnitario:N0}");
                                    table.Cell().AlignRight().Text($"{simbolo} {d.Subtotal:N0}").Bold();
                                }
                            });

                            col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor("#E2E8F0");

                            // Summary & QR Section
                            col.Item().Row(row =>
                            {
                                // Left side QR Code
                                row.RelativeItem().Column(qrCol =>
                                {
                                    qrCol.Item().Row(r =>
                                    {
                                        r.ConstantItem(60).Height(60).Image(qrCodeBytes);
                                        r.RelativeItem().PaddingLeft(8).Column(c =>
                                        {
                                            c.Item().Text("Verificación Fiscal QR").Bold().FontSize(8).FontColor("#1C1D2B");
                                            c.Item().Text($"Comprobante legal registrado en {pais}").FontSize(7).FontColor("#6B7280");
                                        });
                                    });
                                });

                                // Right side Totals & Tax Calculation
                                row.ConstantItem(200).AlignRight().Column(summary =>
                                {
                                    summary.Item().Row(r =>
                                    {
                                        r.ConstantItem(110).Text("Subtotal:");
                                        r.ConstantItem(90).AlignRight().Text($"{simbolo} {transaccion.Subtotal:N0}");
                                    });

                                    if (configuracion.IVA > 0)
                                    {
                                        decimal montoIva = transaccion.Subtotal * (configuracion.IVA / 100);
                                        summary.Item().Row(r =>
                                        {
                                            r.ConstantItem(110).Text($"IVA / Tax ({configuracion.IVA}%):");
                                            r.ConstantItem(90).AlignRight().Text($"{simbolo} {montoIva:N0}").FontColor("#6B7280");
                                        });
                                    }

                                    if (transaccion.MontoDescuento > 0)
                                    {
                                        summary.Item().Row(r =>
                                        {
                                            r.ConstantItem(110).Text($"Descuento ({transaccion.PorcentajeDescuento}%):").FontColor("#F97316");
                                            r.ConstantItem(90).AlignRight().Text($"-{simbolo} {transaccion.MontoDescuento:N0}").FontColor("#F97316");
                                        });
                                    }

                                    if (transaccion.Propina > 0)
                                    {
                                        summary.Item().Row(r =>
                                        {
                                            r.ConstantItem(110).Text("Propina:");
                                            r.ConstantItem(90).AlignRight().Text($"{simbolo} {transaccion.Propina:N0}");
                                        });
                                    }

                                    summary.Item().PaddingTop(4).Row(r =>
                                    {
                                        r.ConstantItem(110).Text("TOTAL PAGADO:").Bold().FontSize(11).FontColor("#1C1D2B");
                                        r.ConstantItem(90).AlignRight().Text($"{simbolo} {transaccion.Total:N0}").Bold().FontSize(13).FontColor("#22C55E");
                                    });
                                });
                            });
                        });

                        page.Footer().Column(footer =>
                        {
                            footer.Item().LineHorizontal(0.5f).LineColor("#E2E8F0");
                            footer.Item().PaddingTop(6).AlignCenter().Text($"¡Gracias por su compra en {configuracion.NombreNegocio}!").Bold().FontSize(9).FontColor("#1C1D2B");
                            footer.Item().AlignCenter().Text(legalFooter).FontSize(7).FontColor("#9CA3AF");
                        });
                    });
                }).GeneratePdf(filePath);

                return filePath;
            });
        }
    }
}
