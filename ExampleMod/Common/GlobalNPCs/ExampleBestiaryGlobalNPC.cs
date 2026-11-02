using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.GlobalNPCs;

/// <summary>
/// Showcases adding a custom bestiary UI element (IBestiaryInfoElement) to a pre-existing Bestiary entry.
/// </summary>
public class ExampleBestiaryGlobalNPC : GlobalNPC
{
	// An example of adding additional flavor text to a bestiary entry!
	private class ImportantFlavorTextElement : IBestiaryInfoElement, IBestiaryPrioritizedElement, ICategorizedBestiaryInfoElement
	{
		// 1 so that it gets placed above the normal flavor text!
		public float OrderPriority => 1f;

		// Puts this element in the same category as the flavor text box, instead of at the bottom.
		public UIBestiaryEntryInfoPage.BestiaryInfoCategory ElementCategory => UIBestiaryEntryInfoPage.BestiaryInfoCategory.FlavorText;

		public UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
			if (info.UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0) {
				return null;
			}

			// This code mostly adapted from vanilla Bestiary code
			UIPanel backPanel = new(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, customBarSize: 7) {
				IgnoresMouseInteraction = true,
				Width = StyleDimension.FromPixelsAndPercent(-11f, 1f),
				Height = StyleDimension.FromPixels(30f),
				BackgroundColor = new Color(43, 56, 101),
				BorderColor = Color.Transparent,
				Left = StyleDimension.FromPixels(-8f),
				HAlign = 1f
			};
			backPanel.SetPadding(0f);

			UIText importantFlavorTextElement = new UIText(ModContent.GetInstance<ExampleMod>().GetLocalization("Bestiary.ImportantFlavorText")) {
				HAlign = 0.5f,
				VAlign = 0.5f,
				TextColor = Color.Red
			};
			backPanel.Append(importantFlavorTextElement);

			return backPanel;
		}
	}

	public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		// Add our "important" flavor text to every bestiary entry!
		bestiaryEntry.Info.Add(new ImportantFlavorTextElement());
	}
}