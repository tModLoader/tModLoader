using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
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

	private static string LogDirectory => Path.Combine(Environment.CurrentDirectory, "logs");

	private static string CurrentLog => Path.Combine(LogDirectory, EnvironmentLogName + ".log");

	private static string DatedLog => Path.Combine(LogDirectory, $"{EnvironmentLogName}-{DateTime.UtcNow:yyyyMMddHHmmss}.log");

	private static string EnvironmentLogName => Main.dedServ ? "server" : "client";

	public static void Initialize(ILoggingBuilder logging)
	{
		logging.ClearProviders();

		logging.SetMinimumLevel(LogLevel.Debug);

		logging.AddProvider(new FileLoggerProvider(LogLevel.Debug));
		logging.AddSimpleConsole(options => {
			options.IncludeScopes = true;
			options.SingleLine = true;
			options.TimestampFormat = "HH:mm:ss ";
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
