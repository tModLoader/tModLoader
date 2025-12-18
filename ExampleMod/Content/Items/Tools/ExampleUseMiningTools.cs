using Newtonsoft.Json.Linq;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

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

		public override void UseMiningTools(Item item, Player player, ref Player.SpecialToolUsageSettings usageSettings) {
			base.UseMiningTools(item, player, ref usageSettings);
			usageSettings.IsAValidTool = true; // 设置他为特殊工具
			usageSettings.UsageCondition = Condition;
			usageSettings.UsageAction = ToolAction;
		}

		private void ToolAction(Player user, Item item, int targetX, int targetY) {
			for (int i = targetX - 1; i <= targetX + 1; i++) {
				for (int j = targetY - 1; j <= targetY + 1; j++) {
					user.PickTile(i, j, 100);
				}
			}
		}

		private bool Condition(Player user, Item item, int targetX, int targetY) {
			return true; // 无论如何都可以执行 ToolAction
		}
	}
}
