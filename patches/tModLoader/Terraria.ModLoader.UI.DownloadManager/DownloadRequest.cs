using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal abstract class DownloadRequest
	{
		internal const string TEMP_EXTENSION = ".tmp";

		public readonly string DisplayText;
		public readonly string OutputFilePath;
		public FileStream FileStream;

		public event Action<double> OnUpdateProgress;
		public event Action OnCancel;
		public event Action OnComplete;

		public bool Success { get; protected set; }
		public object CustomData { get; internal set; }
		public bool Completed { get; internal set; }

		private DateTime _timeStamp;
		private string _downloadPath;

		protected DownloadRequest(string displayText, string outputFilePath, object customData = null,
			Action<double> onUpdateProgress = null, Action onCancel = null, Action onComplete = null) {

			DisplayText = displayText;
			OutputFilePath = outputFilePath;
			CustomData = customData;
			OnUpdateProgress = onUpdateProgress;
			OnCancel = onCancel;
			OnComplete = onComplete;
		}

		/// <summary>
		/// Begin the request
		/// </summary>
		/// <returns></returns>
		public Task Start(CancellationToken cancellationToken) {
			cancellationToken.Register(Cancel);
			_timeStamp = DateTime.Now;
			_downloadPath = $"{new FileInfo(OutputFilePath).Directory.FullName}{Path.DirectorySeparatorChar}{_timeStamp.Ticks}{TEMP_EXTENSION}";
			FileStream = new FileStream(_downloadPath, FileMode.Create);
			return Task.Factory.StartNew(() => {
				Execute();
				while (!Completed && !cancellationToken.IsCancellationRequested) ; // Fully wait for completion of this request
			}, cancellationToken, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
		}

		/// <summary>
		/// Execution of the request
		/// </summary>
		public abstract void Execute();

		/// <summary>
		/// Cancel the request
		/// </summary>
		public virtual void Cancel() {
			try {
				if (FileStream != null) {
					FileStream.Close();
					File.Delete(_downloadPath);
				}
			}
			catch (Exception e) {
				Logging.tML.Error($"Problem during cancellation of DownloadRequest[{DisplayText}]", e);
			}
			OnCancel?.Invoke();
		}

		/// <summary>
		/// Complete the request. When executed you are certain the request is completed successfully
		/// </summary>
		public virtual void Complete() {
			try {
				FileStream?.Close();
				if (Success)
					File.Copy(_downloadPath, OutputFilePath, true);
				File.Delete(_downloadPath);
			}
			catch (Exception e) {
				Logging.tML.Error($"Problem during completion of DownloadRequest[{DisplayText}]", e);
			}

			Completed = true;
			OnComplete?.Invoke();
		}

		protected void UpdateProgress(double progress) {
			OnUpdateProgress?.Invoke(progress);
		}
	}
}
