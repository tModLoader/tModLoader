using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.DownloadManager
{
	// One progress tracker for now, multi-download in the future.
	internal class UIProgress : UIState
	{
		public event Action OnCancel;
		public int gotoMenu = 0;

		protected UIProgressBar _progressBar;
		protected UITextPanel<string> _cancelButton;
		private string _cachedText = "";

		public string DisplayText {
			get => _progressBar?.DisplayText;
			set {
				if (_progressBar?.DisplayText == null) _cachedText = value;
				else _progressBar.DisplayText = value;
			}
		}

		public float Progress {
			get => _progressBar?.Progress ?? 0f;
			set => _progressBar?.UpdateProgress(value);
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
			_cancelButton = new UITextPanel<string>(Language.GetTextValue("UI.Cancel"), 0.75f, true) {
				VAlign = 0.5f,
				HAlign = 0.5f,
				Top = { Pixels = 170 }
			}.WithFadedMouseOver();
			_cancelButton.OnClick += CancelClick;
			Append(_cancelButton);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuOpen);
			Main.menuMode = gotoMenu;
			OnCancel?.Invoke();
		}

		public void Show() {
			Main.menuMode = Interface.progressID;
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			// Sometimes the DisplayText may be set just before the UI initializes and the progressBar is ready
			// In this case the text ends up empty but can be set on the next Update call
			if (!string.IsNullOrEmpty(_cachedText) && _progressBar != null) {
				DisplayText = _cachedText;
				_cachedText = string.Empty;
			}
		}

		public override void OnActivate() {
			_progressBar.DisplayText = _cachedText;
		}

		public override void OnDeactivate() {
			_cachedText = string.Empty;
			DisplayText = string.Empty;
			OnCancel = null;
			gotoMenu = 0;
			Progress = 0f;
		}
	}

}
