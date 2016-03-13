using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExplorerBag : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Explorer's Bag";
			item.width = 20;
			item.height = 20;
			item.toolTip = "Right click for goodies!";
			item.rare = 2;
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void RightClick(Player player)
		{
			player.QuickSpawnItem(ItemID.LifeCrystal, 15);
			player.QuickSpawnItem(ItemID.LifeFruit, 20);
			player.QuickSpawnItem(ItemID.ManaCrystal, 9);
			player.QuickSpawnItem(ItemID.AnkhShield);
			player.QuickSpawnItem(ItemID.FrostsparkBoots);
			player.QuickSpawnItem(ItemID.LavaWaders);
			player.QuickSpawnItem(ItemID.CellPhone);
			player.QuickSpawnItem(ItemID.SuspiciousLookingTentacle);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem", 10);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}