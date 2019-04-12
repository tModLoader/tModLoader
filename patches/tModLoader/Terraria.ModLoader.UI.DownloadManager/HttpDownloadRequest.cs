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
		public bool Completed { get; protected set; }

		public const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;
		public SecurityProtocolType SecurityProtocol = Tls12;
		public Version ProtocolVersion = HttpVersion.Version11;

		public Action<HttpDownloadRequest> OnBufferUpdate;
		public Action<HttpDownloadRequest> OnComplete;
		public Action<HttpDownloadRequest> OnCancel;
		public Action<HttpDownloadRequest> OnFinish;

		public HttpDownloadRequest(string displayText, string outputFilePath, Func<HttpWebRequest> requestCallback,
			Action<HttpDownloadRequest> onFinish = null, Action<HttpDownloadRequest> onCancel = null,
			Action<HttpDownloadRequest> onBufferUpdate = null)
			: base(displayText, outputFilePath) {

			_requestCallback = requestCallback;
			OnFinish = onFinish;
			OnBufferUpdate = onBufferUpdate;
			OnCancel = onCancel;
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

		// TODO HPKP / Expect-CT
		private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
			return errors == SslPolicyErrors.None;
		}

		// This is an asynchronous callback
		// It might be executing on a worker thread
		// so do not update controls directly from here
		private void ResponseCallback(object state) {

			Response = (HttpWebResponse)Request.EndGetResponse(ResponseAsyncResult);

			long contentLength = Response.ContentLength;
			if (contentLength <= -1) {
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
				// Use a buffer update callback
				OnBufferUpdate(this);
			} while (currentIndex < contentLength);

			return data;
		}
	}
}
