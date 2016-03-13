using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class PlanteraItem : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Plantera";
			item.width = 20;
			item.height = 20;
			item.maxStack = 20;
			item.toolTip = "The wrath of the jungle";
			item.value = 100;
			item.rare = 1;
			item.useAnimation = 30;
			item.useTime = 30;
			item.useStyle = 4;
			item.consumable = true;
		}

		public override bool CanUseItem(Player player)
		{
			return Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !NPC.AnyNPCs(NPCID.Plantera);
		}

		public override bool UseItem(Player player)
		{
			NPC.SpawnOnPlayer(player.whoAmI, NPCID.Plantera);
			Main.PlaySound(15, (int)player.position.X, (int)player.position.Y, 0);
			return true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this, 20);
			recipe.AddRecipe();
		}
	}
}