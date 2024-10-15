using ExampleMod.Common.Configs.ModConfigShowcases;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleFullscreenUI
{
	/// <summary>
	/// This is an example of an in-game fullscreen UI.
	/// This UI is shown and managed using the IngameFancyUI class. Since we are using IngameFancyUI, we do not need to write code to Update or Draw a UserInterface, unlike other UI. Since IngameFancyUI is used for non-gameplay fullscreen UI, it  prevents later interface layers from drawing. Vanilla examples of this sort of UI include the bestiary, emote menu, and settings menus.
	/// </summary>
	internal class ExampleFullscreenUI : UIState
	{
		public static readonly ExampleFullscreenUI instance = new ExampleFullscreenUI();

		private UIText itemDefinitionMessage;

		public override void OnInitialize() {
			var panel = new UIPanel() {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Width = new(400, 0f),
				Height = new(240, 0f)
			};
			Append(panel);

			var header = new UIText("Example Fullscreen UI") {
				IsWrapped = true,
				Width = StyleDimension.Fill,
				HAlign = 0.5f
			};
			panel.Append(header);

			var description = new UIText("This is an example fullscreen UI, notice how other UI is hidden.") {
				Top = new(40f, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(description);

			itemDefinitionMessage = new UIText(GetItemDefinitionMessageText()) {
				TextColor = Color.Orange,
				Top = new(110f, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(itemDefinitionMessage);

			var backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f) {
				TextColor = Color.Red,
				VAlign = 1f,
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			backButton.WithFadedMouseOver();
			backButton.OnLeftClick += BackButton_OnLeftClick;
			panel.Append(backButton);

			var openConfigButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModsOpenConfig"), 0.7f);
			openConfigButton.CopyStyle(backButton);
			openConfigButton.HAlign = 0.5f;
			openConfigButton.WithFadedMouseOver();
			openConfigButton.OnLeftClick += OpenConfigButton_OnLeftClick;
			panel.Append(openConfigButton);

			// TODO: Use this for a ModConfig.Save example later
			var otherButton = new UITextPanel<string>("TBD", 0.7f);
			otherButton.CopyStyle(backButton);
			otherButton.HAlign = 1f;
			otherButton.BackgroundColor = Color.Purple * 0.7f;
			/*
			otherButton.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
			otherButton.OnLeftClick += OtherButton_OnLeftClick;
			*/
			panel.Append(otherButton);
		}

		public override void OnActivate() {
			UpdateItemDefinitionMessageText();
		}

		private void BackButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			IngameFancyUI.Close();
		}

		private void OpenConfigButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			// We can use ModContent.GetInstance<ModConfigClassHere>().Open() to open a specific ModConfig UI.
			// This example, however, scrolls to a specific item in the ModConfig and also runs code after the ModConfig UI is closed.
			ModContent.GetInstance<ModConfigShowcaseDataTypes>().Open(onClose: () => {
				// Re-open this UI when the user exits the ModConfig menu.
				IngameFancyUI.OpenUIState(this);
			}, scrollToOption: nameof(ModConfigShowcaseDataTypes.itemDefinitionExample), centerScrolledOption: true);

			// If we want to scroll to the header of an option instead, prepend "Header:"
			// ModContent.GetInstance<ModConfigShowcaseLabels>().Open(scrollToOption: $"Header:{nameof(ModConfigShowcaseLabels.TypicalHeader)}");
		}

		private void UpdateItemDefinitionMessageText() {
			itemDefinitionMessage.SetText(GetItemDefinitionMessageText());
		}

		private string GetItemDefinitionMessageText() {
			ModConfigShowcaseDataTypes config = ModContent.GetInstance<ModConfigShowcaseDataTypes>();
			var itemDefinition = config.itemDefinitionExample;
			string configEntryLabel = Language.GetTextValue(config.GetLocalizationKey("itemDefinitionExample.Label"));
			if (itemDefinition.Type == ItemID.None || itemDefinition.IsUnloaded) {
				return $"\"{configEntryLabel}\" is set to nothing";
			}
			else {
				return $"\"{configEntryLabel}\" is set to \"{itemDefinition.DisplayName}\": [i:{itemDefinition.Type}]";
			}
		}

	}
}
