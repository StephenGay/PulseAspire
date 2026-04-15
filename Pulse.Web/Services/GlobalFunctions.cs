using DocumentFormat.OpenXml.Drawing.Charts;
using Markdig;
using Microsoft.JSInterop;
using Pulse.Models.Api;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Web.Services;

public class GlobalFunctions
{
    private readonly Global_AI_Functions _global_AI;
    private readonly IPulseToastService _pulseToastService;
    private readonly PulseApiService _apiService;
    private readonly AuthService _authService;
    private readonly MessageHubService _msgHubService;
    private readonly IJSRuntime _jsRuntime;

    public GlobalFunctions(
        Global_AI_Functions globalAI,
        IPulseToastService pulseToastService,
        PulseApiService apiService,
        AuthService authService,
        MessageHubService msgHubService,
        IJSRuntime jsRuntime)
    {
        _global_AI = globalAI;
        _pulseToastService = pulseToastService;
        _apiService = apiService;
        _authService = authService;
        _msgHubService = msgHubService;
        _jsRuntime = jsRuntime;
    }

    private async Task showError(string errMsg)
    {
        _pulseToastService.ShowToast("Error", errMsg, true);
        await _global_AI.AISpeak(errMsg, "PulseAI");
    }

    public string ParseToHtml(string markdownContent)
    {
        if (string.IsNullOrEmpty(markdownContent))
        {
            return string.Empty;
        }
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdig.Markdown.ToHtml(markdownContent, pipeline);

        html = System.Text.RegularExpressions.Regex.Replace(
            html,
            @"<a\s+href=""([^""]*)""",
            @"<a href=""$1"" target=""_blank"" rel=""noopener noreferrer"""
        );

        return html;
    }

    public async Task ExportToPdf(string html, string filename)
    {
        if (string.IsNullOrEmpty(html))
        {
            return;
        }
        if (string.IsNullOrEmpty(filename))
        {
            await showError("A filename is required.");
            return;
        }

        try
        {
            filename += ".pdf";
            html = ParseToHtml(html);
            html = AddExportCss(html);

            var request = new PdfRequest
            {
                Html = html,
                FileName = filename
            };

            var response = await _apiService.PostJsonAsync<PdfRequest>(ApiEndpoints.Utilities.ExportPdf, request);
            response.EnsureSuccessStatusCode();

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            var base64 = Convert.ToBase64String(pdfBytes);
            await _jsRuntime.InvokeVoidAsync("downloadPdfFile", filename, base64);
        }
        catch (Exception ex)
        {
            await showError("There was a problem exporting the PDF.<br />Please try again.");
            return;
        }
    }

    public async Task ExportToPulseMsg(PulseMessage msg)
    {
        if (string.IsNullOrEmpty(msg.Content))
        {
            return;
        }
        
        try
        {
            var mC = msg.Content;
            mC = ParseToHtml(mC);
            mC = AddInternalMarkUp(mC);

            PulseMessage msgExp = new PulseMessage()
            {
                SenderUserName = msg.SenderUserName ?? "Pulse AI",
                SenderUserId = msg.SenderUserId ?? "PulseAI",
                SentAt = msg.SentAt ?? DateTime.Now,

                RecipientUserId = msg.RecipientUserId ?? _authService.aspireUserId,
                RecipientUserName = msg.RecipientUserName ?? _authService.aspireFullName,
                Role = msg.Role ?? "PulseAI",
                ContentType = "MARKUP",
                Content = mC,
                Subject = msg.Subject ?? "Pulse Aspire Export"
            };
            await _msgHubService.SendPrivateMessageAsync(msgExp);
        }
        catch (Exception ex)
        {
            await showError("There was an error sending the message.<br />Please try again.");
            return;
        }
    }

    public async Task ExportHtmlFile(string html, string filename)
    {
        if (string.IsNullOrEmpty(html))
        {
            return;
        }
        if (string.IsNullOrEmpty(filename))
        {
            await showError("A filename is required.");
            return;
        }

        try
        {
            html = ParseToHtml(html);
            html = AddExportCss(html);
            await _jsRuntime.InvokeVoidAsync("downloadHTMLFile", $"{filename}.html", html.Replace("h5", "h3"));
        }
        catch (Exception ex)
        {
            await showError("There was a problem exporting the PDF.<br />Please try again.");
            return;
        }
    }

    private string AddInternalMarkUp(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;
        return html
            .Replace("<table>", "<table class=\"table table-striped table-hover table-bordered\">")
            .Replace("<th>", "<th style=\"background-color: var(--pulse-light-colour) ; color: var(--pulse-dark-colour); border: 1px solid var(--pulse-dark-colour);\">")
            .Replace("</table>", "</table><br />")
            .Replace("<hr />", "")
            .Replace("<h3", "<hr /><h3")
            .Replace("</table><br /><hr />", "</table><hr />")
            .Replace("h2", "h5")
         .Replace("h3", "h5");
    }

    private string AddExportCss(string html)
    {
        var css = @"
            <style>
                body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; }
                h1, h2, h3 { color: hsl(0,100%,20%); }
                table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
                th { background-color: hsl(0,100%,20%); color: white; padding: 8px; text-align: left; }
                td { border: 1px solid #ddd; padding: 8px; }
                tr:nth-child(even) { background-color: #f9f9f9; }
                tr:hover { background-color: #f1f1f1; }
            </style>";

        var htmlHeader = "<div style=\"display:flex;width: 100% ; padding: 8px; border-bottom: 1px solid hsl(0,100%,20%); justify-content: start; \">";
        htmlHeader += "<img src=\"https://hmtechnologies.org/Pulse/Aspire/Images/AI/Flapper.jpg\" style=\"width:50px;height:50px;margin-right:10px\"><h3>Conversation With Flapper</h3></div>";


        return css + htmlHeader + html;
        
    }
}

