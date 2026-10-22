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

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoConPdfAsync(string destinatario, string asunto, string cuerpo, byte[] pdfBytes, string nombreArchivo)
        {
            var smtpSettings = _config.GetSection("Smtp");
            var emailOrigen = smtpSettings["Email"];
            var password = smtpSettings["Password"];
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"]!);

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
        }
    }
}