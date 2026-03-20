using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Sync.Extensions
{
    internal class SimpleConsoleFormatter : ConsoleFormatter
    {
        public SimpleConsoleFormatter()
            : base("SimpleConsoleFormatter")
        { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            textWriter.WriteLine($"[{logEntry.LogLevel.ToString()[..3].ToUpper()}] {logEntry.State}");
        }
    }
}
