using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.NPCs
{
	public class ExampleGlobalNPC : GlobalNPC
	{
		// TODO, npc.netUpdate when this changes, and GlobalNPC gets a SendExtraAI hook
		public bool HasBeenHitByPlayer;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			// after ModNPC has run (lateInstantiation), check if the entity is a townNPC
			return lateInstantiation && entity.townNPC;
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) {
			if (projectile.owner != 255) {
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

			foreach (Item item in shop.item) {
				int value = item.shopCustomPrice ?? item.value;
				item.shopCustomPrice = value * 2;
			}
		}

		public override void SaveData(NPC npc, TagCompound tag) {
			if (HasBeenHitByPlayer) {
				tag.Add("HasBeenHitByPlayer", true);
			}
		}

		public override void LoadData(NPC npc, TagCompound tag) {
			HasBeenHitByPlayer = tag.ContainsKey("HasBeenHitByPlayer");
		}
	}
}