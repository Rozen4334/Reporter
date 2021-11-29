namespace Reporter;

public class Logger
{
    // File path
    private readonly string _path;

    // Delegate to return log entries
    private readonly Dictionary<LogSeverity, Func<LogMessage, bool?, LogEntry>> _callback = new();

    /// <summary>
    /// Ctor
    /// </summary>
    public Logger()
    {
        // Set filepath
        _path = Path.Combine(Config.Settings.SavePath, "logs", string.Format("{0:yy-MM-dd_HH-mm-ss}.log", DateTime.UtcNow));

        // Configure delegate
        _callback[LogSeverity.Debug] = Debug;
        _callback[LogSeverity.Verbose] = Verbose;
        _callback[LogSeverity.Info] = Info;
        _callback[LogSeverity.Warning] = Warning;
        _callback[LogSeverity.Error] = Error;
        _callback[LogSeverity.Critical] = Critical;
    }

    private class LogEntry
    {
        /// <summary>
        /// The logmessage struct
        /// </summary>
        public LogMessage Message { get; }

        /// <summary>
        /// To write to console or not
        /// </summary>
        public bool WriteConsole { get; }

        /// <summary>
        /// The color of this entry
        /// </summary>
        public ConsoleColor Color { get; }

        /// <summary>
        /// Creates a new logentry from the parameters provided.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="writeConsole"></param>
        /// <param name="color"></param>
        public LogEntry(LogMessage message, bool writeConsole, ConsoleColor color)
        {
            Message = message;
            WriteConsole = writeConsole;
            Color = color;
        }
    }

    #region EntryConstruction

    // Gets debug logentry
    private LogEntry Debug(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? false, ConsoleColor.DarkGray);

    // Gets verbose logentry
    private LogEntry Verbose(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? false, ConsoleColor.Gray);

    // Gets info logentry
    private LogEntry Info(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.Yellow);

    // Gets warning logentry
    private LogEntry Warning(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.DarkYellow);

    // Gets error logentry
    private LogEntry Error(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.Red);

    // Gets critical logentry
    private LogEntry Critical(LogMessage message, bool? writeConsole)
        => new(message, writeConsole ?? true, ConsoleColor.DarkRed);

    #endregion

    /// <summary>
    /// Logs a message as void, disposes a task to run it async
    /// </summary>
    /// <param name="message"></param>
    /// <param name="writeConsole"></param>
    public void Log(LogMessage message, bool? writeConsole = null)
        => _ = Task.Run(async () => await LogAsync(message, writeConsole));

    /// <summary>
    /// Logs a message as void, disposes a task to run it async
    /// </summary>
    /// <param name="message"></param>
    /// <param name="source"></param>
    /// <param name="severity"></param>
    /// <param name="exception"></param>
    /// <param name="writeConsole"></param>
    public void Log<T>(object message, string? source = null, LogSeverity severity = LogSeverity.Info, Exception? exception = null, bool? writeConsole = null) where T : class
        => _ = Task.Run(async () => await LogAsync(new LogMessage(severity, source ?? "Gateway", message.ToString(), exception), writeConsole));

    /// <summary>
    /// Logs a message as async, can be awaited
    /// </summary>
    /// <param name="message"></param>
    /// <param name="source"></param>
    /// <param name="severity"></param>
    /// <param name="exception"></param>
    /// <param name="writeConsole"></param>
    /// <returns></returns>
    public async Task LogAsync(object message, string? source = null, LogSeverity severity = LogSeverity.Info, Exception? exception = null, bool? writeConsole = null)
        => await LogAsync(new LogMessage(severity, source ?? "Gateway", message.ToString(), exception), writeConsole);

    /// <summary>
    /// Logs a message as async, can be awaited
    /// </summary>
    /// <param name="message"></param>
    /// <param name="writeConsole"></param>
    /// <returns></returns>
    public async Task LogAsync(LogMessage message, bool? writeConsole = null)
    {
        if (_callback.TryGetValue(message.Severity, out var value))
            await Execute(value(message, writeConsole));
    }

#pragma warning disable
    // executes the message, logs to file & console.
    private async Task Execute(LogEntry entry)
    {
        if (entry.WriteConsole)
        {
            Console.ForegroundColor = entry.Color;
            Console.WriteLine(entry.Message);
        }
        using StreamWriter sw = File.AppendText(_path);
        sw.WriteLine(entry.Message.ToString());
    }
#pragma warning restore
}

