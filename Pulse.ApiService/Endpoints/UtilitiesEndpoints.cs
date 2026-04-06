
using Pulse.ApiService.Services;
using Pulse.Models.Api;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using PuppeteerSharp;
using PuppeteerSharp.Media;


namespace Pulse.ApiService.Endpoints
{
    
    internal static class UtilitiesEndpoints
    {
        internal const string BasePath = "/Utilities";
        public static void MapUtilitiesEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Utilities");

            group.MapGet("/health", () =>
            {
                return Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
            })
            .WithName("HealthCheck")
            .Produces(StatusCodes.Status200OK);

            group.MapPost("/SendEmail", async (EMailMessage msg, IEmailService emailService) =>
            {
                try
                {
                    bool res = await emailService.SendEmailAsync(msg.ToEMailAddress, msg.ToEMailName, msg.Subject, msg.HtmlBody);
                    if (res)
                    {
                        return Results.Ok(new ApiResponse
                        {
                            Success = true,
                            Message = "EMail sent successfully",
                            StatusCode = 200
                        });
                    }
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
                catch (Exception ex)
                {
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            });

            group.MapPost("/pdf/export", async (PdfRequest request, CancellationToken ct) =>
            {
                try
                {
                    // Step 1: Download Chrome (cached after first run)
                    await new BrowserFetcher().DownloadAsync(); // Auto-downloads latest Chrome

                    // Step 2: Launch browser (auto-detects downloaded Chrome)
                    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true
                        // No ExecutablePath needed — PuppeteerSharp finds it
                    },null);

                    await using var page = await browser.NewPageAsync();
                    await page.SetContentAsync(request.Html);

                    // Step 3: Generate PDF
                    var pdfBytes = await page.PdfDataAsync(new PdfOptions
                    {
                        Format = PaperFormat.A4,
                        PrintBackground = true,
                        MarginOptions = new MarginOptions
                        {
                            Top = "1cm",
                            Bottom = "1cm",
                            Left = "1cm",
                            Right = "1cm"
                        }
                    });

                    return Results.File(pdfBytes, "application/pdf", $"{request.FileName}.pdf");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"PDF generation failed: {ex.Message}");
                }
            });
        }


    }
}
