using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;
using tp5_Trani_Joaco_Alex.DTOs.Request;
using tp5_Trani_Joaco_Alex.DTOs.Response;
using tp5_Trani_Joaco_Alex.Services;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalidaProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public SalidaProductosController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSalidas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var salidas = await _context.SalidaProductos
                    .Include(s => s.Cliente)
                    .Include(s => s.Detalles).ThenInclude(d => d.Producto)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SalidaProductoResponseDTO
                    {
                        SalidaProductoId = s.SalidaProductoId,
                        Fecha = s.Fecha,
                        ClienteId = s.ClienteId,
                        ClienteNombre = s.Cliente != null ? s.Cliente.Nombre : "Desconocido",
                        UsuarioId = s.UsuarioId,
                        Detalles = s.Detalles.Select(d => new DetalleMovimientoResponseDTO
                        {
                            ProductoId = d.ProductoId,
                            ProductoNombre = d.Producto != null ? d.Producto.Nombre : "Desconocido",
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario
                        }).ToList()
                    }).ToListAsync();

                return Ok(salidas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalida(int id)
        {
            try
            {
                var salida = await _context.SalidaProductos
                    .Include(s => s.Cliente)
                    .Include(s => s.Detalles).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(s => s.SalidaProductoId == id);

                if (salida == null) return NotFound(new { mensaje = "Salida no encontrada." });

                var response = new SalidaProductoResponseDTO
                {
                    SalidaProductoId = salida.SalidaProductoId,
                    Fecha = salida.Fecha,
                    ClienteId = salida.ClienteId,
                    ClienteNombre = salida.Cliente != null ? salida.Cliente.Nombre : "Desconocido",
                    UsuarioId = salida.UsuarioId,
                    Detalles = salida.Detalles.Select(d => new DetalleMovimientoResponseDTO
                    {
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null ? d.Producto.Nombre : "Desconocido",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }).ToList()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarSalida([FromBody] SalidaProductoRequestDTO dto)
        {
            if (dto == null || dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest(new { mensaje = "La salida debe contener al menos un producto en el detalle." });

            try
            {
                // 1. Descontar stock y guardar en BD
                var nuevaSalida = new SalidaProductos
                {
                    ClienteId = dto.ClienteId,
                    UsuarioId = dto.UsuarioId,
                    Fecha = DateTime.Now,
                    Detalles = new List<DetalleSalidaProductos>()
                };

                decimal totalVenta = 0;
                foreach (var item in dto.Detalles)
                {
                    if (item.Cantidad <= 0) return BadRequest(new { mensaje = $"Cantidad para ID {item.ProductoId} inválida." });
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null || !producto.Activo) return NotFound(new { mensaje = $"Producto ID {item.ProductoId} inactivo." });
                    if (producto.Stock < item.Cantidad) return BadRequest(new { mensaje = $"Stock insuficiente de '{producto.Nombre}'." });

                    producto.Stock -= item.Cantidad;
                    nuevaSalida.Detalles.Add(new DetalleSalidaProductos
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio
                    });
                    totalVenta += item.Cantidad * producto.Precio;
                }

                _context.SalidaProductos.Add(nuevaSalida);
                await _context.SaveChangesAsync();

                // 2. Traer la venta completa con nombres para el PDF y el Email
                var salidaCompleta = await _context.SalidaProductos
                    .Include(s => s.Cliente)
                    .Include(s => s.Usuario)
                    .Include(s => s.Detalles).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(s => s.SalidaProductoId == nuevaSalida.SalidaProductoId);

                // 3. Dibujar el Ticket en PDF en memoria
                var pdfBytes = GenerarTicketPdf(salidaCompleta!);

                // 4. Lógica inteligente para definir a quién le mandamos el correo
                string emailDestinatario = salidaCompleta!.Usuario?.Email ?? ""; // Por defecto, al usuario logueado en la app

                // Si seleccionaron un cliente específico (que no sea el ID 1 genérico) y tiene email, lo usamos
                if (salidaCompleta.Cliente != null && salidaCompleta.Cliente.ClienteId != 1 && !string.IsNullOrWhiteSpace(salidaCompleta.Cliente.Email))
                {
                    emailDestinatario = salidaCompleta.Cliente.Email;
                }

                // NUEVO: Si desde el frontend web enviaron un correo a mano, sobrescribe el destino.
                if (!string.IsNullOrWhiteSpace(dto.EmailManual))
                {
                    emailDestinatario = dto.EmailManual;
                }

                // Si logramos obtener un email válido (y no es el ficticio del Consumidor Final)
                if (!string.IsNullOrWhiteSpace(emailDestinatario) && emailDestinatario != "cf@correo.com")
                {
                    // Definir a quién saludamos
                    string nombreDestino = salidaCompleta.Cliente?.ClienteId != 1
                                           ? salidaCompleta.Cliente!.Nombre
                                           : (salidaCompleta.Usuario?.Nombre ?? "Cliente");

                    // NUEVO AGREGADO: Si es Consumidor Final (ID 1) y enviaron un email a mano desde la Web
                    if (salidaCompleta.Cliente?.ClienteId == 1 && !string.IsNullOrWhiteSpace(dto.EmailManual))
                    {
                        nombreDestino = "Estimado/a Cliente";
                    }

                    string cuerpoHtml = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>¡Gracias por tu compra, {nombreDestino}!</h2>
                        <p>Tu pedido ha sido procesado exitosamente. Adjuntamos tu comprobante de compra.</p>
                        <p><strong>Total abonado: ${totalVenta:N2}</strong></p>
                        <hr/>
                        <p style='font-size: 12px; color: #7f8c8d;'>Comercio TP5 ITES - Sistema de Gestión</p>
                    </div>";

                    await _emailService.EnviarCorreoConPdfAsync(
                        destinatario: emailDestinatario,
                        asunto: $"Tu Comprobante de Compra #{salidaCompleta.SalidaProductoId} - Comercio TP5",
                        cuerpo: cuerpoHtml,
                        pdfBytes: pdfBytes,
                        nombreArchivo: $"Comprobante_{salidaCompleta.SalidaProductoId}.pdf"
                    );
                }

                return Ok(new { mensaje = "Venta registrada y comprobante enviado por email.", SalidaId = nuevaSalida.SalidaProductoId, TotalCobrado = totalVenta });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error registrando la salida.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EliminarSalida(int id)
        {
            try
            {
                var salida = await _context.SalidaProductos.Include(s => s.Detalles).FirstOrDefaultAsync(s => s.SalidaProductoId == id);
                if (salida == null) return NotFound(new { mensaje = "Salida no encontrada." });

                foreach (var detalle in salida.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null) producto.Stock += detalle.Cantidad;
                }

                _context.SalidaProductos.Remove(salida);
                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Salida eliminada y stock restaurado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error.", error = ex.Message });
            }
        }

        // --- PDF REDISEÑADO A FORMATO A5 ---
        private byte[] GenerarTicketPdf(SalidaProductos salida)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("COMERCIO TP5").SemiBold().FontSize(20).FontColor(Colors.BlueGrey.Darken2);
                        col.Item().AlignCenter().Text("Comprobante de Venta Electrónico").FontSize(11).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingVertical(10).Row(row =>
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
                            columns.ConstantColumn(35);  // Cantidad
                            columns.RelativeColumn();    // Detalle
                            columns.ConstantColumn(65);  // Precio Unitario
                            columns.ConstantColumn(75);  // Subtotal
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Cant").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Descripción").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("P. Unit").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Subtotal").SemiBold();
                        });

                        decimal totalVenta = 0;
                        foreach (var detalle in salida.Detalles)
                        {
                            var subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                            totalVenta += subtotal;

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(4).Text(detalle.Cantidad.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(4).Text(detalle.Producto?.Nombre ?? "Item");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"${detalle.PrecioUnitario:N2}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"${subtotal:N2}");
                        }

                        table.Cell().ColumnSpan(4).PaddingTop(15).AlignRight().Text("TOTAL A PAGAR:").SemiBold().FontSize(11);
                        table.Cell().ColumnSpan(4).AlignRight().Text($"${totalVenta:N2}").SemiBold().FontSize(16).FontColor(Colors.BlueGrey.Darken3);
                    });

                    page.Footer().AlignCenter().Text("¡Gracias por su compra!").Italic().FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}