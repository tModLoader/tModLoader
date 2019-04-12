using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIDownloadManager : UIState
	{
		private UILoadProgress loadProgress;
		private string _oldName;
		private string _name;

		public string OverrideName { get; internal set; }
		private readonly Queue<DownloadRequest> _requestQueue = new Queue<DownloadRequest>();
		private CancellationTokenSource cts;

		public Action OnQueueProcessed { get; internal set; }

		public void EnqueueRequest(HttpDownloadRequest request) {
			_requestQueue.Enqueue(request);
		}

		public void ClearQueue() {
			_requestQueue.Clear();
		}

		public void ProcessQueue() {
			Task.Factory.StartNew(DispatchWorkersFromQueue, cts.Token)
				.ContinueWith(parent => {

					if (parent.IsCanceled) {
						Logging.tML.Info("DownloadManager processing was cancelled.", parent.Exception?.Flatten());
					}

					if (parent.Status == TaskStatus.RanToCompletion) {
						Logging.tML.Info("DownloadManager processing successfully completed.");
					}

					OnQueueProcessed();
				}, cts.Token);
		}

		private void DispatchWorkersFromQueue() {

			int toProcess = _requestQueue.Count;
			int processed = 0;

			while (_requestQueue.Count > 0) {
				Logging.tML.Info("DownloadManager handling new download");
				processed++;
				var req = _requestQueue.Dequeue();
				_name = req.DisplayText;

				Task.Factory.StartNew(() => {
					try {
						if (!req.SetupRequest(cts.Token)) {
							// Should never happen, but if it does, problem and aborting
							Logging.tML.Error("Problem during setup of HttpDownloadRequest");
							return;
						}

						if (req is HttpDownloadRequest httpRequest) {
							httpRequest.OnBufferUpdate = (httpReq) => { SetProgress(httpReq.Progress); };
							httpRequest.OnComplete = (httpReq) => {
								File.WriteAllBytes(httpReq.OutputFilePath, httpReq.ResponseBytes);
								Logging.tML.Info($"DownloadManager finished downloading a file [{httpReq.DisplayText}] to {httpReq.OutputFilePath}");
							};
							httpRequest.Begin();
							while (!httpRequest.Completed && !cts.IsCancellationRequested) ; // Fully wait for completion
						}
					}
					catch (Exception e) {
						// Problem during setup, such as TLS handshake failure
						Logging.tML.Error($"Problem during processing of HttpDownloadRequest[{req.DisplayText}]", e);
					}
				}, cts.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current)
				.Wait(); // TODO Do not support concurrency for now, make it a TML option (2-4)
			}

			Logging.tML.Info($"DownloadManager processed {processed} out of {toProcess} requests. Waiting for downloading to complete.");
		}

		public override void OnInitialize() {
			loadProgress = new UILoadProgress {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(loadProgress);

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

			loadProgress.SetProgress(0f);

			if (!UIModBrowser.PlatformSupportsTls12) {
				// Needed for downloads from Github
				Interface.errorMessage.Show("TLS 1.2 not supported on this computer.", 0); // github releases
				return;
			}

			cts?.Dispose();
			cts = new CancellationTokenSource();
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
			loadProgress.SetText(GetDisplayText());
		}

		private string GetDisplayText() => Language.GetTextValue("tModLoader.MBDownloadingMod", OverrideName ?? _name);

		internal void SetProgress(double percent) {
			loadProgress?.SetProgress((float)percent);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			cts.Cancel(false);
		}
	}
}