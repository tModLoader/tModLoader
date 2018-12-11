using System;
using System.IO;
using System.Net;
using System.Net.Security;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO: downloads and web exceptions need logging
	//TODO: merge all progress/download UIs
	//TODO: of all the download UIs, this one has been refactored the best
	internal class UIDownloadFile : UIState
	{
		private UILoadProgress loadProgress;
		private string name;
		private string url;
		private string file;
		private Action success;
		private Action failure; //TODO unused?
		private Action cancelAction;
		private WebClient client;

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
			loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", name));
			loadProgress.SetProgress(0f);
			if (!UIModBrowser.PlatformSupportsTls12) { // Needed for downloads from Github
				Interface.errorMessage.Show("TLS 1.2 not supported on this computer.", 0); // github releases
				return;
			}
			
			ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // SecurityProtocolType.Tls12
			
			client = new WebClient();
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => true);
			SetCancel(client.CancelAsync);
			client.DownloadProgressChanged += Client_DownloadProgressChanged;
			client.DownloadFileCompleted += Client_DownloadFileCompleted;
			client.DownloadFileAsync(new Uri(url), file);
		}

		private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
			client.Dispose();
			client = null;

			if (e.Error == null && !e.Cancelled) {
				success();
				return;
			}

			if (e.Cancelled) {
				Main.menuMode = 0;
			}
			else {
				// TODO: Think about what message to put here.
				var errorKey = GetHttpStatusCode(e.Error) == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
				Interface.errorMessage.Show(Language.GetTextValue("tModLoader."+errorKey), 0);
			}

			if (File.Exists(file))
				File.Delete(file);
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			SetProgress(e);
		}

		internal void SetDownloading(string name, string url, string file, Action success) {
			this.name = name;
			this.url = url;
			this.file = file;
			this.success = success;
		}

		public void SetCancel(Action cancelAction) {
			this.cancelAction = cancelAction;
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e) => SetProgress(e.BytesReceived, e.TotalBytesToReceive);
		internal void SetProgress(long count, long len) {
			loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			cancelAction?.Invoke();
		}

		private HttpStatusCode GetHttpStatusCode(Exception err) =>
			err is WebException we && we.Response is HttpWebResponse response ? response.StatusCode : 0;
	}
}
