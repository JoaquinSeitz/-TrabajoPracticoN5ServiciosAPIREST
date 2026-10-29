using System.Net;
using System.Net.Mail;

namespace tp5_Trani_Joaco_Alex.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoConPdfAsync(string destinatario, string asunto, string cuerpo, byte[] pdfBytes, string nombreArchivo);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger; // Agregamos un logger para registrar fallos silenciosos

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task EnviarCorreoConPdfAsync(string destinatario, string asunto, string cuerpo, byte[] pdfBytes, string nombreArchivo)
        {
            try
            {
                var smtpSettings = _config.GetSection("Smtp");
                var emailOrigen = smtpSettings["Email"];
                var password = smtpSettings["Password"];
                var host = smtpSettings["Host"];

                // Le damos un valor por defecto (587) por si alguna vez falta en el appsettings.json
                var port = int.Parse(smtpSettings["Port"] ?? "587");

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(emailOrigen, password),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailOrigen!, "Comercio TP5 ITES"),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(destinatario);

                // Adjuntamos el PDF directamente desde la memoria
                using var ms = new MemoryStream(pdfBytes);
                mailMessage.Attachments.Add(new Attachment(ms, nombreArchivo, "application/pdf"));

                await client.SendMailAsync(mailMessage);

                // Registramos en consola que todo salió bien
                _logger.LogInformation($"Correo enviado exitosamente a {destinatario}");
            }
            catch (Exception ex)
            {
                // Si el correo falla, lo registramos en la consola del servidor pero NO cortamos el flujo de la aplicación.
                // Así evitamos que una venta exitosa devuelva error solo porque falló el SMTP.
                _logger.LogError($"Error al enviar el correo a {destinatario}: {ex.Message}");
            }
        }
    }
}