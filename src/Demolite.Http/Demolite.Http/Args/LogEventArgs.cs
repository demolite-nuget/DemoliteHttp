using Demolite.Http.Enum;

namespace Demolite.Http.Args;

public class LogEventArgs(string message, LogEventLevel level = LogEventLevel.Debug) : EventArgs
{
	public LogEventLevel Level { get; set; } = level;

	public string Message { get; set; } = message;
}