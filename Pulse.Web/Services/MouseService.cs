using Microsoft.AspNetCore.Components.Web;

public interface IMouseService
{
    event EventHandler<PointerEventArgs>? OnMove;
    event EventHandler<PointerEventArgs>? OnUp;
    void FireMove(PointerEventArgs e);
    void FireUp(PointerEventArgs e);
}

public class MouseService : IMouseService
{
    public event EventHandler<PointerEventArgs>? OnMove;
    public event EventHandler<PointerEventArgs>? OnUp;

    public void FireMove(PointerEventArgs e) => OnMove?.Invoke(this, e);
    public void FireUp(PointerEventArgs e) => OnUp?.Invoke(this, e);
}