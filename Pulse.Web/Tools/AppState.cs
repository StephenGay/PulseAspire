
    // AppState.cs
    using System;
    using System.Threading.Tasks;

public class AppState
{
    public event Func<Task>? OnChange;
    public event Func<Task>? OnThemeChange;

    public async Task NotifyStateChanged()
    {
        if (OnChange != null)
            await OnChange.Invoke();
    }
    public async Task NotifyThemeChanged()
    {
        if (OnThemeChange != null)
            await OnThemeChange.Invoke();
    }
}


