using MonoMod.RuntimeDetour;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Engine
{
	internal static class LoggingHooks
	{
		internal static void Init() {
			PrettifyStackTraceSources();
			HookWebRequests();
			HookProcessStart();
		}

		private static void HookProcessStart() {
			_ = new Hook(typeof(Process).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance), new Func<Func<Process, bool>, Process, bool>((orig, self) => {
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

			_ = new Hook(typeof(StackTrace).GetConstructor(new[] { typeof(Exception), typeof(bool) }), new hook_StackTrace(HookStackTraceEx));
		}

		private delegate ValueTask<HttpResponseMessage> orig_SendAsyncCore(object self, HttpRequestMessage request, Uri? proxyUri, bool async, bool doRequestAuth, bool isProxyConnect, CancellationToken cancellationToken);

		private delegate ValueTask<HttpResponseMessage> hook_SendAsyncCore(orig_SendAsyncCore orig, object self, HttpRequestMessage request, Uri? proxyUri, bool async, bool doRequestAuth, bool isProxyConnect, CancellationToken cancellationToken);

		/// <summary>
		/// Attempt to hook the .NET internal methods to log when requests are sent to web addresses.
		/// Use the right internal methods to capture redirects
		/// </summary>
		private static void HookWebRequests() {
			try {
				// .NET 6
				var sendAsyncCoreMethodInfo = typeof(HttpClient).Assembly
					.GetType("System.Net.Http.HttpConnectionPoolManager")
					?.GetMethod("SendAsyncCore", BindingFlags.Public | BindingFlags.Instance);

				if (sendAsyncCoreMethodInfo != null) {
					_ = new Hook(sendAsyncCoreMethodInfo, new hook_SendAsyncCore((orig, self, request, proxyUri, async, doRequestAuth, isProxyConnect, cancellationToken) => {
						Logging.tML.Debug($"Web Request: {request.RequestUri}");
						return orig(self, request, proxyUri, async, doRequestAuth, isProxyConnect, cancellationToken);
					}));
					return;
				}
			}
			catch {
			}

			Logging.tML.Warn("HttpWebRequest send/submit method not found");
		}
	}
}
