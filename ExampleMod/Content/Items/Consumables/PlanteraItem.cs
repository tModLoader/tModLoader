using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	public class PlanteraItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plantera");
			Tooltip.SetDefault("The wrath of the jungle");

			ItemID.Sets.SortingPriorityBossSpawns[item.type] = 12; // This helps sort inventory know that this is a boss summoning item.
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 20;
			item.value = 100;
			item.rare = ItemRarityID.Blue;
			item.useAnimation = 30;
			item.useTime = 30;
			item.useStyle = ItemUseStyleID.HoldUp;
			item.consumable = true;
		}

		public override bool CanUseItem(Player player) {
			return Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !NPC.AnyNPCs(NPCID.Plantera);
		}

		public override bool UseItem(Player player) {
			NPC.SpawnOnPlayer(player.whoAmI, NPCID.Plantera);

			// Avoid trying to call sounds on dedicated servers.
			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Roar, player.position, 0);
			}

			return true;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}