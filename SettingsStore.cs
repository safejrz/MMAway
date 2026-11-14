using System.Text.Json;

namespace MMA;

public sealed class SettingsStore
{
    private const int CurrentSettingsVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string settingsPath;

    public bool ReplacedReservedGlobalHotkey { get; private set; }

    public SettingsStore(string? settingsPath = null)
    {
        this.settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MMAway", "settings.json");
    }

    public async Task<ActivitySettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        ReplacedReservedGlobalHotkey = false;
        FileStream stream;
        try
        {
            stream = File.OpenRead(settingsPath);
        }
        catch (FileNotFoundException)
        {
            return new ActivitySettings();
        }
        catch (DirectoryNotFoundException)
        {
            return new ActivitySettings();
        }
        ActivitySettings settings;
        await using (stream)
        {
            settings = await JsonSerializer.DeserializeAsync<ActivitySettings>(stream, JsonOptions, cancellationToken)
                ?? throw new InvalidDataException($"Settings file '{settingsPath}' is empty or invalid.");
        }
        var reservedHotkey = MMA.Input.HotkeyGesture.Parse(settings.GlobalHotkey) == MMA.Input.HotkeyGesture.Parse("Ctrl+S");
        if (reservedHotkey || (settings.SettingsVersion == 0 && settings.GlobalHotkey == "Ctrl+Alt+M"))
            settings = settings with { GlobalHotkey = new ActivitySettings().GlobalHotkey };
        settings.Validate();
        settings = settings with { SettingsVersion = Math.Max(settings.SettingsVersion, CurrentSettingsVersion) };
        if (reservedHotkey)
        {
            await SaveAsync(settings, cancellationToken);
            ReplacedReservedGlobalHotkey = true;
        }
        return settings;
    }

    public async Task SaveAsync(ActivitySettings settings, CancellationToken cancellationToken = default)
    {
        settings.Validate();
        settings = settings with { SettingsVersion = Math.Max(settings.SettingsVersion, CurrentSettingsVersion) };
        var directory = Path.GetDirectoryName(settingsPath)
            ?? throw new InvalidOperationException("Settings path must include a directory.");
        Directory.CreateDirectory(directory);
        var temporaryPath = settingsPath + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        File.Move(temporaryPath, settingsPath, overwrite: true);
    }
}
