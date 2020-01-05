using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class UIDownloadProgress : UIProgress
	{
		public event Action OnDownloadsComplete;

		private readonly List<DownloadFile> _downloads = new List<DownloadFile>();
		internal CancellationTokenSource _cts;

		public override void OnActivate() {
			base.OnActivate();

			if (_downloads.Count <= 0) {
				Logging.tML.Warn("UIDownloadProgress was activated but no downloads were present.");
				Main.menuMode = gotoMenu;
				return;
			}

			_cts = new CancellationTokenSource();
			OnCancel += () => {
				_cts.Cancel();
			};
			DownloadMods();
		}

		public override void OnDeactivate() {
			base.OnDeactivate();

			foreach (DownloadFile download in _downloads) {
				Logging.tML.Warn($"UIDownloadProgress was deactivated but download [{download.FilePath}] was still present.");
			}

			OnDownloadsComplete = null;
			_cts?.Dispose();
			_downloads.Clear();
			_progressBar.UpdateProgress(0f);
		}

		public void HandleDownloads(params DownloadFile[] downloads) {
			foreach (DownloadFile download in downloads) {
				if (download.Verify()) {
					_downloads.Add(download);
				}
			}
			Show();
		}

		public void Show() {
			Main.menuMode = Interface.downloadProgressID;
		}

		private void DownloadMods() {
			var downloadFile = _downloads.First();
			if (downloadFile == null) return;
			_progressBar.UpdateProgress(0f);
			_progressBar.DisplayText = Language.GetTextValue("tModLoader.MBDownloadingMod", downloadFile.DisplayText);
			var dlTask = downloadFile.Download(_cts.Token, _progressBar.UpdateProgress);
			dlTask.ContinueWith(HandleNextDownload, _cts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
			dlTask.ContinueWith(task => {
				Logging.tML.Error($"There was a problem downloading the mod {downloadFile.DisplayText}", task.Exception);
				HandleNextDownload(task);
			}, _cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void HandleNextDownload(Task<DownloadFile> task) {
			_downloads.Remove(task.Result);
			if (_downloads.Count <= 0) {
				Main.menuMode = gotoMenu;
				OnDownloadsComplete?.Invoke();
				return;
			}
			DownloadMods();
		}
	}
}
