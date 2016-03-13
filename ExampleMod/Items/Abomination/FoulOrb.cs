using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Abomination
{
	//imported from my tAPI mod because I'm lazy
	public class FoulOrb : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Foul Orb";
			item.width = 20;
			item.height = 20;
			item.maxStack = 20;
			item.toolTip = "The underworld would like this.";
			item.rare = 9;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.useSound = 44;
			item.consumable = true;
		}

		public override bool CanUseItem(Player player)
		{
			return NPC.downedPlantBoss && player.position.Y / 16f > Main.maxTilesY - 200 && !NPC.AnyNPCs(mod.NPCType("Abomination")) && !NPC.AnyNPCs(mod.NPCType("CaptiveElement")) && !NPC.AnyNPCs(mod.NPCType("CaptiveElement2"));
		}

		public override bool UseItem(Player player)
		{
			NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("Abomination"));
			Main.PlaySound(15, (int)player.position.X, (int)player.position.Y, 0);
			return true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.BeetleHusk);
			recipe.AddIngredient(null, "ScytheBlade");
			recipe.AddIngredient(null, "Icicle");
			recipe.AddIngredient(null, "Bubble");
			recipe.AddIngredient(ItemID.Ectoplasm, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this, 20);
			recipe.AddRecipe();
		}
	}
}