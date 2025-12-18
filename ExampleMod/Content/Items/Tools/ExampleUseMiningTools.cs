using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleUseMiningTools : ModItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			// clone to 4711
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

		public override void MiningUsage(Player user, int targetX, int targetY) {
			for (int i = targetX - 1; i <= targetX + 1; i++) {
				for (int j = targetY - 1; j <= targetY + 1; j++) {
					user.PickTile(i, j, 100);
				}
			}
		}

		public override bool MiningUsageCondition(Player player, int targetX, int targetY) {
			return true;
		}

		public override bool IsAValidTool(Player player) {
			return true; // setting usage tool is true
		}
	}
}
