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
		// public override bool Autoload(ref string name) {
		// return !GetInstance<ExampleConfigServer>().DisableExampleWings;
		// }

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded wing.");
			ArmorIDs.Wing.Sets.Stats[item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}

		public override void SetDefaults() {
			item.width = 22;
			item.height = 20;
			item.value = 10000;
			item.rare = ItemRarityID.Green;
			item.accessory = true;
		}

		// These wings use the same values as the solar wings
		// public override void UpdateAccessory(Player player, bool hideVisual) {
		// 	player.wingTimeMax = 180;
		// }

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		// public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) {
		// 	speed = 9f;
		// 	acceleration *= 2.5f;
		// }

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>(), 60);
			recipe.AddTile(TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}