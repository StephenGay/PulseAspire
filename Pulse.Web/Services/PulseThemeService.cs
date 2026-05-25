using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Pulse.Web.Services
{
    public class PulseThemeService
    {
        private readonly IJSRuntime _js;
        private readonly IPulseToastService _pulseToastService;
        private readonly Global_AI_Functions _globalAIFunctions;
        

        public string CurrentCustomTheme { get; private set; } = "pulse";
        public bool IsDarkMode { get; private set; } = false;
        public PulseThemeService(IJSRuntime js, IPulseToastService pulseToastService, Global_AI_Functions globalAIFunctions)
        {
            _js = js;
            _pulseToastService = pulseToastService;
            _globalAIFunctions = globalAIFunctions;
            //_thMode = thMode;

        }

        public async Task SetThemeAsync(string themeName)
        {
            bool isSet = false;
            IsDarkMode = themeName.EndsWith("-dark");
            CurrentCustomTheme = themeName.Replace("-dark", "");
            

            try
            {
                await _js.InvokeVoidAsync("applyPulseTheme", themeName);
                isSet = true;
            }
            catch (Exception ex)
            {
                isSet = false;
            }
            finally
            {
                if (!isSet)
                {
                    var errMsg = "There was an error applying your theme.<br />Please try again.";
                    _pulseToastService.ShowToast("Error", errMsg, true);
                    await _globalAIFunctions.AISpeak(errMsg, "PulseAI");
                }
            }
            
        }

        //public async Task<string?> GetCurrentThemeAsync()
        //{
        //    // You can expand this later if needed
        //    return await Task.FromResult(localStorage.GetItem("pulse-theme"));
        //}
    }
}
