using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
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

		// On Mono, hook Exception.StackTrace, generate a StackTrace, and edit it with source and line info
		private static readonly Regex trimParamTypes = new Regex(@"([([,] ?)(?:[\w.+]+[.+])", RegexOptions.Compiled);
		private static readonly Regex dropOffset = new Regex(@" \[.+?\](?![^:]+:-1)", RegexOptions.Compiled);
		private static readonly Regex dropGenericTicks = new Regex(@"`\d+", RegexOptions.Compiled);

		private delegate string orig_GetStackTrace(Exception self, bool fNeedFileInfo);
		private delegate string hook_GetStackTrace(orig_GetStackTrace orig, Exception self, bool fNeedFileInfo);
		private static string HookGetStackTrace(orig_GetStackTrace orig, Exception self, bool fNeedFileInfo) {
			var stackTrace = new StackTrace(self, true);
			MdbManager.Symbolize(stackTrace.GetFrames());
			Logging.PrettifyStackTraceSources(stackTrace.GetFrames());
			var s = stackTrace.ToString();
			s = trimParamTypes.Replace(s, "$1");
			s = dropGenericTicks.Replace(s, "");
			s = dropOffset.Replace(s, "");
			s = s.Replace(":-1", "");
			return s;
		}

		private static void PrettifyStackTraceSources() {
			if (Logging.f_fileName == null)
				return;

			if (FrameworkVersion.Framework == Framework.NetFramework)
				new Hook(typeof(StackTrace).GetConstructor(new[] { typeof(Exception), typeof(bool) }), new hook_StackTrace(HookStackTraceEx));
			else if (FrameworkVersion.Framework == Framework.Mono)
				new Hook(typeof(Exception).FindMethod("GetStackTrace"), new hook_GetStackTrace(HookGetStackTrace));
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
				// .NET 4.7.2
				MethodBase met = typeof(HttpWebRequest).Assembly
					.GetType("System.Net.Connection")
					?.FindMethod("SubmitRequest");
				if (met != null) {
					new Hook(met, new SubmitRequestHook((orig, self, request, forcedsubmit) => {
						Logging.tML.Debug($"Web Request: " + request.Address);
						return orig(self, request, forcedsubmit);
					}));
					return;
				}

				// Mono 5.20
				met = typeof(HttpWebRequest).Assembly
					.GetType("System.Net.WebOperation")
					?.GetConstructors()[0];
				if (met != null && met.GetParameters().Length == 4) {
					new Hook(met, new WebOperation_ctorHook((orig, self, request, buffer, challenge, token) => {
						Logging.tML.Debug($"Web Request: " + request.Address);
						orig(self, request, buffer, challenge, token);
					}));
					return;
				}

				// Mono 4.6.1
				met = typeof(HttpWebRequest).Assembly
					.GetType("System.Net.WebConnection")
					?.FindMethod("SendRequest");
				if (met != null) {
					new Hook(met, new SendRequestHook((orig, self, request) => {
						Logging.tML.Debug($"Web Request: " + request.Address);
						return orig(self, request);
					}));
					return;
				}
			}
			catch { }

			Logging.tML.Warn("HttpWebRequest send/submit method not found");
		}
	}
}
