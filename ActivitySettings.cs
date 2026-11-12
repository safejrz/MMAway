using MMA.Input;

namespace MMA;

public sealed record ActivitySettings
{
    public int IntervalMinimumSeconds { get; init; } = 4;
    public int IntervalMaximumSeconds { get; init; } = 14;
    public int TypingIntervalMinimumSeconds { get; init; } = 2;
    public int TypingIntervalMaximumSeconds { get; init; } = 12;
    public int TypingBurstMinimumCharacters { get; init; } = 2;
    public int TypingBurstMaximumCharacters { get; init; } = 20;
    public bool TypingSpacesEnabled { get; init; } = true;
    public int SpaceMinimumCharacters { get; init; }
    public int SpaceMaximumCharacters { get; init; } = 6;
    public List<string> SelectedPatterns { get; init; } = [.. Enum.GetNames<MovementPattern>()];
    public bool TypingEnabled { get; init; } = true;
    public double Aggressiveness { get; init; } = 1.5;
    public int UserIdleSeconds { get; init; } = 10;
    public bool ShowSessionInfo { get; init; }
    public int AutoStopMinutes { get; init; }
    public string GlobalHotkey { get; init; } = "Ctrl+Alt+Shift+M";
    public int SettingsVersion { get; init; }

    public void Validate()
    {
        ValidateRange(IntervalMinimumSeconds, IntervalMaximumSeconds, 1, 300, "Activity interval");
        ValidateRange(TypingIntervalMinimumSeconds, TypingIntervalMaximumSeconds, 1, 300, "Typing interval");
        ValidateRange(TypingBurstMinimumCharacters, TypingBurstMaximumCharacters, 1, 100, "Typing burst size");
        ValidateRange(SpaceMinimumCharacters, SpaceMaximumCharacters, 0, 100, "Characters between spaces");
        if (SelectedPatterns is null || SelectedPatterns.Count == 0)
            throw new ArgumentException("Select at least one movement pattern.");
        if (SelectedPatterns.Distinct(StringComparer.Ordinal).Count() != SelectedPatterns.Count ||
            SelectedPatterns.Any(pattern => !Enum.GetNames<MovementPattern>().Contains(pattern, StringComparer.Ordinal)))
            throw new ArgumentException("Movement patterns must be unique supported pattern names.");
        if (!double.IsFinite(Aggressiveness) || Aggressiveness is < 0.5 or > 3.0)
            throw new ArgumentOutOfRangeException(nameof(Aggressiveness), "Movement speed must be between 0.5x and 3.0x.");
        if (UserIdleSeconds is < 1 or > 3600)
            throw new ArgumentOutOfRangeException(nameof(UserIdleSeconds), "Resume delay must be between 1 and 3600 seconds.");
        if (AutoStopMinutes is < 0 or > 1440)
            throw new ArgumentOutOfRangeException(nameof(AutoStopMinutes), "Auto-stop must be between 0 and 1440 minutes.");
        _ = HotkeyGesture.Parse(GlobalHotkey);
    }

    private static void ValidateRange(int minimum, int maximum, int lower, int upper, string name)
    {
        if (minimum < lower || minimum > upper || maximum < minimum || maximum > upper)
            throw new ArgumentException($"{name} must be between {lower} and {upper}, with maximum at least minimum.");
    }
}
