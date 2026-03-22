

using Toolbelt.Blazor.SpeechSynthesis;

public class Global_AI_Functions
{
    public event Func<string, string, Task>? OnAISpeakRequested;
    public Task AISpeak(string text, string voice) => OnAISpeakRequested != null ? OnAISpeakRequested.Invoke(text, voice) : Task.CompletedTask;

    //public async Task StopSpeaking()
    //{
    //    await SpeechSynthesis.CancelAsync();
    //}
}

