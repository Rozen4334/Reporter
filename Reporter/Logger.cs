namespace Reporter;

public class Logger
{
    private readonly string _filePath;

    private readonly Dictionary<LogSeverity, Func<LogMessage, bool?, LogEntry>> _callback = new();

    public Logger()
    {
        _filePath = Path.Combine(Config.Settings.SavePath, "Logs", string.Format("{0:yy-MM-dd_HH-mm-ss}.log", DateTime.UtcNow));

        _callback[LogSeverity.Debug] = Debug;
        _callback[LogSeverity.Verbose] = Verbose;
        _callback[LogSeverity.Info] = Info;
        _callback[LogSeverity.Warning] = Warning;
        _callback[LogSeverity.Error] = Error;
        _callback[LogSeverity.Critical] = Critical;
    }

    private class LogEntry
    {
        public LogMessage Message { get; }

        public bool WriteConsole { get; }

        public ConsoleColor Color { get; }

        public LogEntry(LogMessage message, bool writeConsole, ConsoleColor color)
        {
            Message = message;
            WriteConsole = writeConsole;
            Color = color;
        }
    }

    #region EntryConstruction
    private LogEntry Debug(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? false, ConsoleColor.DarkGray);

    private LogEntry Verbose(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? false, ConsoleColor.Gray);

    private LogEntry Info(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.Yellow);

    private LogEntry Warning(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.DarkYellow);

    private LogEntry Error(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.Red);

    private LogEntry Critical(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.DarkRed);
    #endregion

    public void Log(LogMessage message, bool? writeConsole = null)
        => Task.Run(async () => await LogAsync(message, writeConsole));

    public void Log(object message, string? source = null, LogSeverity severity = LogSeverity.Info, Exception? exception = null, bool? writeConsole = null)
        => Task.Run(async () => await LogAsync(new LogMessage(severity, source ?? "Gateway", message.ToString(), exception), writeConsole));

    public async Task LogAsync(object message, string? source = null, LogSeverity severity = LogSeverity.Info, Exception? exception = null, bool? writeConsole = null)
        => await LogAsync(new LogMessage(severity, source ?? "Gateway", message.ToString(), exception), writeConsole);

    public async Task LogAsync(LogMessage message, bool? writeConsole = null)
    {
        if (_callback.TryGetValue(message.Severity, out var value))
            await Execute(value(message, writeConsole));
    }

#pragma warning disable
    private async Task Execute(LogEntry entry)
    {
        if (entry.WriteConsole)
        {
            Console.ForegroundColor = entry.Color;
            Console.WriteLine(entry.Message);
            Console.ResetColor();
        }
        using StreamWriter sw = File.AppendText(_filePath);
        sw.WriteLine(entry.Message.ToString());
    }
#pragma warning restore
}

