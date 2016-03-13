using Terraria;
using Terraria.ModLoader;
using ExampleMod.NPCs;

namespace ExampleMod.Buffs
{
	public class EtherealFlames : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[Type] = "Ethereal Flames";
			Main.buffTip[Type] = "Losing life";
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			longerExpertDebuff = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<ExamplePlayer>(mod).eFlames = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetModInfo<ExampleNPCInfo>(mod).eFlames = true;
		}
	}
}
