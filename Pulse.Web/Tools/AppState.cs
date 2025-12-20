
    // AppState.cs
    using System;
    using System.Threading.Tasks;

    public class AppState
    {
        public event Func<Task>? OnChange;

        public async Task NotifyStateChanged()
        {
            if (OnChange != null)
                await OnChange.Invoke();
        }
    }


