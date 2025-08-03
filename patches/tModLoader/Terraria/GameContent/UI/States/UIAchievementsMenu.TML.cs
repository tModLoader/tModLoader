using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.OS.Windows;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;
namespace Terraria.GameContent.UI.States;

public partial class UIAchievementsMenu : UIState
{
	private void ResetAchievements(UIMouseEvent evt, UIElement listeningElement)
	{
		CloseAchievementConfirm(evt, listeningElement);
		Main.Achievements.ClearAll();
		Main.menuMode = 0;
		IngameFancyUI.Close();
	}

	private void ResetAchievementsConfirm(UIMouseEvent evt, UIElement listeningElement)
	{
		blockingPanel = new UIPanel(blockingBackground, blockingBorder);
		blockingPanel.Width.Set(0, 1f);
		blockingPanel.Height.Set(0, 1f);
		blockingPanel.HAlign = 0.5f;
		blockingPanel.VAlign = 0.5f;
		blockingPanel.BackgroundColor = new Color(0, 0, 0, 125);
		blockingPanel.BorderColor = blockingPanel.BackgroundColor;
		blockingPanel.OnLeftClick += BlockInput;
		blockingPanel.OnMouseOver += BlockInput;
		Append(blockingPanel);

		achievementResetAreYouSure = new UIPanel();
		achievementResetAreYouSure.Width.Set(400f, 0f);
		achievementResetAreYouSure.Height.Set(300, 0f);
		achievementResetAreYouSure.VAlign = 0.5f;
		achievementResetAreYouSure.HAlign = 0.5f;
		Append(achievementResetAreYouSure);

		UITextPanel<LocalizedText> areYouSureText = new UITextPanel<LocalizedText>(Language.GetOrRegister("tModLoader.AchievementResetConfirm", () => "Are you sure? Reg"), 0.6f, large: true);
		areYouSureText.HAlign = 0.5f;
		areYouSureText.SetPadding(13f);
		areYouSureText.Top.Set(-33, 0f);

		areYouSureText.BackgroundColor = new Color(73, 94, 171);
		achievementResetAreYouSure.Append(areYouSureText);

		string text = FontAssets.ItemStack.Value.CreateWrappedText(Language.GetOrRegister("tModLoader.AchievementResetConfirmTooltip", () => "Resetting your achievements means you'll lose them permanently, are you sure you want to continue?").Value, 310, Language.ActiveCulture.CultureInfo);
		UITextPanel<string> areYouSureDesciption = new UITextPanel<string>(text, 1f, large: false);
		areYouSureDesciption.HAlign = 0.5f;
		areYouSureDesciption.Top.Set(20, 0f);
		areYouSureDesciption.SetPadding(13f);
		areYouSureDesciption.Width.Set(-10, 1);
		areYouSureDesciption.Height.Set(-50, 0.9f);
		achievementResetAreYouSure.Append(areYouSureDesciption);

		// Confirm Button
		UITextPanel<LocalizedText> yesButton = new UITextPanel<LocalizedText>(Language.GetOrRegister("tModLoader.AchievementsReset", () => "Reset"), 0.7f, large: true);
		yesButton.Width.Set(0, 0.5f);
		yesButton.Height.Set(40f, 0f);
		yesButton.VAlign = 1;
		yesButton.HAlign = 1;
		yesButton.OnMouseOver += FadedMouseOver;
		yesButton.OnMouseOut += FadedMouseOut;
		yesButton.OnLeftClick += ResetAchievements;
		achievementResetAreYouSure.Append(yesButton);

		// Cancel Button
		UITextPanel<LocalizedText> noButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Cancel"), 0.7f, large: true);
		noButton.Width.Set(-10, 0.5f);
		noButton.Height.Set(40f, 0f);
		noButton.VAlign = 1;
		noButton.OnMouseOver += FadedMouseOver;
		noButton.OnMouseOut += FadedMouseOut;
		noButton.OnLeftClick += CloseAchievementConfirm;
		achievementResetAreYouSure.Append(noButton);
	}

	private void CloseAchievementConfirm(UIMouseEvent evt, UIElement listeningElement)
	{
		RemoveChild(blockingPanel);
		RemoveChild(achievementResetAreYouSure);
		blockingPanel = null;
		achievementResetAreYouSure = null;
	}

	private void BlockInput(UIMouseEvent evt, UIElement listeningElement)
	{
	}

	private void FilterSearch(object sender, EventArgs e)
	{
		_achievementsList.Clear();
		string searchText = uISearchBar.Text?.ToLowerInvariant() ?? string.Empty; // Get the search text, ensuring it's lowercase

		foreach (UIAchievementListItem achievementElement in _achievementElements) {
			string friendlyName = achievementElement.GetAchievement().FriendlyName.Value.ToLowerInvariant(); // Convert to lowercase for case-insensitive comparison
			string modName = achievementElement.GetAchievement().ModAchievement != null ? achievementElement.GetAchievement().ModAchievement.Mod.DisplayName.ToLowerInvariant() : string.Empty; // Convert to lowercase for case-insensitive comparison

			if (friendlyName.Contains(searchText) || modName.Contains(searchText)) {
				_achievementsList.Add(achievementElement);
			}
		}

		Recalculate();
	}

	private void ToggleFilterModded(UIMouseEvent evt, UIElement listeningElement)
	{
		_filterModded = !_filterModded;
		FilterList(evt, listeningElement);
	}

}