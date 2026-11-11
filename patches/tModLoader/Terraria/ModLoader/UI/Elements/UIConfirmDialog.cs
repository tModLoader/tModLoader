using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements;

public class UIConfirmDialog : UIElement
{
	public MouseEvent YesAction { get; }
	public MouseEvent NoAction { get; }
	public MouseEvent DontShowAgainAction { get; }
	public LocalizedText ConfirmText { get; }
	public LocalizedText SubText { get; }
	public bool ShowYesDontShowAgainButton { get; }
	public bool AutoPlaySoundOnClose { get; }
	public Action OnClose { get; set; }

	private UIImage blockInput;
	private UIPanel dialog;

	public UIConfirmDialog(bool showYesDontShowAgainButton, LocalizedText confirmText, LocalizedText subText = null, MouseEvent yesAction = null, MouseEvent noAction = null, MouseEvent dontShowAgainAction = null, bool autoPlaySoundOnClose = true)
	{
		ShowYesDontShowAgainButton = showYesDontShowAgainButton;
		ConfirmText = confirmText;
		SubText = subText;
		YesAction = yesAction;
		NoAction = noAction;
		DontShowAgainAction = dontShowAgainAction;
		AutoPlaySoundOnClose = autoPlaySoundOnClose;

		Width.Set(0, 1f);
		Height.Set(0, 1f);
		CreateUI();
	}

	private void CreateUI()
	{
		blockInput = new UIImage(TextureAssets.MagicPixel.Value) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = Color.Black * 0.5f,
			ScaleToFit = true,
		};
		blockInput.OnLeftMouseDown += (_, _) => Close();
		Append(blockInput);

		dialog = new UIPanel {
			Width = { Percent = 0.30f },
			Height = { Percent = 0.30f },
			HAlign = 0.5f,
			VAlign = 0.5f,
			BackgroundColor = new Color(63, 82, 151),
			BorderColor = Color.Black,
		}.WithPadding(6f);
		Append(dialog);

		var confirmText = new UIText(ConfirmText) {
			Width = { Percent = 0.75f },
			HAlign = 0.5f,
			VAlign = ShowYesDontShowAgainButton ? 0.2f : 0.3f,
			IsWrapped = true,
		};
		dialog.Append(confirmText);

		if (SubText != null) {
			Recalculate();

			var subText = new UIText(SubText) {
				IsWrapped = true,
			};

			subText.CopyStyle(confirmText);
			subText.Top.Pixels = confirmText.MinHeight.Pixels;
			dialog.Append(subText);
		}

		var yesButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.104")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = ShowYesDontShowAgainButton ? 0.6f : 0.85f,
			HAlign = 0.15f,
		}.WithFadedMouseOver();
		dialog.Append(yesButton);
		yesButton.OnLeftClick += Close;

		if (YesAction != null)
			yesButton.OnLeftClick += YesAction;

		var noButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.105")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = ShowYesDontShowAgainButton ? 0.6f : 0.85f,
			HAlign = 0.85f,
		}.WithFadedMouseOver();
		dialog.Append(noButton);
		noButton.OnLeftClick += Close;

		if (NoAction != null)
			yesButton.OnLeftClick += NoAction;

		if (ShowYesDontShowAgainButton) {
			var yesDontAskAgainButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.YesDontAskAgain")) {
				TextColor = Color.White,
				Width = new StyleDimension(0f, 2f / 3f),
				Height = { Pixels = 40 },
				VAlign = 0.85f,
				HAlign = 0.5f,
			}.WithFadedMouseOver();
			dialog.Append(yesDontAskAgainButton);
			yesDontAskAgainButton.OnLeftClick += Close;

			if (YesAction != null)
				yesDontAskAgainButton.OnLeftClick += YesAction;

			if (DontShowAgainAction != null)
				yesDontAskAgainButton.OnLeftClick += DontShowAgainAction;
		}

		Recalculate();
	}

	public void Close(bool playSound) {
		if (playSound)
			SoundEngine.PlaySound(SoundID.MenuClose);

		Remove();
		OnClose?.Invoke();
	}

	public void Close()
	{
		Close(AutoPlaySoundOnClose);
	}

	private void Close(UIMouseEvent evt, UIElement listeningElement)
	{
		Close();
	}
}