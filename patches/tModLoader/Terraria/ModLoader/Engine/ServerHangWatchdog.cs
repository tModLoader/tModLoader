using log4net.Core;
using System;
using System.Diagnostics;
using System.Threading;

namespace Terraria.ModLoader.Engine;

internal static class ServerHangWatchdog
{
	public static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(10);

	private static volatile Ref<DateTime> lastCheckin;
	internal static void Checkin()
	{
		if (Debugger.IsAttached) return;
		bool started = lastCheckin != null;
		lastCheckin = new Ref<DateTime>(DateTime.Now);
		if (!started)
			Start();
	}

	private static void Start()
	{
		var mainThread = Thread.CurrentThread;
		new Thread(() => Run(mainThread)) {
			Name = "Server Hang Watchdog",
			IsBackground = true
		}.Start();
	}

	private static void Run(Thread mainThread)
	{
		while (true) {
			Thread.Sleep(1000);
			if (DateTime.Now - lastCheckin.Value > TIMEOUT) {
				// TODO. https://github.com/dotnet/runtime/issues/31508
#if NETCORE
				Logging.ServerConsoleLine("Server hung for more than 10 seconds. Cannot determine cause from watchdog thread", Level.Warn, log: Logging.tML);
				Checkin();
				continue;
#elif WINDOWS
				//Stacktrace.cs: [MonoLimitation ("Not possible to create StackTraces from other threads")]
				Logging.ServerConsoleLine("Server hung for more than 10 seconds. Cannot determine cause on Mono", Level.Warn, log: Logging.tML);
				Checkin();
				continue;
#else
				var st = GetStackTrace(mainThread);
				Logging.PrettifyStackTraceSources(st.GetFrames());
				Logging.ServerConsoleLine("Server hung for more than 10 seconds:\n" + st, Level.Error, log: Logging.tML);
				mainThread.Abort();
				return;
#endif
			}
		}
	}

#if !NETCORE
	//https://stackoverflow.com/questions/285031/how-to-get-non-current-threads-stacktrace

#pragma warning disable CS0618 // Type or member is obsolete
	private static StackTrace GetStackTrace(Thread targetThread)
	{
		using (ManualResetEvent fallbackThreadReady = new ManualResetEvent(false), exitedSafely = new ManualResetEvent(false)) {
			Thread fallbackThread = new Thread(delegate () {
				fallbackThreadReady.Set();
				while (!exitedSafely.WaitOne(200)) {
					try {
						targetThread.Resume();
					}
					catch (Exception) {/*Whatever happens, do never stop to resume the target-thread regularly until the main-thread has exited safely.*/}
				}
			});
			fallbackThread.Name = "GetStackFallbackThread";
			try {
				fallbackThread.Start();
				fallbackThreadReady.WaitOne();
				//From here, you have about 200ms to get the stack-trace.
				targetThread.Suspend();
				StackTrace trace = null;
				try {
					trace = new StackTrace(targetThread, true);
				}
				catch (ThreadStateException) {
					//failed to get stack trace, since the fallback-thread resumed the thread
					//possible reasons:
					//1.) This thread was just too slow (not very likely)
					//2.) The deadlock occurred and the fallbackThread rescued the situation.
					//In both cases just return null.
				}
				try {
					targetThread.Resume();
				}
				catch (ThreadStateException) {/*Thread is running again already*/}
				return trace;
			}
			finally {
				//Just signal the backup-thread to stop.
				exitedSafely.Set();
				//Join the thread to avoid disposing "exited safely" too early. And also make sure that no leftover threads are cluttering iis by accident.
				fallbackThread.Join();
			}
		}
	}
#endif
}