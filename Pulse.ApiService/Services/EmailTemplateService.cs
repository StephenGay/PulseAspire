namespace Pulse.ApiService.Services;

/// <summary>
/// Service for generating HTML email templates
/// </summary>
public interface IEmailTemplateService
{
    string GetPasswordResetTemplate(string userName, string resetLink, int expirationHours = 24);
    string GetWelcomeTemplate(string userName, string loginUrl);
    string GetReportReadyTemplate(string reportName, string downloadUrl);
    string GetAccountLockedTemplate(string userName);
    string GetPasswordChangedTemplate(string userName);
    string GetGenericNotificationTemplate(string title, string message, string? actionUrl = null, string? actionText = null);
}

public class EmailTemplateService : IEmailTemplateService
{
    private const string BaseStyles = @"
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #f9f9f9;
            padding: 20px;
        }}
        .header {{
            background-color: #2c3e50;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 4px 4px 0 0;
        }}
        .content {{
            background-color: white;
            padding: 30px;
            border-radius: 0 0 4px 4px;
        }}
        .button {{
            display: inline-block;
            background-color: #3498db;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            margin: 20px 0;
            font-weight: bold;
        }}
        .button:hover {{
            background-color: #2980b9;
        }}
        .footer {{
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #ecf0f1;
            color: #95a5a6;
            font-size: 12px;
            text-align: center;
        }}
        .highlight {{
            background-color: #fffacd;
            padding: 10px;
            border-left: 4px solid #ffc107;
            margin: 15px 0;
        }}
        .warning {{
            background-color: #ffe6e6;
            padding: 10px;
            border-left: 4px solid #dc3545;
            margin: 15px 0;
            color: #721c24;
        }}
    ";

    public string GetPasswordResetTemplate(string userName, string resetLink, int expirationHours = 24)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>{BaseStyles}</style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔐 Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{HtmlEncode(userName)}</strong>,</p>
                        <p>We received a request to reset your password for your Pulse account.</p>
                        
                        <div style='text-align: center;'>
                            <a href='{HtmlEncode(resetLink)}' class='button'>Reset Your Password</a>
                        </div>

                        <p>Or copy and paste this link in your browser:</p>
                        <p style='word-break: break-all; background-color: #f5f5f5; padding: 10px; border-radius: 4px;'>
                            {HtmlEncode(resetLink)}
                        </p>

                        <div class='highlight'>
                            <strong>⏱️ Link Expiration:</strong> This link will expire in {expirationHours} hours.
                        </div>

                        <div class='warning'>
                            <strong>⚠️ Security Note:</strong> If you did not request this password reset, please ignore this email or contact support immediately.
                        </div>

                        <p>For security reasons, we never share passwords via email.</p>

                        <div class='footer'>
                            <p>Pulse Aspire | Automated Security Message</p>
                            <p>If you have questions, contact our support team.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
    }

    public string GetWelcomeTemplate(string userName, string loginUrl)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>{BaseStyles}</style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>👋 Welcome to Pulse Aspire!</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{HtmlEncode(userName)}</strong>,</p>
                        <p>Your account has been successfully created. Welcome to Pulse Aspire!</p>

                        <div style='text-align: center;'>
                            <a href='{HtmlEncode(loginUrl)}' class='button'>Log In to Your Account</a>
                        </div>

                        <h3>What's Next?</h3>
                        <ul>
                            <li>Complete your profile information</li>
                            <li>Set up your preferences</li>
                            <li>Explore the dashboard</li>
                            <li>Check out available reports</li>
                        </ul>

                        <h3>Account Security</h3>
                        <p>We recommend:</p>
                        <ul>
                            <li>Using a strong, unique password</li>
                            <li>Enabling two-factor authentication (if available)</li>
                            <li>Keeping your password confidential</li>
                        </ul>

                        <div class='footer'>
                            <p>Pulse Aspire | Welcome Email</p>
                            <p>If you did not create this account, please contact support.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
    }

    public string GetReportReadyTemplate(string reportName, string downloadUrl)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>{BaseStyles}</style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>📊 Your Report is Ready</h1>
                    </div>
                    <div class='content'>
                        <p>Your requested report <strong>{HtmlEncode(reportName)}</strong> has been generated and is ready for download.</p>

                        <div style='text-align: center;'>
                            <a href='{HtmlEncode(downloadUrl)}' class='button'>Download Report</a>
                        </div>

                        <p><strong>Report Details:</strong></p>
                        <ul>
                            <li>Report: {HtmlEncode(reportName)}</li>
                            <li>Generated: {DateTime.Now:g}</li>
                            <li>Expires: {DateTime.Now.AddDays(7):g}</li>
                        </ul>

                        <div class='highlight'>
                            <strong>📌 Note:</strong> This download link will expire in 7 days.
                        </div>

                        <div class='footer'>
                            <p>Pulse Aspire | Report Notification</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
    }

    public string GetAccountLockedTemplate(string userName)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>{BaseStyles}</style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔒 Account Security Alert</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{HtmlEncode(userName)}</strong>,</p>
                        <p>Your account has been temporarily locked due to multiple failed login attempts.</p>

                        <div class='warning'>
                            <strong>⚠️ What happened?</strong> We detected several unsuccessful login attempts. To protect your account, it has been temporarily locked.
                        </div>

                        <h3>What can you do?</h3>
                        <ul>
                            <li>Wait 30 minutes for the lock to be automatically lifted</li>
                            <li>Reset your password to regain access immediately</li>
                            <li>Contact support if you believe this is an error</li>
                        </ul>

                        <div class='footer'>
                            <p>Pulse Aspire | Security Alert</p>
                            <p>If you have questions, contact our security team immediately.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
    }

    public string GetPasswordChangedTemplate(string userName)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>{BaseStyles}</style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>✅ Password Changed Successfully</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{HtmlEncode(userName)}</strong>,</p>
                        <p>Your password has been successfully changed.</p>

                        <div class='highlight'>
                            <strong>✓ Confirmation:</strong> Your password was changed on {DateTime.Now:g}
                        </div>

                        <p>If you did not make this change, please reset your password immediately:</p>
                        <div style='text-align: center;'>
                            <a href='https://yourapp.com/forgot-password' class='button'>Reset Password Now</a>
                        </div>

                        <div class='footer'>
                            <p>Pulse Aspire | Confirmation Email</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
    }

    public string GetGenericNotificationTemplate(string title, string message, string? actionUrl = null, string? actionText = null)
    {
        var actionButton = string.IsNullOrEmpty(actionUrl) ? string.Empty : 
            $@"<div style='text-align: center;'>
                <a href='{HtmlEncode(actionUrl)}' class='button'>{HtmlEncode(actionText ?? "Take Action")}</a>
            </div>";

        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>{BaseStyles}</style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>ℹ️ {HtmlEncode(title)}</h1>
                    </div>
                    <div class='content'>
                        <p>{HtmlEncode(message)}</p>
                        {actionButton}
                        <div class='footer'>
                            <p>Pulse Aspire | Notification</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
    }

    private static string HtmlEncode(string text)
    {
        return System.Net.WebUtility.HtmlEncode(text);
    }
}