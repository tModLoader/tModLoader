using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Terraria.Utilities;

namespace Terraria.Unified.Startup;

internal static class Logging
{
	private sealed class FileLogger : ILogger
	{
		private readonly string category;
		private readonly FileLoggerProvider provider;

		public FileLogger(string category, FileLoggerProvider provider)
		{
			this.category = category;
			this.provider = provider;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= provider.MinLevel;
		}

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel)) {
				return;
			}

			var sb = new StringBuilder(512);
			sb.Append(DateTime.UtcNow.ToString("O"));
			sb.Append(" [");
			sb.Append(logLevel);
			sb.Append("] ");
			sb.Append(category);
			sb.Append(": ");
			sb.Append(formatter(state, exception));

			if (exception != null) {
				sb.AppendLine();
				sb.Append(exception);
			}

			provider.Enqueue(sb.ToString());
		}
	}

	private sealed class FileLoggerProvider : ILoggerProvider
	{
		private readonly BlockingCollection<string> queue = [];
		private readonly Thread worker;

		public LogLevel MinLevel { get; }

		public FileLoggerProvider(LogLevel minLevel)
		{
			MinLevel = minLevel;

			Directory.CreateDirectory(LogDirectory);

			File.WriteAllText(CurrentLog, "");
			File.WriteAllText(DatedLog, "");

			worker = new Thread(WriterLoop) {
				IsBackground = true,
				Name = "FileLogger",
			};
			worker.Start();
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new FileLogger(categoryName, this);
		}

		public void Enqueue(string message)
		{
			queue.Add(message);
		}

		private void WriterLoop()
		{
			using var current = new StreamWriter(
				new FileStream(CurrentLog, FileMode.Append, FileAccess.Write, FileShare.Read),
				Encoding.UTF8
			);

			using var dated = new StreamWriter(
				new FileStream(DatedLog, FileMode.Append, FileAccess.Write, FileShare.Read),
				Encoding.UTF8
			);

			foreach (var msg in queue.GetConsumingEnumerable()) {
				current.WriteLine(msg);
				dated.WriteLine(msg);
				current.Flush();
				dated.Flush();
			}
		}

		public void Dispose()
		{
			queue.CompleteAdding();
			worker.Join();
		}
	}

	private sealed class LoggerTextWriter : TextWriter
	{
		private readonly ILogger logger;
		private readonly LogLevel level;

		public LoggerTextWriter(ILogger logger, LogLevel level)
		{
			this.logger = logger;
			this.level = level;
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void WriteLine(string value)
		{
			if (string.IsNullOrEmpty(value)) {
				return;
			}

			logger.Log(level, value);
		}

		public override void Write(char value) { }
	}

	// Taken from SimpleConsoleFormatter.
	private sealed class CustomConsoleFormatter : ConsoleFormatter, IDisposable
	{
		private const string LoglevelPadding = ": ";
		private static readonly string _messagePadding = new string(' ', GetLogLevelString(LogLevel.Information).Length + LoglevelPadding.Length);
		private static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

		private readonly IDisposable _optionsReloadToken;

		public CustomConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
			: base("custom")
		{
			ReloadLoggerOptions(options.CurrentValue);
			_optionsReloadToken = options.OnChange(ReloadLoggerOptions);
		}

		[MemberNotNull(nameof(FormatterOptions))]
		private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options)
		{
			FormatterOptions = options;
		}

		public void Dispose()
		{
			_optionsReloadToken?.Dispose();
		}

		internal SimpleConsoleFormatterOptions FormatterOptions { get; set; }

		public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
		{
			if (logEntry.State is BufferedLogRecord bufferedRecord) {
				string message = bufferedRecord.FormattedMessage ?? string.Empty;
				WriteInternal(null, textWriter, message, bufferedRecord.LogLevel, bufferedRecord.EventId.Id, bufferedRecord.Exception, logEntry.Category, bufferedRecord.Timestamp);
			}
			else {
				string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
				if (logEntry.Exception == null && message == null) {
					return;
				}

				// We extract most of the work into a non-generic method to save code size. If this was left in the generic
				// method, we'd get generic specialization for all TState parameters, but that's unnecessary.
				WriteInternal(scopeProvider, textWriter, message, logEntry.LogLevel, logEntry.EventId.Id, logEntry.Exception?.ToString(), logEntry.Category, GetCurrentDateTime());
			}
		}

		private void WriteInternal(IExternalScopeProvider? scopeProvider, TextWriter textWriter, string message, LogLevel logLevel,
			int eventId, string? exception, string category, DateTimeOffset stamp)
		{
			ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel);
			string logLevelString = GetLogLevelString(logLevel);

			string? timestamp = null;
			string? timestampFormat = FormatterOptions.TimestampFormat;
			if (timestampFormat != null) {
				timestamp = stamp.ToString(timestampFormat);
			}
			if (timestamp != null) {
				textWriter.Write(timestamp);
			}
			if (logLevelString != null) {
				textWriter.WriteColoredMessage(logLevelString, logLevelColors.Background, logLevelColors.Foreground);
			}

			bool singleLine = FormatterOptions.SingleLine;

			// Example:
			// info: ConsoleApp.Program[10]
			//       Request received

			// category and event id
			textWriter.Write(LoglevelPadding);
			textWriter.Write(AbbreviateCategory(category));

			/*
			textWriter.Write('[');

			Span<char> span = stackalloc char[10];
			if (eventId.TryFormat(span, out int charsWritten))
				textWriter.Write(span.Slice(0, charsWritten));
			else
				textWriter.Write(eventId.ToString());

			textWriter.Write(']');
			*/

			if (!singleLine) {
				textWriter.Write(Environment.NewLine);
			}

			// scope information
			WriteScopeInformation(textWriter, scopeProvider, singleLine);
			WriteMessage(textWriter, message, singleLine);

			// Example:
			// System.InvalidOperationException
			//    at Namespace.Class.Function() in File:line X
			if (exception != null) {
				// exception message
				WriteMessage(textWriter, exception, singleLine);
			}
			if (singleLine) {
				textWriter.Write(Environment.NewLine);
			}
		}

		private static string AbbreviateCategory(string category)
		{
			if (string.IsNullOrEmpty(category)) {
				return category;
			}

			var parts = category.Split('.');

			if (parts.Length == 1) {
				return category;
			}

			var sb = new StringBuilder(category.Length);

			for (int i = 0; i < parts.Length; i++) {
				if (i > 0) {
					sb.Append('.');
				}

				bool isLast = i == parts.Length - 1;
				bool isSecondLast = i == parts.Length - 2;

				if (isLast || isSecondLast) {
					sb.Append(parts[i]);
				}
				else {
					// Abbreviate to first letter
					sb.Append(parts[i][0]);
				}
			}

			return sb.ToString();
		}

		private static void WriteMessage(TextWriter textWriter, string message, bool singleLine)
		{
			if (!string.IsNullOrEmpty(message)) {
				if (singleLine) {
					textWriter.Write(' ');
					WriteReplacing(textWriter, Environment.NewLine, " ", message);
				}
				else {
					textWriter.Write(_messagePadding);
					WriteReplacing(textWriter, Environment.NewLine, _newLineWithMessagePadding, message);
					textWriter.Write(Environment.NewLine);
				}
			}

			static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
			{
				string newMessage = message.Replace(oldValue, newValue);
				writer.Write(newMessage);
			}
		}

		private DateTimeOffset GetCurrentDateTime()
		{
			return FormatterOptions.TimestampFormat != null
				? (FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
				: DateTimeOffset.MinValue;
		}

		private static string GetLogLevelString(LogLevel logLevel)
		{
			return logLevel switch {
				LogLevel.Trace => "trce",
				LogLevel.Debug => "dbug",
				LogLevel.Information => "info",
				LogLevel.Warning => "warn",
				LogLevel.Error => "fail",
				LogLevel.Critical => "crit",
				_ => throw new ArgumentOutOfRangeException(nameof(logLevel))
			};
		}

		private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
		{
			// We shouldn't be outputting color codes for Android/Apple mobile platforms,
			// they have no shell (adb shell is not meant for running apps) and all the output gets redirected to some log file.
			bool disableColors = (FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled) ||
				(FormatterOptions.ColorBehavior == LoggerColorBehavior.Default && !ConsoleUtils.EmitAnsiColorCodes);
			if (disableColors) {
				return new ConsoleColors(null, null);
			}
			// We must explicitly set the background color if we are setting the foreground color,
			// since just setting one can look bad on the users console.
			return logLevel switch {
				LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
				LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
				LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
				LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
				LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
				LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
				_ => new ConsoleColors(null, null)
			};
		}

		private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
		{
			if (FormatterOptions.IncludeScopes && scopeProvider != null) {
				bool paddingNeeded = !singleLine;
				scopeProvider.ForEachScope((scope, state) => {
					if (paddingNeeded) {
						paddingNeeded = false;
						state.Write(_messagePadding);
						state.Write("=> ");
					}
					else {
						state.Write(" => ");
					}
					state.Write(scope);
				}, textWriter);

				if (!paddingNeeded && !singleLine) {
					textWriter.Write(Environment.NewLine);
				}
			}
		}

		private readonly struct ConsoleColors
		{
			public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
			{
				Foreground = foreground;
				Background = background;
			}

			public ConsoleColor? Foreground { get; }

			public ConsoleColor? Background { get; }
		}
	}

	private static string LogDirectory => Path.Combine(Environment.CurrentDirectory, "logs");

	private static string CurrentLog => Path.Combine(LogDirectory, EnvironmentLogName + ".log");

	private static string DatedLog => Path.Combine(LogDirectory, $"{EnvironmentLogName}-{DateTime.UtcNow:yyyyMMddHHmmss}.log");

	private static string EnvironmentLogName => Main.dedServ ? "server" : "client";

	public static void Initialize(ILoggingBuilder logging)
	{
		logging.ClearProviders();

		logging.SetMinimumLevel(LogLevel.Debug);

		logging.AddProvider(new FileLoggerProvider(LogLevel.Debug));

		logging.AddConsoleFormatter<CustomConsoleFormatter, SimpleConsoleFormatterOptions>(options => {
			options.IncludeScopes = true;
			options.SingleLine = true;
			options.TimestampFormat = "HH:mm:ss ";
		});
		logging.AddConsole(options => {
			options.FormatterName = "custom";
		});

		CrashWatcher.Inititialize();
		CrashWatcher.DumpOnException = GameLaunch.LaunchParameters.ContainsKey("-minidump");
		CrashWatcher.LogAllExceptions = GameLaunch.LaunchParameters.ContainsKey("-logerrors");

		if (GameLaunch.LaunchParameters.ContainsKey("-fulldump")) {
			CrashWatcher.EnableCrashDumps(CrashDump.Options.WithFullMemory);
		}
	}

	public static void RedirectConsole(ILoggerFactory factory)
	{
		Console.SetOut(new LoggerTextWriter(factory.CreateLogger("stdout"), LogLevel.Information));
		Console.SetError(new LoggerTextWriter(factory.CreateLogger("stderr"), LogLevel.Error));
	}
}
