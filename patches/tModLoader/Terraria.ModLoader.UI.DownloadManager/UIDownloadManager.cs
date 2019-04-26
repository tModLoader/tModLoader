using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

namespace Terraria.ModLoader.UI.DownloadManager
{
	/// <summary>
	/// Responsible for managing download requests and reporting the progress of it in a UI
	/// </summary>
	internal class UIDownloadManager : UIState
	{
		public string OverrideName { get; internal set; }

		private UILoadProgress _loadProgress;
		private string _oldName;
		private string _name;
		private readonly Queue<DownloadRequest> _requestQueue = new Queue<DownloadRequest>();
		private CancellationTokenSource _cts;

		public Action OnQueueProcessed { get; internal set; }

		public void EnqueueRequest(DownloadRequest request) {
			_requestQueue.Enqueue(request);
		}

		public void ClearQueue() {
			_requestQueue.Clear();
		}

		public void ProcessQueue() {
			Task.Factory.StartNew(DispatchWorkersFromQueue, _cts.Token)
				.ContinueWith(parent => {

					if (parent.IsCanceled) {
						Logging.tML.Info("DownloadManager processing was cancelled.", parent.Exception?.Flatten());
					}

					if (parent.Status == TaskStatus.RanToCompletion) {
						Logging.tML.Info("DownloadManager processing successfully completed.");
					}

					OnQueueProcessed();
				});
		}

		private void DispatchWorkersFromQueue() {

			int toProcess = _requestQueue.Count;
			int processed = 0;

			while (_requestQueue.Count > 0) {
				Logging.tML.Info("DownloadManager handling new download");
				processed++;
				var req = _requestQueue.Dequeue();
				_name = req.DisplayText;

				// TODO Add a concurrency TML option, up to 4 concurrent downloads
				req.OnUpdateProgress += SetProgress;
				req.Start(_cts.Token).Wait(_cts.Token);
			}

			Logging.tML.Info($"DownloadManager processed {processed} out of {toProcess} requests. Waiting for downloading to complete.");
		}

		public override void OnInitialize() {
			_loadProgress = new UILoadProgress {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(_loadProgress);

			var cancel = new UITextPanel<string>(Language.GetTextValue("UI.Cancel"), 0.75f, true) {
				VAlign = 0.5f,
				HAlign = 0.5f,
				Top = { Pixels = 170 }
			}.WithFadedMouseOver();
			cancel.OnClick += CancelClick;
			Append(cancel);
		}

		public override void OnActivate() {
			if (OnQueueProcessed == null)
				OnQueueProcessed = () => Main.menuMode = 0;

			if (OverrideName != null)
				UpdateDisplayText();

			_loadProgress.SetProgress(0f);

			if (!UIModBrowser.PlatformSupportsTls12) {
				// Needed for downloads from Github
				Interface.errorMessage.Show("TLS 1.2 not supported on this computer.", 0); // github releases
				return;
			}

			_cts?.Dispose();
			_cts = new CancellationTokenSource();
			ProcessQueue();
		}

		public override void OnDeactivate() {
			_requestQueue.Clear();
			OnQueueProcessed = null;
			OverrideName = null;
		}

		public override void Update(GameTime gameTime) {
			if (_name != _oldName && OverrideName == null) {
				_oldName = _name;
				UpdateDisplayText();
			}
		}

		private void UpdateDisplayText() {
			_loadProgress.SetText(GetDisplayText());
		}

		private string GetDisplayText() => Language.GetTextValue("tModLoader.MBDownloadingMod", OverrideName ?? _name);

		internal void SetProgress(double percent) {
			_loadProgress?.SetProgress((float)percent);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			_cts.Cancel(false);
		}
	}
}