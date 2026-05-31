using DocumentFormat.OpenXml.Drawing.Charts;
using Markdig;
using Microsoft.JSInterop;
using Pulse.Models.AI.Tools;
using Pulse.Models.Api;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Models.Users;
using Pulse.Web.Components.Pages.EmployeeZone.Customers.Dialogs;
using Pulse.Web.Services;
using Radzen;
using StackExchange.Redis;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GlobalFunctions
{
    private readonly Global_AI_Functions _global_AI;
    private readonly IPulseToastService _pulseToastService;
    private readonly PulseApiService _apiService;
    private readonly AuthService _authService;
    private readonly MessageHubService _msgHubService;
    private readonly IJSRuntime _jsRuntime;
    private readonly DataTransferService _dTrf;
    private readonly Radzen.DialogService _dialogService;
    private string? dialogSettingsName;

    public GlobalFunctions(
        Global_AI_Functions globalAI,
        IPulseToastService pulseToastService,
        PulseApiService apiService,
        AuthService authService,
        MessageHubService msgHubService,
        IJSRuntime jsRuntime,
        Radzen.DialogService dialogService,
        DataTransferService dTrf)
    {
        _global_AI = globalAI;
        _pulseToastService = pulseToastService;
        _apiService = apiService;
        _authService = authService;
        _dTrf = dTrf;
        _msgHubService = msgHubService;
        _jsRuntime = jsRuntime;
        _dialogService = dialogService;
    }

    private async Task showError(string errMsg)
    {
        _pulseToastService.ShowToast("Error", errMsg, true);
        await _global_AI.AISpeak(errMsg, "PulseAI");
    }
    private void showStaticError(string errMsg)
    {
        _pulseToastService.ShowToast("Error", errMsg, true);
        _global_AI.AISpeak(errMsg, "PulseAI");
    }
    public async Task SetUserSpeedDial()
    {
        bool isSuccess = false;
        try
        {
            var speedDialResponse = await _apiService.GetAsync<ApiResponse<List<ApplicationUserSpeedDial>>>(ApiEndpoints.User.SpeedDial.GetByUserId(_authService.aspireUserId));
            if (speedDialResponse != null && speedDialResponse.Success && speedDialResponse.Data != null)
            {
                var speedDials = speedDialResponse.Data;
                await _dTrf.SetgvUserSpeedDial(speedDials);
                isSuccess = true;
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
        }
        finally
        {
            if (!isSuccess)
            {
                await _dTrf.SetgvUserSpeedDial(new List<ApplicationUserSpeedDial>());
                await showError("There was a problem loading your speed dial.<br />Please refresh the page.");
            }
        }
    }

    public ChartConfig DeserialiseChartConfigString(string jsonString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return new ChartConfig();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var config = JsonSerializer.Deserialize<ChartConfig>(jsonString, options);

            return config ?? new ChartConfig();
        }
        catch (JsonException ex)
        {
            showError($"Invalid JSON format: {ex.Message}");
            return new ChartConfig();
        }
        catch (Exception ex)
        {
            showError($"Error creating chart: {ex.Message}");
            return new ChartConfig();
        }
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

    #region Dialog Functions

    public async Task<string> OpenClientSearch(Radzen.DialogService dialogService)
    {
        dialogSettingsName = "ClientSearchDialogSettings";
        //var result = await _jsRuntime.InvokeAsync<string>("window.localStorage.getItem", dialogSettingsName);
        //if (!string.IsNullOrEmpty(result))
        //{
        //    _settings = JsonSerializer.Deserialize<DialogSettings>(result);
        //}
        await LoadStateAsync();
        var result =  await dialogService.OpenAsync<ClientSearchDialog>(title:"Client Search", parameters: new Dictionary<string, object?>() { { "FullClientID", "" } }, options:
               new DialogOptions()
               {
                   Resizable = true,
                   Draggable = true,
                   Resize = OnResize,
                   Drag = OnDrag,
                   Icon = "search",
                   ShowClose = true,
                   CloseDialogOnEsc = false,
                   CloseDialogOnOverlayClick = true,
                   Width = Settings != null ? Settings.Width : "700px",
                   Height = Settings != null ? Settings.Height : "512px",
                   Left = Settings != null ? Settings.Left : null,
                   Top = Settings != null ? Settings.Top : null
               });
        await SaveStateAsync();
        return result;
        //await _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", dialogSettingsName, JsonSerializer.Serialize<DialogSettings>(Settings));
    }

    void OnDrag(System.Drawing.Point point)
    {
        _jsRuntime.InvokeVoidAsync("eval", $"console.log('Dialog drag. Left:{point.X}, Top:{point.Y}')");

        if (Settings == null)
        {
            Settings = new DialogSettings();
        }

        Settings.Left = $"{point.X}px";
        Settings.Top = $"{point.Y}px";
        
        _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", dialogSettingsName, JsonSerializer.Serialize<DialogSettings>(Settings));
    }

    void OnResize(System.Drawing.Size size)
    {
        _jsRuntime.InvokeVoidAsync("eval", $"console.log('Dialog resize. Width:{size.Width}, Height:{size.Height}')");

        if (Settings == null)
        {
            Settings = new DialogSettings();
        }

        Settings.Width = $"{size.Width}px";
        Settings.Height = $"{size.Height}px";
        
        _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", dialogSettingsName, JsonSerializer.Serialize<DialogSettings>(Settings));
    }

    DialogSettings? _settings;
    public DialogSettings Settings
    {
        get
        {
            return _settings;
        }
        set
        {
            if (_settings != value)
            {
                _settings = value;
                _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", dialogSettingsName, JsonSerializer.Serialize<DialogSettings>(Settings));
            }
        }
    }

    private async Task LoadStateAsync()
    {
        await Task.CompletedTask;

        var result = await _jsRuntime.InvokeAsync<string>("window.localStorage.getItem", dialogSettingsName);
        if (!string.IsNullOrEmpty(result))
        {
            _settings = JsonSerializer.Deserialize<DialogSettings>(result);
        }
    }

    private async Task SaveStateAsync()
    {
        await Task.CompletedTask;

        await _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", dialogSettingsName, JsonSerializer.Serialize<DialogSettings>(Settings));
    }

    public class DialogSettings
    {
        public string Left { get; set; }
        public string Top { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    #endregion

    #region Colour Converter

    // Source - https://stackoverflow.com/a/69295742
    // Posted by Mehmet Erdoğdu
    // Retrieved 2026-05-24, License - CC BY-SA 4.0

    public string RgbaToHex(string value)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
                return null;
            Color color;
            value = value.Trim();
            if (value.StartsWith("#"))
                color = ColorTranslator.FromHtml(value);
            else
            {
                if (!value.StartsWith("rgba"))
                {
                    if (value.StartsWith("rgb"))
                    {
                        value = value.Replace("rgb", "rgba").Replace(")", ",100)");
                    }
                    else
                    {
                        showError("I could not convert the colour to Hexadecimal.<br />The value supplied was not a rgba string.");
                        return "#00000000";
                    }
                }
                
                
                var left = value.IndexOf('(');
                var right = value.IndexOf(')');
                if (left < 0 || right < 0)
                {
                    showError("There was an error converting the colour to Hexadecimal.<br />I am returning the colour black.");
                    return "#00000000";
                }
                var noBrackets = value.Substring(left + 1, right - left - 1);
                var parts = noBrackets.Split(',');
                var r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                var g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                var b = int.Parse(parts[2], CultureInfo.InvariantCulture);
                switch (parts.Length)
                {
                    case 3:
                        color = Color.FromArgb(r, g, b);
                        break;
                    case 4:
                        {
                            var a = float.Parse(parts[3], CultureInfo.InvariantCulture);
                            color = Color.FromArgb((int)(a * 255), r, g, b);
                            break;
                        }
                    default:
                        showError("I could not convert the colour to Hexadecimal.<br />The value supplied was not a rgba string.");
                        return "#00000000";
                }
            }

            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
        }
        catch (Exception ex)
        {
            showError($"There was an error converting the colour to Hexadecimal.<br />I will return the default colour black.");
            return "#00000000";
        }
    }

    public string HexToRgba(string value)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
            {
                showError("I could not convert the colour to RGBA.<br />No value was supplied, I will return the colour black.");
                return $"rgba(0, 0, 0, 1)";
            }

            var fixedHex = value.Replace("#", "");
            bool hasAlpha = fixedHex.Length == 8;
            if (hasAlpha)
            {
                fixedHex = value.Substring(value.Length - 2);
                fixedHex += value.Substring(1, 6);
            }
            var argb = int.Parse(fixedHex, NumberStyles.HexNumber);
            var clr = Color.FromArgb(argb);
            int r = Convert.ToInt16(clr.R);
            int g = Convert.ToInt16(clr.G);
            int b = Convert.ToInt16(clr.B);
            int a = Convert.ToInt16(clr.A);
            var trans = hasAlpha ? Math.Round((double)a / 255, 2).ToString("N2").Replace(",", ".") : "1";
            return $"rgba({r}, {g}, {b}, {trans})";
        }
        catch (Exception ex)
        {
            showError($"There was an error converting the colour to RGBA.<br />I will return the default colour black.");
            return $"rgba(0, 0, 0, 1)";
        }
    }


    #endregion
}

