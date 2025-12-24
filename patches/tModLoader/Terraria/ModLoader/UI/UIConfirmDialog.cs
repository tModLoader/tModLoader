using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

// TODO: make UIMods use this, since this is adapted from it
// TODO: change the style of this to the one in the achievements menu, andm ake it use the achivements menu one
public class UIConfirmDialog : UIElement
{
	public MouseEvent YesAction { get; }
	public MouseEvent NoAction { get; }
	public MouseEvent YesDontShowAgainAction { get; }
	public LocalizedText ConfirmText { get; }
	public bool ShowYesDontShowAgainButton { get; }

	private UIImage blockInput;
	private UIPanel dialog;

	public UIConfirmDialog(bool showYesDontShowAgainButton, LocalizedText confirmText, MouseEvent yesAction = null, MouseEvent noAction = null, MouseEvent yesDontShowAgainAction = null)
	{
		ShowYesDontShowAgainButton = showYesDontShowAgainButton;
		ConfirmText = confirmText;
		YesAction = yesAction;
		NoAction = noAction;
		YesDontShowAgainAction = yesDontShowAgainAction;

		Width.Set(0, 1f);
		Height.Set(0, 1f);
		CreateUI();
	}

	private void CreateUI()
	{
		blockInput = new UIImage(TextureAssets.Extra[190]) {
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

		var yesButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.104")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = ShowYesDontShowAgainButton ? 0.6f : 0.85f,
			HAlign = 0.15f,
		}.WithFadedMouseOver();
		yesButton.OnLeftClick += Close;
		if (YesAction != null)
			yesButton.OnLeftClick += YesAction;

		dialog.Append(yesButton);

		var noButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.105")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = ShowYesDontShowAgainButton ? 0.6f : 0.85f,
			HAlign = 0.85f,
		}.WithFadedMouseOver();
		noButton.OnLeftClick += Close;
		if (NoAction != null)
			yesButton.OnLeftClick += NoAction;

		dialog.Append(noButton);

		if (!ShowYesDontShowAgainButton)
			return;

		var yesDontAskAgainButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.YesDontAskAgain")) {
			TextColor = Color.White,
			Width = new StyleDimension(0f, 2f / 3f),
			Height = { Pixels = 40 },
			VAlign = 0.95f,
			HAlign = 0.5f,
		}.WithFadedMouseOver();
		yesDontAskAgainButton.OnLeftClick += Close;
		if (YesDontShowAgainAction != null)
			yesDontAskAgainButton.OnLeftClick += YesDontShowAgainAction;

		dialog.Append(yesDontAskAgainButton);
	}

	public void Close()
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		Remove();
	}

	private void Close(UIMouseEvent evt, UIElement listeningElement)
	{
		Close();
	}
}