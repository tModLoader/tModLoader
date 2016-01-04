using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod;
using ExampleMod.NPCs;

namespace ExampleMod.Buffs
{
	public class EtherealFlames : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[this.Type] = "Ethereal Flames";
			Main.buffTip[this.Type] = "Losing life";
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			longerExpertDebuff = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			((ExamplePlayer)player.GetModPlayer(mod, "ExamplePlayer")).eFlames = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			((ExampleNPCInfo)npc.GetModInfo(mod, "ExampleNPCInfo")).eFlames = true;
		}
	}
}
