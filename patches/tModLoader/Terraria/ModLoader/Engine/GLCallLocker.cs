using log4net;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Engine
{
	/// <summary>
	/// FNA uses a single-threaded GL context. This class helps with tracking down related issues and deadlocks.
	/// See https://github.com/tModLoader/tModLoader/issues/237 and https://github.com/FNA-XNA/FNA3D/blob/fddf4e0607db1f8ae0a46c1f8df9371809a9a066/src/FNA3D_Driver_OpenGL.c#L1131
	/// It also improves loading performance on FNA
	/// </summary>
	public static class GLCallLocker
	{
#if FNA
		private static int mainThreadId;
		public static string DriverIdentifier { get; internal set; } // note, Metal will be "Metal\nDevice Name: %s"

		public static bool GraphicsDriverIsThreadSafe => DriverIdentifier != "OpenGL";

		// if the main thread is going to devote extra time to integrating GL calls from other threads asap through SpeedrunActions
		// disables warning during mod loading
		internal static bool ActionsAreSpeedrun;
		private static readonly ConcurrentQueue<Action> actionQueue = new();
		private static readonly AutoResetEvent actionQueuedEvent = new AutoResetEvent(false);

		private static bool init = false;
		internal static void Init() {
			if (init)
				return;
			init = true;

			mainThreadId = Thread.CurrentThread.ManagedThreadId;

			AssetRepository.SafelyAcquireResourceLock = Enter;
			Main.OnPostDraw += _ => RunGLActions();

			// NOTE! Whole class currently useless without hooking every function in FNA3D
		}

		public static void Enter(object lockObj) {
			if (GraphicsDriverIsThreadSafe || Thread.CurrentThread.ManagedThreadId != mainThreadId) {
				Monitor.Enter(lockObj);
				return;
			}

			while (!Monitor.TryEnter(lockObj)) {
				RunGLActions();
			}
		}
#endif

		internal static Task<T> InvokeAsync<T>(Func<T> task) {
#if XNA
			return Task.Factory.StartNew(task);
#else
			var tcs = new TaskCompletionSource<T>();
			if (GraphicsDriverIsThreadSafe || Thread.CurrentThread.ManagedThreadId == mainThreadId) {
				tcs.SetResult(task());
				return tcs.Task;
			}

			actionQueue.Enqueue(() => {
				try {
					tcs.SetResult(task());
				}
				catch (Exception ex) {
					tcs.SetException(ex);
				}
			});
			actionQueuedEvent.Set();

			return tcs.Task;
#endif
		}

		internal static T Invoke<T>(Func<T> task) {
			return InvokeAsync(task).Result;
		}

		internal static void RedirectLogs() {
#if FNA
			FNALoggerEXT.LogInfo = (s) => {
				if (DriverIdentifier == null && s.StartsWith("FNA3D Driver: "))
					DriverIdentifier = s.Substring("FNA3D Driver: ".Length);

				LogManager.GetLogger("FNA").Info(s);
			};
			FNALoggerEXT.LogWarn = LogManager.GetLogger("FNA").Warn;
			FNALoggerEXT.LogError = LogManager.GetLogger("FNA").Error;
#endif
		}

#if FNA
		internal static void SpeedrunActions() {
			if (GraphicsDriverIsThreadSafe)
				return;

			var sw = new Stopwatch();
			sw.Start();

			// wait for actions to get tossed into the list and then execute them asap for 10ms before returning to the game loop
			int wait;
			while ((wait = (int)(10 - sw.ElapsedMilliseconds)) > 0 && actionQueuedEvent.WaitOne(wait)) {
				RunGLActions();
			}
		}

		internal static void RunGLActions() {
			while (actionQueue.TryDequeue(out var action))
				action();
		}

		/*private static HashSet<string> pastStackTraces = new HashSet<string>();
		private delegate void orig_ForceToMainThread(object self, Action action);
		private delegate void hook_ForceToMainThread(orig_ForceToMainThread orig, object self, Action action);
		private static void HookForceToMainThread(orig_ForceToMainThread orig, object self, Action action) {
			if (ModCompile.activelyModding && !ActionsAreSpeedrun && Thread.CurrentThread.ManagedThreadId != mainThreadId) {
				var stackTrace = new StackTrace(false); // line numbers not supported on mono yet
				var s = stackTrace.ToString();
				if (pastStackTraces.Add(s))
					Logging.tML.Debug("GL function invoked on worker-thread in a single-threaded context. Execution will be delayed till next frame\n" + s);
			}

			orig(self, action);

			if (ActionsAreSpeedrun && Thread.CurrentThread.ManagedThreadId != mainThreadId)
				actionQueuedEvent.Set();
		}*/
#endif
	}
}
