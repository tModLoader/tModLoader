using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.NPCs
{
	public class ExampleGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool HasBeenHitByPlayer = false;

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit){
			if(projectile.owner != 255){
				HasBeenHitByPlayer = true;
			}
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit) {
			HasBeenHitByPlayer = true;
		}

		//If the merchant has been hit by a player, they will double their sell price
		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			if (Main.LocalPlayer.talkNPC == -1 || !Main.npc[Main.LocalPlayer.talkNPC].GetGlobalNPC<ExampleGlobalNPC>().HasBeenHitByPlayer) {
				return;
			}

			foreach (Item item in shop.item)
			{
				int value = item.shopCustomPrice ?? item.value;
				item.shopCustomPrice = value * 2;
			}
		}

		// The data can't be shared on multiplayer for now

		public override void SaveData(NPC npc, TagCompound tag) {
			if (HasBeenHitByPlayer && npc.townNPC) { // No need to save if a non-town NPC has been hit by a player
				tag.Add("HasBeenHitByPlayer", true);
			}
		}

		public override void LoadData(NPC npc, TagCompound tag) {
			HasBeenHitByPlayer = tag.ContainsKey("HasBeenHitByPlayer");
		}
	}
}