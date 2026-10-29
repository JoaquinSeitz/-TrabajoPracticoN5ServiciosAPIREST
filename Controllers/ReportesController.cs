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
    [Authorize(Roles = "Admin")] // Protege ABSOLUTAMENTE TODOS los reportes solo para Admin
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stock")]
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
                        // Formato A5: Ideal para leer en celulares o adjuntar por mail
                        page.Size(PageSizes.A5);
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().AlignCenter().Text("COMERCIO TP5").SemiBold().FontSize(22).FontColor(Colors.BlueGrey.Darken2);
                            col.Item().AlignCenter().Text("Comprobante de Venta Electrónico").FontSize(12).FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            col.Item().PaddingVertical(15).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Nro de Ticket: #{salida.SalidaProductoId:D6}").SemiBold();
                                    c.Item().Text($"Fecha: {salida.Fecha:dd/MM/yyyy HH:mm}");
                                });
                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text($"Cliente: {salida.Cliente?.Nombre ?? "Consumidor Final"}").SemiBold();
                                    c.Item().Text($"Atendido por: {salida.Usuario?.Nombre ?? "Tienda Online"}");
                                });
                            });
                        });

                        page.Content().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // Cantidad
                                columns.RelativeColumn();    // Detalle
                                columns.ConstantColumn(70);  // Precio Unitario
                                columns.ConstantColumn(80);  // Subtotal
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Cant").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Descripción").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("P. Unit").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Subtotal").SemiBold();
                            });

                            decimal totalVenta = 0;

                            foreach (var detalle in salida.Detalles)
                            {
                                var subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                                totalVenta += subtotal;

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(detalle.Cantidad.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(detalle.Producto?.Nombre ?? "Item");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).AlignRight().Text($"${detalle.PrecioUnitario:N2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).AlignRight().Text($"${subtotal:N2}");
                            }

                            table.Cell().ColumnSpan(4).PaddingTop(20).AlignRight().Text("TOTAL A PAGAR:").SemiBold().FontSize(12);
                            table.Cell().ColumnSpan(4).AlignRight().Text($"${totalVenta:N2}").SemiBold().FontSize(18).FontColor(Colors.BlueGrey.Darken3);
                        });

                        page.Footer().AlignCenter().Text("¡Gracias por su compra!").Italic().FontColor(Colors.Grey.Medium);
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

        [HttpGet("auditoria-ventas")]
        public async Task<IActionResult> DescargarAuditoriaVentas([FromQuery] DateTime? fecha)
        {
            try
            {
                var query = _context.SalidaProductos
                    .Include(s => s.Usuario)
                    .Include(s => s.Cliente)
                    .Include(s => s.Detalles)
                    .AsQueryable();

                // Aplicar el filtro de fecha si se recibe desde Blazor
                if (fecha.HasValue)
                {
                    query = query.Where(s => s.Fecha.Date == fecha.Value.Date);
                }

                var salidas = await query.OrderByDescending(s => s.Fecha).ToListAsync();

                // Si no hay resultados para esa fecha, devolvemos 404 para mostrar el Snackbar
                if (!salidas.Any())
                {
                    return NotFound(new { mensaje = "No hay registros en la fecha seleccionada." });
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Auditoría Consolidada de Ventas").SemiBold().FontSize(18).FontColor(Colors.BlueGrey.Darken2);
                            col.Item().PaddingBottom(1, Unit.Centimetre).Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Ticket").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Fecha").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Cliente").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Atendió").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).AlignRight().Text("Total").SemiBold();
                            });

                            decimal granTotal = 0;

                            foreach (var s in salidas)
                            {
                                decimal totalVenta = s.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                                granTotal += totalVenta;

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text($"#{s.SalidaProductoId:D4}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(s.Fecha.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(s.Cliente?.Nombre ?? "Consumidor Final");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(s.Usuario?.Nombre ?? "Admin");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text($"${totalVenta:N2}");
                            }

                            table.Cell().ColumnSpan(4).PaddingTop(10).AlignRight().Text("RECAUDACIÓN TOTAL:").SemiBold().FontSize(12);
                            table.Cell().PaddingTop(10).AlignRight().Text($"${granTotal:N2}").SemiBold().FontSize(12).FontColor(Colors.Green.Darken2);
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
                return File(pdfBytes, "application/pdf", $"Auditoria_Ventas_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error generando la auditoría.", error = ex.Message });
            }
        }

        [HttpGet("auditoria-ingresos")]
        public async Task<IActionResult> DescargarAuditoriaIngresos([FromQuery] DateTime? fecha)
        {
            try
            {
                var query = _context.IngresoProductos
                    .Include(i => i.Usuario)
                    .Include(i => i.Proveedor) // Agregamos la relación con el Proveedor
                    .Include(i => i.Detalles)
                        .ThenInclude(d => d.Producto)
                    .AsQueryable();

                // Aplicar el filtro de fecha si se recibe desde Blazor
                if (fecha.HasValue)
                {
                    query = query.Where(i => i.Fecha.Date == fecha.Value.Date);
                }

                var ingresos = await query.OrderByDescending(i => i.Fecha).ToListAsync();

                // Si no hay resultados para esa fecha, devolvemos 404 para mostrar el Snackbar
                if (!ingresos.Any())
                {
                    return NotFound(new { mensaje = "No hay registros en la fecha seleccionada." });
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9)); // Bajamos 1pt para que entre todo bien

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Auditoría Consolidada de Ingresos").SemiBold().FontSize(18).FontColor(Colors.Green.Darken2);
                            col.Item().PaddingBottom(1, Unit.Centimetre).Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);  // Nro
                                columns.ConstantColumn(80);  // Fecha
                                columns.ConstantColumn(70);  // Usuario
                                columns.RelativeColumn(1);   // Proveedor (Nueva)
                                columns.RelativeColumn(1.5f); // Productos
                                columns.ConstantColumn(45);  // Unidades
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Nro").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Fecha").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Usuario").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Proveedor").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Detalle de Productos").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(2).AlignRight().Text("Unid.").SemiBold();
                            });

                            int totalUnidades = 0;

                            foreach (var i in ingresos)
                            {
                                int cantidadItems = i.Detalles.Sum(d => d.Cantidad);
                                totalUnidades += cantidadItems;

                                var listaProductos = string.Join("\n", i.Detalles.Select(d => $"• {d.Producto?.Nombre}"));

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text($"#{i.IngresoProductoId:D4}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(i.Fecha.ToString("dd/MM/yy HH:mm"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(i.Usuario?.Nombre ?? "Admin");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(i.Proveedor?.RazonSocial ?? "Desconocido"); // Imprimimos el Proveedor
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(listaProductos);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(cantidadItems.ToString());
                            }

                            table.Cell().ColumnSpan(5).PaddingTop(10).AlignRight().Text("TOTAL UNIDADES INGRESADAS:").SemiBold().FontSize(11);
                            table.Cell().PaddingTop(10).AlignRight().Text(totalUnidades.ToString()).SemiBold().FontSize(11).FontColor(Colors.Green.Darken2);
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
                return File(pdfBytes, "application/pdf", $"Auditoria_Ingresos_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error generando la auditoría.", error = ex.Message });
            }
        }
    }
}