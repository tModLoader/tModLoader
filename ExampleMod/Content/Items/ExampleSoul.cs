using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleSoul : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Soul of Exampleness");
			Tooltip.SetDefault("'The essence of example creatures'");
			// Registers a vertical animation with 4 frames and each one will last 5 ticks (1/12 second)
			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 4));
			ItemID.Sets.AnimatesAsSoul[item.type] = true;
			ItemID.Sets.ItemIconPulse[item.type] = true;
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.maxStack = 999;
			item.value = 1000;
			item.rare = ItemRarityID.Orange;
		}

		public override void PostUpdate() => Lighting.AddLight(item.Center, Color.WhiteSmoke.ToVector3() * 0.55f * Main.essScale);
	}

	// todo: implement
	// public class SoulGlobalNPC : GlobalNPC
	// {
	// 	public override void NPCLoot(NPC npc) {
	// 		if (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].GetModPlayer<ExamplePlayer>().ZoneExample) {
	// 			Item.NewItem(npc.getRect(), ItemType<ExampleSoul>());
	// 		}
	// 	}
	// }
}