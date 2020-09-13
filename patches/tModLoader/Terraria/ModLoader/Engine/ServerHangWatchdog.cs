using log4net.Core;
using System;
using System.Diagnostics;
using System.Threading;

namespace Terraria.ModLoader.Engine
{
	internal static class ServerHangWatchdog
	{
		public static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(10);

		private static volatile Ref<DateTime> lastCheckin;
		internal static void Checkin() {
			if (Debugger.IsAttached) return;
			bool started = lastCheckin != null;
			lastCheckin = new Ref<DateTime>(DateTime.Now);
			if (!started)
				Start();
		}

		private static void Start() {
			var mainThread = Thread.CurrentThread;
			new Thread(() => Run(mainThread)) {
				Name = "Server Hang Watchdog",
				IsBackground = true
			}.Start();
		}

		private static void Run(Thread mainThread) {
			while (true) {
				Thread.Sleep(1000);
				if (DateTime.Now - lastCheckin.Value > TIMEOUT) {
					Logging.ServerConsoleLine("Server hung for more than 10 seconds. Cannot determine cause from watchdog thread", Level.Warn, log: Logging.tML);
					Checkin();
					continue;
				}
			}
		}
	}
}
