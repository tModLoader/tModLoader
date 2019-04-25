using System;
using System.Threading;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal abstract class DownloadRequest
	{
		public readonly string DisplayText;
		public readonly string OutputFilePath;

		public object CustomData { get; internal set; }
		public bool Completed { get; internal set; }

		// Events we can hook into
		public Action<DownloadRequest> OnBufferUpdate;		// Should be used to update progress
		public Action<DownloadRequest> OnComplete;			// Should be used when download is completed
		public Action<DownloadRequest> OnCancel;			// Should be used when the download is cancelled
		public Action<DownloadRequest> OnFinish;			// Should be used when the download is finalized (after completion)

		protected DownloadRequest(string displayText, string outputFilePath,
			Action<DownloadRequest> onBufferUpdate = null, Action<DownloadRequest> onComplete = null, 
			Action<DownloadRequest> onCancel = null, Action<DownloadRequest> onFinish = null,
			object customData = null) {

			DisplayText = displayText;
			OutputFilePath = outputFilePath;
			OnBufferUpdate = onBufferUpdate;
			OnComplete = onComplete;
			OnCancel = onCancel;
			OnFinish = onFinish;
			CustomData = customData;
		}

		public virtual bool SetupRequest(CancellationToken cancellationToken) => true;
	}
}
