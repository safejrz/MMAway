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
}
