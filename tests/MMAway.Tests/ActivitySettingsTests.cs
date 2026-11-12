using MMA;

namespace MMAway.Tests;

public sealed class ActivitySettingsTests
{
    [Fact]
    public void DefaultsValidateAndMatchReference()
    {
        var settings = new ActivitySettings();
        settings.Validate();
        Assert.Equal(10, settings.UserIdleSeconds);
        Assert.False(settings.ShowSessionInfo);
        Assert.Equal(1.5, settings.Aggressiveness);
        Assert.Equal(10, settings.SelectedPatterns.Count);
        Assert.Equal("Ctrl+Alt+Shift+M", settings.GlobalHotkey);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3601)]
    public void InvalidIdleThresholdIsRejected(int seconds) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => (new ActivitySettings { UserIdleSeconds = seconds }).Validate());

    [Fact]
    public async Task SettingsRoundTrip()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mmaway-test-{Guid.NewGuid():N}.json");
        try
        {
            var store = new SettingsStore(path);
            await store.SaveAsync(new ActivitySettings { UserIdleSeconds = 12, ShowSessionInfo = true });
            var loaded = await store.LoadAsync();
            Assert.Equal(12, loaded.UserIdleSeconds);
            Assert.True(loaded.ShowSessionInfo);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Ctrl+Alt+M", "Ctrl+Alt+Shift+M")]
    [InlineData("Ctrl+F10", "Ctrl+F10")]
    public async Task LegacySettingsMigrateOnlyFormerDefault(string shortcut, string expected)
    {
        var path = Path.Combine(Path.GetTempPath(), $"mmaway-test-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path, $"{{\"GlobalHotkey\":\"{shortcut}\",\"UserIdleSeconds\":12}}");
            var store = new SettingsStore(path);
            var loaded = await store.LoadAsync();
            Assert.Equal(expected, loaded.GlobalHotkey);
            Assert.Equal(12, loaded.UserIdleSeconds);
            await store.SaveAsync(loaded with { GlobalHotkey = "Ctrl+Alt+M" });
            Assert.Equal("Ctrl+Alt+M", (await store.LoadAsync()).GlobalHotkey);
        }
        finally { File.Delete(path); }
    }
}
