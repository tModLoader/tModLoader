using ExampleMod.Common.Configs.ModConfigShowcases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace ExampleMod.Common.Configs.CustomUI
{
	/// <summary>
	/// This is a completely custom UI for ModConfigShowcaseCustomUI that doesn't use UIModConfig.
	/// See ModConfigShowcaseCustomUI for how it is set.
	/// Heacily based on ExampleFullscreenUI.
	/// Note that this is a toy example.
	/// </summary>
	public class ExampleCustomConfigUI : UIState, IConfigUI, IHaveBackButtonCommand
	{
		// These 2 fields are required by IConfigUI
		public int UpdateCount { get; private set; }
		public string ConfigElementTooltip { get; set; }

		public UIState PreviousUIState { get; set; } // Unused

		private static LocalizedText ClearNotificationText { get; set; }

		private UIModConfig dummyUI; // Used to call into APIs that we don't want to reimplement

		private ModConfigShowcaseCustomUI config; // The actual config
		private ModConfigShowcaseCustomUI pendingConfig; // The clone we make changes to

		private int configElementTop;
		private bool refreshQueued;
		private UIPanel panel;
		private List<Tuple<UIElement, UIElement>> configElements = [];
		private UIText configMessage;

		public ExampleCustomConfigUI() {
			var mod = ModContent.GetInstance<ExampleMod>();
			ClearNotificationText = mod.GetLocalization("UI.ExampleCustomConfigUI.ClearNotification");
		}

		public override void OnInitialize() {
			config = ModContent.GetInstance<ModConfigShowcaseCustomUI>();
			dummyUI = new UIModConfig();
			dummyUI.SetConfig(config);

			CreateUI();
		}

		public override void OnActivate() {
			UpdateCount = 0;
			pendingConfig = (ModConfigShowcaseCustomUI)ConfigManager.GeneratePopulatedClone(config);

			RefreshUI(refreshNow: true);
		}

		public override void Update(GameTime gameTime) {
			if (refreshQueued) {
				RefreshUI(refreshNow: true);
				refreshQueued = false;
			}

			base.Update(gameTime);

			UpdateCount++;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			ConfigElementTooltip = null;

			base.Draw(spriteBatch);

			if (!string.IsNullOrEmpty(ConfigElementTooltip))
				UICommon.TooltipMouseText(ConfigElementTooltip);
		}

		private void CreateUI() {
			panel = new UIPanel() {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Width = new(500, 0f),
				Height = new(0, 0f),
				BackgroundColor = UICommon.DefaultUIBlue,
			};
			Append(panel);

			int top = 0;
			var header = new UIText(config.DisplayName.Value) {
				IsWrapped = true,
				Width = StyleDimension.Fill,
				HAlign = 0.5f
			};
			panel.Append(header);
			top += 40;

			// Add space for config elements
			configElementTop = top;
			top += 30 + 30 + 5 + 10;

			configMessage = new UIText(Language.GetTextValue("tModLoader.ModConfigNotification")) {
				Top = new(top, 0f),
				Left = new(0f, 0f),
				TextOriginX = 0f,
				Width = StyleDimension.Fill
			};
			panel.Append(configMessage);
			top += 30;

			var backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f),
				HAlign = 0f,
			};
			backButton.WithFadedMouseOver();
			backButton.OnLeftClick += BackButton_OnLeftClick;
			panel.Append(backButton);

			var saveButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigSaveConfig"), 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f),
				HAlign = 0.5f,
			};
			saveButton.WithFadedMouseOver();
			saveButton.OnLeftClick += SaveButton_OnLeftClick;
			panel.Append(saveButton);

			var clearMessageButton = new UITextPanel<LocalizedText>(ClearNotificationText, 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f),
				HAlign = 1f,
			};
			clearMessageButton.WithFadedMouseOver();
			clearMessageButton.OnLeftClick += ClearMessageButton_OnLeftClick;
			panel.Append(clearMessageButton);
			top += 40;

			panel.Height.Pixels = top + panel.PaddingTop + panel.PaddingBottom;
		}

		private void BackButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			HandleBackButtonUsage();
		}

		private void SaveButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			config.SaveChanges(pendingConfig, SetMessage, silent: true);
			pendingConfig = (ModConfigShowcaseCustomUI)ConfigManager.GeneratePopulatedClone(config);
			RefreshUI();
		}

		private void ClearMessageButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			ClearMessage();
		}

		public void HandleBackButtonUsage() {
			dummyUI.HandleBackButtonUsage();
		}

		private void RefreshUI(bool refreshNow = false) {
			if (!refreshNow) {
				refreshQueued = true;
				return;
			}

			// Read UIModConfig.WrapIt to understand this code a bit better

			foreach (var element in configElements) {
				element.Item1.Remove();
			}
			configElements.Clear();

			// Create ConfigElements to handle the editing of the config UI for us.
			int unused = 0;
			var boolField = new PropertyFieldWrapper(config.GetType().GetField(nameof(config.SomeBool)));
			var intField = new PropertyFieldWrapper(config.GetType().GetField(nameof(config.SomeInt)));

			var boolElement = dummyUI.WrapIt(null, ref unused, boolField, pendingConfig, unused);
			boolElement.Item1.Top.Pixels = configElementTop;
			var intElement = dummyUI.WrapIt(null, ref unused, intField, pendingConfig, unused);
			intElement.Item1.Top.Pixels = boolElement.Item1.Top.Pixels + boolElement.Item1.Height.Pixels + 5;

			panel.Append(boolElement.Item1);
			panel.Append(intElement.Item1);

			configElements = [boolElement, intElement];
		}

		public void SetMessage(string message, Color color) {
			string text = Language.GetText("tModLoader.ModConfigNotification") + message;
			float textWidth = FontAssets.MouseText.Value.MeasureString(text).X;
			float scale = Math.Clamp(configMessage.GetInnerDimensions().Width / textWidth, 0.25f, 1f);
			configMessage.TextColor = color; // Must be before SetText
			configMessage.SetText(text, scale, false);
		}

		private void ClearMessage() {
			SetMessage("", Color.White);
		}
	}
}
