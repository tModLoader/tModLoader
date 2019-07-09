using ExampleMod.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	// Ethereal Flames is an example of a buff that causes constant loss of life.
	// See ExamplePlayer.UpdateBadLifeRegen and ExampleGlobalNPC.UpdateLifeRegen for more information.
	public class EtherealFlames : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Ethereal Flames");
			Description.SetDefault("Losing life");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			longerExpertDebuff = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExamplePlayer>().eFlames = true;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ExampleGlobalNPC>().eFlames = true;
		}
	}
}
