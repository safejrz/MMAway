namespace MMA.Input;

public interface IInputInjector : IDisposable
{
    uint? UserInputVersion => null;
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
