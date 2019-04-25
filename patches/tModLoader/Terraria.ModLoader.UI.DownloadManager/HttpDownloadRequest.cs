using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class HttpDownloadRequest : DownloadRequest
	{
		private readonly Func<HttpWebRequest> _requestCallback;

		public double Progress { get; private set; }

		public HttpWebResponse Response { get; protected set; }
		public HttpWebRequest Request { get; protected set; }
		public IAsyncResult ResponseAsyncResult { get; protected set; }
		public byte[] ResponseBytes { get; protected set; }

		public const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;
		public SecurityProtocolType SecurityProtocol = Tls12;
		public Version ProtocolVersion = HttpVersion.Version11;

		public HttpDownloadRequest(string displayText, string outputFilePath, Func<HttpWebRequest> requestCallback,
			Action<DownloadRequest> onBufferUpdate = null, Action<DownloadRequest> onComplete = null, 
			Action<DownloadRequest> onCancel = null, Action<DownloadRequest> onFinish = null, 
			object customData = null) 
			: base(displayText, outputFilePath, onBufferUpdate, onComplete, onCancel, onFinish, customData) {

			_requestCallback = requestCallback;
		}

		public override bool SetupRequest(CancellationToken cancellationToken) {
			ServicePointManager.SecurityProtocol = SecurityProtocol;
			ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidation;
			Request = _requestCallback();
			Request.ProtocolVersion = ProtocolVersion;
			Request.UserAgent = $"tModLoader/{ModLoader.versionTag}";
			Request.KeepAlive = true;
			cancellationToken.Register(() => {
				Request?.Abort();
				OnCancel?.Invoke(this);
			});
			return true;
		}

		public IAsyncResult Begin() {
			ResponseAsyncResult = Request.BeginGetResponse(ResponseCallback, null);
			return ResponseAsyncResult;
		}

		// TODO Jof: HPKP / Expect-CT Manager
		private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
			return errors == SslPolicyErrors.None;
		}

		// This is an asynchronous callback
		// It might be executing on a worker thread
		// so do not update controls directly from here
		private void ResponseCallback(object state) {

			Response = (HttpWebResponse)Request.EndGetResponse(ResponseAsyncResult);

			long contentLength = Response.ContentLength;
			if (contentLength < 0) {
				Logging.tML.Error($"Could not get a proper content length for HttpDownloadRequest [{DisplayText}]");
				// error
				return;
			}

			Stream responseStream = Response.GetResponseStream();
			ResponseBytes = HandleAsynchronousResponse(responseStream, contentLength);
			Response.Close();

			OnComplete?.Invoke(this);
			OnFinish?.Invoke(this);
			Completed = true;
		}

		// This is also on a worker thread, use callback to update UI
		private byte[] HandleAsynchronousResponse(Stream responseStream, long contentLength) {
			var data = new byte[contentLength];
			int currentIndex = 0;
			var buffer = new byte[256];

			do {
				int bytesReceived = responseStream.Read(buffer, 0, 256);
				Array.Copy(buffer, 0, data, currentIndex, bytesReceived);
				currentIndex += bytesReceived;
				Progress = (double)currentIndex / contentLength;
				// Use a buffer update callback to update UI
				OnBufferUpdate(this);
			} while (currentIndex < contentLength);

			return data;
		}
	}
}
