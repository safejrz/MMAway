namespace MMA.Input;

public interface IInputInjector : IDisposable
{
    Point GetCursorPosition();
    void SetCursorPosition(Point position);
    void SignalActivity();
    void LeftClick();
}

public interface IKeepAwake : IDisposable
{
    void Enable();
    void Disable();
}
