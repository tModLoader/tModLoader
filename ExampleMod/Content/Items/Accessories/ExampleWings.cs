using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Wings)]
	public class ExampleWings : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded wing.");

			// These wings use the same values as the solar wings
			// Fly time: 180 ticks = 3 seconds
			// Fly speed: 9
			// Acceleration multiplier: 2.5
			ArmorIDs.Wing.Sets.Stats[item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}

		public override void SetDefaults() {
			item.width = 22;
			item.height = 20;
			item.value = 10000;
			item.rare = ItemRarityID.Green;
			item.accessory = true;
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(Mod);
			recipe.AddIngredient(ItemType<ExampleItem>(), 60);
			recipe.AddTile(TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}