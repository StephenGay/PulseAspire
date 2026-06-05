using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulse.Models.UI;

namespace Pulse.Web.Services
{
    public class PulseThemeService_v2
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        //private readonly IPulseToastService _pulseToastService;
        //private readonly Global_AI_Functions _globalAIFunctions;

        public PulseTheme? CurrentTheme { get; private set; }

        public event Action? OnThemeChanged;
        //public string CurrentCustomTheme { get; private set; } = "pulse";
        //public bool IsDarkMode { get; private set; } = false;
        public PulseThemeService_v2(IHttpClientFactory factory, IJSRuntime js) //, IPulseToastService pulseToastService, Global_AI_Functions globalAIFunctions)
        {
            _http = factory.CreateClient("PulseApiClient");
            _js = js;
            //_pulseToastService = pulseToastService;
            //_globalAIFunctions = globalAIFunctions;
            //_thMode = thMode;

        }

        public async Task LoadActiveThemeAsync()
        {
            try
            {
                CurrentTheme = await _http.GetFromJsonAsync<PulseTheme>("/ui/pulsethemes/active");
                if (CurrentTheme != null)
                    await ApplyThemeAsync(CurrentTheme);
            }
            catch { /* fallback to defaults */ }
        }

        public async Task ApplyThemeAsync(PulseTheme theme)
        {
            var vars = new Dictionary<string, string>
            {
                ["--primary-color"] = theme.PrimaryColor,
                ["--background-color"] = theme.BackgroundColor,
                ["--surface-color"] = theme.SurfaceColor,
                ["--text-primary"] = theme.TextPrimaryColor,
                ["--text-secondary"] = theme.TextSecondaryColor,
                ["--font-family-base"] = theme.FontFamily,
                ["--base-font-size"] = $"{theme.BaseFontSize}px",
                ["--base-border-radius"] = $"{theme.BaseBorderRadius}px",
                // map more...
            };

            if (theme.Settings?.CustomCssVariables != null)
                foreach (var kv in theme.Settings.CustomCssVariables)
                    vars[kv.Key] = kv.Value;

            await _js.InvokeVoidAsync("applyThemeVariables", vars);

            // Integrate with FluentDesignTheme if you expose Mode/OfficeColor
            // (use a cascading value or global state service)
            OnThemeChanged?.Invoke();
        }
    }
}
