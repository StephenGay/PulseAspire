using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Pulse.Web.Services;

public interface IHtmlCaptureService
{
    Task<string> GetInnerHtmlAsync(ElementReference element);
    Task<string> GetOuterHtmlAsync(ElementReference element);
    Task<string> CaptureForExportAsync(ElementReference element);
}

public class HtmlCaptureService : IHtmlCaptureService
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public HtmlCaptureService(IJSRuntime js)
    {
        _js = js;
    }

    private async Task EnsureModuleAsync()
    {
        if (_module is null)
        {
            _module = await _js.InvokeAsync<IJSObjectReference>(
                "import", "./js/html-capture.js");
        }
    }

    public async Task<string> GetInnerHtmlAsync(ElementReference element)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<string>("getInnerHtml", element);
    }

    public async Task<string> GetOuterHtmlAsync(ElementReference element)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<string>("getOuterHtml", element);
    }

    /// <summary>
    /// Captures HTML + automatically replaces all graphs with high-quality PNG images.
    /// Ideal for export files (PDF, Word, etc.)
    /// </summary>
    public async Task<string> CaptureForExportAsync(ElementReference element)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<string>("captureForExport", element);
    }
}