using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class DownloadFile
	{
		internal const string TEMP_EXTENSION = ".tmp";
		public const int CHUNK_SIZE = 1 << 20; //1MB
		public const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;

		public HttpWebRequest Request { get; private set; }

		public event Action<float> OnUpdateProgress;
		public event Action OnComplete;

		public readonly string Url;
		public readonly string FilePath;
		public readonly string DisplayText;

		private FileStream _fileStream;

		// Note: these will be completely ignored on old Mono versions
		public SecurityProtocolType SecurityProtocol = Tls12;
		public Version ProtocolVersion = HttpVersion.Version11;

		public DownloadFile(string url, string filePath, string displayText) {
			Url = url;
			FilePath = filePath;
			DisplayText = displayText;
		}

		public bool Verify() {
			if (string.IsNullOrWhiteSpace(Url)) return false;
			if (string.IsNullOrWhiteSpace(FilePath)) return false;
			//if (File.Exists(FilePath)) return false;
			return true;
		}

		public Task<DownloadFile> Download(CancellationToken token, Action<float> updateProgressAction = null) {
			SetupDownloadRequest();
			if (updateProgressAction != null) OnUpdateProgress = updateProgressAction;
			return Task.Factory.FromAsync(
				Request.BeginGetResponse,
				asyncResult => Request.EndGetResponse(asyncResult),
				null
			).ContinueWith(t => HandleResponse(t.Result, token), token);
		}

		private bool _aborted;

		private void AbortDownload(string filePath) {
			_aborted = true;
			Request?.Abort();
			_fileStream?.Flush();
			_fileStream?.Close();
			if (File.Exists(filePath)) {
				File.Delete(filePath);
			}
		}

		private DownloadFile HandleResponse(WebResponse response, CancellationToken token) {
			var contentLength = response.ContentLength;
			if (contentLength < 0) {
				var txt = $"Could not get a proper content length for DownloadFile[{DisplayText}]";
				Logging.tML.Error(txt);
				throw new Exception(txt);
			}

			string _downloadPath = $"{new FileInfo(FilePath).Directory.FullName}{Path.DirectorySeparatorChar}{DateTime.Now.Ticks}{TEMP_EXTENSION}";
			_fileStream = new FileStream(_downloadPath, FileMode.Create);

			var responseStream = response.GetResponseStream();
			int currentIndex = 0;
			byte[] buf = new byte[CHUNK_SIZE];

			try {
				// Use a standard read loop, attempting to read small amounts causes it to lock up and die on mono
				int r;
				while ((r = responseStream.Read(buf, 0, buf.Length)) > 0) {
					token.ThrowIfCancellationRequested();
					_fileStream.Write(buf, 0, r);
					currentIndex += r;
					OnUpdateProgress?.Invoke((float)(currentIndex / (double)contentLength));
				}
			}
			catch (OperationCanceledException e) {
				AbortDownload(_downloadPath);
				Logging.tML.Info($"DownloadFile[{DisplayText}] operation was cancelled", e);
			}
			catch (Exception e) {
				AbortDownload(_downloadPath);
				Logging.tML.Info("Unknown error", e);
			}

			if (!_aborted) {
				_fileStream?.Close();
				PreCopy();
				File.Copy(_downloadPath, FilePath, true);
				File.Delete(_downloadPath);
				OnComplete?.Invoke();
			}

			return this;
		}

		private void SetupDownloadRequest() {
			ServicePointManager.SecurityProtocol = SecurityProtocol;
			ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidation;
			Request = WebRequest.CreateHttp(Url);
			if (FrameworkVersion.Framework == Framework.NetFramework) {
				Request.ServicePoint.ReceiveBufferSize = CHUNK_SIZE;
			}

			Request.Proxy = WebRequest.DefaultWebProxy;
			Request.Method = WebRequestMethods.Http.Get;
			Request.ProtocolVersion = ProtocolVersion;
			Request.UserAgent = $"tModLoader/{ModLoader.versionTag}";
			Request.KeepAlive = true;
		}

		private static readonly List<string> validCerts = new List<string> {
			GetCertHashString("github-com.pem"),
			GetCertHashString("dropbox-com.pem")
		};

		// TODO Jof: HPKP / Expect-CT Manager
		// Do not simply 'return true' here, that will cause: The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.
		// It is simply a security risk, as ANY certificate will be accepted. Don't do it
		private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
			return errors == SslPolicyErrors.None && validCerts.Contains(certificate.GetCertHashString());
		}

		internal virtual void PreCopy() {
		}

		private static string GetCertHashString(string certFile) {
			try {
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(certFile)) {
					if (stream == null) throw new NullReferenceException($"Stream for certificate {certFile} was null");
					var buf = new byte[stream.Length];
					stream.ReadBytes(buf);
					return new X509Certificate(buf).GetCertHashString();
				}
			}
			catch(Exception exception) {
				Logging.tML.Error("Problem getting certificate hash string", exception);
				return string.Empty;
			}
		}
	}
}