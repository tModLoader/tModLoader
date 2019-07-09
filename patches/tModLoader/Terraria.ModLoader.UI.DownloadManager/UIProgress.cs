using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.DownloadManager
{
	// One progress tracker for now, multi-download in the future.
	internal class UIProgress : UIState
	{

		public event Action OnCancel;
		protected UIProgressBar _progressBar;

		public string DisplayText {
			get => _progressBar.DisplayText;
			set => _progressBar.DisplayText = value;
		}

		public float Progress {
			get => _progressBar.Progress;
			set => _progressBar.UpdateProgress(value);
		}

		public override void OnInitialize() {
			_progressBar = new UIProgressBar {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(_progressBar);
			var cancel = new UITextPanel<string>(Language.GetTextValue("UI.Cancel"), 0.75f, true) {
				VAlign = 0.5f,
				HAlign = 0.5f,
				Top = { Pixels = 170 }
			}.WithFadedMouseOver();
			cancel.OnClick += CancelClick;
			Append(cancel);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuOpen);
			Main.menuMode = 0;
			OnCancel?.Invoke();
		}

		public void ActivateUI() {
			Main.menuMode = Interface.progressID;
		}

		public override void OnDeactivate() {
			OnCancel = null;
		}
	}

}
