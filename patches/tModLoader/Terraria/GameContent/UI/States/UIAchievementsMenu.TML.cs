using System;
using Microsoft.Xna.Framework;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.GameContent.UI.States;

public partial class UIAchievementsMenu : UIState, IHaveBackButtonCommand
{
	public UIState PreviousUIState { get; set; } // Unused interface property, manual logic in HandleBackButtonUsage instead

	private UIImage blockInput;
	private UIInputTextField filterTextBox;
	UITextPanel<LocalizedText> resetAchievementsButton;
	private UIPanel achievementResetAreYouSure;
	UITextPanel<LocalizedText> yesButton;
	UITextPanel<LocalizedText> noButton;
	private bool moddedOnly = false;

	private void ResetAchievements(UIMouseEvent evt, UIElement listeningElement)
	{
		CloseAchievementConfirm(evt, listeningElement);
		Main.Achievements.ClearAll();
		Main.menuMode = 0;
		IngameFancyUI.Close();
		SoundEngine.PlaySound(SoundID.MenuClose);
	}

	private void ResetAchievementsConfirm(UIMouseEvent evt, UIElement listeningElement)
	{
		blockInput = new UIImage(TextureAssets.Extra[190]) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = Color.Black * 0.5f,
			ScaleToFit = true
		};
		blockInput.Width = StyleDimension.Fill;
		blockInput.Height = StyleDimension.Fill;
		blockInput.OnLeftClick += CloseAchievementConfirm;
		Append(blockInput);

		achievementResetAreYouSure = new UIPanel();
		achievementResetAreYouSure.Width.Set(400f, 0f);
		achievementResetAreYouSure.Height.Set(300, 0f);
		achievementResetAreYouSure.VAlign = 0.5f;
		achievementResetAreYouSure.HAlign = 0.5f;
		Append(achievementResetAreYouSure);

		UITextPanel<LocalizedText> areYouSureText = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.AchievementsResetConfirm"), 0.6f, large: true);
		areYouSureText.HAlign = 0.5f;
		areYouSureText.SetPadding(13f);
		areYouSureText.Top.Set(-33, 0f);

		areYouSureText.BackgroundColor = new Color(73, 94, 171);
		achievementResetAreYouSure.Append(areYouSureText);

		string text = FontAssets.ItemStack.Value.CreateWrappedText(Language.GetText("tModLoader.AchievementsResetConfirmTooltip").Value, 310, Language.ActiveCulture.CultureInfo);
		UITextPanel<string> areYouSureDescription = new UITextPanel<string>(text, 1f, large: false);
		areYouSureDescription.HAlign = 0.5f;
		areYouSureDescription.Top.Set(20, 0f);
		areYouSureDescription.SetPadding(13f);
		areYouSureDescription.Width.Set(-10, 1);
		areYouSureDescription.Height.Set(-50, 0.9f);
		achievementResetAreYouSure.Append(areYouSureDescription);

		// Confirm Button
		yesButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.AchievementsReset"), 0.7f, large: true);
		yesButton.Width.Set(0, 0.5f);
		yesButton.Height.Set(40f, 0f);
		yesButton.VAlign = 1;
		yesButton.HAlign = 1;
		yesButton.OnMouseOver += FadedMouseOver;
		yesButton.OnMouseOut += FadedMouseOut;
		yesButton.OnLeftClick += ResetAchievements;
		achievementResetAreYouSure.Append(yesButton);

		// Cancel Button
		noButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Cancel"), 0.7f, large: true);
		noButton.Width.Set(-10, 0.5f);
		noButton.Height.Set(40f, 0f);
		noButton.VAlign = 1;
		noButton.OnMouseOver += FadedMouseOver;
		noButton.OnMouseOut += FadedMouseOut;
		noButton.OnLeftClick += CloseAchievementConfirm;
		achievementResetAreYouSure.Append(noButton);

		SoundEngine.PlaySound(SoundID.MenuOpen);
		UILinkPointNavigator.ChangePoint(3009); // noButton
	}

	private void SetupGamepadPoints_TML(int startPointID, int currentPointID, UILinkPoint uILinkPoint_BackPanel)
	{
		currentPointID++;
		UILinkPointNavigator.SetPosition(currentPointID, resetAchievementsButton.GetInnerDimensions().ToRectangle().Center.ToVector2());
		UILinkPoint uILinkPoint_ResetAchievementsButton = UILinkPointNavigator.Points[currentPointID];
		uILinkPoint_ResetAchievementsButton.Left = startPointID;
		uILinkPoint_ResetAchievementsButton.Up = startPointID + 1;

		uILinkPoint_BackPanel.Right = currentPointID;

		if (blockInput != null && HasChild(blockInput)) {
			currentPointID++;
			UILinkPointNavigator.SetPosition(currentPointID, yesButton.GetInnerDimensions().ToRectangle().Center.ToVector2());
			UILinkPoint uILinkPoint_YesButton = UILinkPointNavigator.Points[currentPointID];
			uILinkPoint_YesButton.Left = currentPointID + 1;

			currentPointID++;
			UILinkPointNavigator.SetPosition(currentPointID, noButton.GetInnerDimensions().ToRectangle().Center.ToVector2());
			UILinkPoint uILinkPoint_NoButton = UILinkPointNavigator.Points[currentPointID];
			uILinkPoint_NoButton.Right = currentPointID - 1;
		}
		UILinkPointNavigator.Shortcuts.FANCYUI_HIGHEST_INDEX = currentPointID;
	}

	private void CloseAchievementConfirm(UIMouseEvent evt, UIElement listeningElement)
	{
		RemoveChild(blockInput);
		RemoveChild(achievementResetAreYouSure);
		blockInput = null;
		achievementResetAreYouSure = null;
		SoundEngine.PlaySound(SoundID.MenuClose);
		UILinkPointNavigator.ChangePoint(3000); // _backpanel
	}

	private void ClearSearchField(UIMouseEvent evt, UIElement listeningElement) => filterTextBox.Text = "";

	private bool PassSearchFilter(Achievement achievement)
	{
		string searchText = filterTextBox.Text ?? string.Empty; // Get the search text
		string friendlyName = !achievement.Hidden ? achievement.FriendlyName.Value : string.Empty;
		string description = !achievement.Hidden ? achievement.Description.Value : string.Empty;
		string modName = achievement.ModAchievement?.Mod.DisplayName ?? string.Empty;

		return friendlyName.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) || description.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) || modName.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
	}

	// Note that Escape key while in-game won't call this without the additional code in Draw.
	public void HandleBackButtonUsage()
	{
		if (blockInput != null && HasChild(blockInput)) {
			CloseAchievementConfirm(null, null);
			return;
		}
		Main.menuMode = 0;
		IngameFancyUI.Close();
	}

	private void ToggleFilterModded(UIMouseEvent evt, UIElement listeningElement)
	{
		moddedOnly = !moddedOnly;
		FilterList(evt, listeningElement);
	}
}
