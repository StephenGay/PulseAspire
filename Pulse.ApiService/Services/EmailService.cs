using System.Net;
using System.Net.Mail;
using System.Net.Security;
using Microsoft.Extensions.Options;
using Pulse.Models.CustomComponents;

namespace Pulse.ApiService.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody);
    Task<bool> SendPasswordResetAsync(string userEmail, string userName, string resetLink);
    Task<bool> SendWelcomeAsync(string userEmail, string userName, string loginUrl);
    Task<bool> SendReportAsync(string recipientEmail, string reportName, byte[] reportContent, string contentType = "application/pdf");
    Task<bool> SendReportReadyNotificationAsync(string recipientEmail, string reportName, string downloadUrl);
    Task<bool> SendAccountLockedNotificationAsync(string userEmail, string userName);
    Task<bool> SendPasswordChangedNotificationAsync(string userEmail, string userName);
    Task<bool> SendNotificationAsync(string recipientEmail, string subject, string message, string? actionUrl = null);
}

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailConfiguration> emailOptions,
        IEmailTemplateService templateService,
        ILogger<EmailService> logger)
    {
        _emailConfig = emailOptions.Value;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody)
    {
        try
        {
            if (string.IsNullOrEmpty(_emailConfig.SmtpServer))
            {
                _logger.LogWarning("SMTP server not configured. Email not sent to {RecipientEmail}", recipientEmail);
                return false;
            }

            using var client = new SmtpClient()
            {
                Host = _emailConfig.SmtpServer,
                Port = _emailConfig.SmtpPort,
                EnableSsl = _emailConfig.EnableSsl,
                Timeout = 15000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword)
            };

            // Configure SSL/TLS certificate validation if needed
            if (_emailConfig.EnableSsl)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback =
                    (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                     System.Security.Cryptography.X509Certificates.X509Chain chain,
                     SslPolicyErrors sslPolicyErrors) => true;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };

            mailMessage.To.Add(new MailAddress(recipientEmail, recipientName));

            _logger.LogDebug("Sending email to {RecipientEmail} via {SmtpServer}:{SmtpPort}",
                recipientEmail, _emailConfig.SmtpServer, _emailConfig.SmtpPort);

            await client.SendMailAsync(mailMessage);
            mailMessage.Dispose();

            _logger.LogInformation("Email sent successfully to {RecipientEmail} with subject '{Subject}'", recipientEmail, subject);
            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {RecipientEmail}. Status: {StatusCode}, Message: {Message}",
                recipientEmail, ex.StatusCode, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {RecipientEmail} with subject '{Subject}'", recipientEmail, subject);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetAsync(string userEmail, string userName, string resetLink)
    {
        var subject = "Reset Your Password";
        var htmlBody = _templateService.GetPasswordResetTemplate(userName, resetLink);
        return await SendEmailAsync(userEmail, userName, subject, htmlBody);
    }

    public async Task<bool> SendWelcomeAsync(string userEmail, string userName, string loginUrl)
    {
        var subject = "Welcome to Pulse!";
        var htmlBody = _templateService.GetWelcomeTemplate(userName, loginUrl);
        return await SendEmailAsync(userEmail, userName, subject, htmlBody);
    }

    public async Task<bool> SendReportAsync(string recipientEmail, string reportName, byte[] reportContent, string contentType = "application/pdf")
    {
        try
        {
            if (string.IsNullOrEmpty(_emailConfig.SmtpServer))
            {
                _logger.LogWarning("SMTP server not configured. Report email not sent to {RecipientEmail}", recipientEmail);
                return false;
            }

            using var client = new SmtpClient()
            {
                Host = _emailConfig.SmtpServer,
                Port = _emailConfig.SmtpPort,
                EnableSsl = _emailConfig.EnableSsl,
                Timeout = 15000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword)
            };

            if (_emailConfig.EnableSsl)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            }

            var subject = $"Report: {reportName}";
            var htmlBody = _templateService.GetGenericNotificationTemplate(
                "Your Report",
                $"Please find your requested report attached: {reportName}");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };

            mailMessage.To.Add(recipientEmail);

            var fileExtension = contentType == "application/pdf" ? ".pdf" : ".xlsx";
            var attachment = new Attachment(new MemoryStream(reportContent), reportName + fileExtension, contentType);
            mailMessage.Attachments.Add(attachment);

            await client.SendMailAsync(mailMessage);
            mailMessage.Dispose();

            _logger.LogInformation("Report email sent successfully to {RecipientEmail}", recipientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send report to {RecipientEmail}", recipientEmail);
            return false;
        }
    }

    public async Task<bool> SendReportReadyNotificationAsync(string recipientEmail, string reportName, string downloadUrl)
    {
        var subject = "Your Report is Ready";
        var htmlBody = _templateService.GetReportReadyTemplate(reportName, downloadUrl);
        return await SendEmailAsync(recipientEmail, string.Empty, subject, htmlBody);
    }

    public async Task<bool> SendAccountLockedNotificationAsync(string userEmail, string userName)
    {
        var subject = "Account Security Alert";
        var htmlBody = _templateService.GetAccountLockedTemplate(userName);
        return await SendEmailAsync(userEmail, userName, subject, htmlBody);
    }

    public async Task<bool> SendPasswordChangedNotificationAsync(string userEmail, string userName)
    {
        var subject = "Password Changed Successfully";
        var htmlBody = _templateService.GetPasswordChangedTemplate(userName);
        return await SendEmailAsync(userEmail, userName, subject, htmlBody);
    }

    public async Task<bool> SendNotificationAsync(string recipientEmail, string subject, string message, string? actionUrl = null)
    {
        var htmlBody = _templateService.GetGenericNotificationTemplate(subject, message, actionUrl);
        return await SendEmailAsync(recipientEmail, string.Empty, subject, htmlBody);
    }
}