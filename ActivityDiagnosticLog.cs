using System.ComponentModel;
using System.Text;

namespace MMA;

internal sealed class ActivityDiagnosticLog
{
    private const long MaximumFileBytes = 1_000_000;
    private readonly string path;
    private readonly long maximumFileBytes;
    private readonly object gate = new();

    public ActivityDiagnosticLog(string? path = null, long maximumFileBytes = MaximumFileBytes)
    {
        this.path = path ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MMAway", "activity-diagnostics.log");
        this.maximumFileBytes = maximumFileBytes;
    }

    public string Path => path;

    public void Write(string eventName, Exception? error)
    {
        var entry = new StringBuilder();
        entry.AppendLine($"[{DateTimeOffset.Now:O}] {eventName}");
        entry.AppendLine($"MMAway={typeof(ActivityEngine).Assembly.GetName().Version}; OS={Environment.OSVersion}; remoteSession={SystemInformation.TerminalServerSession}");
        if (error is not null)
        {
            if (error is Win32Exception nativeError) entry.AppendLine($"Windows error code: {nativeError.NativeErrorCode}");
            entry.AppendLine(error.ToString());
            foreach (System.Collections.DictionaryEntry detail in error.Data)
                entry.AppendLine($"{detail.Key}: {detail.Value}");
        }
        entry.AppendLine();
        var text = entry.ToString();
        lock (gate)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            if (File.Exists(path) && new FileInfo(path).Length + Encoding.UTF8.GetByteCount(text) > maximumFileBytes)
                File.Move(path, path + ".1", overwrite: true);
            File.AppendAllText(path, text, Encoding.UTF8);
        }
    }
}
