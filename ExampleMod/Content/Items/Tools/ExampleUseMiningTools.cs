using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleUseMiningTools : ModItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			// copy to 4711
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 22;
			Item.useTime = 14;
			Item.autoReuse = true;
			Item.width = 24;
			Item.height = 28;
			Item.damage = 12;
			Item.UseSound = SoundID.Item1;
			Item.DamageType = DamageClass.Melee;
		}

		public override void MiningUsage(Player player, int targetX, int targetY) {
			if(player.statLife > 200) { //Mine a 3x3 area only when player's current health is below 200.
				return;
			}

			for (int i = targetX - 1; i <= targetX + 1; i++) {
				for (int j = targetY - 1; j <= targetY + 1; j++) {
					bool isStoneBlock = Main.tile[i, j].TileType == TileID.Stone;
					if(isStoneBlock == false)
						continue; //This tool only picks StoneBlocks, so skip this tile.
					player.PickTile(i, j, 100); //Use PickTile to destroy blocks with a damage value of 100.
				}
			}
		}

		public override bool MiningUsageCondition(Player player, int targetX, int targetY) {
			bool isStoneBlock = Main.tile[targetX, targetY].TileType == TileID.Stone;
			return isStoneBlock; //Only performs a 3x3 area excavation when the mouse is pointing at a Stone Block.
		}

		public override bool IsAValidTool(Player player) {
			return true; // setting usage tool is true
		}
	}
}
