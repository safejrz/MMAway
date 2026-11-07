namespace MMA.Input;

internal readonly record struct HotkeyGesture(uint Modifiers, uint Key)
{
    public static HotkeyGesture Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Global hotkey cannot be empty.");
        var parts = value.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) throw new ArgumentException("Use a modifier and key, for example Ctrl+Alt+M.");
        uint modifiers = 0;
        foreach (var part in parts[..^1])
        {
            var modifier = part.ToLowerInvariant() switch
            {
                "ctrl" or "control" => 0x0002u,
                "alt" => 0x0001u,
                "shift" => 0x0004u,
                "win" or "super" or "meta" => 0x0008u,
                _ => throw new ArgumentException($"Unsupported hotkey modifier '{part}'.")
            };
            if ((modifiers & modifier) != 0) throw new ArgumentException($"Duplicate hotkey modifier '{part}'.");
            modifiers |= modifier;
        }
        var key = parts[^1].ToUpperInvariant();
        uint virtualKey = key switch
        {
            "SPACE" => 0x20,
            "ENTER" => 0x0D,
            "ESC" => 0x1B,
            "TAB" => 0x09,
            _ when key.Length == 1 && char.IsAsciiLetterOrDigit(key[0]) => key[0],
            _ when key.Length is 2 or 3 && key[0] == 'F' && int.TryParse(key[1..], out var number) && number is >= 1 and <= 12 => (uint)(0x70 + number - 1),
            _ => throw new ArgumentException($"Unsupported hotkey key '{parts[^1]}'. Use A-Z, 0-9, F1-F12, Space, Enter, Esc, or Tab.")
        };
        return new HotkeyGesture(modifiers, virtualKey);
    }
}
