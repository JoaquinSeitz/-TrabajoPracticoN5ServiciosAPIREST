using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using tp5_Trani_Joaco_Alex.Data;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stock")]
        [AllowAnonymous]
        public async Task<IActionResult> DescargarReporteStock()
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Reporte de Stock de Inventario").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            // Corrección CS1929: Padding aplicado antes del Text
                            col.Item().PaddingBottom(1, Unit.Centimetre).Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("ID").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Producto").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Categoría").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).AlignRight().Text("Precio").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).AlignCenter().Text("Stock").SemiBold();
                            });

                            foreach (var p in productos)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(p.ProductoId.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(p.Nombre);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(p.Categoria?.Nombre ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text($"${p.Precio:N2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text(p.Stock.ToString());
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"Reporte_Stock_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error generando el reporte de stock.", error = ex.Message });
            }
        }

        [HttpGet("ticket/{salidaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DescargarTicketVenta(int salidaId)
        {
            try
            {
                var salida = await _context.SalidaProductos
                    .Include(s => s.Cliente)
                    .Include(s => s.Usuario)
                    .Include(s => s.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(s => s.SalidaProductoId == salidaId);

                if (salida == null)
                    return NotFound(new { mensaje = "Venta no encontrada." });

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Corrección CS0117: Usamos ContinuousSize para impresoras térmicas de 80mm
                        page.ContinuousSize(80, Unit.Millimetre);
                        page.Margin(5, Unit.Millimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().AlignCenter().Column(col =>
                        {
                            col.Item().Text("COMERCIO TP5").SemiBold().FontSize(14);
                            col.Item().Text("Ticket de Venta").FontSize(10);
                            col.Item().Text($"Nro: {salida.SalidaProductoId:D6}");

                            // Corrección CS1929: Padding aplicado antes del Text
                            col.Item().PaddingBottom(5, Unit.Millimetre).Text($"Fecha: {salida.Fecha:dd/MM/yyyy HH:mm}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Black);

                            col.Item().AlignLeft().Text($"Cliente: {salida.Cliente?.Nombre ?? "Consumidor Final"}");

                            // Corrección CS1929: Padding aplicado antes del AlignLeft/Text
                            col.Item().PaddingBottom(5, Unit.Millimetre).AlignLeft().Text($"Atendido por: {salida.Usuario?.Nombre ?? "Caja"}");
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(20);
                                columns.RelativeColumn();
                                columns.ConstantColumn(40);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Cant").SemiBold();
                                header.Cell().Text("Descripción").SemiBold();
                                header.Cell().AlignRight().Text("Subt.").SemiBold();
                            });

                            decimal totalVenta = 0;

                            foreach (var detalle in salida.Detalles)
                            {
                                var subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                                totalVenta += subtotal;

                                table.Cell().Text(detalle.Cantidad.ToString());
                                table.Cell().Text(detalle.Producto?.Nombre ?? "Item");
                                table.Cell().AlignRight().Text($"${subtotal:N2}");
                            }

                            table.Cell().ColumnSpan(3).PaddingTop(5).LineHorizontal(1).LineColor(Colors.Black);
                            table.Cell().ColumnSpan(2).PaddingTop(5).AlignRight().Text("TOTAL:").SemiBold().FontSize(11);
                            table.Cell().PaddingTop(5).AlignRight().Text($"${totalVenta:N2}").SemiBold().FontSize(11);
                        });

                        page.Footer().AlignCenter().PaddingTop(10, Unit.Millimetre).Text("¡Gracias por su compra!");
                    });
                });

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"Ticket_Venta_{salidaId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error generando el ticket de venta.", error = ex.Message });
            }
        }
        [HttpGet("ingreso/{ingresoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DescargarRemitoIngreso(int ingresoId)
        {
            try
            {
                var ingreso = await _context.IngresoProductos
                    .Include(i => i.Usuario)
                    .Include(i => i.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(i => i.IngresoProductoId == ingresoId);

                if (ingreso == null)
                    return NotFound(new { mensaje = "Ingreso no encontrado." });

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Comprobante de Ingreso de Mercadería").SemiBold().FontSize(18).FontColor(Colors.Green.Darken2);
                            col.Item().Text($"Nro de Remito: {ingreso.IngresoProductoId:D6}");
                            col.Item().PaddingBottom(1, Unit.Centimetre).Text($"Fecha: {ingreso.Fecha:dd/MM/yyyy HH:mm} | Registrado por: {ingreso.Usuario?.Nombre ?? "Admin"}").FontSize(10);
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Cant").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Producto").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).AlignRight().Text("Subtotal").SemiBold();
                            });

                            decimal totalIngreso = 0;

                            foreach (var detalle in ingreso.Detalles)
                            {
                                // Nota: Asumimos que la propiedad se llama PrecioUnitario. Cambiar a PrecioCompra si corresponde.
                                var subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                                totalIngreso += subtotal;

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(detalle.Cantidad.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(detalle.Producto?.Nombre ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text($"${subtotal:N2}");
                            }

                            table.Cell().ColumnSpan(3).PaddingTop(10).AlignRight().Text($"TOTAL INVERTIDO: ${totalIngreso:N2}").SemiBold().FontSize(12);
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"Remito_Ingreso_{ingresoId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error generando el reporte de ingreso.", error = ex.Message });
            }
        }
    }

}