using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal sealed class HttpDownloadRequest : DownloadRequest
	{
		public HttpWebRequest Request { get; private set; }
		// Must use callback, or else will not use ServicePoint settings
		private readonly Func<HttpWebRequest> _requestSupplier;

		public HttpWebResponse Response { get; private set; }

		public const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;
		public SecurityProtocolType SecurityProtocol = Tls12;
		public Version ProtocolVersion = HttpVersion.Version11;

		public HttpDownloadRequest(string displayText, string outputFilePath, Func<HttpWebRequest> supplier,
			object customData = null, Action<double> onUpdateProgress = null, Action onCancel = null, Action onComplete = null)
			: base(displayText, outputFilePath, customData, onUpdateProgress, onCancel, onComplete) {

			_requestSupplier = supplier;
		}

		public override void Execute() {
			ServicePointManager.SecurityProtocol = SecurityProtocol;
			ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidation;
			Request = _requestSupplier();
			Request.ServicePoint.ReceiveBufferSize = ModNet.CHUNK_SIZE;
			Request.ProtocolVersion = ProtocolVersion;
			Request.UserAgent = $"tModLoader/{ModLoader.versionTag}";
			Request.KeepAlive = true;
			Request.Proxy = null;

			UpdateProgress(0);
			var task = Task.Factory.FromAsync(Request.BeginGetResponse, Request.EndGetResponse, TaskCreationOptions.AttachedToParent);
			task.ContinueWith(HandleResponse, TaskContinuationOptions.OnlyOnRanToCompletion);
			task.ContinueWith(HandleErrors, TaskContinuationOptions.OnlyOnFaulted);
		}

		public override void Cancel() {
			Logging.tML.Warn($"HttpDownloadRequest [{DisplayText}] was cancelled.");
			Request?.Abort();
			base.Cancel();
		}

		// TODO Jof: HPKP / Expect-CT Manager
		private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
			return errors == SslPolicyErrors.None;
		}

		private void HandleErrors(Task task) {
			var aggregate = task.Exception;
			foreach (var exception in aggregate.InnerExceptions) {
				if (exception.Message.Contains("The request was canceled.")) {
					Logging.tML.Warn($"HttpDownloadRequest [{DisplayText}] was aborted");
					break;
				}
			}
			Response?.Close();
			FileStream?.Dispose();
			UpdateProgress(0);
		}

		// This is an asynchronous callback
		// It might be executing on a worker thread
		// so do not update controls directly from here
		private void HandleResponse(Task<WebResponse> task) {
			Response = (HttpWebResponse)task.Result;
			Debug.Assert(Response.StatusCode == HttpStatusCode.OK);

			var contentLength = Response.ContentLength;
			if (contentLength < 0) {
				var txt = $"Could not get a proper content length for HttpDownloadRequest [{DisplayText}]";
				Logging.tML.Error(txt);
				throw new Exception(txt);
			}

			var responseStream = Response.GetResponseStream();
			int currentIndex = 0;
			byte[] buf;

			do {
				try {
					buf = responseStream.ReadBytes((int)Math.Min(contentLength - FileStream.Position, ModNet.CHUNK_SIZE));
					FileStream.Write(buf, 0, buf.Length);
					currentIndex += buf.Length;
					UpdateProgress(currentIndex / (double)contentLength);
				}
				catch {
					// during cancellation of request ReadBytes will throw
					break;
				}
			} while (currentIndex < contentLength);

			Success = currentIndex == contentLength;
			Response.Close();
			Complete();
		}
	}
}
