using System.Collections.Concurrent;

namespace Pulse.Web.Services;

public class PulseToastService : IPulseToastService
{
    
    private readonly ConcurrentQueue<PulseToastMessage> _messages = new();

    public event Action<PulseToastMessage>? OnShow;
    public event Action<Guid>? OnRemove;

    public void ShowSuccess(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 5000)
        => Show(new PulseToastMessage { Level = ToastLevel.Success, AiEmoji = aiEmoji, Message = message, Title = title ?? "Success", AutoHideMs = autoHideMs });

    public void ShowInformation(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 5000)
        => Show(new PulseToastMessage { Level = ToastLevel.Information, AiEmoji = aiEmoji, Message = message, Title = title ?? "Information", AutoHideMs = autoHideMs });

    public void ShowWarning(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 8000)
        => Show(new PulseToastMessage { Level = ToastLevel.Warning, AiEmoji = aiEmoji, Message = message, Title = title ?? "Warning", AutoHideMs = autoHideMs });

    public void ShowError(string message, string aiEmoji  = "PulseAI", string? title = null, int autoHideMs = 8000)
        => Show(new PulseToastMessage { Level = ToastLevel.Error, AiEmoji = aiEmoji, Message = message, Title = title ?? "Error", AutoHideMs = autoHideMs });
    
    public void ShowToast(string tType, string message, bool isHtml = false, bool autoHide = true, string AiEmoji = "PulseAI", string? title = null)
    {
        
        ToastLevel level = ToastLevel.Information; // default
        int defaultAutoHideMs = 5000;

        switch(tType.ToLower())
        {
            case "success":
                level = ToastLevel.Success;
                break;
            case "information":
                level = ToastLevel.Information;
                break;
            case "warning":
                level = ToastLevel.Warning;
                defaultAutoHideMs = 8000;
                break;
            case "error":
                level = ToastLevel.Error;
                defaultAutoHideMs = 8000;
                break;
        }
        if (!autoHide) { defaultAutoHideMs = int.MaxValue; }

        var pToastMessage = new PulseToastMessage
        {
            Level = level,
            AiEmoji = AiEmoji,
            Message = message,
            Title = title ?? tType,
            IsHtml = isHtml,
            AutoHideMs = defaultAutoHideMs
        };

        Show(pToastMessage);
    }

    public void Show(PulseToastMessage message)
    {
        _messages.Enqueue(message);
        OnShow?.Invoke(message);
    }

    // Optional: method to remove by ID if needed
    public void Remove(Guid id)
    {
        OnRemove?.Invoke(id);
    }
}