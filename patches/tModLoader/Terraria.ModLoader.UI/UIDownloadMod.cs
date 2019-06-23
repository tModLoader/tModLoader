using System;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO: yet another progress UI, this time with cancel button
	internal class UIDownloadMod : UIState
	{
		private UILoadProgress loadProgress;
		private string name;
		private Action cancelAction;

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
		}

		internal void SetDownloading(string name) {
			Logging.tML.InfoFormat("Downloading Mod: {0}", name);
			this.name = name;
		}

		public void SetCancel(Action cancelAction) {
			this.cancelAction = cancelAction;
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e) => SetProgress(e.BytesReceived, e.TotalBytesToReceive);
		internal void SetProgress(long count, long len) {
			//loadProgress?.SetText("Downloading: " + name + " -- " + count+"/" + len);
			loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Logging.tML.InfoFormat("Download Cancelled");
			Main.PlaySound(10);
			cancelAction();
		}
	}
}