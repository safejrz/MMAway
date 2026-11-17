using System.ComponentModel;
using MMA;
using MMA.Input;

namespace MMAway.Tests;

public sealed class ActivityDiagnosticLogTests
{
    [Fact]
    public void LogPersistsErrorDetailsAndRotates()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"mmaway-diagnostic-{Guid.NewGuid():N}.log");
        try
        {
            var log = new ActivityDiagnosticLog(path, maximumFileBytes: 1);
            var error = new Win32Exception(5, "Cursor failed");
            error.Data["Desktop context"] = "thread desktop=Default; input desktop=Winlogon";
            log.Write("Cursor failed", error);
            var first = File.ReadAllText(path);
            Assert.Contains("Cursor failed", first);
            Assert.Contains("Windows error code: 5", first);
            Assert.Contains("thread desktop=Default; input desktop=Winlogon", first);
            Assert.Contains("remoteSession=", first);
            Assert.Contains("MMAway=", first);
            log.Write("Cursor movement recovered", null);
            Assert.Equal(first, File.ReadAllText(path + ".1"));
            Assert.Contains("Cursor movement recovered", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".1");
        }
    }

    [Fact]
    public void LogWriteFailureIsNotSuppressed()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"mmaway-missing-{Guid.NewGuid():N}.log");
        File.WriteAllText(path, "not a directory");
        try
        {
            var log = new ActivityDiagnosticLog(System.IO.Path.Combine(path, "activity.log"));
            Assert.ThrowsAny<IOException>(() => log.Write("failure", null));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DesktopSnapshotProvidesBothDesktopResults()
    {
        var snapshot = DesktopDiagnostics.Capture();
        Assert.Contains("thread desktop=", snapshot);
        Assert.Contains("input desktop", snapshot);
    }
}
