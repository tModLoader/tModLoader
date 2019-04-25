using System;
using System.Collections.Generic;
using System.IO;
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
		internal bool Active;
		private UILoadProgress loadProgress;
		private string _oldName;
		private string _name;

		public string OverrideName { get; internal set; }
		private readonly Queue<DownloadRequest> _requestQueue = new Queue<DownloadRequest>();
		private CancellationTokenSource cts;

		public Action OnQueueProcessed { get; internal set; }

		public void EnqueueRequest(DownloadRequest request) {
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

				// TODO Add a concurrency TML option, up to 4 concurrent downloads
				// Start a new task to handle this download
				Task.Factory.StartNew(() => {
					try {
						if (!req.SetupRequest(cts.Token)) {
							// Should never happen, but if it does, problem and aborting
							Logging.tML.Error("Problem during setup of HttpDownloadRequest");
							return;
						}

						if (req is HttpDownloadRequest httpRequest) {
							httpRequest.OnBufferUpdate = (_) => { SetProgress(httpRequest.Progress); };
							httpRequest.OnComplete = (_) => {
								File.WriteAllBytes(httpRequest.OutputFilePath, httpRequest.ResponseBytes);
								Logging.tML.Info($"DownloadManager finished downloading a file [{httpRequest.DisplayText}] to {httpRequest.OutputFilePath}");
							};
							httpRequest.Begin();
						}
						else if (req is StreamingDownloadRequest streamingRequest) {
							streamingRequest.OnBufferUpdate = (_) => { SetProgress(streamingRequest.FileStream.Position / (double)streamingRequest.DownloadingLength); };
							streamingRequest.OnComplete = (_) => { Logging.tML.Info($"DownloadManager finished downloading a file [{req.DisplayText}] to {req.OutputFilePath} when syncing mods"); };
						}

						while (!req.Completed && !cts.IsCancellationRequested); // Fully wait for completion of this request
					}
					catch (Exception e) {
						// Problem during setup, such as TLS handshake failure for web dls
						Logging.tML.Error($"Problem during processing of HttpDownloadRequest[{req.DisplayText}]", e);
					}
				}, cts.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current)
				.Wait(cts.Token); // Wait for dl completion
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
			if (!Active) {
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
				Active = true;
			}
		}

		public override void OnDeactivate() {
			_requestQueue.Clear();
			OnQueueProcessed = null;
			OverrideName = null;
			Active = false;
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