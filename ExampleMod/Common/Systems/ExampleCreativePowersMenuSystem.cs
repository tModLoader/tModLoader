using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	/// <summary>
	/// Shows how to add a custom parent category and child buttons to Journey Mode's powers menu.
	/// </summary>
	public class ExampleCreativePowersMenuSystem : ModSystem
	{
		private bool exampleMainToggle;
		private bool exampleToggle;
		private float exampleMultiplierSliderValue = 0.5f;
		private float examplePercentSliderValue = 0.25f;

		public override void ModifyCreativePowersMenu(CreativePowerMenuEntries entries) {
			entries.AddToggle(
				"Mods.ExampleMod.CreativePowers.ExampleMainToggle",
				CreativePowersHelper.CreativePowerIconLocations.StopBiomeSpread,
				() => exampleMainToggle = !exampleMainToggle,
				() => exampleMainToggle
			);

			CreativePowerMenuCategory category = entries.AddCategory(
				"Mods.ExampleMod.CreativePowers.ExampleCategory",
				CreativePowersHelper.CreativePowerIconLocations.GameEvents
			);

			category.AddButton(
				"Mods.ExampleMod.CreativePowers.ExampleToggle",
				CreativePowersHelper.CreativePowerIconLocations.Godmode,
				() => exampleToggle = !exampleToggle,
				isSelected: () => exampleToggle
			);

			category.AddButton(
				"Mods.ExampleMod.CreativePowers.ExampleItemButton",
				CreativePowersHelper.CreativePowerIconLocations.ItemDuplication,
				GiveExampleItem
			);

			category.AddSlider(
				"Mods.ExampleMod.CreativePowers.ExampleMultiplierSlider",
				CreativePowersHelper.CreativePowerIconLocations.EnemySpawnRate,
				() => exampleMultiplierSliderValue,
				value => exampleMultiplierSliderValue = MathHelper.Clamp(value, 0f, 1f),
				settings => {
					settings.PanelWidth = 82f;
					settings.GetFilledColor = value => Color.Lerp(Color.LightSkyBlue, Main.OurFavoriteColor, value);
					settings.UseMultiplierHoverText(0f, 10f);
					settings.ClearLabels()
						.AddLabel("x10", 1f, 0f)
						.AddLabel("x1", 0.5f, 0.5f)
						.AddLabel("x0", 0f, 1f);
				}
			);

			category.AddSlider(
				"Mods.ExampleMod.CreativePowers.ExamplePercentSlider",
				CreativePowersHelper.CreativePowerIconLocations.RainStrength,
				() => examplePercentSliderValue,
				value => examplePercentSliderValue = MathHelper.Clamp(value, 0f, 1f),
				settings => {
					Asset<Texture2D> icon = ModContent.Request<Texture2D>("ExampleMod/Content/Items/ExampleItem");
					settings.FilledColor = Color.DeepSkyBlue;
					settings.UsePercentageHoverText();
					settings.ClearLabels()
						.AddIconLabel(icon, 1f, 0f)
						.AddIconLabel(icon, 0.5f, 0.5f)
						.AddIconLabel(icon, 0f, 1f);
				}
			);
		}

		private static void GiveExampleItem() {
			Player player = Main.LocalPlayer;
			player.QuickSpawnItem(player.GetSource_Misc("ExampleCreativePowerButton"), ModContent.ItemType<ExampleItem>());
		}
	}
}
