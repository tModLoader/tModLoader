using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	public class PlanteraItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plantera");
			Tooltip.SetDefault("The wrath of the jungle");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12; // This helps sort inventory know that this is a boss summoning Item.
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 20;
			Item.value = 100;
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
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

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}