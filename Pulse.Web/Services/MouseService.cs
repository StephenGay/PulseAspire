using Microsoft.AspNetCore.Components.Web;

public interface IMouseService
{
    event EventHandler<MouseEventArgs>? OnMove;
    event EventHandler<MouseEventArgs>? OnUp;
    void FireMove(MouseEventArgs e);
    void FireUp(MouseEventArgs e);
}

public class MouseService : IMouseService
{
    public event EventHandler<MouseEventArgs>? OnMove;
    public event EventHandler<MouseEventArgs>? OnUp;

    public void FireMove(MouseEventArgs e) => OnMove?.Invoke(this, e);
    public void FireUp(MouseEventArgs e) => OnUp?.Invoke(this, e);
}