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
using tp5_Trani_Joaco_Alex.Services; // Importamos el servicio de correo

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalidaProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Inyectamos la dependencia

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

                // 4. Mandar el mail si el cliente tiene un correo cargado
                if (salidaCompleta!.Cliente != null && !string.IsNullOrWhiteSpace(salidaCompleta.Cliente.Email))
                {
                    string cuerpoHtml = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2 style='color: #2c3e50;'>¡Gracias por tu compra, {salidaCompleta.Cliente.Nombre}!</h2>
                            <p>Tu pedido ha sido procesado exitosamente. Adjuntamos tu ticket de comprobante.</p>
                            <p><strong>Total abonado: ${totalVenta:N2}</strong></p>
                            <hr/>
                            <p style='font-size: 12px; color: #7f8c8d;'>Comercio TP5 ITES - Sistema de Gestión</p>
                        </div>";

                    await _emailService.EnviarCorreoConPdfAsync(
                        destinatario: salidaCompleta.Cliente.Email,
                        asunto: $"Tu Ticket de Compra #{salidaCompleta.SalidaProductoId} - Comercio TP5",
                        cuerpo: cuerpoHtml,
                        pdfBytes: pdfBytes,
                        nombreArchivo: $"Ticket_{salidaCompleta.SalidaProductoId}.pdf"
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

        // Método privado para generar el PDF (mismo formato que en ReportesController)
        private byte[] GenerarTicketPdf(SalidaProductos salida)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(5, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().AlignCenter().Column(col =>
                    {
                        col.Item().Text("COMERCIO TP5").SemiBold().FontSize(14);
                        col.Item().Text("Ticket de Venta").FontSize(10);
                        col.Item().Text($"Nro: {salida.SalidaProductoId:D6}");
                        col.Item().PaddingBottom(5, Unit.Millimetre).Text($"Fecha: {salida.Fecha:dd/MM/yyyy HH:mm}");
                        col.Item().LineHorizontal(1).LineColor(Colors.Black);
                        col.Item().AlignLeft().Text($"Cliente: {salida.Cliente?.Nombre ?? "Consumidor Final"}");
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
            return document.GeneratePdf();
        }
    }
}