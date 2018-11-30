using System;
using System.Threading;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System.Diagnostics;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// FNA uses a single-threaded GL context. This class helps with tracking down related issues and deadlocks.
	/// See https://github.com/blushiemagic/tModLoader/issues/237 and https://github.com/FNA-XNA/FNA/blob/master/src/FNAPlatform/OpenGLDevice.cs#L4676
	/// </summary>
	public static class GLCallLocker
	{
		public static void Enter(object lockObj) {
#if WINDOWS
			Monitor.Enter(lockObj);
#else
			if (Thread.CurrentThread.ManagedThreadId != Main.mainThreadId) {
				Monitor.Enter(lockObj);
				return;
			}

			while (!Monitor.TryEnter(lockObj)) {
				RunGLActions();
			}
#endif
		}

		internal static void EnableWarnings() {
#if !WINDOWS
			var t_OpenGLDevice = typeof(GraphicsDevice).Assembly.GetType("Microsoft.Xna.Framework.Graphics.OpenGLDevice");
			var m_ForceToMainThread = t_OpenGLDevice.GetMethod("ForceToMainThread", BindingFlags.Instance | BindingFlags.NonPublic);

			new Hook(m_ForceToMainThread, new hook_ForceToMainThread(HookForceToMainThread));
#endif
		}

#if !WINDOWS
		private static FieldInfo f_GLDevice = typeof(GraphicsDevice).GetField("GLDevice", BindingFlags.Instance | BindingFlags.NonPublic);
		private static MethodInfo m_RunActions;

		private static void RunGLActions() {
			var glDevice = f_GLDevice.GetValue(Main.instance.GraphicsDevice);
			if (m_RunActions == null)
				m_RunActions = glDevice.GetType().GetMethod("RunActions", BindingFlags.Instance | BindingFlags.NonPublic);

			m_RunActions.Invoke(glDevice, new object[0]);
		}

		private static HashSet<string> pastStackTraces = new HashSet<string>();
		private delegate void orig_ForceToMainThread(object self, Action action);
		private delegate void hook_ForceToMainThread(orig_ForceToMainThread orig, object self, Action action);
		private static void HookForceToMainThread(orig_ForceToMainThread orig, object self, Action action)
		{
			if (Thread.CurrentThread.ManagedThreadId != Main.mainThreadId) {
				var stackTrace = new StackTrace(false); // line numbers not supported on mono yet
				var s = stackTrace.ToString();
				if (pastStackTraces.Add(s))
					Logging.tML.Debug("GL function invoked on worker-thread in a single-threaded context. Execution will be delayed till next frame\n" + s);
			}

			orig(self, action);
		}
#endif
	}
}
