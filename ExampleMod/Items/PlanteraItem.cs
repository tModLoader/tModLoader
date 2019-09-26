using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	public class PlanteraItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plantera");
			Tooltip.SetDefault("The wrath of the jungle");
			ItemID.Sets.SortingPriorityBossSpawns[item.type] = 12; // This helps sort inventory know this is a boss summoning item.
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 20;
			item.value = 100;
			item.rare = 1;
			item.useAnimation = 30;
			item.useTime = 30;
			item.useStyle = 4;
			item.consumable = true;
		}

		public override bool CanUseItem(Player player) {
			return Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !NPC.AnyNPCs(NPCID.Plantera);
		}

		public override bool UseItem(Player player) {
			NPC.SpawnOnPlayer(player.whoAmI, NPCID.Plantera);
			Main.PlaySound(SoundID.Roar, player.position, 0);
			return true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<BossItem>(), 10);
			recipe.AddTile(TileType<Tiles.ExampleWorkbench>());
			recipe.SetResult(this, 20);
			recipe.AddRecipe();
		}
	}
}