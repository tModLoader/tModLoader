using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

// One progress tracker for now, multi-download in the future.
internal class UIProgress : UIState
{
	public event Action OnCancel;
	public int gotoMenu = 0;

	protected UIProgressBar _progressBar;
	protected UITextPanel<LocalizedText> _cancelButton;

	// separate variable copied to progress bar in Update, allows for thread safety and setting display text before UI initialization
	public string DisplayText;

	// Little text used by the mod loading progress and mod upload progress
	protected UIText subProgress;

	public float Progress {
		get => _progressBar?.Progress ?? 0f;
		set => _progressBar?.UpdateProgress(value);
	}

	public string SubProgressText {
		set => subProgress?.SetText(value);
	}

	public override void OnInitialize()
	{
		_progressBar = new UIProgressBar {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Height = { Pixels = 150 },
			HAlign = 0.5f,
			VAlign = 0.5f,
			Top = { Pixels = 10 }
		};
		Append(_progressBar);
		_cancelButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Cancel"), 0.75f, true) {
			VAlign = 0.5f,
			HAlign = 0.5f,
			Top = { Pixels = 170 }
		}.WithFadedMouseOver();
		_cancelButton.OnLeftClick += CancelClick;
		Append(_cancelButton);

		subProgress = new UIText("", 0.5f, true) {
			Top = { Pixels = 65 },
			HAlign = 0.5f,
			VAlign = 0.5f
		};
		Append(subProgress);
	}

	private void CancelClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(ID.SoundID.MenuOpen);
		Main.menuMode = gotoMenu;
		OnCancel?.Invoke();
	}

	public void Show(string displayText = "", int gotoMenu = 0, Action cancel = null)
	{
		if (Main.MenuUI.CurrentState == this)
			Main.MenuUI.RefreshState();
		else
			Main.menuMode = Interface.progressID;

		DisplayText = displayText;
		this.gotoMenu = gotoMenu;

		if (cancel != null)
			OnCancel += cancel;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		_progressBar.DisplayText = DisplayText;
	}

	public override void OnDeactivate()
	{
		DisplayText = null;
		OnCancel = null;
		gotoMenu = 0;
		Progress = 0f;
	}
}

