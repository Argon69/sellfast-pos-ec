using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SellFast.Core.Data;
using SellFast.Core.Models;

namespace SellFast.Core.Services
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly SellFastContext _context;

        public ExcelImportService(SellFastContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportarClientesAsync(string filePath)
        {
            var result = new ImportResult();
            if (!File.Exists(filePath))
            {
                result.Mensaje = "El archivo especificado no existe.";
                return result;
            }

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name.ToLower().Contains("cliente")) ?? workbook.Worksheets.First();
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                int count = 0;
                int skipped = 0;

                var tipoDefault = await _context.TiposCliente.FirstOrDefaultAsync() ?? new TipoCliente { Nombre = "Cliente General" };
                if (tipoDefault.Id == 0)
                {
                    _context.TiposCliente.Add(tipoDefault);
                    await _context.SaveChangesAsync();
                }

                foreach (var row in rows)
                {
                    result.TotalFilas++;
                    string doc = row.Cell(1).GetValue<string>().Trim();
                    string nombreCompleto = row.Cell(2).GetValue<string>().Trim();
                    string tel = row.Cell(3).GetValue<string>().Trim();
                    string email = row.Cell(4).GetValue<string>().Trim();
                    string notas = row.Cell(5).GetValue<string>().Trim();

                    if (string.IsNullOrWhiteSpace(nombreCompleto))
                    {
                        skipped++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(doc))
                    {
                        doc = $"CLI-{DateTime.Now.Ticks % 1000000}-{count + 1}";
                    }

                    string nombre = nombreCompleto;
                    string apellido = "";
                    var parts = nombreCompleto.Split(' ', 2);
                    if (parts.Length > 1)
                    {
                        nombre = parts[0];
                        apellido = parts[1];
                    }

                    var existing = await _context.Clientes.FirstOrDefaultAsync(c => c.Documento == doc);
                    if (existing == null)
                    {
                        var nuevoCliente = new Cliente
                        {
                            Documento = doc,
                            Nombre = nombre,
                            Apellido = apellido,
                            Telefono = tel,
                            Email = email,
                            Notas = notas,
                            TipoClienteId = tipoDefault.Id,
                            Activo = true,
                            FechaRegistro = DateTime.Now
                        };
                        _context.Clientes.Add(nuevoCliente);
                        count++;
                    }
                    else
                    {
                        existing.Nombre = nombre;
                        existing.Apellido = apellido;
                        if (!string.IsNullOrWhiteSpace(tel)) existing.Telefono = tel;
                        if (!string.IsNullOrWhiteSpace(email)) existing.Email = email;
                        if (!string.IsNullOrWhiteSpace(notas)) existing.Notas = notas;
                        count++;
                    }
                }

                await _context.SaveChangesAsync();
                result.RegistrosImportados = count;
                result.RegistrosOmitidos = skipped;
                result.Mensaje = $"Se procesaron {count} clientes exitosamente.";
            }
            catch (Exception ex)
            {
                result.Mensaje = $"Error al leer el archivo Excel: {ex.Message}";
            }

            return result;
        }

        public async Task<ImportResult> ImportarProductosAsync(string filePath)
        {
            var result = new ImportResult();
            if (!File.Exists(filePath))
            {
                result.Mensaje = "El archivo especificado no existe.";
                return result;
            }

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name.ToLower().Contains("producto")) ?? workbook.Worksheets.First();
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

                int count = 0;
                int skipped = 0;

                foreach (var row in rows)
                {
                    result.TotalFilas++;
                    string codigo = row.Cell(1).GetValue<string>().Trim();
                    string nombre = row.Cell(2).GetValue<string>().Trim();
                    string catNombre = row.Cell(3).GetValue<string>().Trim();
                    decimal precio = row.Cell(4).GetValue<decimal>();
                    int stock = row.Cell(5).GetValue<int>();

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        skipped++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(codigo))
                    {
                        codigo = $"PROD-{DateTime.Now.Ticks % 1000000}-{count + 1}";
                    }

                    if (!Enum.TryParse<CategoriaProducto>(catNombre, true, out var catEnum))
                    {
                        catEnum = CategoriaProducto.Otro;
                    }

                    var existing = await _context.Productos.FirstOrDefaultAsync(p => p.Codigo == codigo);
                    if (existing == null)
                    {
                        var prod = new Producto
                        {
                            Codigo = codigo,
                            Nombre = nombre,
                            Categoria = catEnum,
                            Precio = precio,
                            StockDisponible = stock,
                            StockMinimo = 5,
                            Activo = true,
                            FechaCreacion = DateTime.Now
                        };
                        _context.Productos.Add(prod);
                        count++;
                    }
                    else
                    {
                        existing.Nombre = nombre;
                        existing.Categoria = catEnum;
                        existing.Precio = precio;
                        existing.StockDisponible = stock;
                        count++;
                    }
                }

                await _context.SaveChangesAsync();
                result.RegistrosImportados = count;
                result.RegistrosOmitidos = skipped;
                result.Mensaje = $"Se procesaron {count} productos exitosamente.";
            }
            catch (Exception ex)
            {
                result.Mensaje = $"Error al leer el archivo Excel: {ex.Message}";
            }

            return result;
        }

        public async Task<string> GenerarPlantillaImportacionAsync(string destinationFolder)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                string filePath = Path.Combine(destinationFolder, "Plantilla_Importacion_SellFast.xlsx");
                using var workbook = new XLWorkbook();

                // 1. Clientes Sheet
                var wsClientes = workbook.Worksheets.Add("Clientes");
                wsClientes.Cell(1, 1).Value = "Documento";
                wsClientes.Cell(1, 2).Value = "Nombre Completo";
                wsClientes.Cell(1, 3).Value = "Teléfono";
                wsClientes.Cell(1, 4).Value = "Email";
                wsClientes.Cell(1, 5).Value = "Notas / Dirección";

                var headerRange1 = wsClientes.Range("A1:E1");
                headerRange1.Style.Font.Bold = true;
                headerRange1.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C1D2B");
                headerRange1.Style.Font.FontColor = XLColor.FromHtml("#D2F835");

                // Sample Client Data
                wsClientes.Cell(2, 1).Value = "123456789";
                wsClientes.Cell(2, 2).Value = "Juan Pérez";
                wsClientes.Cell(2, 3).Value = "+57 300 123 4567";
                wsClientes.Cell(2, 4).Value = "juan.perez@email.com";
                wsClientes.Cell(2, 5).Value = "Calle 10 # 45-12";

                wsClientes.Columns().AdjustToContents();

                // 2. Productos Sheet
                var wsProductos = workbook.Worksheets.Add("Productos");
                wsProductos.Cell(1, 1).Value = "Código";
                wsProductos.Cell(1, 2).Value = "Nombre Producto";
                wsProductos.Cell(1, 3).Value = "Categoría (Comida, Bebida, Postre, Snack, Combo, Otro)";
                wsProductos.Cell(1, 4).Value = "Precio Venta";
                wsProductos.Cell(1, 5).Value = "Stock Inicial";

                var headerRange2 = wsProductos.Range("A1:E1");
                headerRange2.Style.Font.Bold = true;
                headerRange2.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C1D2B");
                headerRange2.Style.Font.FontColor = XLColor.FromHtml("#D2F835");

                // Sample Product Data
                wsProductos.Cell(2, 1).Value = "P001";
                wsProductos.Cell(2, 2).Value = "Café Americano 8oz";
                wsProductos.Cell(2, 3).Value = "Bebida";
                wsProductos.Cell(2, 4).Value = 4500;
                wsProductos.Cell(2, 5).Value = 100;

                wsProductos.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
                return filePath;
            });
        }
    }
}
