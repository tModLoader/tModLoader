using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal sealed class HttpDownloadRequest : DownloadRequest
	{
		public HttpWebRequest Request { get; private set; }
		private readonly Func<HttpWebRequest> _requestSupplier;

		public HttpWebResponse Response { get; private set; }
		public IAsyncResult ResponseAsyncResult { get; private set; }

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
			ResponseAsyncResult = Request.BeginGetResponse(ResponseCallback, null);
		}

		public override void Cancel() {
			Logging.tML.Warn($"The HttpRequest [{DisplayText}] was cancelled.");
			Request?.Abort();
			base.Cancel();
		}

		// TODO Jof: HPKP / Expect-CT Manager
		private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
			return errors == SslPolicyErrors.None;
		}

		// This is an asynchronous callback
		// It might be executing on a worker thread
		// so do not update controls directly from here
		private void ResponseCallback(object state) {
			try {
				Response = (HttpWebResponse)Request.EndGetResponse(ResponseAsyncResult);
			}
			catch (Exception e) {
				Logging.tML.Error($"The HttpRequest [{DisplayText}] failed to get a response.", e);
				return;
			}
			long contentLength = Response.ContentLength;
			if (contentLength < 0) {
				Logging.tML.Error($"Could not get a proper content length for HttpDownloadRequest [{DisplayText}]");
				return;
			}

			Stream responseStream = Response.GetResponseStream();

			int currentIndex = 0;

			do {
				byte[] buf = responseStream.ReadBytes((int)Math.Min(contentLength - FileStream.Position, ModNet.CHUNK_SIZE));
				FileStream.Write(buf, 0, buf.Length);
				currentIndex += buf.Length;
				UpdateProgress(currentIndex / (double)contentLength);
			} while (currentIndex < contentLength);

			Response.Close();
			Complete();
		}
	}
}
