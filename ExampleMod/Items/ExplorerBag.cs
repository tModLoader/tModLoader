using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	public class ExplorerBag : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Explorer's Bag");
			Tooltip.SetDefault("<right> for goodies!");
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.rare = ItemRarityID.Green;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void RightClick(Player player) {
			player.QuickSpawnItem(ItemID.LifeCrystal, 15);
			player.QuickSpawnItem(ItemID.LifeFruit, 20);
			player.QuickSpawnItem(ItemID.ManaCrystal, 9);
			player.QuickSpawnItem(ItemID.AnkhShield);
			player.QuickSpawnItem(ItemID.FrostsparkBoots);
			player.QuickSpawnItem(ItemID.LavaWaders);
			player.QuickSpawnItem(ItemID.CellPhone);
			player.QuickSpawnItem(ItemID.SuspiciousLookingTentacle);
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>(), 10);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}