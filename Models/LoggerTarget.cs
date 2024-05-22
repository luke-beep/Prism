using System.ComponentModel;
using NLog;
using NLog.Targets;

namespace Prism.Models;

[Target("ListBoxTarget")]
public class LoggerTarget : TargetWithLayout
{
    public static ListBox? LogListBox { get; set; }
    private static SynchronizationContext? _syncContext;

    public LoggerTarget()
    {
        _syncContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
    }

    protected override void Write(LogEventInfo logEvent)
    {
        var logMessage = Layout.Render(logEvent);
        if (LogListBox == null || LogListBox.IsDisposed || !LogListBox.IsHandleCreated)
            return;

        try
        {
            _syncContext.Post(_ =>
            {
                if (LogListBox is { IsDisposed: false, IsHandleCreated: true })
                {
                    LogListBox.Items.Add(logMessage);
                }
            }, null);
        }
        catch (InvalidAsynchronousStateException) { }
    }

}