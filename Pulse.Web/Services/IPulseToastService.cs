using Microsoft.AspNetCore.Components;

namespace Pulse.Web.Services;

public interface IPulseToastService
{
    event Action<PulseToastMessage>? OnShow;

    void ShowSuccess(string message, string AiEmoji  = "PulseAI", string? title = null, int autoHideMs = 5000);
    void ShowInformation(string message, string AiEmoji  = "PulseAI", string? title = null, int autoHideMs = 5000);
    void ShowWarning(string message, string AiEmoji  = "PulseAI", string? title = null, int autoHideMs = 8000);
    void ShowError(string message, string AiEmoji  = "PulseAI", string? title = null, int autoHideMs = 8000);
    void Show(PulseToastMessage message);
}

public class PulseToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public ToastLevel Level { get; set; }
    public string? Title { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AiEmoji { get; set; } = "PulseAI";
    public int AutoHideMs { get; set; } = 5000000;
    public DateTime Created { get; } = DateTime.UtcNow;
}

public enum ToastLevel
{
    Success,
    Information,
    Warning,
    Error
}

