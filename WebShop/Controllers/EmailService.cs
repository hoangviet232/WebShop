/*using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendOrderConfirmationEmail(string recipientEmail, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var smtpClient = new SmtpClient
        {
            Host = emailSettings["SmtpServer"],
            Port = int.Parse(emailSettings["Port"]),
            Credentials = new NetworkCredential(emailSettings["UserName"], emailSettings["Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailSettings["SenderEmail"], emailSettings["SenderName"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(recipientEmail);

        smtpClient.Send(mailMessage);
    }
}*/