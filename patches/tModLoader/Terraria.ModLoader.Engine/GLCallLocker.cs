using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.Engine
{
	/// <summary>
	/// FNA uses a single-threaded GL context. This class helps with tracking down related issues and deadlocks.
	/// See https://github.com/tModLoader/tModLoader/issues/237 and https://github.com/FNA-XNA/FNA/blob/master/src/FNAPlatform/OpenGLDevice.cs#L4676
	/// It also improves loading performance on FNA
	/// </summary>
	public static class GLCallLocker
	{
		private static int mainThreadId;

		public static void Enter(object lockObj) {
#if XNA
			Monitor.Enter(lockObj);
#else
			if (Thread.CurrentThread.ManagedThreadId != mainThreadId) {
				Monitor.Enter(lockObj);
				return;
			}

			while (!Monitor.TryEnter(lockObj)) {
				RunGLActions();
			}
#endif
		}

		internal static void Init() {
			mainThreadId = Thread.CurrentThread.ManagedThreadId;
#if FNA
			var t_OpenGLDevice = typeof(GraphicsDevice).Assembly.GetType("Microsoft.Xna.Framework.Graphics.OpenGLDevice");
			var m_ForceToMainThread = t_OpenGLDevice.GetMethod("ForceToMainThread", BindingFlags.Instance | BindingFlags.NonPublic);

			new Hook(m_ForceToMainThread, new hook_ForceToMainThread(HookForceToMainThread));
#endif
		}

		internal static Task<T> InvokeAsync<T>(Func<T> task) {
#if XNA
			return Task.Factory.StartNew(task);
#else
			var tcs = new TaskCompletionSource<T>();
			if (Thread.CurrentThread.ManagedThreadId == mainThreadId) {
				tcs.SetResult(task());
				return tcs.Task;
			}

			var glDevice = f_GLDevice.GetValue(Main.instance.GraphicsDevice);
			if (f_actions == null)
				f_actions = glDevice.GetType().GetField("actions", BindingFlags.Instance | BindingFlags.NonPublic);

			var actions = (IList<Action>)f_actions.GetValue(glDevice);
			lock (actions) {
				actions.Add(() => {
					try {
						tcs.SetResult(task());
					}
					catch (Exception ex) {
						tcs.SetException(ex);
					}
				});
			}

			return tcs.Task;
#endif
		}

		// if the main thread is going to devote extra time to integrating GL calls from other threads asap through SpeedrunActions
		// disables warning during mod loading
		internal static bool ActionsAreSpeedrun;
		internal static void SpeedrunActions() {
#if FNA
			var sw = new Stopwatch();
			sw.Start();

			// wait for actions to get tossed into the list and then execute them asap for 10ms before returning to the game loop
			int wait;
			while ((wait = (int)(10 - sw.ElapsedMilliseconds)) > 0) {
				if (actionQueuedEvent.WaitOne(wait))
					RunGLActions();
			}
#endif
		}

#if FNA
		private static FieldInfo f_GLDevice = typeof(GraphicsDevice).GetField("GLDevice", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo f_actions;
		private static MethodInfo m_RunActions;

		private static void RunGLActions() {
			var glDevice = f_GLDevice.GetValue(Main.instance.GraphicsDevice);
			if (m_RunActions == null)
				m_RunActions = glDevice.GetType().GetMethod("RunActions", BindingFlags.Instance | BindingFlags.NonPublic);

			m_RunActions.Invoke(glDevice, new object[0]);
		}

		private static AutoResetEvent actionQueuedEvent = new AutoResetEvent(false);

		private static HashSet<string> pastStackTraces = new HashSet<string>();
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
		}
#endif
	}
}
