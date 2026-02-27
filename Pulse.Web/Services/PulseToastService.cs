using System.Collections.Concurrent;

namespace Pulse.Web.Services;

public class PulseToastService : IPulseToastService
{
    private readonly ConcurrentQueue<PulseToastMessage> _messages = new();

    public event Action<PulseToastMessage>? OnShow;

    public void ShowSuccess(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 5000)
        => Show(new PulseToastMessage { Level = ToastLevel.Success, AiEmoji = aiEmoji, Message = message, Title = title ?? "Success", AutoHideMs = autoHideMs });

    public void ShowInfo(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 5000)
        => Show(new PulseToastMessage { Level = ToastLevel.Info, AiEmoji = aiEmoji, Message = message, Title = title ?? "Information", AutoHideMs = autoHideMs });

    public void ShowWarning(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 8000)
        => Show(new PulseToastMessage { Level = ToastLevel.Warning, AiEmoji = aiEmoji, Message = message, Title = title ?? "Warning", AutoHideMs = autoHideMs });

    public void ShowError(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 8000)
        => Show(new PulseToastMessage { Level = ToastLevel.Error, AiEmoji = aiEmoji, Message = message, Title = title ?? "Error", AutoHideMs = autoHideMs });

    public void Show(PulseToastMessage message)
    {
        _messages.Enqueue(message);
        OnShow?.Invoke(message);
    }

    // Optional: method to remove by ID if needed
    public void Remove(Guid id) { /* implement if you want manual dismiss tracking */ }
}