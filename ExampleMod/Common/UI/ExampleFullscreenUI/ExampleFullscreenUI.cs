using ExampleMod.Common.Configs.ModConfigShowcases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleFullscreenUI
{
	/// <summary>
	/// This is an example of an in-game fullscreen UI.
	/// This UI is shown and managed using the IngameFancyUI class. Since we are using IngameFancyUI, we do not need to write code to Update or Draw a UserInterface, unlike other UI. Since IngameFancyUI is used for non-gameplay fullscreen UI, it prevents later interface layers from drawing. Vanilla examples of this sort of UI include the bestiary, emote menu, and settings menus.
	/// To view this in-game, type "/fullscreenui" into chat (see ShowFullscreenUICommand.cs)
	///
	/// Another thing this example showcases is working with ModConfigs, such as displaying current config values, opening the Mod Configuration menu, and saving ModConfig changes via code. If your mod has a UI, it can be very useful for changes to that UI such as position or toggles to save automatically so the players choices persist when they play the game next.
	///
	///	This file showcases 2 approaches:
	/// The configA example (ModConfigShowcaseDataTypes) modifies the active config directly. These changes take effect immediately. With this approach you'll usually want to save the config whenever changes occur so the changes are remembered. This is useful for changes to client configs without any reload requirements, such as saving UI toggles or positioning.
	///
	/// The configB example (ModConfigShowcaseAcceptClientChanges) modifies a clone of the active config. Changes are not applied until the user clicks "Save Config". By modifying a clone, users can make several changes without affecting the active configs and then decide whether or not to save and apply them. This approach is necessary for server configs because the server is in charge of the configs and we need to avoid multiplayer clients having different server config values.
	/// </summary>
	internal class ExampleFullscreenUI : UIState, ILoadable
	{
		public static ExampleFullscreenUI instance;
		private static LocalizedText HeaderText { get; set; }
		private static LocalizedText DescriptionText { get; set; }
		private static LocalizedText RandomizeItemText { get; set; }
		private static LocalizedText NothingText { get; set; }
		private static LocalizedText IsSetText { get; set; }
		private static LocalizedText SaveValueText { get; set; }
		private static Asset<Texture2D> SomeBoolToggleTexture { get; set; }

		private UIText itemDefinitionMessage;
		private UICycleImage onlyChangeableDuringNightToggle;
		private UIText onlyChangeableDuringNightMessage;
		private UIText someNumberMessage;
		private UIText ConfigSaveStatusMessage;

		private ModConfigShowcaseDataTypes configA;
		private ModConfigShowcaseAcceptClientChanges configB;
		// This is a clone of the active config. Holds working changes before the config is saved.
		private ModConfigShowcaseAcceptClientChanges configB_pending;

		public void Load(Mod mod) {
			instance = this;
			HeaderText = mod.GetLocalization("UI.ExampleFullscreenUI.Header");
			DescriptionText = mod.GetLocalization("UI.ExampleFullscreenUI.Description");
			RandomizeItemText = mod.GetLocalization("UI.ExampleFullscreenUI.RandomizeItem");
			NothingText = mod.GetLocalization("UI.ExampleFullscreenUI.Nothing");
			IsSetText = mod.GetLocalization("UI.ExampleFullscreenUI.IsSet");
			SaveValueText = mod.GetLocalization("UI.ExampleFullscreenUI.SaveValue");
			SomeBoolToggleTexture = ModContent.Request<Texture2D>($"{GetType().Namespace.Replace('.', '/')}/SomeBoolToggle");
			// Since ModConfig are loaded before content, it is safe to access these in Load in this instance.
			configA = ModContent.GetInstance<ModConfigShowcaseDataTypes>();
			configB = ModContent.GetInstance<ModConfigShowcaseAcceptClientChanges>();
		}

		public void Unload() {
		}

		public override void OnInitialize() {
			var panel = new UIPanel() {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Width = new(450, 0f),
				// Using the top local variable, we will set Height at the end of the method to fit all elements neatly within the panel.
				Height = new(0, 0f),
				// The default background color is "new Color(63, 82, 151) * 0.7f;", which is slightly transparent.
				BackgroundColor = new Color(63, 82, 151)
			};
			Append(panel);

			int top = 0;
			var header = new UIText(HeaderText.Value) { // "Example Fullscreen UI"
				IsWrapped = true,
				Width = StyleDimension.Fill,
				HAlign = 0.5f
			};
			panel.Append(header);
			top += 40;

			var description = new UIText(DescriptionText.Value) { // "This is an example fullscreen UI, notice how other UI is hidden."
				Top = new(top, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(description);
			top += 60;

			AddSpacer(panel, ref top);

			itemDefinitionMessage = new UIText(GetItemDefinitionMessageText()) {
				TextColor = Color.Orange,
				Top = new(top, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(itemDefinitionMessage);
			top += 60;

			var randomizeButton = new UITextPanel<LocalizedText>(RandomizeItemText, 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			randomizeButton.HAlign = 0f;
			randomizeButton.WithFadedMouseOver();
			randomizeButton.OnLeftClick += RandomizeButton_OnLeftClick;
			panel.Append(randomizeButton);

			var openConfigAButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModsOpenConfig"), 0.7f);
			openConfigAButton.CopyStyle(randomizeButton);
			openConfigAButton.HAlign = 0.5f;
			openConfigAButton.WithFadedMouseOver();
			openConfigAButton.OnLeftClick += OpenConfigAButton_OnLeftClick;
			panel.Append(openConfigAButton);
			top += 40;

			AddSpacer(panel, ref top);

			onlyChangeableDuringNightToggle = new UICycleImage(SomeBoolToggleTexture, 2, 32, 32, 0, 0) {
				Left = new(0, 0f),
				Top = new(top, 0f),
			};
			onlyChangeableDuringNightToggle.OnLeftClick += OnlyChangeableDuringNightToggle_OnLeftClick;
			panel.Append(onlyChangeableDuringNightToggle);

			onlyChangeableDuringNightMessage = new UIText(GetOnlyChangeableDuringNightMessageText()) {
				Top = new(top + 6, 0f),
				Left = new(40f, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = new(-36, 1f)
			};
			panel.Append(onlyChangeableDuringNightMessage);
			top += 38;

			someNumberMessage = new UIText(GetSomeNumberMessageText()) {
				Top = new(top + 6, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(someNumberMessage);
			top += 32;

			var openConfigBButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModsOpenConfig"), 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			openConfigBButton.HAlign = 0.5f;
			openConfigBButton.WithFadedMouseOver();
			openConfigBButton.OnLeftClick += OpenConfigBButton_OnLeftClick;
			panel.Append(openConfigBButton);

			var saveConfigBButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigSaveConfig"), 0.7f);
			saveConfigBButton.CopyStyle(openConfigBButton);
			saveConfigBButton.HAlign = 1f;
			saveConfigBButton.BackgroundColor = Color.Purple * 0.7f;
			saveConfigBButton.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
			saveConfigBButton.OnLeftClick += SaveConfigBButton_OnLeftClick;
			panel.Append(saveConfigBButton);
			top += 40;

			AddSpacer(panel, ref top);

			ConfigSaveStatusMessage = new UIText(Language.GetTextValue("tModLoader.ModConfigNotification")) {
				Top = new(top, 0f),
				Left = new(0f, 0f),
				TextOriginX = 0f,
				Width = StyleDimension.Fill
			};
			panel.Append(ConfigSaveStatusMessage);
			top += 30;

			var backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f) {
				TextColor = Color.Red,
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			backButton.WithFadedMouseOver();
			backButton.OnLeftClick += BackButton_OnLeftClick;
			panel.Append(backButton);
			top += 40;

			panel.Height.Pixels = top + panel.PaddingTop + panel.PaddingBottom;
		}

		private void AddSpacer(UIPanel panel, ref int top) {
			var spacer = new UIHorizontalSeparator() {
				Width = StyleDimension.Fill,
				Color = new Color(89, 116, 213, 255),
				Top = new StyleDimension(top, 0f)
			};
			panel.Append(spacer);
			top += 16;
		}

		public override void OnActivate() {
			RefreshContents();

			UpdateConfigSaveStatusMessage("", Color.White);
		}

		public void RefreshContents() {
			configB_pending = (ModConfigShowcaseAcceptClientChanges)ConfigManager.GeneratePopulatedClone(configB);

			// This UI is only initialized when first viewed, so this might be null.
			if (onlyChangeableDuringNightToggle == null) {
				return;
			}

			onlyChangeableDuringNightToggle.CurrentState = configB_pending.OnlyChangeableDuringNight.ToInt();
			onlyChangeableDuringNightMessage.SetText(GetOnlyChangeableDuringNightMessageText());
			someNumberMessage.SetText(GetSomeNumberMessageText());

			itemDefinitionMessage.SetText(GetItemDefinitionMessageText());
		}

		private void RandomizeButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			// This action changes and saves the active config immediately.
			configA.itemDefinitionExample = new ItemDefinition(Main.rand.Next(ItemLoader.ItemCount));
			configA.SaveChanges(null, UpdateConfigSaveStatusMessage, silent: false);

			RefreshContents();
		}

		private void OpenConfigAButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			// We can use ModContent.GetInstance<ModConfigClassHere>().Open() to open a specific ModConfig UI.
			// This example, however, scrolls to a specific item in the ModConfig and also runs code after the ModConfig UI is closed.
			configA.Open(onClose: () => {
				// Re-open this UI when the user exits the ModConfig menu.
				IngameFancyUI.OpenUIState(this);
			}, scrollToOption: nameof(configA.itemDefinitionExample), centerScrolledOption: true);

			// If we want to scroll to the header of an option instead, prepend "Header:"
			// ModContent.GetInstance<ModConfigShowcaseLabels>().Open(scrollToOption: $"Header:{nameof(ModConfigShowcaseLabels.TypicalHeader)}");
		}

		private void OpenConfigBButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			configB.Open(onClose: () => {
				IngameFancyUI.OpenUIState(this);
			}, scrollToOption: nameof(configB.OnlyChangeableDuringNight), centerScrolledOption: true);
		}

		private void SaveConfigBButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			// Unlike with configA, we only save changes to the config if this button is clicked.
			var result = configB.SaveChanges(configB_pending, UpdateConfigSaveStatusMessage, silent: true, broadcast: false);

			// In this examples we set the silent parameter to true, letting us play custom sounds instead of the default sounds. See also ModConfigShowcaseAcceptClientChanges.HandleAcceptClientChangesReply
			if (result == ConfigSaveResult.Success) {
				SoundEngine.PlaySound(SoundID.CoinPickup);

				// In multiplayer, ModConfigShowcaseAcceptClientChanges.HandleAcceptClientChangesReply will call RefreshContents to update the UI rather than here in SaveConfigBButton_OnLeftClick.
				RefreshContents();
			}
		}

		private void BackButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			IngameFancyUI.Close();
		}

		internal void UpdateConfigSaveStatusMessage(string message, Color color) {
			// This method communicates the results of configA/B.SaveChanges to the user.
			// For simple configs, such as client side configs without any ReloadRequired logic, this wouldn't be necessary at all.
			// configA (ModConfigShowcaseDataTypes) should always succeed.
			// configB (ModConfigShowcaseAcceptClientChanges), however, is a server side config and has AcceptClientChanges logic. This will show the results of that operation and demonstrates that ModConfig.SaveChanges won't always succeed.

			// This UI is only initialized when first viewed, so this might be null.
			if (ConfigSaveStatusMessage == null) {
				return;
			}

			string text = Language.GetText("tModLoader.ModConfigNotification") + message;
			float textWidth = FontAssets.MouseText.Value.MeasureString(text).X;
			float scale = Math.Clamp(ConfigSaveStatusMessage.GetInnerDimensions().Width / textWidth, 0.25f, 1f);
			ConfigSaveStatusMessage.SetText(text, scale, false);
			ConfigSaveStatusMessage.TextColor = color;
		}

		private string GetItemDefinitionMessageText() {
			var itemDefinition = configA.itemDefinitionExample;
			string configEntryLabel = Language.GetTextValue(configA.GetLocalizationKey("itemDefinitionExample.Label"));
			if (itemDefinition.Type == ItemID.None || itemDefinition.IsUnloaded) {
				return NothingText.Format(configEntryLabel);
			}
			else {
				return IsSetText.Format(configEntryLabel, itemDefinition.DisplayName, itemDefinition.Type);
			}
		}

		private void OnlyChangeableDuringNightToggle_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			configB_pending.OnlyChangeableDuringNight = onlyChangeableDuringNightToggle.CurrentState == 1;
		}

		private string GetOnlyChangeableDuringNightMessageText() {
			string configEntryLabel = Language.GetTextValue(configB.GetLocalizationKey($"{nameof(configB.OnlyChangeableDuringNight)}.Label"));
			return SaveValueText.Format(configEntryLabel, configB.OnlyChangeableDuringNight);
		}

		private string GetSomeNumberMessageText() {
			string configEntryLabel = Language.GetTextValue(configB.GetLocalizationKey($"{nameof(configB.SomeNumber)}.Label"));
			return SaveValueText.Format(configEntryLabel, configB.SomeNumber);
		}
	}
}
