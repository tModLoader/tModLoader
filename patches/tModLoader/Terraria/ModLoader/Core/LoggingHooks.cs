using MonoMod.RuntimeDetour;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Terraria.ModLoader.Core
{
	internal static class LoggingHooks
	{
		internal static void Init() {
			PrettifyStackTraceSources();
			HookWebRequests();
			HookProcessStart();
		}

		private static void HookProcessStart() {
			new Hook(typeof(Process).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance), new Func<Func<Process, bool>, Process, bool>((orig, self) => {
				Logging.tML.Debug($"Process.Start (UseShellExecute = {self.StartInfo.UseShellExecute}): \"{self.StartInfo.FileName}\" {self.StartInfo.Arguments}");
				return orig(self);
			}));
		}

		// On .NET, hook the StackTrace constructor
		private delegate void ctor_StackTrace(StackTrace self, Exception e, bool fNeedFileInfo);
		private delegate void hook_StackTrace(ctor_StackTrace orig, StackTrace self, Exception e, bool fNeedFileInfo);
		private static void HookStackTraceEx(ctor_StackTrace orig, StackTrace self, Exception e, bool fNeedFileInfo) {
			orig(self, e, fNeedFileInfo);
			if (fNeedFileInfo)
				Logging.PrettifyStackTraceSources(self.GetFrames());
		}

		private static void PrettifyStackTraceSources() {
			if (Logging.f_fileName == null)
				return;

			new Hook(typeof(StackTrace).GetConstructor(new[] { typeof(Exception), typeof(bool) }), new hook_StackTrace(HookStackTraceEx));
		}

		private delegate EventHandler SendRequest(object self, HttpWebRequest request);
		private delegate EventHandler SendRequestHook(SendRequest orig, object self, HttpWebRequest request);

		private delegate void WebOperation_ctor(object self, HttpWebRequest request, object writeBuffer, bool isNtlmChallenge, CancellationToken cancellationToken);
		private delegate void WebOperation_ctorHook(WebOperation_ctor orig, object self, HttpWebRequest request, object writeBuffer, bool isNtlmChallenge, CancellationToken cancellationToken);

		private delegate bool SubmitRequest(object self, HttpWebRequest request, bool forcedsubmit);
		private delegate bool SubmitRequestHook(SubmitRequest orig, object self, HttpWebRequest request, bool forcedsubmit);

		/// <summary>
		/// Attempt to hook the .NET internal methods to log when requests are sent to web addresses.
		/// Use the right internal methods to capture redirects
		/// </summary>
		private static void HookWebRequests() {
			try {
				// TODO re-implement for Core?
			}
			catch {
				Logging.tML.Warn("HttpWebRequest send/submit method not found");
			}
		}
	}
}
